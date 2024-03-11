using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Core.h3x;
using K3.Hex;
using UnityEngine;

namespace Scanner.Atomship {

    internal class ModuleEditor : MonoBehaviour {

        [SerializeField] GameObject[] partPrefabs;
        [SerializeField] GameObject[] connectorPrefabs; // 0: allowed, 1: forbidden, 2: primary, 3: implicit
        [SerializeField] GameObject prohibitorPrefab;
        [SerializeField] Camera editorCamera;

        [SerializeField] Transform root;
        
        public StructureModel CurrentModel { get; private set; }

        [SerializeField][Range(0.5f, 2f)] float radialDistance = 1f;
        [SerializeField][Range(0.5f, 2f)] float zedDistanceMultiplier = 1f;

        internal void CreateNew() {
            CurrentModel = new StructureModel() {
                features = new List<Feature> {
                    new Feature {
                        type = FeatureTypes.Part,
                        coords = new Hex3(0, 0, 0),
                    },
                },
            };

            SyncModel();
        }

        // q+ : up-right
        // r+ : up

        private void Start() {

            var hr3 = Mathf.Sqrt(3f) / 2f;
            var margin = 0.1f;

            _poses.Add(QRZDir.Top,      RadialPose(0,   hr3*radialDistance - margin));
            _poses.Add(QRZDir.RightTop, RadialPose(60,  hr3*radialDistance - margin));
            _poses.Add(QRZDir.RightBot, RadialPose(120, hr3*radialDistance - margin));
            _poses.Add(QRZDir.Bottom,   RadialPose(180, hr3*radialDistance - margin));
            _poses.Add(QRZDir.LeftBot,  RadialPose(240, hr3*radialDistance - margin));
            _poses.Add(QRZDir.LeftTop,  RadialPose(300, hr3*radialDistance - margin));

            _poses.Add(QRZDir.Forward, new Pose(
                new Vector3(0,0, zedDistanceMultiplier/2-margin),
                Quaternion.Euler(90, 0,0)
            ));

            _poses.Add(QRZDir.Backward, new Pose(
                new Vector3(0,0, -zedDistanceMultiplier/2+margin),
                Quaternion.Euler(-90, 0,0)
            ));

            CreateNew();
        }

        Pose RadialPose(float angle, float distance) {
            var rad = Mathf.Deg2Rad * angle;
            var sin = Mathf.Sin(rad);
            var cos = Mathf.Cos(rad);
            return new Pose(
                new Vector3(sin * distance, cos * distance, 0),
                Quaternion.Euler(0,0, -angle)
            );
        }

        private void Update() {
            var mp = editorCamera.ScreenPointToRay(Input.mousePosition);
            var res = RaycastModel(mp);
            if (res.HasValue) {
                var shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

                if (Input.GetMouseButtonDown(0)) {
                    if (Input.GetKey(KeyCode.Delete)) TryCreateProhibition(res.Value.coords + res.Value.dir.QRZOffset());
                    else if (shift)     TryRemovePart(res.Value.coords, res.Value.dir);
                    else if (Input.GetKey(KeyCode.A)) SetAllForbiddenConnectionsPermissive(res.Value.coords);
                    else if (Input.GetKey(KeyCode.P)) SetPrimaryConnection(res.Value.coords, res.Value.dir);
                    else CycleConnection(res.Value.coords, res.Value.dir);

                } else if (Input.GetMouseButtonDown(2)) {
                    TryAddPart(res.Value.coords, res.Value.dir);
                } 
            }
        }

        private void SetAllForbiddenConnectionsPermissive(Hex3 coords) {
            foreach (var dir in HexExpansions.AllDirections) {
                var c = FindConnectionFeature(coords, dir, true);

                if (c == null) {                    
                    var ctype = InferConnectionType(coords, dir);
                    if (ctype == ConnectionTypes.Forbidden) { 
                        CurrentModel.features.Add(new Feature {
                            coords = coords,
                            direction = dir,
                            connType = ConnectionTypes.Allowed,
                            type = FeatureTypes.Connector,
                        });
                    }
                }
            }
            SyncModel();
        }

