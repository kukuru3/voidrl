using UnityEngine;

namespace Scanner {
    class UIGroupAnimationHandle : MonoBehaviour {
        public void PlayAnimation(string name) {
            var anim = GetComponent<Animation>();
            if (anim == null) return;
            anim.Play(name);
        }
    }
}