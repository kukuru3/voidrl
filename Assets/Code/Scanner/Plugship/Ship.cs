using System;
using System.Collections.Generic;
using UnityEngine;

namespace Scanner.Plugship {
    internal class Ship : MonoBehaviour {
        List<Module> graphRoots = new();
        List<Joint> joints = new();
        List<Module> crunchedModuleList = new();

        public void RecalculateCrunchedModuleList() {
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
        }

        public void Connect(PointPlug a, PointPlug b) {
            var joint = new Joint(a, b);
            a.Joint = joint;
            b.Joint = joint;
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

    internal enum Polarity {
        In,
        Out,
        Both,
    }

}
