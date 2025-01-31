using UnityEngine;

public class YRotate : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private float bobStart;
    [SerializeField] private float bobSpeed, bobIntensity;

    private void Update()
    {
        transform.localEulerAngles = new Vector3(bobStart + (Mathf.Sin(Time.unscaledTime * bobSpeed) * bobIntensity), transform.localEulerAngles.y + (speed * Time.unscaledDeltaTime), transform.localEulerAngles.z);
    }
}
