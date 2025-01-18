using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera cam;
    [SerializeField] private PlayerInputManager playerInputManager;

    [Header("Field of View")]
    [SerializeField] private float fov;

    public bool LockCamera = false;

    public bool MouseLock {
        set {
            Cursor.lockState = value ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible   = !value;
        }
    }

    public Vector3 ViewPosition {
        get {
            return cam.transform.forward;
        }
    }

    public Vector3 ForwardNoY {
        get {
            return new Vector3(cam.transform.forward.x, 0, cam.transform.forward.z).normalized;
        }
    }

    public Vector3 RightNoY {
        get {
            return new Vector3(cam.transform.right.x, 0, cam.transform.right.z).normalized;
        }
    }

    public Camera Cam => cam;

    private void Awake() => MouseLock = true;

    private void LateUpdate()
    {
        cam.fieldOfView = fov;

        if (LockCamera) return;

        float x = cam.transform.eulerAngles.x;
        if (x > 180) x -= 360;

        cam.transform.rotation = Quaternion.Euler(
            new Vector3(
            Mathf.Clamp(x - playerInputManager.AlteredMouseDelta.y, -89.9f, 89.9f),
            cam.transform.eulerAngles.y + playerInputManager.AlteredMouseDelta.x, 
            0));
    }
}
