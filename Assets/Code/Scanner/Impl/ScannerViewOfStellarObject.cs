using System.Linq;
using Core;
using Scanner.ScannerView;
using UnityEngine;
using Void.Entities.Components;

namespace Scanner.Impl {
    internal class ScannerViewOfStellarObject : MonoBehaviour {
        internal StellarObject StellarObject { get; private set; }

        CameraController3D camCtrlr;

        [SerializeField] GameObject main;
        [SerializeField] GameObject shadow;
        [SerializeField] Shapes.Line shadowLine;
        [SerializeField] TMPro.TMP_Text label;

        const float SCALE = 3.0f;

        public void Initialize(StellarObject obj) {
            this.StellarObject = obj;
            main.transform.position = obj.galacticPosition * SCALE;
            
            var zeroPos = obj.galacticPosition;
            zeroPos.y = 0;
            shadow.transform.position = zeroPos * SCALE;

            var h = obj.galacticPosition.y;
            shadowLine.Start = new Vector3(0,0,0);
            shadowLine.End = new Vector3(0, 0, h * SCALE);

            camCtrlr = CustomTag.Find(ObjectTags.ScannerCamera).GetComponent<CameraController3D>();

            var primaries = obj.Primaries;
            if (primaries.Count == 0) {
                Debug.LogWarning($"{obj.name} contains no substellars");
                return;
            }
            var firstPrimary = obj.Primaries.First();

            var typeString = firstPrimary.type.ToString();

            if (primaries.Count == 1 && firstPrimary.type == StellarSubobjects.MainSequenceStar) {
                typeString = "";
            }

            if (primaries.Count == 2) typeString = "Binary star system";
            if (primaries.Count == 3) typeString = "Ternary star system";
            if (primaries.Count == 4) typeString = "Quaternary star system";
            if (primaries.Count >= 5) typeString = $"{primaries.Count}x system";

            string color = "444";

            if (primaries.Count == 1 && firstPrimary.type == StellarSubobjects.BrownDwarf) {
                color = "200";
            } else if (primaries.Count == 1 && firstPrimary.type == StellarSubobjects.WhiteDwarf) {
                color = "00c";
            }

            label.text = $"<color=#{color}>{obj.name}</color>"; 
            if (!string.IsNullOrWhiteSpace(typeString)) label.text += $"\r\n<size=80%>{typeString}</size>";

            var neps = obj.ContainedSubstellars.Where(ss => ss.type == StellarSubobjects.NeptunianPlanet).Count();
            var jups = obj.ContainedSubstellars.Where(ss => ss.type == StellarSubobjects.JovianPlanet).Count();
            var ter  = obj.ContainedSubstellars.Where(ss => ss.type == StellarSubobjects.TerrestrialPlanet).Count();

            var numPlanets = neps + jups + ter;

            if (numPlanets > 0) {
                label.text += $"\r\n<size=80%><color=#fa0>{numPlanets} planets</color></size>";
            }

            if (Mathf.Abs(obj.galacticPosition.y)< 0.05f) {
                shadow.SetActive(false);
                shadowLine.gameObject.SetActive(false);
            }
        }

        private void LateUpdate() {
            label.fontSize = camCtrlr.Zoom * 0.22f;
            // Debug.Log($"{camCtrlr.Zoom:F3}");
        }
    }
}
