using System.Collections.Generic;
using Scanner.Sweeteners;
using UnityEngine;
using Void;
using Void.Entities;
using Void.Entities.Components;
using Void.Impl;

namespace Scanner.Impl {
    internal class NewStarmapView : ScreenView {
        [SerializeField] GameObject stellarObjectPrefab;
        [SerializeField] __StellarNavCameraOld navCam;
        [SerializeField] __ZoomerEffectOld zoomEffect;

        Gameworld world;
        List<ScannerViewOfStellarObject> allViews = new();

        private void Start() {
            var g = new InitialGenerator();
            this.world = g.GenerateWorld();

            var gg = new StarmapGenerator();
            var starmap = gg.GenerateStarmap(world);

            GenerateViews(starmap);
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
