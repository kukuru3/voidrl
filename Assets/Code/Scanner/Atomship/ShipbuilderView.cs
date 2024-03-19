using System;
using System.Collections.Generic;
using Core.H3;
using UnityEngine;

namespace Scanner.Atomship {
    using HNode = HexModelDefinition.HexNode;
    using HConnector = HexModelDefinition.HexConnector;
    class ShipbuilderView : MonoBehaviour  {
        [SerializeField] Transform root;
        [SerializeField] Transform phantomRoot;

        [SerializeField] GameObject nodePrefab;
        [SerializeField] GameObject tubePrefab;

        [Header("UI")]
        [SerializeField] Camera editorCamera;
        [SerializeField] Transform uiRoot;
        [SerializeField] GameObject buttonPrefab;

        [SerializeField] Material hologram;

        Ship ship;

        Dictionary<string, StructureDeclaration> declarations = new();

        StructureDeclaration currentBlueprint;
        ModuleToShipFitter fitter;

        private void Start() {
            // fill toolbar with blueprints
            GenerateDeclarations();
            GenerateUI();
            fitter = new ModuleToShipFitter();

            ship = GenerateShipStub();

            HandleShipModelChanged();
        }

        private void Update() {
            // raycast
            var ray = editorCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit, Mathf.Infinity, 1 << 20, QueryTriggerInteraction.Collide)) {
                var hitObject = hit.collider.gameObject;
                var node = hitObject.GetComponentInParent<NodeView>().Node;
                var normal = hit.normal;
                var dir = GetDirectionFromCartesianFacing(normal);
                Debug.Log($"{node.Structure.name} [node {node.IndexInStructure}] : {dir}");

                var fit = fitter.TryGetFit(currentBlueprint, node.Pose.position, dir);
                if (fit.success)
                    PositionPhantom(fit.poseOfPhantom);
                else 
                    PositionPhantom(new H3Pose((0,0,5)));
                
                Debug.Log(fit.remarks);
            }
        }

        PrismaticHexDirection GetDirectionFromCartesianFacing(Vector3 cartesianFacing) {
            var v = cartesianFacing.normalized;
            if (v.z > 0.8f) return new PrismaticHexDirection(HexDir.None, 1);
            if (v.z <-0.8f) return new PrismaticHexDirection(HexDir.None, -1);
            var atandeg = Mathf.Atan2(v.x, v.y) * Mathf.Rad2Deg;
            // Debug.Log($"{atandeg}");
            var rotations = Mathf.RoundToInt(atandeg / 60f);
            return new PrismaticHexDirection(HexUtils.FromClockwiseRotationSteps(rotations), 0);
        }


        private void GenerateUI() {
            var y = 0;
            foreach (var decl in declarations.Values) {
                var buttonGO = Instantiate(buttonPrefab, uiRoot);
                buttonGO.transform.SetLocalPositionAndRotation(new Vector3(0,y,0), Quaternion.identity);
                var btn = buttonGO.GetComponent<Button>();
                btn.Clicked += () => SelectDeclaration(decl);
                btn.Label = decl.ID;
                y+=30;
            }
        }

        private void SelectDeclaration(StructureDeclaration decl) {
            currentBlueprint = decl;
            ConstructPhantom(decl);
            PositionPhantom(
                new H3Pose((0,0,4), 0)
            );
        }

        private void GenerateDeclarations() {
            DeclareStructure("Spine Segment", "spine");
            DeclareStructure("Habitat module", "omni");
            DeclareStructure("Radiator", "radiator3");
        }

        private void DeclareStructure(string structureID, string modelID) {
            var m = new ModelIO();
            var l = m.LoadModel(modelID);
            var sd = new StructureDeclaration {
                hexModel = l,
                ID = structureID,
            };
            declarations.Add(structureID, sd);
        }

        private Ship GenerateShipStub() {
            var s = new Ship();
            s.BuildStructure(declarations["Spine Segment"], new H3(0,0,0), 0);
            return s;
        }
        
        void HandleShipModelChanged() {
            RegenerateShipVisuals();
            fitter.PrecomputeAttachpoints(ship);
            RegenerateShipAttachPoints();
        }

        private void RegenerateShipAttachPoints() {
            foreach (var att in fitter.attachments) {
                var p1 = att.connectorWorldspaceOriginHex.CartesianPosition();
                var p2 = (att.connectorWorldspaceOriginHex + att.connectorWorldspaceDirection).CartesianPosition();
                var p = Vector3.Lerp(p1, p2, 0.5f);
                var dir = Quaternion.LookRotation(p2 - p1);

                var holoInstance = Instantiate(tubePrefab, root);
                holoInstance.transform.SetLocalPositionAndRotation(p, dir);
                foreach (var cmp in holoInstance.GetComponentsInChildren<MeshRenderer>()) cmp.sharedMaterial = hologram;
                holoInstance.name = $"Attachment {att.structure.name}/{att.connector.index}/";
            }
        }

        private void RegenerateShipVisuals() {
            foreach (var node in ship.Nodes) {
                var p = node.Pose.CartesianPose();
                var instance = Instantiate(nodePrefab, root);
                instance.transform.SetLocalPositionAndRotation(p.position, p.rotation);
                instance.GetComponent<NodeView>().Node = node;
                instance.name = $"{node.Structure.name}[{node.IndexInStructure}]";
            }

            foreach (var tube in ship.Tubes) {
                var instance = Instantiate(tubePrefab, root);
                var from = tube.CrdsFrom.CartesianPosition();
                var to = tube.CrdsTo.CartesianPosition();
                var coords = Vector3.Lerp(from, to, 0.5f);
                var rot = Quaternion.LookRotation(to - from);
                instance.transform.SetLocalPositionAndRotation(coords, rot);
            }
        }


        // concept: CURRENT BLUEPRINT
        // concept: EXISTING SHIP MODEL
        // concept: FITTING

        // on update, raycast into the existing model to check where we are hitting.
        // if we hit something, use hit position info to try to FIT the CURRENT BLUEPRINT to the EXISTING SHIP MODEL
        // if such a fit is possible, show the PHANTOM
        // phantom should be separate from the ship, should accept no raycasts, etc.

        public void ConstructPhantom(StructureDeclaration declaration) {
            
            foreach (Transform child in phantomRoot.transform) { Destroy(child.gameObject); }

            phantomRoot.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

            var index = 0; 

            foreach (var node in declaration.hexModel.nodes) {
                var nodeInstance = Instantiate(nodePrefab, node.hex.CartesianPosition(), Quaternion.identity, phantomRoot);
                nodeInstance.name = $"Blueprint {declaration.ID} node {index++}";

                foreach (var t in nodeInstance.GetComponentsInChildren<Transform>()) t.gameObject.layer = 0;
                foreach (var mr in nodeInstance.GetComponentsInChildren<MeshRenderer>()) mr.sharedMaterial = hologram;
            }

            index = 0;
            foreach (var tube in declaration.hexModel.connections) {
                var hexA = tube.sourceHex;
                var hexB = tube.sourceHex + tube.direction;
                var a = hexA.CartesianPosition();
                var b = hexB.CartesianPosition();
                var coords = Vector3.Lerp(a,b, 0.5f);
                var rot = Quaternion.LookRotation(b-a, Vector3.up);

                var tubeInstance = Instantiate(tubePrefab, coords, rot, phantomRoot);
                tubeInstance.name = $"Blueprint {declaration.ID} tube {index++}";

                foreach (var t in tubeInstance.GetComponentsInChildren<Transform>()) t.gameObject.layer = 0;
                foreach (var mr in tubeInstance.GetComponentsInChildren<MeshRenderer>()) mr.sharedMaterial = hologram;
            }
        }

        void PositionPhantom(H3Pose pose) {
            var parentPose = pose.CartesianPose();
            phantomRoot.transform.SetLocalPositionAndRotation(parentPose.position, parentPose.rotation);
        }

    }
}