        private void TryCreateProhibition(Hex3 coords) {
            if (FindPartFeature(coords) != null) throw new System.Exception("already occupied");
             CurrentModel.features.Add(new Feature {
                 coords = coords,
                 type = FeatureTypes.ProhibitedSpace,
             });
            SyncModel();
        }

        void SetPrimaryConnection(Hex3 coords, QRZDir dir) {
             var conn = FindConnectionFeature(coords, dir, true);
            if (conn == null) {
                conn = new Feature {
                    type = FeatureTypes.Connector,
                    coords = coords,
                    direction = dir,
                };
                CurrentModel.features.Add(conn);
            }
            conn.connType = ConnectionTypes.Primary;
            SyncModel();
        }

        private void CycleConnection(Hex3 coords, QRZDir dir) {
            // var destination = value.coords + QRZOffset(value.dir);
            var existingConnection = FindConnectionFeature(coords, dir, true);
            if (existingConnection == null) {
                existingConnection = new Feature {
                    type = FeatureTypes.Connector,
                    coords = coords,
                    direction = dir,
                    connType = ConnectionTypes.Forbidden,
                };
                CurrentModel.features.Add(existingConnection);
            }
            existingConnection.connType = NextInCycle(existingConnection.connType);

            if (existingConnection.connType == ConnectionTypes.Forbidden) {
                CurrentModel.features.Remove(existingConnection);
            }

            SyncModel();
            
            ConnectionTypes NextInCycle(ConnectionTypes old) => old switch { 
                ConnectionTypes.Allowed => ConnectionTypes.Forbidden,
                _ => ConnectionTypes.Allowed,
            };
        }

        private void TryAddPart(Hex3 coords, QRZDir dir) {
            var q = coords + dir.QRZOffset();
            if (CurrentModel.features.Any(a => a.coords == q)) throw new Exception("Already occupied");
            CurrentModel.features.Add(new Feature {
                type = FeatureTypes.Part,
                coords = q,
            });
            SyncModel();
        }

        Feature FindConnectionFeature(Hex3 coords, QRZDir dir, bool symmetrical = false) { 
            var f = CurrentModel.features.FirstOrDefault(a => a.type == FeatureTypes.Connector && a.coords == coords && a.direction == dir);
            if (symmetrical) { 
                coords += dir.QRZOffset();
                dir = dir.Inverse();
                f ??= CurrentModel.features.FirstOrDefault(a => a.type == FeatureTypes.Connector && a.coords == coords && a.direction == dir);
            }
            return f;
        }

        Feature FindPartFeature(Hex3 coords) =>
            CurrentModel.features.FirstOrDefault(a => a.type == FeatureTypes.Part && a.coords == coords);

        Feature FindProhibitionFeature(Hex3 coords) => 
            CurrentModel.features.FirstOrDefault(a => a.type == FeatureTypes.ProhibitedSpace && a.coords == coords);
        private void TryRemovePart(Hex3 coords, QRZDir dir) { 
            if (coords == default) throw new Exception("Cannot remove the root module");
            var existingFeature = FindPartFeature(coords) ?? FindProhibitionFeature(coords);
            if (existingFeature == null) throw new Exception("No module or data to remove");
            CurrentModel.features.Remove(existingFeature);
            RemoveOrphans();
            SyncModel();
        }

        public (Hex3 coords, QRZDir dir, FeatureTypes t)? RaycastModel(Ray worldRay) {
            if (Physics.Raycast(worldRay, out var rayHit, 100f, 1 << 20, QueryTriggerInteraction.Collide)) {
                var f = FindFeatureOf(rayHit.collider.gameObject);
                if (f != null) {
                    var dir = HexExpansions.ComputeDirectionFromNormal(rayHit.normal);
                    return (f.coords, dir, f.type);
                }                
            }
            return null;
        }

        List<(Feature f, GameObject go)> featureViews = new();

        Feature FindFeatureOf(GameObject view) => featureViews.FirstOrDefault(a => a.go == view).f;

        void RemoveOrphans() {
            List<Feature> toRemove = new();
            foreach (var f in CurrentModel.features) {
                if (f.type == FeatureTypes.Connector) {
                    if (FindPartFeature(f.coords) == null) {
                        toRemove.Add(f);
                    }
                }
            }

            foreach (var item in toRemove) CurrentModel.features.Remove(item);
        }


