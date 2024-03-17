using Core;
using Core.h3x;
using Microsoft.Scripting.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Scanner.Atomship {
    class ReadyAttachment {
        public Structure structure;
        public Feature   targetConnectorInModel;
        public Hex3      sourceHexWS;
        public Hex3      targetHexWS;
        public Hex3Dir   connectorWorldspaceDirection;
    }

    class Fitter {

         internal void PreComputeAttachmentData(Ship ship) { 
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
                };
            }

            Debug.Log($"Precomputed attachment data, open attachment slots:{attachments.Count}");
        }

        internal ReadyAttachment FindAttachment(Hex3 nodePosition, Hex3Dir direction) {
            foreach (var att in attachments) {
                if (att.sourceHexWS == nodePosition && att.connectorWorldspaceDirection == direction ) {
                    return att;
                }
            }
            return null;
        }

        List<ReadyAttachment> attachments = new();

        bool LogicalFit(StructureDeclaration blueprint, Feature blueprintFeature, int blueprintFeatureIndex, ReadyAttachment attachment) {
            if (blueprintFeature.type != FeatureTypes.Connector) return false;
            
            var dt = GetDirType(blueprintFeature.localDirection);
            if (dt != GetDirType(attachment.connectorWorldspaceDirection)) return false;

            // if both are Forward, or both are Backward, ignore.
            if (dt == DirTypes.Longitudinal && blueprintFeature.localDirection == attachment.connectorWorldspaceDirection) return false;            

            return true;
        }

        Hex3Dir TransformDirection(Hex3Dir source, int hexRotation) {
            if (source == Hex3Dir.Forward) return Hex3Dir.Forward;
            if (source == Hex3Dir.Backward) return Hex3Dir.Backward;
            if (source == Hex3Dir.None) return Hex3Dir.None;

            return source.Rotated(hexRotation);
        }
        private ReadyAttachment FindInSituFit(Feature f, HexPose alignment){
            var featureWorldCoords = alignment * new HexPose(f.localCoords, 0);
            var rot = TransformDirection(f.localDirection, 6 - alignment.rotation);
            var attachmentTile = featureWorldCoords.position + rot;
            var attachmentDirection = rot.Inverse();
            
            return FindAttachment(attachmentTile, attachmentDirection);
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

        private HexPose GetAlignment(Feature blueprintFeature, ReadyAttachment targetedAttachment) { 

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
            }

            // first I had to calculate this on my own on paper.
            // then, when I started typing it, Copilot casually autocompleted it, correctly I might add. Fml.
            var originPos = featurePositionWS - blueprintFeature.localCoords.Rotated(6 - rotations);
            
            return new HexPose(originPos, 6 - rotations);
        }

        public class BlueprintFit {
            public bool fits;
            public HexPose alignmentOfBlueprint;
            internal List<Coupling> couplings = new();
            public string failReason;

            static internal BlueprintFit Fail(string reason) {
                return new BlueprintFit { fits = false, failReason = reason };
            }
        }

        internal class Coupling {
            internal Feature blueprintFeature;
            internal ReadyAttachment attachment;
        }

        internal BlueprintFit TryFitStructure(StructureDeclaration currentTool, ReadyAttachment targetedAttachment) {
            
            List<Coupling> couplings = new();

            // if L contains only one fit and it is longitudinal (fwd or back),
            //  - additional step where you adjust the circular rotation of the blueprint, around F<=> A axis

            // optionally:

            //  - for all remaining nonprimary connectors in blueprint, try to find opposite attach point. If such a point exists, add to optional set O.

            //  - then, offer the user to also construct the optional tubes in set O.
                    
            if (currentTool == null) return null;

            var primaryConnectors = currentTool.nodeModel.features.Where(f => f.type == FeatureTypes.Connector && f.connType == ConnectionTypes.Primary).ToList();
            var normalConnectors = currentTool.nodeModel.features.Where(f => f.type == FeatureTypes.Connector && f.connType == ConnectionTypes.Allowed).ToList();

            

            if (primaryConnectors.Count > 0) {
                var primaryLogicalFitsWithTargetAttachment = new List<Feature>();
                foreach (var c in primaryConnectors) {
                    if (LogicalFit(currentTool, c, currentTool.nodeModel.features.IndexOf(c), targetedAttachment )) {
                        primaryLogicalFitsWithTargetAttachment.Add(c);
                    }
                }

                if (primaryLogicalFitsWithTargetAttachment.Count == 0)
                    return BlueprintFit.Fail("There is no logical fit between any primary connector and target attachment");
                
                foreach (var lf in primaryLogicalFitsWithTargetAttachment) {
                    couplings.Clear();
                    var alignment = GetAlignment(lf, targetedAttachment);

                    bool anyInvalidCouplings = false;

                    foreach (var primaryConnector in primaryConnectors) {
                        var isf = FindInSituFit(primaryConnector, alignment);
                        if (isf != null) {
                            couplings.Add(new Coupling { blueprintFeature = primaryConnector, attachment = isf });
                        } else {
                            anyInvalidCouplings = true; 
                            break;
                        }
                    }             
                    
                    if (!anyInvalidCouplings) {
                        return new BlueprintFit {
                            alignmentOfBlueprint = alignment,
                            couplings = couplings,
                            failReason = "",
                            fits = true
                        };
                    }
                }

                return BlueprintFit.Fail("There is no valid alignment for any primary connector");

            } else {
                var fits = new List<Feature>();
                foreach (var c in normalConnectors) {
                    if (LogicalFit(currentTool, c, currentTool.nodeModel.features.IndexOf(c), targetedAttachment)) fits.Add(c);
                    
                }
                if (fits.Count == 0) {         
                    return BlueprintFit.Fail("There is no logical fit between any normal connector and target attachment");
                } else {
                    var item = fits.First();                    
                    var alignment = GetAlignment(item, targetedAttachment);
                    return new BlueprintFit {
                        alignmentOfBlueprint = alignment,
                        couplings = new List<Coupling> { new Coupling { blueprintFeature = item, attachment = targetedAttachment } },
                        fits = true,
                    };
                }
            }
        }
    }
}