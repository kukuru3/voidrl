
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

    public enum Hex3Dir : byte{
        None,
        Top, RightTop, RightBot, Bottom, LeftBot, LeftTop, 
        Forward, Backward
    }

    public static class Hex3Utils {

         public static Hex3 Rotated(this Hex3 source, int rotation) {
            return new Hex3(Hexes.Rotate(source.hex, rotation), source.zed);
        }

        static Hex3[] directOffsets = new[] {
            new Hex3(Hexes.Neighbours[0], 0),
            new Hex3(Hexes.Neighbours[1], 0),
            new Hex3(Hexes.Neighbours[2], 0),
            new Hex3(Hexes.Neighbours[3], 0),
            new Hex3(Hexes.Neighbours[4], 0),
            new Hex3(Hexes.Neighbours[5], 0),
            new Hex3(new Hex(), 1),
            new Hex3(new Hex(), -1),
        };

        static public IEnumerable<Hex3> DirectNeighbours(Hex3 coords) {
            foreach (var offset in directOffsets) yield return coords + offset;
        }

        public static float RadialDistance = 1f;
        public static float ZedDistance = 1f;
        public static GridTypes gridType = GridTypes.FlatTop;

        public static Vector3 Cartesian(this Hex3 hex) {
            var (x, y) = Hexes.HexToPixel(hex.hex, gridType, RadialDistance);
            return new Vector3(x, y, hex.zed * ZedDistance);
        }

        static (Vector3 away, Vector3 tangent) Vectors(this Hex3Dir dir) {
            return dir switch {
              Hex3Dir.Forward => (Vector3.forward, Vector3.down),
              Hex3Dir.Backward => (Vector3.back, Vector3.up),
              Hex3Dir.Top => (Vector3.up, Vector3.forward),
              Hex3Dir.Bottom => (-Vector3.up, Vector3.forward),

              Hex3Dir.LeftTop    => (Radial(-60), Vector3.forward),
              Hex3Dir.RightTop   => (Radial(60), Vector3.forward),
              Hex3Dir.LeftBot    => (Radial(-120), Vector3.forward),
              Hex3Dir.RightBot   => (Radial(120), Vector3.forward),


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

        public static Quaternion Orientation(this Hex3Dir dir) {        
            if (dir == Hex3Dir.None) throw new Exception("Invalid direction NONE");
            var vecs = Vectors(dir);
            return Quaternion.LookRotation(vecs.away, vecs.tangent);
        }

        static public Hex3Dir ComputeDirectionFromNormal(Vector3 normalFacing) {
            if (normalFacing.z < - 0.5f) return Hex3Dir.Backward;
            if (normalFacing.z >   0.5f) return Hex3Dir.Forward;

            var angle = Mathf.Atan2(normalFacing.y, normalFacing.x) * Mathf.Rad2Deg;
            var index = Mathf.RoundToInt((150 + angle)) / 60 ;

            if (index 
                
                >= 0 && index < _dirlookup.Length) return _dirlookup[index];
            return Hex3Dir.None;
        }

        static public Hex3Dir Inverse(this Hex3Dir dir) => dir switch { 
            Hex3Dir.Forward => Hex3Dir.Backward, Hex3Dir.Backward => Hex3Dir.Forward, 
            Hex3Dir.Top => Hex3Dir.Bottom, Hex3Dir.Bottom => Hex3Dir.Top, 
            Hex3Dir.LeftBot => Hex3Dir.RightTop, Hex3Dir.RightTop => Hex3Dir.LeftBot, 
            Hex3Dir.LeftTop => Hex3Dir.RightBot, Hex3Dir.RightBot => Hex3Dir.LeftTop, 
            _ => Hex3Dir.None
        };

        static public int ToHexRotation(this Hex3Dir dir) => dir switch {            
            Hex3Dir.Top => 0, 
            Hex3Dir.RightTop => 1, 
            Hex3Dir.RightBot => 2, 
            Hex3Dir.Bottom => 3, 
            Hex3Dir.LeftTop => 4,
            Hex3Dir.LeftBot => 5,
            _ => -1
        };

        static public Hex3Dir FromParameters(int hexRot, int zOff ) {
            return (zOff, hexRot) switch {
                (-1, _) => Hex3Dir.Backward,
                ( 1, _) => Hex3Dir.Forward,
                ( 0, _) => hexRot switch {
                    0 => Hex3Dir.Top, 
                    1 => Hex3Dir.RightTop, 
                    2 => Hex3Dir.RightBot, 
                    3 => Hex3Dir.Bottom, 
                    4 => Hex3Dir.LeftTop, 
                    5 => Hex3Dir.LeftBot, 
                    _ => Hex3Dir.None
                },
                _ => Hex3Dir.None
            };
        }

        static Hex3Dir RotatedOnceClockwise(this Hex3Dir dir) {
            return dir switch { 
                Hex3Dir.Top => Hex3Dir.RightTop,
                Hex3Dir.RightTop => Hex3Dir.RightBot,
                Hex3Dir.RightBot => Hex3Dir.Bottom,
                Hex3Dir.Bottom => Hex3Dir.LeftBot,
                Hex3Dir.LeftBot => Hex3Dir.LeftTop,
                Hex3Dir.LeftTop => Hex3Dir.Top,
                _ => dir 
            };
        }

        public static Hex3Dir Rotated(this Hex3Dir dir, int rotation) {
            rotation %= 6; if (rotation < 0) rotation += 6;
            for (int i = 0; i < rotation; i++) dir = dir.RotatedOnceClockwise();
            return dir;
        }

        static public Hex3 Offset(this Hex3Dir dir) => dir switch {
            Hex3Dir.Forward => (0,0, 1),
            Hex3Dir.Backward => (0,0, -1),
            Hex3Dir.Top => (0, 1, 0),
            Hex3Dir.Bottom => (0, -1, 0),
            Hex3Dir.LeftBot => (-1, 0, 0),
            Hex3Dir.RightTop => (1, 0, 0),
            Hex3Dir.LeftTop => (-1, 1, 0),
            Hex3Dir.RightBot => (1, -1, 0),
            _ => default
        };

        static Hex3Dir[] _dirlookup = new[] { Hex3Dir.LeftBot, Hex3Dir.Bottom, Hex3Dir.RightBot, Hex3Dir.RightTop, Hex3Dir.Top, Hex3Dir.LeftTop, Hex3Dir.Forward, Hex3Dir.Backward };

        static public Hex3Dir[] AllDirections => _dirlookup;
    }
}
