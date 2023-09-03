using System.Linq;
using Core;
using Scanner.ScannerView;
using Shapes;
using UnityEngine;
using Void.AppContext;
using Void.Entities.Components;

namespace Scanner.Impl {
    internal class ScannerViewOfStellarObject : MonoBehaviour {
        internal StellarObject StellarObject { get; private set; }

        [SerializeField] GameObject shadow;
        [field:SerializeField] internal Line ShadowLine { get; private set; }
        [SerializeField] TMPro.TMP_Text label;
        [field:SerializeField] internal Sweeteners.SurrogateObject DiscHandle { get; private set; }
        [field:SerializeField] internal Sweeteners.SurrogateText   LabelHandle { get; private set;}

        Element myElement;

        const float SCALE = 1.0f;

        void Start() {
            myElement = GetComponent<Element>();
        }

        public void Initialize(StellarObject obj) {
            this.StellarObject = obj;
            transform.position = obj.galacticPosition * SCALE;
            
            var zeroPos = obj.galacticPosition;
            zeroPos.y = 0;
            shadow.transform.position = zeroPos * SCALE;

            var h = obj.galacticPosition.y;
            // ShadowLine.Start = new Vector3(0,0,0);
            // ShadowLine.End = new Vector3(0, 0, h * SCALE);

            // camCtrlr = CustomTag.Find(ObjectTags.ScannerCamera).GetComponent<CameraController3D>();

            SubstellarObjectDeclaration firstPrimary;
            string typeString;

            var primaries = obj.Primaries;
            if (primaries.Count == 0) {                
                if (obj.ContainedSubstellars.Count() == 1) {
                    firstPrimary = obj.ContainedSubstellars.First();
                } else {
                    Debug.LogWarning($"{obj.name} contains no substellars");
                    return;
                }
            } else {
                firstPrimary = obj.Primaries.First();
            }

            typeString = firstPrimary.type.ToString();

            if (primaries.Count == 1 && firstPrimary.type == StellarSubobjects.MainSequenceStar) {
                typeString = "";
            }

            if (primaries.Count == 2) typeString = "Binary system";
            if (primaries.Count == 3) typeString = "Ternary system";
            if (primaries.Count == 4) typeString = "Quaternary system";
            if (primaries.Count >= 5) typeString = $"{primaries.Count}-star system";

            string color = "fff";

            if (primaries.Count == 1 && firstPrimary.type == StellarSubobjects.BrownDwarf) {
                color = "610";
            } else if (primaries.Count == 1 && firstPrimary.type == StellarSubobjects.WhiteDwarf) {
                color = "48c";
            } else if (primaries.Count == 0) {
                color = "522";
            }

            label.text = $"<color=#{color}>{obj.name}</color>"; 

            if (!string.IsNullOrWhiteSpace(typeString)) label.text += $"\r\n<size=80%>{typeString}</size>";
            if (primaries.Count == 1) {
                if (firstPrimary.starSequence != StarTypes.NotAStar && firstPrimary.starSequence != StarTypes.Unknown) {
                    // label.text += $"\r\n<size=80%>{firstPrimary.starSequence}</size>";
                }
            }

            var neps = obj.ContainedSubstellars.Where(ss => ss.type == StellarSubobjects.NeptunianPlanet).Count();
            var jups = obj.ContainedSubstellars.Where(ss => ss.type == StellarSubobjects.JovianPlanet).Count();
            var ter  = obj.ContainedSubstellars.Where(ss => ss.type == StellarSubobjects.TerrestrialPlanet).Count();

            var numPlanets = neps + jups + ter;

            //if (Mathf.Abs(obj.galacticPosition.y)< 0.05f) {
            //    shadow.SetActive(false);
            //    ShadowLine.gameObject.SetActive(false);
            //}
        }

        float hiliteAlpha;

        float upTime = 0f;

        private void OnEnable() {
            upTime = 0f;
        }

        private void OnDisable() {
            upTime = 0f;
        }

        bool prevDisplay;
        private void LateUpdate() {
            if (prevDisplay != LabelHandle.Display) upTime = 0;            
            prevDisplay = LabelHandle.Display;
            
            upTime += Time.deltaTime;
            if (upTime < 0.23f) { 
                var isOn = (upTime % 0.09f < 0.045f);
                LabelHandle.ColorMultiplier = isOn ? Color.white : Color.clear;
            } else {
                LabelHandle.ColorMultiplier = Color.white;
            }
            
            if (myElement.IsHighlighted) hiliteAlpha += Time.deltaTime;
            else hiliteAlpha -= Time.deltaTime;
            hiliteAlpha = Mathf.Clamp01(hiliteAlpha);


            if (myElement.IsHighlighted && Input.GetMouseButtonDown(1)) {
                SceneUtil.GetScannerCamera.GetComponent<IHasWorldFocus>().Focus(StellarObject.galacticPosition, false);
            }
        }
    }
}
