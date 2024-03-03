using System.Collections.Generic;
using UnityEngine;

namespace Xomenjorld {

    // simple gameplay: 
    // - select
    // - box-select
    // - right click to move.
    // - stances (aggressive, defensive, passive) to replace attack-move



    // design: 
    // - strike craft have less independence
    // - fighter scrambling is a more meaningful action.
    // - we do not subscribe to frigates. Destroyers or bust. Some faction can have light destroyers.

    // - harvesting and salvaging


    // "clusterer" that is aware of unit clusters and HAPPENINGS. You can try to join a battle.


    // mark 

    public class Unit : MonoBehaviour {
        // a single "unit" that can be independently controlled and commanded.

        List<UnitPart> parts = new();

        internal void AddPart(UnitPart part) {

        }

        public bool Destroyed { get; private set; }
    }

    public class StructuralPart : UnitPart {

    }

    public static class UnitBuilder {
        public static void Attach(this Unit unit, UnitPart part) {
            part.AttachTo(unit);
            unit.AddPart(part);
        }
    }

    /// <summary>High level commands. Issued by the player. </summary>
    public enum Missions {
        Stand,
        Move,
        Attack,
        Defend, // requires target
    }

    public abstract class Order {
        
    }

    public interface ITarget {
        Vector3 Target { get; }
        float   Radius { get; }

        bool    Valid { get; }

        bool    Visible { get; }

        Vector3 LastKnownPosition { get; }
    }

    public class UnitTarget : ITarget {
        public Vector3 Target { get; set; }

        public float Radius { get; set; }

        public bool Valid { get; set; }

        public bool Visible { get; set; }

        public Vector3 LastKnownPosition { get; set; }
    }

    public class CoordsTarget : ITarget {
        public CoordsTarget(Vector3 coords) {
            Target = coords;
        }

        public Vector3 Target { get; }

        public float Radius => 0;

        public bool Valid => true;

        public bool Visible => true;

        public Vector3 LastKnownPosition => Target;
    }

    public class SimpleMoveOrder : Order {
        public ITarget target;
    }

    public class AttackOrder : Order {
        public ITarget target;
    }

    public enum Stances {
        Avoid,
        Engage,
        Seek,
    }
    
    public class PilotAI {
        public List<Order> ordersList = new();
        public Stances stance;
    }

    public abstract class UnitPart : MonoBehaviour {
        public Unit Unit { get; set; }

        internal void AttachTo(Unit unit) {
            this.Unit = unit;
        }
    }
}