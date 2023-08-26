using UnityEngine;

public class Rotor : MonoBehaviour
{
    [SerializeField] float rpm;

    private void LateUpdate() {
        var delta = 360 * rpm * Time.deltaTime / 60;
        
        transform.Rotate(new Vector3(0,0,delta));
    }
}
