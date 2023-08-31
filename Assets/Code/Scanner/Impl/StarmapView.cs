using System.Collections.Generic;
using UnityEngine;
using Void;
using Void.Entities;
using Void.Entities.Components;
using Void.Impl;

namespace Scanner.Impl {
    [System.Obsolete("Replace with NewStarmapView", false)]
    internal class StarmapView : MonoBehaviour {
        [SerializeField] GameObject stellarObjectPrefab;
        [SerializeField] SteppedPerspectiveCamera targetCamera;
        
        //[SerializeField][Range(10, 50)] float maxRange;

        // [SerializeField] NumberSlider slider;

        [SerializeField] float distanceZoom0;
        [SerializeField] float distanceZoomFull;

        private Gameworld world;

        IHasWorldFocus focusProvider;
        Vector3 _prevCenter;

        // public void SetSliderValue(float val) => slider.SetSliderTFromValue(val);

        private void Start() {
            var g = new InitialGenerator();
            this.world = g.GenerateWorld();

            var gg = new StarmapGenerator();
            var starmap = gg.GenerateStarmap(world);

            GenerateViews(starmap);
            // slider.SetSliderTFromValue(13);

            focusProvider = targetCamera;
        }

        List<ScannerViewOfStellarObject> allViews = new();

        float _lastMaxRange = -999;
        private void Update() {
            var maxRange = Mathf.Lerp(distanceZoom0, distanceZoomFull,targetCamera.GetOrbitDistanceNormalized());

            var center = focusProvider.WorldFocus;
            var centersDifferSignificantly = (_prevCenter - center).sqrMagnitude > 1e-8f;
            _prevCenter = center;

            if (centersDifferSignificantly || !Mathf.Approximately(_lastMaxRange, maxRange)) {
                ApplyRangeFilter(maxRange);
                _lastMaxRange = maxRange;
            }
        }

        private void ApplyRangeFilter(float maxRange) {
            var center = focusProvider.WorldFocus;
            foreach (var view in allViews) {
                var d = Vector3.Distance(center, view.StellarObject.galacticPosition);
                view.gameObject.SetActive(d <= maxRange);
            }
        }

        private void GenerateViews(Void.Entities.Components.Starmap starmap) {
            var contained = starmap.ListContainedEntities();
            foreach (var e in contained) {
                var v = GenerateView(e);
                if (v != null) {
                    allViews.Add(v);
                }
            }
        }

        private ScannerViewOfStellarObject GenerateView(Entity e) {
            var so = e.Get<StellarObject>();
            if (so == null) return null;

            var child = Instantiate(stellarObjectPrefab, transform);
            child.name = $"Stellar View: {so.name}";

            var svoso = child.GetComponent<ScannerViewOfStellarObject>();
            svoso.Initialize(so);          
            return svoso;
        }
    }
}
