using System.Collections.Generic;
using Core.H3;
using UnityEngine;
using Void.ColonySim.Model;
using Void.ColonySim.BuildingBlocks;
using Void.ColonySim;
using Void;
using System;
using System.IO;

namespace Scanner.Atomship {

    enum Tools {
        Inspect,
        Construct,
        Destroy,
    }

    abstract class ShipScreen : MonoBehaviour {
        [SerializeField] protected Transform root;
        [SerializeField] protected Camera editorCamera;
        [SerializeField] protected Transform uiRoot;
        [SerializeField] protected GameObject buttonPrefab;
        [SerializeField] protected TMPro.TMP_Text tooltip;

        protected void SetTooltip(string text) {
            tooltip.text = text;
        }

    }

    class ShipbuilderView : ShipScreen {
        
        [SerializeField] Transform phantomRoot;

        [SerializeField] GameObject nodePrefab;
        [SerializeField] GameObject tubePrefab;        
        
        [SerializeField] bool autopersistChanges;

        [SerializeField] Material hologramMaterial;
        

        ModuleDeclaration currentBlueprint;
        ModuleToShipFitter fitter;

        ColonyShipStructure CurrentShipStructure => Void.Game.Colony.ShipStructure;

        private void Start() {
            GenerateUI();
            fitter = new ModuleToShipFitter();

            if (autopersistChanges) {
                LoadShip();
            }

            Game.Colony.AddSystem(new TemperatureGrid());
            Game.Colony.AddSystem(new LifeSupportGrid());

            HandleShipModelChanged();
        }

        const string shipname = "_currentShip.ship";
        
        private void SaveShip() {
            var s = new Void.Serialization.ShipSerializer();
            var blob = s.SerializeStructure(CurrentShipStructure);
            var finalPath = Path.Combine(Application.persistentDataPath, shipname);
            File.WriteAllBytes(finalPath, blob);
        }

        private void LoadShip() {
            var s = new Void.Serialization.ShipSerializer();
            var finalPath = Path.Combine(Application.persistentDataPath, shipname);
            if (!File.Exists(finalPath)) { Debug.LogWarning("Ship save file does not exist, skipping"); return; }
            try {
                var blob = File.ReadAllBytes(finalPath);
                var newStructure = s.DeserializeStructure(blob);
                var c = new Colony(newStructure);
                Game.ReplaceColony(c);

            } catch (Exception e) {
                Debug.LogError("Could not load ship. Autopersist set to FALSE in case you want to repro. Exception as follows:"); 
                Debug.LogException(e);
                autopersistChanges = false;
            }
        }

        H3 lastHitHex;
        PrismaticHexDirection lastHitPose;
        Fit lastGoodFit;
        bool previewPhantom;

