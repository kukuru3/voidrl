using UnityEngine;

namespace Scanner.Socketship {
    public class ContactView : MonoBehaviour, IPartOfShipbuilderView {
        // public bool IsCandidate { get; set; }

        public enum States {
            Inert,
            Slotted,
            BuildCandidate,
            ActiveBuildCandidate,
        }

        public States State { get; set; }
        
        [SerializeField] Shapes.ShapeRenderer shape;
        [SerializeField] Button button;
        [SerializeField] Shapes.ShapeRenderer activeIndicator;

        internal Contact Contact { get; private set; }
        internal ShipBuilderController ParentView { get; private set; }

        public void Bind(ShipBuilderController parent, Contact contact) {
            this.ParentView = parent;
            this.Contact = contact;

            // RecalculateVisuals();
            // shape.Color = contact.decl is PlugDecl ? Color.red : Color.blue;
        }

        void Start() {
            button.Clicked += () => this.ParentView.HandleContactButtonClicked(this);
        }

        void Update() {
            RecalculateVisuals();
        }

        public void RecalculateVisuals() {            
            var isPlug = Contact.decl is PlugDecl;

            if (shape is Shapes.Disc disc) {
                disc.Type = isPlug ? Shapes.DiscType.Disc : Shapes.DiscType.Ring;
                disc.Radius = isPlug ? 3 : 5;
                activeIndicator.enabled = false;
                button.gameObject.SetActive(false);
                switch (State) {
                    case States.Inert:
                        disc.enabled = true; disc.Thickness = 0.3f; disc.Color = Color.red;
                        break;
                    case States.Slotted:
                        
                        disc.enabled = true; disc.Thickness = 0.3f; disc.Color = Color.white;
                        break;
                    case States.ActiveBuildCandidate:
                        button.gameObject.SetActive(true);
                        disc.enabled = false;
                        activeIndicator.enabled = true;
                        break;
                    case States.BuildCandidate:
                        disc.enabled = false;
                        button.gameObject.SetActive(true);
                        break;
                }
            }
        }

        
    }
}
