using System;
using K3.Hex;
using UnityEngine;

namespace Core.H3 {

    [System.Serializable]
    /// <summary>A prismatic hex</summary>
    public struct H3 {
        public readonly Hex hex;
        public readonly int zed;

        public H3(int q, int r, int z) : this(new Hex(q, r), z) { }
        public H3(Hex hex, int z) {
            this.hex = hex; this.zed = z;
        }

        public static H3 operator +(H3 a, H3 b) => new H3(a.hex + b.hex, a.zed + b.zed);
        public static H3 operator -(H3 a, H3 b) => new H3(a.hex - b.hex, a.zed - b.zed);

        public static H3 operator +(H3 a, PrismaticHexDirection b) => a + b.ToHexOffset();

        public static bool operator ==(H3 a, H3 b) => a.hex == b.hex && a.zed == b.zed;
        public static bool operator !=(H3 a, H3 b) => a.hex != b.hex || a.zed != b.zed;

        public override bool Equals(object other) => other is H3 h && hex == h.hex && zed == h.zed;
        public override int GetHashCode() => HashCode.Combine(hex, zed);

        public static implicit operator H3((int q, int r, int z) t) => new H3(new Hex(t.q, t.r), t.z);
    }

    public struct H3Pose {
        public readonly H3 position;
        public readonly int rotation;
        public H3Pose(Hex hex, int z, int hexrot = 0) {
            this.position = new H3(hex, z);
            this.rotation = hexrot;
        }
        public H3Pose(H3 h3, int hexrot = 0) {
            this.position = h3;
            this.rotation = hexrot;
        }

        public H3Pose(Hex hex, int z, HexDir upDirection) : this(new H3(hex, z), upDirection) { }

        public H3Pose(H3 h3, HexDir upDirection) {
            this.position = h3;
            this.rotation = upDirection switch { 
                HexDir.None => 0,
                _ => HexUtils.GetRotationSteps(HexDir.Top, upDirection)
            };
        }
            
        public static H3Pose operator *(H3Pose a, H3Pose b) {
            return new H3Pose(a.position.hex + b.position.hex.RotateAroundZero(a.rotation), a.position.zed + b.position.zed, a.rotation + b.rotation);
        }

        public HexDir RadialUp => HexUtils.FromClockwiseRotationSteps(rotation);


        public Pose CartesianPose() {
            var hexPos = position.hex.HexToPixel(GridTypes.FlatTop, 1f);
            var zedPos = position.zed * HexUtils.CartesianZMultiplier;
            return new Pose(new Vector3(hexPos.x, hexPos.y, zedPos), Quaternion.Euler(0, 0, 60 * rotation));
        }
    }

    public enum HexDir {
        None,
        Top,
        TopRight,
        BottomRight,
        Bottom,
        BottomLeft,
        TopLeft,
    }

    public struct PrismaticHexDirection {
        public readonly HexDir radial;
        public readonly int longitudinal;
        public PrismaticHexDirection(HexDir dir, int zOffset) {
            this.radial = dir;
            this.longitudinal = zOffset;
        }
        public H3 ToHexOffset() {
            return new H3(radial.Offset(), longitudinal);
        }

        public PrismaticHexDirection Inverse() => new PrismaticHexDirection(radial.Inverse(), -longitudinal);

        public PrismaticHexDirection RotatedRadially(int steps) {
            if (radial == HexDir.None) return this;
            return new PrismaticHexDirection(radial.Rotated(steps), longitudinal);
        }

        public static bool operator == (PrismaticHexDirection a, PrismaticHexDirection b) => a.radial == b.radial && a.longitudinal == b.longitudinal;
        public static bool operator != (PrismaticHexDirection a, PrismaticHexDirection b) => a.radial != b.radial || a.longitudinal != b.longitudinal;
    }

    
    public static class HexUtils {
        
        public const float CartesianZMultiplier = 1.73205081f;

        // a and b must be adjacent
        public static HexDir GetDirectionAtoB(Hex a, Hex b) {
            var offset = b - a;
            return GetDirection(offset);
        }

        public static int GetRotationSteps(HexDir from, HexDir to) {
            if (from == HexDir.None) throw new System.InvalidOperationException($"From is None");
            return (ToClockwiseRotationSteps(to) - ToClockwiseRotationSteps(from) + 6) % 6;
        }

        public static int ToDelta(int rotation) {
            rotation %= 6;
            if (rotation <-3) rotation+=6;
            if (rotation > 3) rotation-=6;
            return rotation;
        }

        public static int ToClockwiseRotationSteps(this HexDir dir) {
            if (dir == HexDir.None) throw new System.InvalidOperationException($"dir is None");
            return (int)dir-1;
        }

        public static HexDir FromClockwiseRotationSteps(int rotation) {
            return (HexDir)((rotation+6)%6+1);
        }

        public static HexDir RotateClockwiseOnce(this HexDir source) {
            return source switch { HexDir.Top => HexDir.TopRight, HexDir.TopRight => HexDir.BottomRight, HexDir.BottomRight => HexDir.Bottom, HexDir.Bottom => HexDir.BottomLeft, HexDir.BottomLeft => HexDir.TopLeft, HexDir.TopLeft => HexDir.Top, _ => HexDir.None };
        }

        public static HexDir Rotated(this HexDir source, int rotations) {
            rotations = (rotations + 6) % 6;
            for (var i = 0; i < rotations; i++) source = source.RotateClockwiseOnce();
            return source;
        }

        public static HexDir Inverse(this HexDir source) {
            var invertedOffset = new Hex(0,0) - source.Offset();
            return GetDirection(invertedOffset);
        }

        public static Hex Offset(this HexDir dir) => offsets[(int)dir];

        public static HexDir GetDirection(Hex offset) {
            var idx = Array.IndexOf(offsets, offset);
            if (idx == -1) throw new System.Exception("Invalid direction");
            return (HexDir)idx;
        }

        public static Vector3 CartesianPosition(this H3 h3) {
            var hexPos = h3.hex.HexToPixel(GridTypes.FlatTop, 1f);
            var zedPos = h3.zed * HexUtils.CartesianZMultiplier;
            return new Vector3(hexPos.x, hexPos.y, zedPos);
        }

        // each offset corresponds to direction:
        static Hex[] offsets = new Hex[] {
            new Hex(0,0),
            new Hex(0, 1),
            new Hex(1,0),
            new Hex(1,-1),
            new Hex(0,-1),
            new Hex(-1, 0),
            new Hex(-1, 1)            
        };
    }
}
