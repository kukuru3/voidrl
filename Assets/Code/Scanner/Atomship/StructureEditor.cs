using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Core.H3;
using K3.Hex;
using UnityEngine;
using Void.ColonySim.Model;


namespace Scanner.Atomship {
    using HNode = HexModelDefinition.HexNode;
    using HConnector = HexModelDefinition.HexConnector;

    internal class StructureEditor : MonoBehaviour {
        [SerializeField] GameObject _blankHexPrefab;
        [SerializeField] GameObject _directionPrefab;
        [SerializeField] GameObject _buttonPrefab;
        [SerializeField] GameObject _togglePrefab;

        [SerializeField] GameObject _listItemPrefab;

        [SerializeField] GameObject _nodePrefab;
        [SerializeField] GameObject _connectorPrefab;

        [SerializeField] Transform _modelRoot;

        [SerializeField] TMPro.TMP_Text     label;
        [SerializeField] TMPro.TMP_InputField shipNameInput;

        [SerializeField] Transform  uiRoot;
        [SerializeField] Transform  flagsUIRoot;
        [SerializeField] Transform  loadList;
        [SerializeField] RectTransform loadListContent;



        HexModelDefinition model;
        ModelIO modelIO;

        H3 cursor3d = default;
        PrismaticHexDirection direction = new PrismaticHexDirection(HexDir.Top, 0);

        GameObject posCursorGO;
        GameObject dirCursorGO;

        bool needsToUpdateModelGeometry;
        List<ModelValidator> validators = new();
        bool modelValid;
        public delegate (bool valid, string fault) ModelValidator(HexModelDefinition model);

        enum BtnState {
            Hide, 
            ShowInactive,
            ShowActive
        }

        string ShipName => shipNameInput.text;

        private void Start() {
            model = new();
            modelIO = new();
            posCursorGO = Instantiate(_blankHexPrefab);
            dirCursorGO = Instantiate(_directionPrefab);

            CreateButton("Set Node", NodeNotUnderCursor, SetNodeAtCursor);
            CreateButton("Delete Node", NodeUnderCursor, ClearNodeAtCursor);

            CreateButton("Create connector", NoConnectorUnderCursor, CreateConnector);
            CreateButton("Remove connector", ConnectorUnderCursor, RemoveConnector);

            CreateButton("Save", () => {
                if (string.IsNullOrEmpty(ShipName)) return BtnState.Hide;
                if (!modelValid) return BtnState.Hide; else return BtnState.ShowActive;
            }, DoSaveModel);

            CreateButton("Load...", IsLoadWindowNotShown, ShowLoadWindow);
            CreateButton("New", null, CreateNew);

            CreateFlagToggle("Aligner");
            CreateFlagToggle("Mandatory");
            CreateFlagToggle("Socket");

            CreateFlagToggle("Structural");

            CreateFlagToggle("Flavour 1");
            CreateFlagToggle("Flavour 2");
            CreateFlagToggle("Flavour 3");
            CreateFlagToggle("Flavour 4");
            CreateFlagToggle("Flavour 5");
            CreateFlagToggle("Flavour 6");
            
            InjectValidator(m => { if (m == null) return (false, "Model is null"); return (true, ""); });
            InjectValidator(m => { 
                var numRemoved = 0;
                foreach (var connector in m.connections.ToArray()) {
                    var a = GetNode(connector.sourceHex);
                    var b = GetNode(connector.sourceHex + connector.direction);
                    if (a != null && b != null) {
                        if (m.connections.Remove(connector)) numRemoved++;    
                    }
                }
                return (true, (numRemoved > 0) ? $"Removed {numRemoved} internal connectors" : "");
            });

            InjectValidator(m => {
                if (model.nodes.Count ==0) return (false, "No nodes");
                if (model.connections.Count == 0) return (false, "No connectors");
                return (true, "");
            });
        }

        List<Toggle> flagToggles= new();

        private void CreateFlagToggle(string name) { 
            var obj = Instantiate(_togglePrefab, flagsUIRoot);
            var toggle = obj.GetComponent<Toggle>();
            var bit = flagToggles.Count;
            flagToggles.Add(toggle);
            toggle.ValueChanged += () => OnToggleValueChanged(bit, toggle.ToggleState);
            obj.GetComponentInChildren<TMPro.TMP_Text>().text = name;
            obj.transform.localPosition = new Vector3(0, (flagToggles.Count-1) * 30, 0);
        }

        private void OnToggleValueChanged(int bit, bool toggleState) {
            var c = GetConnector(cursor3d, direction);
            
            if (toggleState)
                c.flags |= 1 << bit;
            else {
                var mask = ~(1 << bit);
                c.flags = c.flags & mask;
            }

            ModelChanged();
        }

        private void DoSaveModel() {
            loadList.gameObject.SetActive(false);
            model.identity = ShipName;
            modelIO.SaveModel(model);
        }