        private void Update() {

            if (Input.GetKeyDown(KeyCode.T)) Game.Colony.SimTick();

            var ray = editorCamera.ScreenPointToRay(Input.mousePosition);
            previewPhantom = false;
            SetTooltip("");
            
            if (Physics.Raycast(ray, out var hit, Mathf.Infinity, 1 << 20, QueryTriggerInteraction.Collide)) {
                var hitObject = hit.collider.gameObject;
                var node = hitObject.GetComponentInParent<NodeView>().Node;
                var normal = hit.normal;
                var dir = GetDirectionFromCartesianFacing(normal);

                SetTooltip($"{node.Structure.name}");

                var doCheckForNewFit = node.Pose.position != lastHitHex || dir != lastHitPose;

                lastHitHex = node.Pose.position; lastHitPose = dir;

                previewPhantom = lastGoodFit != null;
                
                if (doCheckForNewFit) { 
                    lastGoodFit = fitter.TryGetFit(currentBlueprint, node.Pose.position, dir);
                    if (lastGoodFit.success) { 
                        PositionPhantom(lastGoodFit.poseOfPhantom);
                    }
                }

            } else {
                lastGoodFit = null;
                lastHitHex = default; lastHitPose = default;
            }

            phantomRoot.gameObject.SetActive(previewPhantom);
            
            if (Input.GetMouseButtonDown(0)) {
                if (lastGoodFit != null) { 
                    CurrentShipStructure.BuildModule(currentBlueprint, lastGoodFit.poseOfPhantom.position, lastGoodFit.poseOfPhantom.rotation);
                    foreach (var conn in lastGoodFit.connections)
                        CurrentShipStructure.BuildTube(conn.from, conn.to, "default");
                    
                    HandleShipModelChanged();
                    lastGoodFit = null;
                }
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
            foreach (var decl in Void.Game.Rules.Modules) {
                var buttonGO = Instantiate(buttonPrefab, uiRoot);
                buttonGO.transform.SetLocalPositionAndRotation(new Vector3(0,y,0), Quaternion.identity);
                var btn = buttonGO.GetComponent<Button>();
                btn.Clicked += () => SelectDeclaration(decl);
                btn.Label = decl.id;
                y+=30;
            }
        }

        private void SelectDeclaration(ModuleDeclaration decl) {
            currentBlueprint = decl;
            ConstructPhantom(decl);
            PositionPhantom(
                new H3Pose((0,0,4), 0)
            );
            lastGoodFit = null; // so the phantom is hidden?
            
        }

        void HandleShipModelChanged() {
            RegenerateShipVisuals();
            fitter.PrecomputeAttachpoints(CurrentShipStructure);
            Game.Colony.GetSystem<TemperatureGrid>().RegenerateGraph(Game.Colony);
            Game.Colony.GetSystem<LifeSupportGrid>().RegenerateGraph(Game.Colony);
            if (autopersistChanges) SaveShip();
            // RegenerateShipAttachPoints();
        }

        private void RegenerateShipAttachPointVisuals() {
            foreach (var att in fitter.attachments) {
                var p1 = att.connectorWorldspaceOriginHex.CartesianPosition();
                var p2 = (att.connectorWorldspaceOriginHex + att.connectorWorldspaceDirection).CartesianPosition();
                var p = Vector3.Lerp(p1, p2, 0.5f);
                var dir = Quaternion.LookRotation(p2 - p1);

                var holoInstance = Instantiate(tubePrefab, root);
                holoInstance.transform.SetLocalPositionAndRotation(p, dir);
                foreach (var cmp in holoInstance.GetComponentsInChildren<MeshRenderer>()) cmp.sharedMaterial = hologramMaterial;
                holoInstance.name = $"Attachment {att.structure.name}/{att.connector.index}/";
            }
        }

        private void RegenerateShipVisuals() {
            foreach (Transform t in root) Destroy(t.gameObject);

            foreach (var node in CurrentShipStructure.Nodes) {
                var p = node.Pose.CartesianPose();
                var instance = Instantiate(nodePrefab, root);
                instance.transform.SetLocalPositionAndRotation(p.position, p.rotation);
                instance.GetComponent<NodeView>().Node = node;
                instance.name = $"{node.Structure.name}[{node.IndexInStructure}]";
            }

            foreach (var tube in CurrentShipStructure.Tubes) {
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

        public void ConstructPhantom(ModuleDeclaration declaration) {
            
            foreach (Transform child in phantomRoot.transform) { Destroy(child.gameObject); }

            phantomRoot.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

            var index = 0; 

            var blueprint = declaration.GetBlueprint();

            foreach (var node in blueprint.nodes) {
                var nodeInstance = Instantiate(nodePrefab, node.hex.CartesianPosition(), Quaternion.identity, phantomRoot);
                nodeInstance.name = $"Blueprint {declaration.id} node {index++}";

                foreach (var t in nodeInstance.GetComponentsInChildren<Transform>()) t.gameObject.layer = 0;
                foreach (var mr in nodeInstance.GetComponentsInChildren<MeshRenderer>()) mr.sharedMaterial = hologramMaterial;
            }

            index = 0;
            foreach (var tube in blueprint.connections) {
                var hexA = tube.sourceHex;
                var hexB = tube.sourceHex + tube.direction;
                var a = hexA.CartesianPosition();
                var b = hexB.CartesianPosition();
                var coords = Vector3.Lerp(a,b, 0.5f);
                var rot = Quaternion.LookRotation(b-a, Vector3.up);

                var tubeInstance = Instantiate(tubePrefab, coords, rot, phantomRoot);
                tubeInstance.name = $"Blueprint {declaration.id} tube {index++}";

                foreach (var t in tubeInstance.GetComponentsInChildren<Transform>()) t.gameObject.layer = 0;
                foreach (var mr in tubeInstance.GetComponentsInChildren<MeshRenderer>()) mr.sharedMaterial = hologramMaterial;
            }
        }

        void PositionPhantom(H3Pose pose) {
            var parentPose = pose.CartesianPose();
            phantomRoot.transform.SetLocalPositionAndRotation(parentPose.position, parentPose.rotation);
        }

    }
}