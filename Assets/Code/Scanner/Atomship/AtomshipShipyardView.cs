using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using Core;
using Core.h3x;
using UnityEngine;
using static IronPython.Modules._ast;

namespace Scanner.Atomship {
    class AtomshipShipyardView : MonoBehaviour {
        [SerializeField] GameObject nodePrefab;
        [SerializeField] GameObject tubePrefab;

        [SerializeField] GameObject phantomNodePrefab;

        [SerializeField] GameObject buttonPrefab;
        [SerializeField] Transform uiRoot;

        [SerializeField] Transform shipRoot;
        [SerializeField] Transform phantomRoot;

        [SerializeField] Camera editorCamera;


        Ship ship;

        private void Start() {
            Hardcoder.HardcodeRules();
            this.ship = Hardcoder.GenerateInitialShip();
            GenerateUI();
            RegenerateShipView();
            PreComputeAttachmentData();
        }

        class ReadyAttachment {
            public Structure structure;
            public Feature   targetConnectorInModel;
            public Hex3      sourceHexWS;
            public Hex3      targetHexWS;
            public Hex3Dir   connectorWorldspaceDirection;
        }

        List<ReadyAttachment> attachments = new();

        private void PreComputeAttachmentData() { 
            attachments.Clear();

            foreach (var @struct in ship.ListStructures()) {
                var pose0 = @struct.Pose;
                var connectors = @struct.Declaration.nodeModel.features.Where(f => f.type == FeatureTypes.Connector).ToList();
                foreach (var conn in connectors) {

                    // a connector does not have a local hex rotation, but it does have a direction
                    // and we can infer its "rotation" from that.
                    var offZ = conn.localDirection.Offset().zed;
                    var connPose = new HexPose(conn.localCoords, conn.localDirection.ToHexRotation());
                    var finalPose = pose0 * connPose;

                    // the final worldspace hextile of the connector, and its final worldspace direction

                    var connectorWorldspaceOriginHex = finalPose.position;
                    var connectorWorldspaceDir = Hex3Utils.FromParameters(finalPose.rotation, offZ);
                    var connectorWorldspaceTargetHex = finalPose.position + connectorWorldspaceDir;

                    attachments.Add(new ReadyAttachment {
                        structure = @struct,
                        targetConnectorInModel = conn,
                        connectorWorldspaceDirection = connectorWorldspaceDir,
                        sourceHexWS = connectorWorldspaceOriginHex,
                        targetHexWS = connectorWorldspaceTargetHex,
                    });

                    // the connector originates at target pos and has direction finalWorldspaceOffset

                    //var from = finalPose.Cartesian().position;
                    //var to = targetPos.Cartesian();
                    //from.z *= 1.73f;
                    //to.z *= 1.73f;

                    //var go = Instantiate(tubePrefab, root);
                    //go.transform.SetPositionAndRotation((from + to)/2, Quaternion.LookRotation(to - from));
                    //go.name = $"structure {@struct.Declaration.ID} : connector {conn.connType}; parent hex local = {conn.localCoords}, local dir = {conn.localDirection}";
                    // foreach (var tube in ship.Tubes) {

                        // var from 
                        //var from = tube.moduleFrom.Pose.Cartesian().position;
                        //var to = tube.moduleTo.Pose.Cartesian().position;
                        //from.z *= 1.73f;
                        //to.z *= 1.73f;
                        //var go = Instantiate(tubePrefab, root);
                        //go.transform.SetPositionAndRotation((from + to) / 2, Quaternion.LookRotation(to - from));
                        //// go.transform.localScale = new Vector3(0.1f, 0.1f, Vector3.Distance(from, to));
                        //go.name = $"Tube:{tube.moduleFrom}=>{tube.moduleTo}";
                    // }


                };
            }

            UnityEngine.Debug.Log($"Precomputed attachment data, open attachment slots:{attachments.Count}");
        }

        private void GenerateUI() { 
            var sdecls = RuleContext.Repo.ListRules<StructureDeclaration>();
            var y = 0;
            foreach (var sd in sdecls) {
                var button = Instantiate(buttonPrefab, uiRoot);
                var bc = button.GetComponent<Button>();
                bc.Clicked += () => SelectDeclaration(sd);
                bc.Label = sd.ID;
                bc.transform.localPosition = new Vector3(0, y, 0);
                y+=30;
            }
        }

