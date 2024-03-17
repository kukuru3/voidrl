using System.Collections.Generic;
using Core.H3;
using K3.Hex;
using UnityEngine;
using UnityEngine.UIElements;

namespace Scanner.Atomship {
    internal class StructureEditor : MonoBehaviour {
        [SerializeField] private GameObject _blankHexPrefab;
        [SerializeField] private GameObject _directionPrefab;
        [SerializeField] private GameObject _connectorPrefab;

        [SerializeField] TMPro.TMP_Text     label;

        H3 cursor3d = default;
        PrismaticHexDirection direction = new PrismaticHexDirection(HexDir.Top, 0);

        GameObject posCursorGO;
        GameObject dirCursorGO;

        private void Start() {
            posCursorGO = Instantiate(_blankHexPrefab);
            dirCursorGO = Instantiate(_directionPrefab);
        }

        private void Update() {
            if (Input.GetKeyDown(KeyCode.W)) cursor3d += new H3(0,1, 0);
            if (Input.GetKeyDown(KeyCode.S)) cursor3d += new H3(0,-1, 0);
            if (Input.GetKeyDown(KeyCode.D)) cursor3d += new H3(1, 0, 0);
            if (Input.GetKeyDown(KeyCode.A)) cursor3d += new H3(-1,0, 0);
            if (Input.GetKeyDown(KeyCode.Z)) cursor3d += new H3(0,0,-1);
            if (Input.GetKeyDown(KeyCode.X)) cursor3d += new H3(0,0,1);

            if (Input.GetKeyDown(KeyCode.Q)) { 
                if (direction.longitudinal != 0) direction = new PrismaticHexDirection(HexDir.Top, 0);
                else direction = direction.RotatedRadially(-1);
            }
            if (Input.GetKeyDown(KeyCode.E)) { 
                if (direction.longitudinal != 0) direction = new PrismaticHexDirection(HexDir.Top, 0);
                else direction = direction.RotatedRadially(1);
            }

            if (Input.GetKeyDown(KeyCode.F)) direction = new PrismaticHexDirection(HexDir.None, -1);
            if (Input.GetKeyDown(KeyCode.G)) direction = new PrismaticHexDirection(HexDir.None,  1);

            PositionCursors();

        }
        void PositionCursors() {
            var pose = new H3Pose(cursor3d.hex, cursor3d.zed, 0);
            var p = pose.CartesianPose();
            posCursorGO.transform.SetPositionAndRotation(p.position, Quaternion.identity);

            // position direction cursor.
            var xy = Hexes.HexToPixel(direction.radial.Offset(), GridTypes.FlatTop, 0.5f);
            var z = direction.longitudinal * HexUtils.CartesianZMultiplier;

            var v = new Vector3(xy.x, xy.y, z);
            Quaternion rot = Quaternion.identity;

            if (v.sqrMagnitude > float.Epsilon) { 
                var up = Vector3.up; if (v.normalized.y > 0.99f) up = Vector3.forward;
                rot = Quaternion.LookRotation(v, up);
            }

            dirCursorGO.transform.SetPositionAndRotation(p.position + v, rot);
        }
    
        // ui buttons: 
        // - create node at this position
        // - delete node at this position
        // - create connection at this marker.

        // validation: 
        // - connections that originate from a blank node are called ORPHAN and are deleted on validation.
    }

    public class ModelIO {
        public const string Path = "Assets/Data";
    }

    
    [System.Serializable]
    public class HexModelDefinition {
        public string identity;
        public List<HexNode> nodes = new();
        public List<HexConnector> connections = new();

        [System.Serializable] public class HexNode {
            public string specialID;
            public H3 hex;
            public List<string> attributes = new();
        }

        [System.Serializable] public class HexConnector {
            public H3 sourceHex;
            public PrismaticHexDirection direction;
            public List<string> attributes = new();
        }
    }

    

}