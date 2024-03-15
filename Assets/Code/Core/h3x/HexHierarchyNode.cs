
using System;
using System.Collections.Generic;
using Core.h3x;
using K3.Hex;
using UnityEngine;

namespace Core {
    public class HexHierarchyNode {

        public Hex3 localPosition;
        public int  localRotation;

        private HexHierarchyNode parent = null;
        private List<HexHierarchyNode> children = new();

        public HexHierarchyNode Parent => parent;
        public IReadOnlyList<HexHierarchyNode> Children => children;

        static public HexHierarchyNode SetParent(HexHierarchyNode parent, HexHierarchyNode child) {
            if (child.parent != null) child.parent.children.Remove(child);
            child.parent = parent;
            parent.children.Add(child);
            return child;
        }

        public void ClearChildren() {
            foreach (var child in children) child.parent = null;
            children.Clear();
        }

        public (Hex3 worldPosition, int worldRotation) WorldTransform() {
            var pos = localPosition;
            var rot = localRotation;
            if (parent != null) {
                var pw = parent.WorldTransform();
                pos = pw.worldPosition + localPosition.Rotated(pw.worldRotation);
                rot = localRotation + pw.worldRotation;
            }
            return (pos, rot);
        }
    }

    public struct HexPose {
        public readonly Hex3 position;
        public readonly int  rotation;
        public HexPose(Hex3 position, int rotation = 0) {
            this.position = position;
            this.rotation = rotation;
        }

        /// <summary>a and b ordering matters. A is the "parent".</summary>
        public static HexPose operator *(HexPose a, HexPose b) {
            return new HexPose(a.position + b.position.Rotated(a.rotation), a.rotation + b.rotation);
        }
        
        public Pose Cartesian() => new Pose(position.Cartesian(), Quaternion.Euler(0,0,60 * rotation));
    }

    public enum QRZDir : byte{
        None,
        Top, RightTop, RightBot, Bottom, LeftBot, LeftTop, 
        Forward, Backward
    }

    public static class HexExpansions {

        public static float RadialDistance = 1f;
        public static float ZedDistance = 1f;
        public static GridTypes gridType = GridTypes.FlatTop;

        public static Vector3 Cartesian(this Hex3 hex) {
            var (x, y) = Hexes.HexToPixel(hex.hex, gridType, RadialDistance);
            return new Vector3(x, y, hex.zed * ZedDistance);
        }

        static (Vector3 away, Vector3 tangent) Vectors(this QRZDir dir) {
            return dir switch {
              QRZDir.Forward => (Vector3.forward, Vector3.down),
              QRZDir.Backward => (Vector3.back, Vector3.up),
              QRZDir.Top => (Vector3.up, Vector3.forward),
              QRZDir.Bottom => (-Vector3.up, Vector3.forward),

              QRZDir.LeftTop    => (Radial(-60), Vector3.forward),
              QRZDir.RightTop   => (Radial(60), Vector3.forward),
              QRZDir.LeftBot    => (Radial(-120), Vector3.forward),
              QRZDir.RightBot   => (Radial(120), Vector3.forward),


              _ => (Vector3.zero, Vector3.zero),
            };
            
            Vector3 Radial(int degrees) {
                var rads = degrees * Mathf.Deg2Rad;
                return new Vector3(Mathf.Cos(rads), Mathf.Sin(rads), 0);
            }

            //var px = Hexes.PixelToHex(off.x, off.y, gridType, 1f);
            //var up = Hexes.PixelToHex(0, 0, gridType, 1f);
            //var forward = Hexes.PixelToHex(0, 0, gridType, 1f);
            //return (up, forward);
        }

        public static Quaternion Orientation(this QRZDir dir) {        
            if (dir == QRZDir.None) throw new Exception("Invalid direction NONE");
            var vecs = Vectors(dir);
            return Quaternion.LookRotation(vecs.away, vecs.tangent);
        }

        static public QRZDir ComputeDirectionFromNormal(Vector3 normalFacing) {
            if (normalFacing.z < - 0.5f) return QRZDir.Backward;
            if (normalFacing.z >   0.5f) return QRZDir.Forward;

            var angle = Mathf.Atan2(normalFacing.y, normalFacing.x) * Mathf.Rad2Deg;
            var index = Mathf.RoundToInt((150 + angle)) / 60 ;

            if (index >= 0 && index < _dirlookup.Length) return _dirlookup[index];
            return QRZDir.None;
        }

        static public QRZDir Inverse(this QRZDir dir) => dir switch { 
            QRZDir.Forward => QRZDir.Backward, QRZDir.Backward => QRZDir.Forward, 
            QRZDir.Top => QRZDir.Bottom, QRZDir.Bottom => QRZDir.Top, 
            QRZDir.LeftBot => QRZDir.RightTop, QRZDir.RightTop => QRZDir.LeftBot, 
            QRZDir.LeftTop => QRZDir.RightBot, QRZDir.RightBot => QRZDir.LeftTop, 
            _ => QRZDir.None
        };

        static  public Vector3Int QRZOffset(this QRZDir dir) => dir switch {
            QRZDir.Forward => new Vector3Int(0, 0, 1),
            QRZDir.Backward => new Vector3Int(0, 0, -1),
            QRZDir.Top => new Vector3Int(0, 1, 0),
            QRZDir.Bottom => new Vector3Int(0, -1, 0),
            QRZDir.LeftBot => new Vector3Int(-1, 0, 0),
            QRZDir.RightTop => new Vector3Int(1, 0, 0),

            QRZDir.LeftTop => new Vector3Int(-1, 1, 0),
            QRZDir.RightBot => new Vector3Int(1, -1, 0),
            _ => default
        };

        static QRZDir[] _dirlookup = new[] { QRZDir.LeftBot, QRZDir.Bottom, QRZDir.RightBot, QRZDir.RightTop, QRZDir.Top, QRZDir.LeftTop, QRZDir.Forward, QRZDir.Backward };

        static public QRZDir[] AllDirections => _dirlookup;
    }
}