        StructureDeclaration currentBlueprint;

        private void SelectDeclaration(StructureDeclaration sd) {
            currentBlueprint = sd;
        }

        void RegenerateShipView() {
            foreach (Transform child in shipRoot) Destroy(child.gameObject);

            foreach (var node in ship.Nodes) {
                var p = node.Pose;
                var cartesianPose = p.Cartesian();
                cartesianPose.position.z *= 1.73f;
                var go = Instantiate(nodePrefab, shipRoot);
                go.transform.SetLocalPositionAndRotation(cartesianPose.position, cartesianPose.rotation);
                go.GetComponent<NodeView>().Node = node;
                go.name = $"Node:{node.Structure.Declaration.ID}[{node.IndexInStructure}]";
            }

            foreach (var tube in ship.Tubes) {
                var from = tube.moduleFrom.Pose.Cartesian().position;
                var to = tube.moduleTo.Pose.Cartesian().position;
                from.z *= 1.73f;
                to.z *= 1.73f;
                var go = Instantiate(tubePrefab, shipRoot);
                go.transform.SetPositionAndRotation((from + to) / 2, Quaternion.LookRotation(to - from));
                // go.transform.localScale = new Vector3(0.1f, 0.1f, Vector3.Distance(from, to));
                go.name = $"Tube:{tube.moduleFrom}=>{tube.moduleTo}";
            }   
        }

        void RegeneratePhantomView(StructureDeclaration decl, HexPose phantomPose) {
            foreach (Transform t in phantomRoot) Destroy(t.gameObject);
            foreach (var feat in decl.nodeModel.features) {
                if (feat.type == FeatureTypes.Part) {
                    var p = phantomPose * new HexPose(feat.localCoords, 0);
                    var cartesianPose = p.Cartesian(); cartesianPose.position.z *= 1.73f;
                    var go = Instantiate(phantomNodePrefab, phantomRoot);
                    go.transform.SetLocalPositionAndRotation(cartesianPose.position, cartesianPose.rotation);
                    go.name= $"Phantom node [{feat.localCoords}]";
                }
            }
        }


        private void Update() {
            var mousePos = Input.mousePosition;
            var ray = editorCamera.ScreenPointToRay(mousePos);
            if (Physics.Raycast(ray, out var hit, Mathf.Infinity, 1 << 20, QueryTriggerInteraction.Collide)) {
                var go = hit.collider.gameObject;
                var nv = go.GetComponentInParent<NodeView>();
                var node = nv.Node;
                var worldspaceDirection = Hex3Utils.ComputeDirectionFromNormal(hit.normal);

                // "node" and "dir" are sufficient for us

                ReadyAttachment targetedAttachment = null;
                foreach (var att in attachments) {
                    if (att.sourceHexWS == node.Pose.position && att.connectorWorldspaceDirection == worldspaceDirection) {
                        targetedAttachment = att;
                        break;
                    }
                }

                if (targetedAttachment == null) { Debug.Log("Cannot fit: targeted connector is null"); return; }


                TryFitStructure(currentBlueprint, targetedAttachment);
            }
        }

        
        bool LogicalFit(StructureDeclaration blueprint, Feature blueprintFeature, int blueprintFeatureIndex, ReadyAttachment attachment) {
            if (blueprintFeature.type != FeatureTypes.Connector) return false;
            
            var dt = GetDirType(blueprintFeature.localDirection);
            if (dt != GetDirType(attachment.connectorWorldspaceDirection)) return false;

            // if both are Forward, or both are Backward, ignore.
            if (dt == DirTypes.Longitudinal && blueprintFeature.localDirection == attachment.connectorWorldspaceDirection) return false;            

            return true;
        }

        enum DirTypes {
            Longitudinal,
            Radial
        }

        DirTypes GetDirType(Hex3Dir dir) {
            return dir switch {
                Hex3Dir.Forward or Hex3Dir.Backward => DirTypes.Longitudinal,
                _ => DirTypes.Radial
            };
        }

