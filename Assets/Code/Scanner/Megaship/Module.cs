using UnityEngine;

namespace Scanner.Megaship {
    public class Module : MonoBehaviour {
        [field:SerializeField] public string Name { get; private set; }
        internal Ship Ship { get; set; }
    }
}
