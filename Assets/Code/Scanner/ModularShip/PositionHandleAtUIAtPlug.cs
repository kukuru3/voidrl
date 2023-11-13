using Scanner.ScannerView;
using UnityEngine;

namespace Scanner.ModularShip {

    public interface ITweakComponent {
        void Bind(Tweak tweak);
    }
    public class PositionHandleAtUIAtPlug : MonoBehaviour, ITweakComponent {
        private IPlug plug;
        private Camera uiCamera;
        private Camera enviroCam;

        public void Bind(Tweak tweak) {
            if (tweak is AttachAndConstructModule m) {
                plug = m.attachment.shipPlug;
            } else if (tweak is AttachAndConstructButMustChoose mm) {
                plug = mm.attachments[0].shipPlug;
            }
            uiCamera = SceneUtil.UICamera;            
            enviroCam = GameObject.Find("EnviroCam").GetComponent<Camera>();
        }

        private void LateUpdate() {
            if (plug == null) return;
            var worldPos = (plug as Component).transform.position;
            var screenPos = enviroCam.WorldToScreenPoint(worldPos);
            screenPos.z = 400;
            worldPos = uiCamera.ScreenToWorldPoint(screenPos);
            transform.SetPositionAndRotation(worldPos, uiCamera.transform.rotation);
        }
    }
}
