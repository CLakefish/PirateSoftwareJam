using UnityEngine;

public class RotateToCamera : MonoBehaviour
{
    private void Update()
    {
        transform.LookAt(Camera.main.transform.position);
    }
}
