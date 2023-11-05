using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Scanner.Socketship {

    public class ShipBuilderController : MonoBehaviour {
        [SerializeField] Selector2 partSelector;
        [SerializeField] float distance;

        [Header("Prefabs")]
        [SerializeField] GenericShipPartView partViewPrefab;
        [SerializeField] ContactView contactViewPrefab;
        [SerializeField] Button buildButtonPrefab;

        [Serializable]
        struct SpecializedPrefab {
            [SerializeField] internal string id;
            [SerializeField] internal ShipPartView viewPrefab;
        }
        [SerializeField] SpecializedPrefab[] prefabs;

        ShipBuilder builder;

        private void Start() {
            partSelector.IndexChanged += OnPartIndexChanged;
            Bind(new ShipBuilder());

            builder.AddedRootPart += HandleNewRootPart;
            builder.ConnectionMade += HandleNewConnection;
            builder.ghostPart = null;
            builder.GenerateShipPart("Engine Block"); 

        }

        private void HandleNewConnection(Connexion obj) {
            UpdateVisualisers();
        }

        private void HandleNewRootPart(Part obj) {
            UpdateVisualisers();
        }

        Dictionary<Part, ShipPartView> maintainedVisualisers = new();

        void UpdateVisualisers() {

            var currentParts = builder.ship.ListAllParts();

            var visualisedParts = maintainedVisualisers.Select(v => v.Key);

            var partsWithexcessVisualisers = visualisedParts.Except(currentParts).ToList();
            var missingVisualisers = currentParts.Except(visualisedParts).ToList(); 

            foreach (var missing in missingVisualisers) {
                Debug.Log($"Adding MISSING visualiser for {missing.declaration.name}");
                var view = GeneratePartView(missing);
                maintainedVisualisers.Add(missing, view);
            }

            foreach (var excess in partsWithexcessVisualisers) {
                Debug.Log($"Destroying excess visualiser for {excess.declaration.name}");
                Destroy(maintainedVisualisers[excess].gameObject);
                maintainedVisualisers.Remove(excess);
            }

            foreach (var vis in maintainedVisualisers) {
                var t = vis.Value.transform;
                var pos = BuilderToLocal(vis.Key.ResultingPosition());
                t.localPosition = new Vector3(pos.x, pos.y, vis.Key.PlugDepth() * -5);
            }
            ClearButtons();
            UpdateContactVis();
        }

        //int GetDepth(Transform trans) {
        //    var d = 0; 
        //    for (var t = trans; t != null; t = t.parent) {
        //        d++;
        //    }
        //    return d;
        //}

        private ShipPartView GeneratePartView(Part part) {

            var p = prefabs.FirstOrDefault(p => p.id == part.declaration.name);
            var prefab = p.viewPrefab ?? partViewPrefab;
            
            var partView = Instantiate(prefab, transform);
            partView.name = $"{part.declaration.name}";
            partView.Bind(this, part);
            var i = 0;
            foreach (var contact in part.contacts) {
                var contactView = Instantiate(contactViewPrefab, partView.transform);
                contactView.Bind(this, contact);
                var pos = this.BuilderToLocal(contact.decl.offset);
                var depth = contact.part.PlugDepth() - 1;
                contactView.transform.localPosition = new Vector3(pos.x, pos.y, (depth - 2) * -5);
                contactView.name = contact.decl is PlugDecl ? $"Plug {i++}" : $"Socket {i++}";
            }
            return partView;
        }

        internal void Bind(ShipBuilder builder) {
            this.builder = builder;
            partSelector.ClearItems();
            foreach (var part in builder.database.parts) {
                partSelector.AddItem(part.name, part);  
            }
        }

        internal void Update() {

            //if (Input.GetKeyDown(KeyCode.Mouse0)) {
            //    // var match = TryFindMatch();
            //}

            //if (Input.GetKeyDown(KeyCode.Space)) {
            //    //if (builder.ghostPart == null) return;
            //    //var allParts = builder.ship.ListAllParts();
            //    //var matches = builder.ListContactMatches(builder.ghostPart).ToList();
            //    //var str = string.Join(",", allParts.Select(p => p.declaration.name));
            //    //var sb = new StringBuilder();
            //    //sb.AppendLine($"all parts: {str}");
            //    //sb.AppendLine($"{matches.Count} matches!");
            //    //foreach (var item in matches) {
            //    //    sb.AppendLine($"- at {item.socket.part.declaration.name}");
            //    //}
            //    //Debug.Log(sb);
            //}

            //if (ghostPartView != null) { 
            //// update phantom
            //    var worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            //    var localP = transform.InverseTransformPoint(worldPoint);
            //    var builderP = LocalToBuilder(localP);

            //    localP.z = 0;
            //    ghostPartView.transform.localPosition = localP;
            //}
        }

        private void OnPartIndexChanged(int idx) {
            builder.ghostPart = new Part(builder.database.parts[idx], null);
            UpdateGhostPartView();
            ClearButtons();
            UpdateContactVis();
        }

        ShipPartView ghostPartView;
        private void UpdateGhostPartView() {
            return; // 
            if (ghostPartView != null) {
                Destroy(ghostPartView.gameObject);
            }

            if (builder.ghostPart != null) { 
                ghostPartView = GeneratePartView(builder.ghostPart);
            }
            ghostPartView.gameObject.name = $"GHOST : {builder.ghostPart.declaration.name}";
        }

        List<ContactMatch> _matchesCache;
        List<Part> _partCache;

        Contact activeContact;

        private void UpdateContactVis() {
            _partCache = builder.ship.ListAllParts();
            // _matchesCache  = builder.ListContactMatches(builder.ghostPart).ToList();
            _matchesCache = builder.ListAllPossibleContactMatches().ToList();

            foreach (var view in maintainedVisualisers.Values) {
                var contactViews = view.GetComponentsInChildren<ContactView>();

                foreach (var contactView in contactViews) {
                    contactView.State = ContactView.States.Inert;
                    if (contactView.Contact.connection != null) {
                        contactView.State = ContactView.States.Slotted;
                    } else if (_matchesCache.Any(c => c.socket == contactView.Contact)) {
                        contactView.State = ContactView.States.BuildCandidate;
                        if (activeContact == contactView.Contact) {
                            contactView.State = ContactView.States.ActiveBuildCandidate;
                        }
                    }
                }
            }
        }

        internal Vector2 BuilderToLocal(Vector2 builderPos) {
            return builderPos * distance;
        }



        internal Vector2 LocalToBuilder(Vector2 localPos) {
            return localPos / distance;
        }

        public void HandleContactButtonClicked(ContactView contactView) {
            var applicableStructures = _matchesCache.Where(m => m.socket == contactView.Contact);

            var strings = string.Join(",", applicableStructures.Select(m => m.plug.part.declaration.name));
            Debug.Log($"Generating buttons for contact [{contactView.Contact}]. Can attach: {strings}");
            activeContact = contactView.Contact;
            
            RegenerateButtons(activeContact, applicableStructures);
            UpdateContactVis();
        }

        List<GameObject> maintainedButtons = new List<GameObject>();

        void ClearButtons() {
            foreach (var btn in maintainedButtons) Destroy(btn.gameObject);
            maintainedButtons.Clear();
        }


        
        private void RegenerateButtons(Contact activeContact, IEnumerable<ContactMatch> applicableStructures) { 
            ClearButtons();

            var i = 0; 
            foreach (var item in applicableStructures) { 
                var btn = Instantiate(buildButtonPrefab, transform);
                var p = BuilderToLocal(activeContact.part.ResultingPosition() + item.socket.decl.offset) + new Vector2(90, i-- * -35);
                btn.transform.localPosition = new Vector3(p.x, p.y, -50);
                btn.name = $"Construct Btn : {item.plug.part.declaration.name}";
                btn.Clicked += () => Construct(item);
                foreach (var txt in btn.GetComponentsInChildren<TMPro.TMP_Text>(true)) txt.text = item.plug.part.declaration.name;
                maintainedButtons.Add(btn.gameObject);
            }
        }

        private void Construct(ContactMatch match) {
            builder.ship.Connect(match.plug, match.socket);
            UpdateVisualisers();
        }
    }
}