        BtnState IsLoadWindowNotShown() {
            return loadList.gameObject.activeInHierarchy ? BtnState.Hide: BtnState.ShowActive;
        }

        void CreateNew() {
            shipNameInput.text = "";
            model = new() { identity = "" };
            ModelChanged();
        }
        
        void ShowLoadWindow() {
            loadList.gameObject.SetActive(true);
            // repopulate list
            foreach (Transform t in loadListContent) Destroy(t.gameObject);
            
            var files = modelIO.EnumerateStructures();
            foreach (var f in files) {
                var bareFileName = Path.GetFileNameWithoutExtension(f);
                var go = Instantiate(_listItemPrefab, loadListContent);
                go.GetComponentInChildren<TMPro.TMP_Text>().text = bareFileName;
                go.GetComponentInChildren<UnityEngine.UI.Button>().onClick.AddListener(() => {
                    model = modelIO.LoadModel(bareFileName);
                    ModelChanged();
                    shipNameInput.text = model.identity;
                    loadList.gameObject.SetActive(false);
                });  
            }
        }

        BtnState NodeUnderCursor() {
            if (model == null) return BtnState.Hide;
            return model.nodes.Any(n => n.hex == cursor3d) ? BtnState.ShowActive : BtnState.Hide;
        }

        BtnState NodeNotUnderCursor() {
            if (model == null) return BtnState.Hide;
            return model.nodes.Any(n => n.hex == cursor3d) ? BtnState.Hide : BtnState.ShowActive;
        }

        BtnState ConnectorUnderCursor() => GetConnector(cursor3d, direction) != null ? BtnState.ShowActive : BtnState.Hide;
        BtnState NoConnectorUnderCursor() => GetConnector(cursor3d, direction) != null ? BtnState.Hide: BtnState.ShowActive;

        void ModelChanged() {
            needsToUpdateModelGeometry = true;
        }

        void SetNodeAtCursor() {
            model.nodes.Add(new HNode { hex = cursor3d });
            ValidateModel();
            ModelChanged();
        }

        void CreateConnector() {
            model.connections.Add(new HConnector { sourceHex = cursor3d, direction = direction });
            ValidateModel();
            ModelChanged();
        }

        void RemoveConnector() {
            var c = GetConnector(cursor3d, direction);
            if (c != null) { 
                model.connections.Remove(c);
                ValidateModel();
                ModelChanged();
            }
        }

        public void InjectValidator(ModelValidator validator) {
            validators.Add(validator);
        }

        private void ValidateModel() { 

            List<string> diag = new();
            bool anyFaulty = false;
            foreach (var vld in validators) {
                (var valid, string fault) = vld(model);
                if (!valid) anyFaulty = true; 
                if (!string.IsNullOrWhiteSpace(fault)) diag.Add(fault); 
            }

            if (anyFaulty) {
                label.text = string.Join("\r\n", diag);
                modelValid = false;
            } else {
                label.text = "";
                modelValid = true;
            }

            // remove orphan connectors

            // remove connectors between two nodes (these are always direct, by convention)

            // check if all nodes are part of the same "island"

            // check no overlapping nodes
        }

        HNode GetNode(H3 pos) => model?.nodes.FirstOrDefault(n => n.hex == pos);

        HConnector GetConnector(H3 at, PrismaticHexDirection dir) {
            var conn = model?.connections.FirstOrDefault(c => c.sourceHex == at && c.direction == dir);
            if (conn == null) {
                // symmetry
                at = at + dir.ToHexOffset();
                dir = dir.Inverse();
                conn = model?.connections.FirstOrDefault(c => c.sourceHex == at && c.direction == dir);
            }
            return conn;
        }

        void ClearNodeAtCursor() {
            var n = GetNode(cursor3d);
            if (n != null) { 
                model.nodes.Remove(n);
                ValidateModel();
                ModelChanged();
            }
        }

        private void RegenerateModelGeometry() { 
            foreach (Transform t in _modelRoot) Destroy(t.gameObject);

            foreach (var node in model.nodes) {
                var go = Instantiate(_nodePrefab, _modelRoot);
                go.transform.SetLocalPositionAndRotation(node.hex.CartesianPosition(), Quaternion.identity);
            }

            foreach (var conn in model.connections) {
                var go = Instantiate(_connectorPrefab, _modelRoot);
                var a = conn.sourceHex;
                var b = a + conn.direction;
                var A = a.CartesianPosition();
                var B = b.CartesianPosition();
                var pos = Vector3.Lerp(A, B, 0.5f);
                var rot = Quaternion.LookRotation(B - A, Vector3.up);
                go.transform.SetPositionAndRotation(pos, rot);
                go.GetComponentInChildren<MeshRenderer>().material.color = GetConnectorColor(conn);
            }
        }

