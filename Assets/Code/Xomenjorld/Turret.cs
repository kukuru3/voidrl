using UnityEngine;

namespace Xomenjorld {
    // even fixed mount weapons have at least a few degrees of turretlike motion

    public abstract class Weapon : UnitPart {
        WeaponBehaviour behaviour;
    }
    class Turret : Weapon {

        [SerializeField] float yawMin;
        [SerializeField] float yaWMax;
        [SerializeField] float pitchMin;
        [SerializeField] float pitchMax;

        [SerializeField] Transform trackingTransform;

        private void Update() {
            
        }
    }
}