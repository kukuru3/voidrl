using System.Collections.Generic;
using System.Linq;
using K3;
using UnityEngine;

namespace Scanner.ModularShip {
    internal class Shipbuilder : MonoBehaviour, IShipBuilder {
        internal Ship PrimaryShip { get; set; }

        internal TweakMaintainer Tweaker { get; private set; }

        public Tweak ActiveTweak { get; set; }

        void Awake() {
            var shipGO = new GameObject("Ship");
            shipGO.transform.parent = transform;
            shipGO.transform.localPosition = default;
            shipGO.transform.localRotation = default;
            PrimaryShip = shipGO.AddComponent<Ship>();

            Tweaker = new TweakMaintainer(this);
        }

        public void InsertModuleWithoutPlugs(OldModule instance) {
            PrimaryShip.AttachRootModule(instance);
            instance.transform.parent = PrimaryShip.transform;
            instance.transform.localPosition = default;
            instance.transform.localRotation = default;
            Tweaker.Regenerate();
        }

        public void PositionModuleForPlugInterface(IPlug dependentPlug, IPlug shipbornePlug) {
            // both plug transforms need to coincide
            TransformUtility.MoveAndRotateParentSoThatChildCoincidesWithReferenceTransform(
               P: dependentPlug.Module.transform,
               C: dependentPlug.OrientationMatchingTransform,
               R: shipbornePlug.OrientationMatchingTransform
            );

        }

        public void Connect(IPlug a, IPlug b) {        
            if (a.Module.Ship != null && b.Module.Ship != null) {
                throw new System.InvalidOperationException($"Cannot connect rigid module in two places... for now :3 ");
            }

            PrimaryShip.Connect(a, b);
            Tweaker.Regenerate();
        }

        internal List<OldModule> phantoms = new List<OldModule>();

        public void RegisterTemplates(IEnumerable<OldModule> phantomModuleInstances) {
            this.phantoms = new List<OldModule>(phantomModuleInstances);
        }

        void IShipBuilder.ApplyUIMode(IShipbuildingContext.UIStates uistate) {
            foreach (var tweak in Tweaker.currentHandles) {
                tweak.gameObject.SetActive(uistate == IShipbuildingContext.UIStates.Tweaks);
            }
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

        HardcodedShipbuildController shipController;
        private TweakHandle GenerateAndTrackHandle(Tweak tweak) {

            var idx = DecideTweakHandleIndex(tweak);
            if (idx < 0) {
                Debug.LogWarning($"No handle for tweak {tweak}");
                return null;
            }
            shipController ??= GameObject.FindObjectOfType<HardcodedShipbuildController>();
            var prefab = shipController.tweakHandlePrefabs[idx];
            
            var go = GameObject.Instantiate(prefab, builder.transform);
            var handle = go.GetComponent<TweakHandle>();
            handle.Tweak = tweak;
            go.name = $"Tweak handle: {tweak.GetType().Name}";
            currentHandles.Add(handle);

            foreach (var item in go.GetComponentsInChildren<ITweakComponent>(true)) item.Bind(tweak);
            return handle;
        }

        int DecideTweakHandleIndex(Tweak tweak) {
            if (tweak is AttachAndConstructModule) return 1;
            if (tweak is AttachAndConstructButMustChoose) return 1;
            if (tweak is DeconstructModule) return 1;
            return -1;
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
                        attachment = new PotentialAttachment() { 
                            phantom = attachablePhantomPlugs[0].Module,
                            shipPlug = plug,
                            indexOfPlugInPhantomList = attachablePhantomPlugs[0].IndexInParentModule,
                        },
                    };
                } else {
                    yield return new AttachAndConstructButMustChoose {
                        attachments = attachablePhantomPlugs.Select(plg => new PotentialAttachment {
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
        Tweak ActiveTweak { get; set; }

        void ApplyUIMode(IShipbuildingContext.UIStates uistate);
        void Connect(IPlug a, IPlug b);
        void InsertModuleWithoutPlugs(OldModule instance);
        void PositionModuleForPlugInterface(IPlug dependentPlug, IPlug shipbornePlug);
        void RegisterTemplates(IEnumerable<OldModule> phantomModuleInstances);
    }
}
