﻿using System;
using System.Collections.Generic;
using System.Linq;
using Core.H3;
using K3.Hex;

namespace Scanner.Atomship {
    using Connector = HexModelDefinition.HexConnector;

    class ModuleToShipFitter {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="declaration"></param>
        /// <param name="fitShipHex"></param>
        /// <param name="fitDirection">Direction from the ship hex to thje blueprint</param>
        /// <returns></returns>
        public Fit TryGetFit(StructureDeclaration declaration, H3 fitShipHex, PrismaticHexDirection fitDirection) {
            var att = GetAttachment(fitShipHex, fitDirection);

            if (att == null) return new Fit {
                success = false,
                remarks = "No attachment under cursor",
            };

            var compatibleConnectorsInPhantom = declaration.hexModel.connections.Where(c => SpatiallyCompatible(att, c)).ToList();

            var aligner = compatibleConnectorsInPhantom.FirstOrDefault(c => (c.flags & 1) > 0);

            

            var stageOne = new List<Connector>(); if (aligner != null) stageOne.Add(aligner);
            var stageTwo = compatibleConnectorsInPhantom.Except(stageOne).ToList();

            foreach (var item in stageOne) {
                var initialFit = TryExecuteFit(item, att);
                if (initialFit.success) return initialFit;
            }

            foreach (var item in stageTwo) {
                var initialFit = TryExecuteFit(item, att);
                if (initialFit.success) return initialFit;
            }
            
            return new Fit {
                success = false,
                remarks = (aligner == null) ? "No aligner found" : "No aligner fit?",
            };
            // try fit with other nodes now
        }

        private Fit TryExecuteFit(Connector aligner, Attachment att) {
            // the million dollar question: orient the phantom in order for aligner to be the OPPOSITE of attachment.
            var alignerWorldspacePos = att.connectorWorldspaceOriginHex + att.connectorWorldspaceDirection;
            var alignerWorldspaceDir = att.connectorWorldspaceDirection.Inverse();

            var rotationSteps = 0;

            if (alignerWorldspaceDir.longitudinal == 0) {
                rotationSteps = 6-HexUtils.GetRotationSteps(aligner.direction.radial, alignerWorldspaceDir.radial);
            }

            var rotatedSourceHex = new H3(aligner.sourceHex.hex.RotateAroundZero(rotationSteps), aligner.sourceHex.zed);
            var originPos = alignerWorldspacePos - rotatedSourceHex;

            return new Fit {
                poseOfPhantom = new H3Pose(originPos, rotationSteps),
                remarks = "Fit successful",
                success = true,
            };
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
        internal bool success;
        internal string remarks;
        internal H3Pose poseOfPhantom;
    }
}