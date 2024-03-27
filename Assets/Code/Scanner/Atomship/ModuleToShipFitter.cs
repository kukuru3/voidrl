using System.Collections.Generic;
using System.Linq;
using Core.H3;
using K3.Hex;
using Void.ColonySim.Model;
using Void.ColonySim.BuildingBlocks;
using Void.ColonySim;


namespace Scanner.Atomship {
    using Connector = HexBlueprint.HexConnector;

    class ModuleToShipFitter {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="declaration"></param>
        /// <param name="fitShipHex"></param>
        /// <param name="fitDirection">Direction from the ship hex to thje blueprint</param>
        /// <returns></returns>
        public Fit TryGetFit(ModuleDeclaration declaration, H3 fitShipHex, PrismaticHexDirection fitDirection) {
            var att = GetAttachment(fitShipHex, fitDirection);

            if (att == null) return new Fit {
                success = false,
                remarks = "No attachment under cursor",
            };

            if (declaration.id == default) return new Fit {
                success = false,
                remarks = "No blueprint selected",
            };
            var compatibleConnectorsInPhantom = declaration.GetBlueprint().connections.Where(c => SpatiallyCompatible(att, c)).ToList();

            var aligner = compatibleConnectorsInPhantom.FirstOrDefault(c => (c.flags & 1) > 0);

            var stageOne = new List<Connector>(); if (aligner != null) stageOne.Add(aligner);
            var stageTwo = compatibleConnectorsInPhantom.Except(stageOne).ToList();

            foreach (var item in stageOne) {
                var initialFit = TryExecuteFit(declaration, item, att);
                if (initialFit.success) return initialFit;
            }

            foreach (var item in stageTwo) {
                var initialFit = TryExecuteFit(declaration,item, att);
                if (initialFit.success) return initialFit;
            }
            
            return new Fit {
                success = false,
                remarks = (aligner == null) ? "No aligner found" : "No aligner fit?",
            };
            // try fit with other nodes now
        }

        private Fit TryExecuteFit(ModuleDeclaration decl, Connector primaryConnector, Attachment primaryAttachment) {
            // the million dollar question: orient the phantom in order for aligner to be the OPPOSITE of attachment.
            var alignerWorldspacePos = primaryAttachment.connectorWorldspaceOriginHex + primaryAttachment.connectorWorldspaceDirection;
            var alignerWorldspaceDir = primaryAttachment.connectorWorldspaceDirection.Inverse();

            var rotationSteps = 0;

            if (alignerWorldspaceDir.longitudinal == 0) {
                rotationSteps = HexUtils.GetRotationSteps(primaryConnector.direction.radial, alignerWorldspaceDir.radial);
            }

            var rotatedSourceHex = new H3(primaryConnector.sourceHex.hex.RotateAroundZero(rotationSteps), primaryConnector.sourceHex.zed);
            var originPos = alignerWorldspacePos - rotatedSourceHex;

            var pose = new H3Pose(originPos, rotationSteps);
            (var fit, var remark) = ValidateFit(decl, pose);

            List<Fit.Connection> connections = new();
            if (fit) connections.AddRange(FindConnections(decl, pose, primaryAttachment));
            
            return new Fit {
                poseOfPhantom = new H3Pose(originPos, rotationSteps),
                success = fit,
                remarks = remark,
                connections = connections,
            };
        }

        private IEnumerable<Fit.Connection> FindConnections(ModuleDeclaration decl, H3Pose pose, Attachment primaryAttach) { 
            foreach (var conn in decl.GetBlueprint().connections) {
                var worldCrds = TransformLocalToWorld(pose, conn.sourceHex, conn.direction);
                var attachment = GetAttachment(worldCrds.hex + worldCrds.direction, worldCrds.direction.Inverse());
                if (attachment != null) {
                    yield return new Fit.Connection {
                        from = attachment.connectorWorldspaceOriginHex,
                        to = worldCrds.hex,
                        critical = primaryAttach == attachment || ((conn.flags & 2) > 0)
                    };
                }
            }
        }