        private Color GetConnectorColor(HConnector conn) {
            var c = new Color(0f, 1f, 0f);

            if ((conn.flags & (1 << 1)) > 0) c = new Color(1f, 1f, 0); // plug
            if ((conn.flags & (1 << 2)) > 0) c = new Color(1f, 0, 0f); // socket
            if ((conn.flags & (1 << 3)) > 0) c = new Color(1f,1f,1f); // structural

            if ((conn.flags & (1 << 0)) > 0) c += new Color(0f, 0f, 1f); // aligner

            return c;
        }

        List<(Button b, Func<BtnState> condition, Action onclick)> buttons = new();

        void CreateButton(string caption, Func<BtnState> enabledCondition, Action clickAction) {
            var btn = Instantiate(_buttonPrefab, uiRoot);
            var btnC = btn.GetComponent<Button>();
            btnC.Label = caption;

            buttons.Add((btnC, enabledCondition, clickAction));
            if (clickAction != null) btnC.Clicked += () => clickAction();
        }

        void UpdateToolbar() {
            int shownBtns = 0;
            for (var i = 0; i < buttons.Count; i++) {

                BtnState resolvedState;
                if (buttons[i].condition != null) resolvedState = buttons[i].condition(); else resolvedState = BtnState.ShowActive;
                
                buttons[i].b.Enabled = resolvedState == BtnState.ShowActive;
                var shown = resolvedState != BtnState.Hide;
                buttons[i].b.gameObject.SetActive(shown);
                if (shown) {
                    buttons[i].b.transform.localPosition = new Vector3(0, shownBtns * 30, 0);
                    shownBtns++;
                }
            }
        }

        private void Update() {
            var prevc3d = cursor3d;
            var prevdir = direction;

            if (Input.GetKeyDown(KeyCode.W)) cursor3d += new H3(0,1, 0);
            if (Input.GetKeyDown(KeyCode.S)) cursor3d += new H3(0,-1, 0);
            if (Input.GetKeyDown(KeyCode.D)) cursor3d += new H3(1, 0, 0);
            if (Input.GetKeyDown(KeyCode.A)) cursor3d += new H3(-1,0, 0);
            if (Input.GetKeyDown(KeyCode.Z)) cursor3d += new H3(0,0,-1);
            if (Input.GetKeyDown(KeyCode.X)) cursor3d += new H3(0,0,1);

            if (Input.GetKeyDown(KeyCode.Q)) { 
                if (direction.longitudinal != 0) direction = new PrismaticHexDirection(HexDir.Top, 0);
                else direction = direction.RotatedRadially(-1);
            }
            if (Input.GetKeyDown(KeyCode.E)) { 
                if (direction.longitudinal != 0) direction = new PrismaticHexDirection(HexDir.Top, 0);
                else direction = direction.RotatedRadially(1);
            }

            if (Input.GetKeyDown(KeyCode.F)) direction = new PrismaticHexDirection(HexDir.None, -1);
            if (Input.GetKeyDown(KeyCode.G)) direction = new PrismaticHexDirection(HexDir.None,  1);

            PositionCursors();

            if (cursor3d != prevc3d || direction != prevdir) {                
                UpdateFlagsWindow();
            }

            UpdateToolbar();

            if (needsToUpdateModelGeometry) {
                needsToUpdateModelGeometry = false;
                RegenerateModelGeometry();
                UpdateFlagsWindow();
                // update model geometry
            }
        }

        private void UpdateFlagsWindow() {
            var c = GetConnector(cursor3d, direction);
            flagsUIRoot.gameObject.SetActive(c != null);
            if (c != null) {
                for (var i = 0 ; i < flagToggles.Count; i++) {
                    flagToggles[i].ToggleState = (c.flags & (1 << i)) != 0;
                }
            }
        }

        void PositionCursors() {
            var pose = new H3Pose(cursor3d.hex, cursor3d.zed, 0);
            var p = pose.CartesianPose();
            posCursorGO.transform.SetPositionAndRotation(p.position, Quaternion.identity);

            // position direction cursor.
            var xy = Hexes.HexToPixel(direction.radial.Offset(), GridTypes.FlatTop, 0.5f);
            var z = direction.longitudinal * HexUtils.CartesianZMultiplier;

            var v = new Vector3(xy.x, xy.y, z);
            Quaternion rot = Quaternion.identity;

            if (v.sqrMagnitude > float.Epsilon) { 
                var up = Vector3.up; if (v.normalized.y > 0.99f) up = Vector3.forward;
                rot = Quaternion.LookRotation(v, up);
            }

            dirCursorGO.transform.SetPositionAndRotation(p.position + v, rot);

            posCursorGO.GetComponent<MeshRenderer>().enabled = Time.time % 0.3f < 0.15f;
        }
    
        // ui buttons: 
        // - create node at this position
        // - delete node at this position
        // - create connection at this marker.

        // validation: 
        // - connections that originate from a blank node are called ORPHAN and are deleted on validation.
    }
}