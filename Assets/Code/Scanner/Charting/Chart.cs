using System.Collections.Generic;
using Shapes;
using UnityEngine;

namespace Scanner.Charting {
    internal abstract class Chart : ImmediateModeShapeDrawer {
        
        [System.Serializable]
        public struct Entry {
            public float amount;
            public Color color;
            public string name;
        }

        [SerializeField] protected List<Entry> entries;

        public void ClearEntries() {
            entries.Clear();
        }
        public void AddEntry(string name, float amount, Color color) {
            var entry = new Entry { amount = amount, name = name, color = color };
            entries.Add(entry);
        }

    }
}
