using System.Collections.Generic;
using System.Linq;
using Scanner.ScannerView;
using Shapes;
using UnityEngine;
using Void.Entities.Components;

namespace Scanner.Impl {
    internal class ScannerViewOfStellarObject : MonoBehaviour {
        internal StellarObject StellarObject { get; private set; }

        [SerializeField] GameObject shadow;
        [SerializeField] GameObject stellarDiscPrefab;
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

            if (primaries.Count >= 1 && firstPrimary.type == StellarSubobjects.MainSequenceStar) {
                typeString = "";
            }

            //if (primaries.Count == 2) typeString = "Binary system";
            //if (primaries.Count == 3) typeString = "Ternary system";
            //if (primaries.Count == 4) typeString = "Quaternary system";
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

            InstantiateStarDiscs(primaries.Count);

            //if (Mathf.Abs(obj.galacticPosition.y)< 0.05f) {
            //    shadow.SetActive(false);
            //    ShadowLine.gameObject.SetActive(false);
            //}
        }

        static Vector3[][] offsets;

        
        
           

        private void InstantiateStarDiscs(int count) {

            if (offsets == null) GenerateOffsets();
            
            // var isPlanet = false;
            if (count == 0) {
                count = 1; // isPlanet = true;
            }

            var list = new List<Shapes.ShapeRenderer>();

            if (count >= offsets.Length) count = offsets.Length - 1;

            for (var ix = 0; ix < count; ix++) {
                var go = Instantiate(stellarDiscPrefab, DiscHandle.transform);
                go.transform.localPosition = offsets[count-1][ix] * 20f;
                go.layer = 5;
                list.Add(go.GetComponent<Shapes.ShapeRenderer>());
            }

            DiscHandle.ReplaceShapes(list);

        }

        private void GenerateOffsets() {
            offsets = new Vector3[4][];
            offsets[0] = new Vector3[] { Vector3.zero };
            offsets[1] = new Vector3[] { -Vector3.right * 0.5f, Vector3.right * 0.5f };
            offsets[2] = new Vector3[] { -Vector3.right * 0.5f, Vector3.right * 0.5f, Vector3.up * 0.71f };
            offsets[3] = new Vector3[] { new Vector2(0.5f, 0), new Vector2(-0.5f, 0), new Vector2(0.5f, 1f), new Vector2(-0.5f, 1f)  };
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
