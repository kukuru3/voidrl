using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Scanner.Sweeteners { 

    public class Rotator : MonoBehaviour {
        [SerializeField] Vector3 eulerRot;
        private void Update() {
            transform.Rotate( eulerRot * Time.deltaTime, Space.Self );
        }
    }
}