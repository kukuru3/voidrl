using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace Scanner.TubeShip.View {

    public struct TubePoint {
        public Vector3 position;
        public Vector3 up;
        public int arcPos;
        public int axisPos;
    }

    internal class TubeshipView : MonoBehaviour {
        // a tubeship is composed of a bunch of parts that are attached to one another.
        // a tube can be unrolled to a rectangle.
        // structurally speaking, extending the spine is like adding more tiles to the rectangle's width.
        
        // tubes can have custom ZED adjacency tunnels.
        // within a tube, adjacency is always rectangular with Y-wraparound

        [field:SerializeField][field:Range(0f, 1f)] public float Unroll { get; set; } = 0f;

        List<Structure> allStructures = new();

        public IReadOnlyList<Structure> Structures => allStructures;

        public void DestroyStructure(Structure structure) {
            foreach (var tile in structure.occupiesTiles) {
                if (tile != null) { tile.occupiedBy = null;}
                allStructures.Remove(structure);
            }
        }

        public void Build(Structure structure, Tile initialTile) {
            var tube = initialTile.tube;

            var a0 = initialTile.arcPos;
            var s0 = initialTile.spinePos;

            structure.initialTile = initialTile;
            var l = new List<Tile>();
            for (var s = 0; s < structure.spineDimension; s++) {
                for (var a = 0; a < structure.arcDimension; a++) {
                    var t = tube.GetTile(a0 + a, s0 + s);
                    if (t == null) throw new System.Exception("Cannot build on a null tile");
                    t.occupiedBy = structure;
                    l.Add(t);
                }
            }
            structure.occupiesTiles = l.ToArray();

            allStructures.Add(structure);
        }

    }

    public class Structure {
        public int arcDimension;
        public int spineDimension;
        public string identity;
        public Tile initialTile;
        public Tile[] occupiesTiles;
    }
    
}