        private void SyncModel() { 
            HexExpansions.ZedDistance = radialDistance * zedDistanceMultiplier;
            HexExpansions.RadialDistance = radialDistance;

            foreach (Transform child in root) Destroy(child.gameObject);

            featureViews.Clear();

            foreach (var feature in CurrentModel.features) {
                if (feature.type == FeatureTypes.Part) {
                    var prefab = partPrefabs[feature.graphicVariant];
                    var partView = Instantiate(prefab, root);
                    partView.transform.localPosition = feature.coords.Cartesian();
                    partView.transform.localRotation = Quaternion.identity;
                    partView.layer = 20;
                    partView.name = $"Part @ {feature.coords}";
                    featureViews.Add((feature, partView));
                } else if (feature.type == FeatureTypes.Connector) {
                    // Forbidden, Allowed, Implicit, Primary,
                    var prefab = connectorPrefabs[(int)feature.connType];
                    var connView = Instantiate(prefab, root);
                    connView.name = $"Connector ({feature.connType}) @ {feature.coords} => {feature.coords + feature.direction.QRZOffset()}";
                    var dir = feature.direction;

                    if (dir == QRZDir.Forward || dir == QRZDir.Backward)
                        connView.GetComponentInChildren<MeshRenderer>().material.color = Color.yellow;

                    var p = _poses[dir];
                    connView.transform.SetPositionAndRotation(feature.coords.Cartesian() + p.position, p.rotation);
                    featureViews.Add((feature, connView));
                } else if (feature.type == FeatureTypes.ProhibitedSpace) {
                    var prefab = prohibitorPrefab;
                    var view = Instantiate(prefab, root);
                    view.layer = 20;
                    view.name = $"Prohibitor {feature.coords}";
                    view.transform.SetPositionAndRotation(feature.coords.Cartesian(), Quaternion.identity);
                    featureViews.Add((feature, view));
                }
            }

            foreach (var partFeat in CurrentModel.features) {
                if (partFeat.type == FeatureTypes.Part) {
                    foreach (var dir in HexExpansions.AllDirections) {
                        var alreadyExtantConnectionFeature = FindConnectionFeature(partFeat.coords, dir, true);
                        if (alreadyExtantConnectionFeature == null) {
                            var ctype = InferConnectionType(partFeat.coords, dir);
                            var prefab = connectorPrefabs[(int)ctype];
                            var phantomView = Instantiate(prefab, root);
                            phantomView.name = $"(Phantom) Connector ({ctype}) @ {partFeat.coords} => {dir}";

                            var p = _poses[dir];
                            phantomView.transform.SetPositionAndRotation(partFeat.coords.Cartesian() + p.position, p.rotation);
                        }
                    }
                }
            }
        }

        Dictionary<QRZDir, Pose> _poses = new();

        private ConnectionTypes InferConnectionType(Hex3 coords, QRZDir dir) {
            var other = coords + dir.QRZOffset();
            if (FindPartFeature(other) != null) return ConnectionTypes.Implicit;
            return ConnectionTypes.Forbidden;
        }

        internal void Replace(StructureModel sm) {
            CurrentModel = sm;
            SyncModel();
        }
    }

    public static class HexExpansions {

        public static float RadialDistance = 1f;
        public static float ZedDistance = 1f;
        public static GridTypes gridType = GridTypes.FlatTop;

        public static Vector3 Cartesian(this Hex3 hex) {
            var (x, y) = Hexes.HexToPixel(hex.hex, gridType, RadialDistance);
            return new Vector3(x, y, hex.zed * ZedDistance);
        }

        static (Vector3 away, Vector3 tangent) Vectors(this QRZDir dir) {
            return dir switch {
              QRZDir.Forward => (Vector3.forward, Vector3.down),
              QRZDir.Backward => (Vector3.back, Vector3.up),
              QRZDir.Top => (Vector3.up, Vector3.forward),
              QRZDir.Bottom => (-Vector3.up, Vector3.forward),

              QRZDir.LeftTop    => (Radial(-60), Vector3.forward),
              QRZDir.RightTop   => (Radial(60), Vector3.forward),
              QRZDir.LeftBot    => (Radial(-120), Vector3.forward),
              QRZDir.RightBot   => (Radial(120), Vector3.forward),


              _ => (Vector3.zero, Vector3.zero),
            };
            
            Vector3 Radial(int degrees) {
                var rads = degrees * Mathf.Deg2Rad;
                return new Vector3(Mathf.Cos(rads), Mathf.Sin(rads), 0);
            }

            //var px = Hexes.PixelToHex(off.x, off.y, gridType, 1f);
            //var up = Hexes.PixelToHex(0, 0, gridType, 1f);
            //var forward = Hexes.PixelToHex(0, 0, gridType, 1f);
            //return (up, forward);
        }

