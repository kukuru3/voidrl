using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Core;
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


        public Hex3 offset;
        public int  hexrot;

        internal void CreateNew() {
            CurrentModel = new StructureModel() {
                features = new List<Feature> {
                    new Feature {
                        type = FeatureTypes.Part,
                        localCoords = new Hex3(0, 0, 0),
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

            _poses.Add(Hex3Dir.Top,      RadialPose(0,   hr3*radialDistance - margin));
            _poses.Add(Hex3Dir.RightTop, RadialPose(60,  hr3*radialDistance - margin));
            _poses.Add(Hex3Dir.RightBot, RadialPose(120, hr3*radialDistance - margin));
            _poses.Add(Hex3Dir.Bottom,   RadialPose(180, hr3*radialDistance - margin));
            _poses.Add(Hex3Dir.LeftBot,  RadialPose(240, hr3*radialDistance - margin));
            _poses.Add(Hex3Dir.LeftTop,  RadialPose(300, hr3*radialDistance - margin));

            _poses.Add(Hex3Dir.Forward, new Pose(
                new Vector3(0,0, zedDistanceMultiplier/2-margin),
                Quaternion.Euler(90, 0,0)
            ));

            _poses.Add(Hex3Dir.Backward, new Pose(
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
                    if (Input.GetKey(KeyCode.Delete)) TryCreateProhibition(res.Value.coords + res.Value.dir);
                    else if (shift)     TryRemovePart(res.Value.coords, res.Value.dir);
                    else if (Input.GetKey(KeyCode.A)) SetAllForbiddenConnectionsPermissive(res.Value.coords);
                    else if (Input.GetKey(KeyCode.P)) SetPrimaryConnection(res.Value.coords, res.Value.dir);
                    else CycleConnection(res.Value.coords, res.Value.dir);

                } else if (Input.GetMouseButtonDown(2)) {
                    TryAddPart(res.Value.coords, res.Value.dir);
                } 
            }

            if (Input.GetKeyDown(KeyCode.Q)) { hexrot--; SyncModel(); }
            if (Input.GetKeyDown(KeyCode.E)) { hexrot++; SyncModel(); }
        }

        private void SetAllForbiddenConnectionsPermissive(Hex3 coords) {
            foreach (var dir in Hex3Utils.AllDirections) {
                var c = FindConnectionFeature(coords, dir, true);

                if (c == null) {                    
                    var ctype = InferConnectionType(coords, dir);
                    if (ctype == ConnectionTypes.Forbidden) { 
                        CurrentModel.features.Add(new Feature {
                            localCoords = coords,
                            localDirection = dir,
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
                 localCoords = coords,
                 type = FeatureTypes.ProhibitedSpace,
             });
            SyncModel();
        }

        void SetPrimaryConnection(Hex3 coords, Hex3Dir dir) {
             var conn = FindConnectionFeature(coords, dir, true);
            if (conn == null) {
                conn = new Feature {
                    type = FeatureTypes.Connector,
                    localCoords = coords,
                    localDirection = dir,
                };
                CurrentModel.features.Add(conn);
            }
            conn.connType = ConnectionTypes.Primary;
            SyncModel();
        }

        private void CycleConnection(Hex3 coords, Hex3Dir dir) {
            // var destination = value.coords + QRZOffset(value.dir);
            var existingConnection = FindConnectionFeature(coords, dir, true);
            if (existingConnection == null) {
                existingConnection = new Feature {
                    type = FeatureTypes.Connector,
                    localCoords = coords,
                    localDirection = dir,
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

        private void TryAddPart(Hex3 coords, Hex3Dir dir) {
            var q = coords + dir;
            if (CurrentModel.features.Any(a => a.localCoords == q)) throw new Exception("Already occupied");
            CurrentModel.features.Add(new Feature {
                type = FeatureTypes.Part,
                localCoords = q,
            });
            SyncModel();
        }

        Feature FindConnectionFeature(Hex3 coords, Hex3Dir dir, bool symmetrical = false) { 
            var f = CurrentModel.features.FirstOrDefault(a => a.type == FeatureTypes.Connector && a.localCoords == coords && a.localDirection == dir);
            if (symmetrical) { 
                coords += dir;
                dir = dir.Inverse();
                f ??= CurrentModel.features.FirstOrDefault(a => a.type == FeatureTypes.Connector && a.localCoords == coords && a.localDirection == dir);
            }
            return f;
        }

        Feature FindPartFeature(Hex3 coords) =>
            CurrentModel.features.FirstOrDefault(a => a.type == FeatureTypes.Part && a.localCoords == coords);

        Feature FindProhibitionFeature(Hex3 coords) => 
            CurrentModel.features.FirstOrDefault(a => a.type == FeatureTypes.ProhibitedSpace && a.localCoords == coords);
        private void TryRemovePart(Hex3 coords, Hex3Dir dir) { 
            if (coords == default) throw new Exception("Cannot remove the root module");
            var existingFeature = FindPartFeature(coords) ?? FindProhibitionFeature(coords);
            if (existingFeature == null) throw new Exception("No module or data to remove");
            CurrentModel.features.Remove(existingFeature);
            RemoveOrphans();
            SyncModel();
        }

        public (Hex3 coords, Hex3Dir dir, FeatureTypes t)? RaycastModel(Ray worldRay) {
            if (Physics.Raycast(worldRay, out var rayHit, 100f, 1 << 20, QueryTriggerInteraction.Collide)) {
                var f = FindFeatureOf(rayHit.collider.gameObject);
                if (f != null) {
                    var dir = Hex3Utils.ComputeDirectionFromNormal(rayHit.normal);
                    return (f.localCoords, dir, f.type);
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
                    if (FindPartFeature(f.localCoords) == null) {
                        toRemove.Add(f);
                    }
                }
            }

            foreach (var item in toRemove) CurrentModel.features.Remove(item);
        }


        private void SyncModel() { 
            Hex3Utils.ZedDistance = radialDistance * zedDistanceMultiplier;
            Hex3Utils.RadialDistance = radialDistance;

            foreach (Transform child in root) Destroy(child.gameObject);

            featureViews.Clear();

            var comp = Quaternion.Euler(0, 0, 60 * hexrot);

            foreach (var feature in CurrentModel.features) {
                if (feature.type == FeatureTypes.Part) {
                    var prefab = partPrefabs[feature.graphicVariant];
                    var partView = Instantiate(prefab, root);
                    partView.transform.localPosition = Cartesian(feature.localCoords);
                    partView.transform.localRotation = Quaternion.identity;
                    partView.layer = 20;
                    partView.name = $"Part @ {feature.localCoords}";
                    featureViews.Add((feature, partView));
                } else if (feature.type == FeatureTypes.Connector) {
                    // Forbidden, Allowed, Implicit, Primary,
                    var prefab = connectorPrefabs[(int)feature.connType];
                    var connView = Instantiate(prefab, root);
                    connView.name = $"Connector ({feature.connType}) @ {feature.localCoords} => {feature.localCoords + feature.localDirection}";
                    var dir = feature.localDirection;

                    if (dir == Hex3Dir.Forward || dir == Hex3Dir.Backward)
                        connView.GetComponentInChildren<MeshRenderer>().material.color = Color.yellow;

                    var p = _poses[dir];
                    connView.transform.SetPositionAndRotation(Cartesian(feature.localCoords) + comp * p.position, comp * p.rotation);


                    featureViews.Add((feature, connView));
                } else if (feature.type == FeatureTypes.ProhibitedSpace) {
                    var prefab = prohibitorPrefab;
                    var view = Instantiate(prefab, root);
                    view.layer = 20;
                    view.name = $"Prohibitor {feature.localCoords}";
                    view.transform.SetPositionAndRotation(Cartesian(feature.localCoords), comp * Quaternion.identity);
                    featureViews.Add((feature, view));
                }
            }

            foreach (var partFeat in CurrentModel.features) {
                if (partFeat.type == FeatureTypes.Part) {
                    foreach (var dir in Hex3Utils.AllDirections) {
                        var alreadyExtantConnectionFeature = FindConnectionFeature(partFeat.localCoords, dir, true);
                        if (alreadyExtantConnectionFeature == null) {
                            var ctype = InferConnectionType(partFeat.localCoords, dir);
                            var prefab = connectorPrefabs[(int)ctype];
                            var phantomView = Instantiate(prefab, root);
                            phantomView.name = $"(Phantom) Connector ({ctype}) @ {partFeat.localCoords} => {dir}";

                            var p = _poses[dir];
                            phantomView.transform.SetPositionAndRotation(Cartesian(partFeat.localCoords) + comp * p.position, comp * p.rotation);
                        }
                    }
                }
            }
        }

        Vector3 Cartesian(Hex3 hex) {
            var h2 = Hexes.Rotate(hex.hex, hexrot);
            var newHex = new Hex3(h2, hex.zed);
            return (newHex + offset).Cartesian();
        }

        /// <summary>Cartesian offsets and rotations for each QRZDir</summary>
        Dictionary<Hex3Dir, Pose> _poses = new();

        private ConnectionTypes InferConnectionType(Hex3 coords, Hex3Dir dir) {
            var other = coords + dir;
            if (FindPartFeature(other) != null) return ConnectionTypes.Implicit;
            return ConnectionTypes.Forbidden;
        }

        internal void Replace(StructureModel sm) {
            CurrentModel = sm;
            SyncModel();
        }
    }
    

    static class ModelSerializer {
        public static byte[] Serialize(StructureModel model) {
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);
            writer.Write(model.features.Count);
            foreach (var feature in model.features) {
                writer.Write((byte)feature.type);
                writer.Write(feature.localCoords.hex.q);
                writer.Write(feature.localCoords.hex.r);
                writer.Write(feature.localCoords.zed);
                writer.Write(feature.graphicVariant);
                writer.Write((byte)feature.localDirection);
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
                var direction = (Hex3Dir)reader.ReadByte();
                var connType = (ConnectionTypes)reader.ReadByte();
                features.Add(new Feature {
                    type = type,
                    localCoords = coords,
                    graphicVariant = graphicVariant,
                    localDirection = direction,
                    connType = connType,
                });
            }
            return new StructureModel { features = features };
        }
    }
}
