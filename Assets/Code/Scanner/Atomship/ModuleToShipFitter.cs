using System;
using System.Collections.Generic;
using Core.H3;
using K3.Hex;
using Shapes;

namespace Scanner.Atomship {
    class ModuleToShipFitter {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="declaration"></param>
        /// <param name="fitShipHex"></param>
        /// <param name="fitDirection">Direction from the ship hex to thje blueprint</param>
        /// <returns></returns>
        public Fit GetFit(StructureDeclaration declaration, H3 fitShipHex, PrismaticHexDirection fitDirection) {
            return null;
        }

        (H3 hex, PrismaticHexDirection direction) TransformLocalToWorld(H3Pose parentFrameOfReference, H3 localPosition, PrismaticHexDirection localDirection) {
            var resultDirRadial = localDirection.radial.Rotated(parentFrameOfReference.rotation);
            var resultDirLongit = localDirection.longitudinal;

            var hex = parentFrameOfReference.position.hex + localPosition.hex.RotateAroundZero(parentFrameOfReference.rotation);
            var zed = parentFrameOfReference.position.zed + localPosition.zed;
            return (new H3(hex, zed), new PrismaticHexDirection(resultDirRadial, resultDirLongit));
        }

        internal List<Attachment> attachments = new();

        internal void PrecomputeAttachpoints(Ship ship) {
            attachments = new();

            foreach (var structure in ship.ListStructures()) {
                var connectors = structure.Declaration.hexModel.connections;
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
        public Structure structure;
        public HexModelDefinition.HexConnector connector;
        public H3 connectorWorldspaceOriginHex;
        public PrismaticHexDirection connectorWorldspaceDirection;
    }

    public class Fit {

    }
}