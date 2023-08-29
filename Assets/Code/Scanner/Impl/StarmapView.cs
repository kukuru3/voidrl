using System;
using System.Collections.Generic;
using Scanner.Windows;
using UnityEngine;
using Void;
using Void.Entities;
using Void.Entities.Components;
using Void.Impl;

namespace Scanner.Impl {
    internal class StarmapView : MonoBehaviour {
        [SerializeField] GameObject stellarObjectPrefab;

        //[SerializeField][Range(10, 50)] float maxRange;

        [SerializeField] NumberSlider slider;

        private Gameworld world;

        private void Start() {
            var g = new InitialGenerator();
            this.world = g.GenerateWorld();

            var gg = new StarmapGenerator();
            var starmap = gg.GenerateStarmap(world);

            GenerateViews(starmap);
        }

        List<ScannerViewOfStellarObject> allViews = new();

        float _lastMaxRange = -999;
        private void Update() {
            var maxRange = slider.NumericValue;
            if (!Mathf.Approximately(_lastMaxRange, maxRange)) {
                ApplyRangeFilter(maxRange);
                _lastMaxRange = maxRange;
            }
        }

        private void ApplyRangeFilter(float maxRange) {
            foreach (var view in allViews) {
                var d = view.StellarObject.galacticPosition.magnitude;
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
