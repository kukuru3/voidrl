using Core;
using Core.H3;
// using Core.h3x;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Scanner.Atomship.Old {
    class ReadyAttachment {
        public Structure structure;
        public Feature   targetConnectorInModel;
        public H3        sourceHexWS;
        public H3        targetHexWS;
        public PrismaticHexDirection   connectorWorldspaceDirection;
    }


    class Fitter {

         internal void PreComputeAttachmentData(Ship ship) { 
            //attachments.Clear();

            //foreach (var @struct in ship.ListStructures()) {
            //    var pose0 = @struct.Pose;
            //    var connectors = @struct.Declaration.nodeModel.features.Where(f => f.type == FeatureTypes.Connector).ToList();
            //    foreach (var conn in connectors) {

            //        var offZ = conn.localDirection.longitudinal;

            //        // here: get the final connector direction                    
            //        var connectorLocalPose = new H3Pose(conn.localCoords.hex, conn.localCoords.zed, conn.localDirection.radial);

            //        var finalPose = pose0 * connectorLocalPose;

            //        // the final worldspace hextile of the connector, and its final worldspace direction

            //        var connectorWorldspaceOriginHex = finalPose.position;
            //        var connectorWorldspaceDir = new PrismaticHexDirection(finalPose.RadialUp, offZ);
            //        var connectorWorldspaceTargetHex = finalPose.position + connectorWorldspaceDir;

            //        attachments.Add(new ReadyAttachment {
            //            structure = @struct,
            //            targetConnectorInModel = conn,
            //            connectorWorldspaceDirection = connectorWorldspaceDir,
            //            sourceHexWS = connectorWorldspaceOriginHex,
            //            targetHexWS = connectorWorldspaceTargetHex,
            //        });
            //    };
            //}

            //Debug.Log($"Precomputed attachment data, open attachment slots:{attachments.Count}");
        }

        internal ReadyAttachment FindAttachment(H3 nodePosition, HexDir direction) {
            throw new System.NotImplementedException("Reimplement this");
            //foreach (var att in attachments) {
            //    if (att.sourceHexWS == nodePosition && att.connectorWorldspaceDirection == direction ) {
            //        return att;
            //    }
            //}
            //return null;
        }

        List<ReadyAttachment> attachments = new();

        bool LogicalFit(StructureDeclaration blueprint, Feature blueprintFeature, int blueprintFeatureIndex, ReadyAttachment attachment) {
            throw new System.NotImplementedException("Reimplement this");
            //if (blueprintFeature.type != FeatureTypes.Connector) return false;
            
            //var dt = GetDirType(blueprintFeature.localDirection);
            //if (dt != GetDirType(attachment.connectorWorldspaceDirection)) return false;

            //// if both are Forward, or both are Backward, ignore.
            //if (dt == DirTypes.Longitudinal && blueprintFeature.localDirection == attachment.connectorWorldspaceDirection) return false;            

            return true;
        }

        private ReadyAttachment FindInSituFit(Feature f, H3Pose alignment){
            throw new System.NotImplementedException("Reimplement this");
            //var featureWorldCoords = alignment * new HexPose(f.localCoords, 0);
            //var rot = TransformDirection(f.localDirection, alignment.rotation);
            //var attachmentTile = featureWorldCoords.position + rot;
            //var attachmentDirection = rot.Inverse();
            
            //return FindAttachment(attachmentTile, attachmentDirection);
        }

        private H3Pose GetAlignment(Feature blueprintFeature, ReadyAttachment targetedAttachment) { 
            throw new System.NotImplementedException("Reimplement this");
            //var featurePositionWS  = targetedAttachment.targetHexWS;
            //var featureDirection = targetedAttachment.connectorWorldspaceDirection.Inverse();
            //// position blueprint pivot such that, in worldspace, it is at above position and direction.

            //var rotations = 0; 
            //if (GetDirType(featureDirection) == DirTypes.Radial) { 
            //    var dir = blueprintFeature.localDirection;
            //    while ( dir != featureDirection) {
            //        rotations++; dir = dir.Rotated(1);
            //        if (rotations >= 6) throw new Exception("wtf - hyperbolic space much?");
            //    }
            //}

            //// first I had to calculate this on my own on paper.
            //// then, when I started typing it, Copilot casually autocompleted it, correctly I might add. Fml.
            //var originPos = featurePositionWS - blueprintFeature.localCoords.Rotated(rotations);
            
            //return new HexPose(originPos, 6);
        }

        public class BlueprintFit {
            public bool fits;
            public H3Pose alignmentOfBlueprint;
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