using System;
using System.Collections.Generic;
using UnityEngine;
using Void;
using Void.Entities;
using Void.Entities.Components;
using Void.Impl;

namespace Scanner.Impl {
    internal class NewStarmapView : ScreenView {
        [SerializeField] GameObject stellarObjectPrefab;
        
        [SerializeField] float distanceZoom0;
        [SerializeField] float distanceZoomFull;

        [SerializeField] SteppedPerspectiveCamera targetCamera;
        
        Gameworld world;
        List<ScannerViewOfStellarObject> allViews = new();

        private void Start() {
            var g = new InitialGenerator();
            this.world = g.GenerateWorld();

            var gg = new StarmapGenerator();
            var starmap = gg.GenerateStarmap(world);

            GenerateViews(starmap);
        }

        Vector3 _previousCameraCenter;
        float   _previousOrbitDistance;

        private void Update() {

            var needsUpdate = false;

            var orbitDistance = targetCamera.GetOrbitDistanceNormalized();
            if (!Mathf.Approximately(orbitDistance, _previousOrbitDistance)) {
                _previousOrbitDistance = orbitDistance;
                needsUpdate = true;
            }

            var d = _previousCameraCenter - CameraFocus;
            needsUpdate |= d.sqrMagnitude > 1e-8;
            _previousCameraCenter = CameraFocus; 

            if (needsUpdate) UpdateStarmapParameters();
        }

        private void UpdateStarmapParameters() {

            // if the camera is at max zoom in, display all items that are up to 12 LY with full multipliers.
            // if the camera is at max zoom out, display all items that are up to 20 LY with lean multipliers

            var t = targetCamera.GetOrbitDistanceNormalized();

            var toleratedDistance = Mathf.Lerp(11, 16, t);
            var discOnlyDistance  = Mathf.Lerp(11, 20, t);

            foreach (var view in allViews) {
                var d = Vector3.Distance(CameraFocus, view.StellarObject.galacticPosition);
                var doDisplayLabel = d < toleratedDistance;
                var doDisplayDisc = d < discOnlyDistance;
                view.DiscHandle.Display = doDisplayDisc;
                view.LabelHandle.Display = doDisplayLabel;

                if (t > 0.1f) {
                    view.DiscHandle.ScaleMultiplier = 0.3f;
                    view.LabelHandle.SizeMultiplier = 0.8f;
                } else {
                    view.DiscHandle.ScaleMultiplier = 1f;
                    view.LabelHandle.SizeMultiplier = 1f;
                }
            }
        }

        Vector3 CameraFocus => ((IHasWorldFocus)targetCamera).WorldFocus;

        void UpdateGalacticView() {
            foreach (var view in allViews) {
                UpdateView(view);
            }
        }

        private void UpdateView(ScannerViewOfStellarObject view) {
            
        }

        private void GenerateViews(Starmap starmap) {
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
