using System;
using System.Collections.Generic;
using K3.Hex;
using UnityEngine;

namespace Core.h3x {
    public struct Hex3 {
        public readonly Hex hex;
        public readonly int zed;

        public Hex3(int q, int r, int z) {
            hex = new Hex(q,r); zed = z;
        }

        public Hex3(Hex c, int z) {
            hex = c; zed = z;
        }

        public Vector3Int QRZ => new Vector3Int(hex.q, hex.r, zed);

        public static implicit operator Hex3((Hex c , int z) t) => new Hex3(t.c, t.z);

        public static implicit operator Hex3((int q, int r, int z) t) => new Hex3(t.q, t.r, t.z);

        public static Hex3 operator +(Hex3 a, Hex3 b) => new Hex3(a.hex + b.hex, a.zed + b.zed);

        public static Hex3 operator +(Hex3 a, Hex3Dir offset) => a + offset.Offset();

        public static bool operator ==(Hex3 a, Hex3 b) => a.QRZ == b.QRZ;
        public static bool operator !=(Hex3 a, Hex3 b) => a.QRZ != b.QRZ;
        public override bool Equals(object other) => other is Hex3 hex && this.hex.Equals(hex.hex) && zed == hex.zed;
        public override int GetHashCode() => HashCode.Combine(hex.q, hex.r, zed);

        public override string ToString() => $"{hex.q},{hex.r}, z:{zed}";
    }

    public interface IHasHex3Coords {
        Hex3 Coords { get; }
    }

    public class Hex3SparseGrid<T> where T : class, IHasHex3Coords  {
        Dictionary<Hex3, T> _grid = new Dictionary<Hex3, T>();

        public T At(Hex3 item) {
            _grid.TryGetValue(item, out var result);
            return result;
        }

        public T this[Hex3 item] => At(item);
        public T this[int q, int r, int z] => At(new Hex3(q, r, z));
        public T this[Vector3Int qrz] => At(new Hex3(qrz.x, qrz.y, qrz.z));

        public bool HasValueAt(Hex3 item) => _grid.ContainsKey(item);

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

        public IEnumerable<Hex3> OccupiedHexes => _grid.Keys;

        public IEnumerable<T> SolidNeighbours(Hex3 coords) {
            var nn = Hex3Utils.DirectNeighbours(coords);
            foreach (var coord in nn) {
                var item = At(coord);
                if (item != null) yield return item;
            }
        }

        public IEnumerable<Hex3> EmptyNeighbours(Hex3 coords) {
            var nn = Hex3Utils.DirectNeighbours(coords);
            foreach (var coord in nn) {
                var item = At(coord);
                if (item == null) yield return coord;
            }
        }

    }
}