        private (bool fits, string remarks) ValidateFit(ModuleDeclaration declaration, H3Pose pose) {
            // validate no nodes overlap with ship
            var blueprint = declaration.GetBlueprint();
            foreach (var node in blueprint.nodes) {                
                var worldPos = TransformLocalToWorld(pose, node.hex, new PrismaticHexDirection(HexDir.Top, 0)).hex;
                if (ship.GetNode(worldPos) != null) return (false, "some nodes overlap");
            }

            // validate all mandatory nodes have compatibles
            foreach (var c in blueprint.connections) {
                if ((c.flags & 2) == 0) continue; // not mandatory
                var worldCrds = TransformLocalToWorld(pose, c.sourceHex, c.direction);
                var attachment = GetAttachment(worldCrds.hex + worldCrds.direction, worldCrds.direction.Inverse());
                
                if (attachment == null || !SpatiallyCompatible(attachment, c)) {
                    return (false, "no or incompatible attachment for a mandatory connector");
                }
            }

            return (true, "oll korrekt");
        }

        bool SpatiallyCompatible(Attachment a, Connector phantomConnector) {
            // do not mix radial and longitudinal connectors:
            if (a.connectorWorldspaceDirection.longitudinal + phantomConnector.direction.longitudinal != 0) return false;

            if ((a.connector.flags & (1 << 1)) > 0) return false; // shipborne mandatory ones? madness
            if ((phantomConnector.flags & (1 << 2)) > 0) return false; // phantom socket ones? also madness!

            // flavour mismatch:
            var flavourMask = 0b11111111111000;
            var flavourShipC = a.connector.flags & flavourMask;
            var flavourPhantomC = phantomConnector.flags & flavourMask;
            if (flavourShipC == 0 && flavourPhantomC == 0) {

            } else {
                if ((flavourShipC & flavourPhantomC) == 0) return false;
            }

            return true;
        }

        Attachment GetAttachment(H3 hex, PrismaticHexDirection direction) {
            return attachments.FirstOrDefault(a => a.connectorWorldspaceOriginHex == hex && a.connectorWorldspaceDirection == direction);
        }

        (H3 hex, PrismaticHexDirection direction) TransformLocalToWorld(H3Pose parentFrameOfReference, H3 localPosition, PrismaticHexDirection localDirection) {
            var resultDirRadial = localDirection.radial.Rotated(parentFrameOfReference.rotation);
            var resultDirLongit = localDirection.longitudinal;

            var hex = parentFrameOfReference.position.hex + localPosition.hex.RotateAroundZero(parentFrameOfReference.rotation);
            var zed = parentFrameOfReference.position.zed + localPosition.zed;
            return (new H3(hex, zed), new PrismaticHexDirection(resultDirRadial, resultDirLongit));
        }

        ColonyShipStructure ship;
        internal List<Attachment> attachments = new();

        internal void PrecomputeAttachpoints(ColonyShipStructure ship) {
            this.ship = ship;
            attachments = new();

            foreach (var structure in ship.ListModules()) {
                var connectors = structure.Declaration.GetBlueprint().connections;
                foreach (var  conn in connectors) {
                    // so what is the worldspace position
                    (var worldHex, var worldDir) = TransformLocalToWorld(structure.Pose, conn.sourceHex, conn.direction);                    

                    var neighbour = worldHex + worldDir;
                    if (ship.GetNode(neighbour) != null) continue; // connector points to an occupied ship tile
                    
                    attachments.Add(new Attachment {
                        connector = conn,
                        structure = structure,
                        connectorWorldspaceOriginHex = worldHex,
                        connectorWorldspaceDirection = worldDir
                    });
                }
            }
        }
    }

    public class Attachment {
        public Module structure;
        public Connector connector;
        public H3 connectorWorldspaceOriginHex;
        public PrismaticHexDirection connectorWorldspaceDirection;
    }

    public class Fit {
        internal bool success;
        internal string remarks;
        internal H3Pose poseOfPhantom;

        internal List<Connection> connections = new();
    
        internal class Connection {
            internal H3 from;
            internal H3 to;
            internal bool critical;
        }
    }

    
}