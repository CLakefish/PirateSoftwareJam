using UnityEngine;

public class RotateToCamera : MonoBehaviour
{
    [SerializeField] private bool perserveXZ = true;
    private Transform pos;

    private void Start()
    {
        pos = PlayerManager.Instance.HomunculusController.Camera.CamComponent.transform;
    }

    private void LateUpdate()
    {
        transform.LookAt(pos);

        if (!perserveXZ) transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
    }
}