        private void TryFitStructure(StructureDeclaration currentTool, ReadyAttachment targetedAttachment) {
            // algo: 

            // find ship precomputed connector at position being hit (attach point A)
            // if attach point does not exist, return no fit

            // if the blueprint has at least one primary:
                // find first primary connector that fits attach point A
                //  - exists: call it F, add to list of fits L
                //  - does not exist: return no fit.

                // blueprint tentative orientation is such that (attach point A <=> primary connector F)
                // for all other primary connectors, if any:
                    // - find a fittable connector opposite
                        // - if no fittable connector opposite, return no fit
                        // - otherwise, add to list of fits L

                

            // OTHERWISE, if the blueprint has no primary connectors:
                // find any allowed connector that fits attach point A
                //  - exists: call it F, add to list of fits L
                //  - does not exist: return no fit.


            // if you are here, good - you have a fit. Tubes to be created are in the set L.
            // if L contains only one fit and it is longitudinal (fwd or back),
            //  - additional step where you adjust the circular rotation of the blueprint, around F<=> A axis

            // optionally:

            //  - for all remaining nonprimary connectors in blueprint, try to find opposite attach point. If such a point exists, add to optional set O.

            //  - then, offer the user to also construct the optional tubes in set O.
                    
            if (currentTool == null) return;

            var primaryConnectors = currentTool.nodeModel.features.Where(f => f.type == FeatureTypes.Connector && f.connType == ConnectionTypes.Primary).ToList();
            var normalConnectors = currentTool.nodeModel.features.Where(f => f.type == FeatureTypes.Connector && f.connType == ConnectionTypes.Allowed).ToList();

            var logicalFits = new List<Feature>();

            if (primaryConnectors.Count > 0) {
                foreach (var c in primaryConnectors) {
                    if (LogicalFit(currentTool, c, currentTool.nodeModel.features.IndexOf(c), targetedAttachment )) {
                        logicalFits.Add(c);
                    }
                }
                if (logicalFits.Count == 0) {
                    Debug.Log("There is no logical fit between any primary connector and target attachment");
                    return;
                }

                foreach (var lf in logicalFits) {

                    var alignment = Align(lf, targetedAttachment);
                    RegeneratePhantomView(currentTool, alignment);
                    break;

                    var others = new HashSet<Feature>(logicalFits);
                    others.Remove(lf);
                    if (others.Count > 0) {
                        // 
                    }

                }
            } else {
                foreach (var c in normalConnectors) {
                    if (LogicalFit(currentTool, c, currentTool.nodeModel.features.IndexOf(c), targetedAttachment)) {
                        logicalFits.Add(c);
                    }
                }
                if (logicalFits.Count == 0) {                     
                    Debug.Log("There is no logical fit between any normal connector and target attachment");
                    return;
                } else {
                    foreach (var item in logicalFits) {
                        var alignment = Align(item, targetedAttachment);
                        RegeneratePhantomView(currentTool, alignment);
                        break;
                    }
                }
            }
        }

        private HexPose Align(Feature blueprintFeature, ReadyAttachment targetedAttachment) { 

            var featurePositionWS  = targetedAttachment.targetHexWS;
            var featureDirection = targetedAttachment.connectorWorldspaceDirection.Inverse();
            // position blueprint pivot such that, in worldspace, it is at above position and direction.

            var rotations = 0; 
            if (GetDirType(featureDirection) == DirTypes.Radial) { 
                var dir = blueprintFeature.localDirection;
                while ( dir != featureDirection) {
                    rotations++; dir = dir.Rotated(1);
                    if (rotations >= 6) throw new Exception("wtf - hyperbolic space much?");
                }

                // first I had to calculate this on my own on paper.
                // then, when I started typing it, Copilot casually autocompleted it, correctly I might add. Fml.
            }

            var originPos = featurePositionWS - blueprintFeature.localCoords.Rotated(6-rotations);
            
            return new HexPose(originPos, 6-rotations);

            // blueprint feature has a local direction, and a local hex position.
            // in other words: find the pose of blueprint pivot, such that 
            // - worldspace 
            
        }

    }
}