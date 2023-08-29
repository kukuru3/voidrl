using System;
using UnityEngine;
using Void;
using Void.Entities;
using Void.Entities.Components;
using Void.Impl;

namespace Scanner.Impl {
    internal class StarmapView : MonoBehaviour {
        [SerializeField] GameObject stellarObjectPrefab;
        private Gameworld world;

        private void Start() {
            var g = new InitialGenerator();
            this.world = g.GenerateWorld();

            var gg = new StarmapGenerator();
            var starmap = gg.GenerateStarmap(world);

            GenerateViews(starmap);
        }

        private void GenerateViews(Void.Entities.Components.Starmap starmap) {
            var contained = starmap.ListContainedEntities();
            foreach (var e in contained) {
                GenerateView(e);
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
