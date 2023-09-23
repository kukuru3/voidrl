using UnityEngine;

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
    }
    
}


