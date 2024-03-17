using System.Collections.Generic;
using K3.Hex;
using UnityEngine;

namespace Core.H3 {

    public interface IHasH3Coords {
        H3 Coords { get; }
    }
    public class H3SparseGrid<T> where T : class, IHasH3Coords  {
        Dictionary<H3, T> _grid = new Dictionary<H3, T>();

        public T At(H3 item) {
            _grid.TryGetValue(item, out var result);
            return result;
        }

        public T this[H3 item] => At(item);
        public T this[int q, int r, int z] => At(new H3(new Hex(q, r), z));
        public T this[Vector3Int qrz] => At(new H3(new Hex(qrz.x, qrz.y), qrz.z));

        public bool HasValueAt(H3 item) => _grid.ContainsKey(item);

        public bool TryInsert(T item) {
            if (HasValueAt(item.Coords)) return false;
            _grid[item.Coords] = item;
            return true;
        }

        public void InsertRange(IEnumerable<T> items) {
            foreach (var item in items) TryInsert(item);
        }

        public bool TryRemove(T item) {
            var c = item.Coords;
            if (_grid.TryGetValue(item.Coords, out var r) && r == item) return _grid.Remove(c);
            return false;
        }

        public void Clear() => _grid.Clear();

        public IEnumerable<H3> OccupiedHexes => _grid.Keys;
    }
}
