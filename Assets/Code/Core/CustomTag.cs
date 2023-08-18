using System.Linq;
using UnityEngine;

namespace Core {

    public enum ObjectTags {
        None,
        ScannerCamera,
    }
    public class CustomTag : MonoBehaviour {
        [field:SerializeField] public ObjectTags Tag { get; private set; }

        static public GameObject Find(ObjectTags tag) {
            var allObjs = GameObject.FindObjectsOfType<CustomTag>(true);
            return allObjs
                .Where(o => o.Tag == tag)
                .FirstOrDefault()?.gameObject;
        }
    }
}