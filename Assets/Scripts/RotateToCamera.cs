using UnityEngine;

public class RotateToCamera : MonoBehaviour
{
    [SerializeField] private bool perserveXZ = true;

    private void Update()
    {
        transform.LookAt(Camera.main.transform.position);

        if (!perserveXZ) transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
    }
}
