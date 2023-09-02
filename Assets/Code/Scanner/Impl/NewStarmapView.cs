using System;
using System.Collections.Generic;
using K3;
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
        [SerializeField] float verticalDistanceModifier;

        [SerializeField] SteppedPerspectiveCamera targetCamera;
        [SerializeField] Transform grid;
        
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

            // var gridY = ((IOrbitCamera)targetCamera).WorldFocus.y;
            var gridY = 0;
            grid.position = new Vector3(0, gridY, 0);

            // if the camera is at max zoom in, display all items that are up to 12 LY with full multipliers.
            // if the camera is at max zoom out, display all items that are up to 20 LY with lean multipliers

            var t = targetCamera.GetOrbitDistanceNormalized();

            var discAndLabelDistance = Mathf.Lerp(12, 12, t);
            var discOnlyDistance  = Mathf.Lerp(12, 35, t);
            var smolDiscDistance = Mathf.Lerp(12, 20, t);

            var shadowLineDistance = Mathf.Lerp(13, 15, t);

            foreach (var view in allViews) {
                var delta = CameraFocus - view.StellarObject.galacticPosition;
                delta.y *= verticalDistanceModifier;

                var d = delta.magnitude;

                delta.y = 0;
                var dFlat = delta.magnitude;
                
                var doDisplayLabel = d < discAndLabelDistance;
                var doDisplayDisc = d < discOnlyDistance;
                view.DiscHandle.Display = doDisplayDisc;
                view.LabelHandle.Display = doDisplayLabel;
                view.ShadowLine.enabled = t < 0.1f && doDisplayDisc; // && dFlat < 10f;
                var sy = gridY - view.StellarObject.galacticPosition.y;
                view.ShadowLine.End = new Vector3(0, sy, 0);
                var color = sy > 0 ? Color.red : Color.green;
                color.a = 0.25f;
                view.ShadowLine.Color = color;

                if (t > 0.1f) {
                    view.DiscHandle.ScaleMultiplier = 0.3f;
                    view.LabelHandle.SizeMultiplier = 0.8f;                    
                } else {
                    view.DiscHandle.ScaleMultiplier = 1f;
                    view.LabelHandle.SizeMultiplier = 1f;
                }

                if (d > smolDiscDistance) view.DiscHandle.ScaleMultiplier = 0.1f;
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
