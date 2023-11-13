using System;
using System.Collections.Generic;
using UnityEngine;

namespace Scanner.ModularShip {
    internal class Ship : MonoBehaviour {
        List<Module> graphRoots = new();
        List<Joint> joints = new();
        List<Module> crunchedModuleList = null;

        public Joint TryGetJoint(IPlug a, IPlug b) {
            foreach (var j in joints) {
                if (j.IndexOf(a) > -1 && j.IndexOf(b) > -1) return j;
            }
            return default;
        }

        public IEnumerable<Joint> ListJoints(Module a, Module b) {
            foreach (var j in joints) {
                if (j.A.IsConnected && j.B.IsConnected) {
                    if (j.A.Module == a && j.B.Module == b) yield return j;
                    else if (j.A.Module == b && j.B.Module == a) yield return j;
                }
            }
        }

        public IEnumerable<Module> AllShipModules() {
            if (crunchedModuleList == null) RecalculateCrunchedModuleList();
            return crunchedModuleList;
        }

        public IEnumerable<IPlug> AllAttachedButUnconnectedPlugs() {
            foreach (var module in AllShipModules()) {
                foreach (var plug in module.AllPlugs) {
                    if (plug.IsConnected) continue;
                    if (plug.EvaluateConditions())
                        yield return plug;
                }
            }
        }

        void RecalculateCrunchedModuleList() {
            var closedList = new HashSet<Module>(graphRoots);
            var queue = new Queue<Module>(graphRoots);

            var l = new List<Module>();
            while (queue.Count > 0) {
                var m = queue.Dequeue();
                l.Add(m);
                var others = m.ListDirectlyConnectedModules();
                foreach (var o in others) {
                    if (closedList.Contains(o)) continue;
                    queue.Enqueue(o);
                    closedList.Add(o);
                }
            }
            crunchedModuleList = l;
        }

        public void AttachRootModule(Module m) {
            graphRoots.Add(m);
            InvalidateModuleList();
        }

        public void Connect(IPlug shipside, IPlug newPlug) {
            Debug.Assert(shipside.Module.Ship != null, "shipside module doesn't belong to a ship");
            Debug.Assert(newPlug.Module.Ship == null, "Module already has a ship");

            var joint = new Joint(shipside, newPlug);
            shipside.Joint = joint;
            newPlug.Joint = joint;
            this.joints.Add(joint);
            newPlug.Module.Ship = this;
            InvalidateModuleList();
        }

        public void Break(Joint joint) {
            Debug.Assert(joint.A.Module.Ship == this);
            Debug.Assert(joint.B.Module.Ship == this);
            joints.Remove(joint);
            InvalidateModuleList();
        }

        private void InvalidateModuleList() {
            crunchedModuleList = null;
        }
    }

    public class Joint {
        public readonly IPlug A;
        public readonly IPlug B;

        public Joint(IPlug a, IPlug b) {
            A = a;
            B = b;
        }

        public int IndexOf(IPlug  p) {
            if (A == p) return 0;
            if (B == p) return 1;
            return -1;
        }

        internal IPlug Other(IPlug plug) {
            if (plug == A) return B;
            if (plug == B) return A;
            throw new InvalidOperationException("Other? It's not itself in the joint!");
        }
    }

    public enum Polarity {
        In,
        Out,
        Both,
    }

}