        public static Quaternion Orientation(this QRZDir dir) {        
            if (dir == QRZDir.None) throw new Exception("Invalid direction NONE");
            var vecs = Vectors(dir);
            return Quaternion.LookRotation(vecs.away, vecs.tangent);
        }

        static internal QRZDir ComputeDirectionFromNormal(Vector3 normalFacing) {
            if (normalFacing.z < - 0.5f) return QRZDir.Backward;
            if (normalFacing.z >   0.5f) return QRZDir.Forward;

            var angle = Mathf.Atan2(normalFacing.y, normalFacing.x) * Mathf.Rad2Deg;
            var index = Mathf.RoundToInt((150 + angle)) / 60 ;

            if (index >= 0 && index < _dirlookup.Length) return _dirlookup[index];
            return QRZDir.None;
        }

        static internal QRZDir Inverse(this QRZDir dir) => dir switch { 
            QRZDir.Forward => QRZDir.Backward, QRZDir.Backward => QRZDir.Forward, 
            QRZDir.Top => QRZDir.Bottom, QRZDir.Bottom => QRZDir.Top, 
            QRZDir.LeftBot => QRZDir.RightTop, QRZDir.RightTop => QRZDir.LeftBot, 
            QRZDir.LeftTop => QRZDir.RightBot, QRZDir.RightBot => QRZDir.LeftTop, 
            _ => QRZDir.None
        };

        static  internal Vector3Int QRZOffset(this QRZDir dir) => dir switch {
            QRZDir.Forward => new Vector3Int(0, 0, 1),
            QRZDir.Backward => new Vector3Int(0, 0, -1),
            QRZDir.Top => new Vector3Int(0, 1, 0),
            QRZDir.Bottom => new Vector3Int(0, -1, 0),
            QRZDir.LeftBot => new Vector3Int(-1, 0, 0),
            QRZDir.RightTop => new Vector3Int(1, 0, 0),

            QRZDir.LeftTop => new Vector3Int(-1, 1, 0),
            QRZDir.RightBot => new Vector3Int(1, -1, 0),
            _ => default
        };

        static QRZDir[] _dirlookup = new[] { QRZDir.LeftBot, QRZDir.Bottom, QRZDir.RightBot, QRZDir.RightTop, QRZDir.Top, QRZDir.LeftTop, QRZDir.Forward, QRZDir.Backward };

        static internal QRZDir[] AllDirections => _dirlookup;
    }

    static class ModelSerializer {
        public static byte[] Serialize(StructureModel model) {
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);
            writer.Write(model.features.Count);
            foreach (var feature in model.features) {
                writer.Write((byte)feature.type);
                writer.Write(feature.coords.hex.q);
                writer.Write(feature.coords.hex.r);
                writer.Write(feature.coords.zed);
                writer.Write(feature.graphicVariant);
                writer.Write((byte)feature.direction);
                writer.Write((byte)feature.connType);
            }
            return ms.ToArray();
        }

        public static StructureModel Deserialize(byte[] blob) {
            
            using var ms = new MemoryStream(blob);
            using var reader = new BinaryReader(ms);

            var count = reader.ReadInt32();
            var features = new List<Feature>(count);
            for (int i = 0; i < count; i++) {
                var type = (FeatureTypes)reader.ReadByte();
                var coords = new Hex3(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());
                var graphicVariant = reader.ReadInt32();
                var direction = (QRZDir)reader.ReadByte();
                var connType = (ConnectionTypes)reader.ReadByte();
                features.Add(new Feature {
                    type = type,
                    coords = coords,
                    graphicVariant = graphicVariant,
                    direction = direction,
                    connType = connType,
                });
            }
            return new StructureModel { features = features };
        }
    }
}
