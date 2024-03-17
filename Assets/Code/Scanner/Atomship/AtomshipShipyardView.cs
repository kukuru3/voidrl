using Core;
using Core.H3;
using UnityEngine;

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
        [SerializeField] TMPro.TMP_Text reason;


        Ship ship;
        Fitter fitter;

        private void Start() {
            fitter = new Fitter();
            Hardcoder.HardcodeRules();
            this.ship = Hardcoder.GenerateInitialShip();
            GenerateUI();
            RegenerateShipView();
            fitter.PreComputeAttachmentData(ship);
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
                var cartesianPose = p.CartesianPose();
                cartesianPose.position.z *= 1.73f;
                var go = Instantiate(nodePrefab, shipRoot);
                go.transform.SetLocalPositionAndRotation(cartesianPose.position, cartesianPose.rotation);
                go.GetComponent<NodeView>().Node = node;
                go.name = $"Node:{node.Structure.Declaration.ID}[{node.IndexInStructure}]";
            }

            foreach (var tube in ship.Tubes) {
                var from = tube.moduleFrom.Pose.CartesianPose().position;
                var to = tube.moduleTo.Pose.CartesianPose().position;
                from.z *= 1.73f;
                to.z *= 1.73f;
                var go = Instantiate(tubePrefab, shipRoot);
                go.transform.SetPositionAndRotation((from + to) / 2, Quaternion.LookRotation(to - from));
                // go.transform.localScale = new Vector3(0.1f, 0.1f, Vector3.Distance(from, to));
                go.name = $"Tube:{tube.moduleFrom}=>{tube.moduleTo}";
            }   
        }

        void RegeneratePhantomView(StructureDeclaration decl, H3Pose phantomPose) {
            
            foreach (Transform t in phantomRoot) Destroy(t.gameObject);
            foreach (var feat in decl.nodeModel.features) {
                if (feat.type == FeatureTypes.Part) {
                    var p = phantomPose * new H3Pose(feat.localCoords, 0);
                    var cartesianPose = p.CartesianPose(); cartesianPose.position.z *= 1.73f;
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
                var worldspaceDirection = default(HexDir); // Hex3Utils.ComputeDirectionFromNormal(hit.normal);

                // "node" and "dir" are sufficient for us
                var attachmentPoint = fitter.FindAttachment(node.Pose.position, worldspaceDirection);

                if (attachmentPoint == null) {
                    ClearPhantomView();
                    reason.text = "Select attachment point";
                    return;
                }

                var fit = fitter.TryFitStructure(currentBlueprint, attachmentPoint);
                if (fit.fits) {
                    RegeneratePhantomView(currentBlueprint, fit.alignmentOfBlueprint);
                    reason.text = "";
                } else {
                    ClearPhantomView();
                    reason.text = fit.failReason;
                }

                if (Input.GetMouseButtonDown(0)) {
                    if (fit.fits) {
                        ship.BuildStructure(currentBlueprint, 0,  fit.alignmentOfBlueprint.position, fit.alignmentOfBlueprint.rotation);
                        foreach (var couple in fit.couplings) {
                            ship.BuildTube(couple.attachment.sourceHexWS, couple.attachment.targetHexWS, "tube");
                        }
                        ClearPhantomView();
                        RegenerateShipView();
                        fitter.PreComputeAttachmentData(ship);
                    }
                }
            } else {
                ClearPhantomView();
            }
        }

        private void ClearPhantomView() {
            foreach (Transform t in phantomRoot) Destroy(t.gameObject);
        }
    }
}