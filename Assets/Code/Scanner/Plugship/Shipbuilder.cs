using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;

namespace Scanner.Plugship {
    internal class Shipbuilder : MonoBehaviour, IShipBuilder {
        internal Ship PrimaryShip { get; set; }

        internal TweakMaintainer Tweaker { get; private set; }

        void Awake() {
            var shipGO = new GameObject("Ship");
            shipGO.transform.parent = transform;
            shipGO.transform.localPosition = default;
            shipGO.transform.localRotation = default;
            PrimaryShip = shipGO.AddComponent<Ship>();

            Tweaker = new TweakMaintainer(this);
        }

        public void InsertModuleWithoutPlugs(Module instance) {
            PrimaryShip.AttachRootModule(instance);
            instance.transform.parent = PrimaryShip.transform;
            instance.transform.localPosition = default;
            instance.transform.localRotation = default;
            Tweaker.Regenerate();
        }

        public void Connect(IPlug a, IPlug b) {        
            if (a.Module.Ship != null && b.Module.Ship != null) {
                throw new System.InvalidOperationException($"Cannot connect rigid module in two places");
            }

            PrimaryShip.Connect(a, b);
            Tweaker.Regenerate();
        }

        internal List<Module> phantoms = new List<Module>();
        public void RegisterPhantoms(IEnumerable<Module> phantomModuleInstances) {
            this.phantoms = new List<Module>(phantomModuleInstances);
        }
    }

    class TweakMaintainer {
        private readonly Shipbuilder builder;
        internal List<TweakHandle> currentHandles = new();

        public TweakMaintainer(Shipbuilder builder) {
            this.builder = builder;
        }

        public void Regenerate() {
            foreach (var handle in currentHandles) GameObject.Destroy(handle.gameObject);
            currentHandles.Clear();

            var tweaks = EvaluatePossibleTweaks();
            foreach (var tweak in tweaks) {
                GenerateAndTrackHandle(tweak);
            }
            
        }

        private TweakHandle GenerateAndTrackHandle(Tweak tweak) {
            var go = new GameObject($"Tweak handle: {tweak.GetType().Name}");
            var handle = go.AddComponent<TweakHandle>();
            currentHandles.Add(handle);
            return handle;
        }

        IEnumerable<Tweak> EvaluatePossibleTweaks() {
            var unconnectedPlugsOnShip = builder.PrimaryShip.AllAttachedButUnconnectedPlugs()
                .ToArray();
            
            foreach (var plug in unconnectedPlugsOnShip) {

                var attachablePhantomPlugs = new List<IPlug>();

                foreach (var phantom in builder.phantoms) {                 
                    var phantomPlugs = phantom.AllPlugs;
                    foreach (var phantomPlug in phantomPlugs) {
                        if (phantomPlug.IsConnected) continue;
                        if (phantomPlug.EvaluateConditions()) {
                            var pct = PlugsCompatible(plug, phantomPlug);
                            if (pct.compatible) attachablePhantomPlugs.Add(phantomPlug);
                        }
                    }
                }
                
                if (attachablePhantomPlugs.Count == 0) continue;
                else if (attachablePhantomPlugs.Count == 1) {
                    yield return new AttachAndConstructModule() {
                        attachment = new Attachment() { 
                            phantom = attachablePhantomPlugs[0].Module,
                            shipPlug = plug,
                            indexOfPlugInPhantomList = attachablePhantomPlugs[0].IndexInParentModule,
                        },
                    };
                } else {
                    yield return new AttachAndConstructChoice {
                        attachments = attachablePhantomPlugs.Select(plg => new Attachment {
                            phantom = plg.Module,
                            shipPlug = plug,
                            indexOfPlugInPhantomList = plg.IndexInParentModule,
                        }).ToList(),
                    };
                }
                
            }
        }

        private (bool compatible, string reason) PlugsCompatible(IPlug shipsidePlug, IPlug phantomPlug) {
            if (shipsidePlug == null || phantomPlug == null) return (false, "Some plugs were null");
            if (shipsidePlug.IsConnected || phantomPlug.IsConnected) return (false, "some plugs were already connected");
            if (!PolaritiesCompatible(shipsidePlug.Polarity, phantomPlug.Polarity)) return (false, "polarities incompatible");
            if (shipsidePlug.SlotTag != phantomPlug.SlotTag) return (false, "slot tags incompatible");
            return (true, "all good bossmang");
        }

        static bool PolaritiesCompatible(Polarity a, Polarity b) => (a, b) switch {
            (Polarity.In, Polarity.Out) => true,
            (Polarity.Out, Polarity.In) => true,
            (Polarity.Both, Polarity.Both) => true,
            _ => false
        };
    }

    public interface IShipBuilder {
        void Connect(IPlug a, IPlug b);
        void InsertModuleWithoutPlugs(Module instance);
        void RegisterPhantoms(IEnumerable<Module> phantomModuleInstances);
    }
}
