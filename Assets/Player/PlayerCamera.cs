using System.Collections;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera cam;
    [SerializeField] private PlayerInputManager input;

    [Header("Field of View")]
    [SerializeField] private float fov;
    [SerializeField] private float pulseStartInterpolate = 0.05f;
    [SerializeField] private float pulseEndInterpolate   = 0.4f;

    [Header("View Tilting")]
    [SerializeField] private float viewTiltAngle;
    [SerializeField] private float viewRotationSmoothing;

    [Header("Recoil")]
    [SerializeField] private float recoilReturnSpeed = 0.2f;

    [Header("Locking")]
    [SerializeField] private bool resetRotation = false;
    [SerializeField] public bool  LockCamera    = false;

    [Header("Settings")]
    [SerializeField] private SliderSettingScriptableObject fovSettings;

    private Coroutine fovPulse;

    private Vector3 recoil;
    private Vector3 recoilVel;

    private Vector2 viewTilt;
    private float cameraVel;
    private float camTilt;
    private bool resettingRotation;

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

    public Camera CamComponent => cam;

    public void Reload()
    {
        if (fovPulse != null) StopCoroutine(fovPulse);
        fov = fovSettings.Load();
        CamComponent.fieldOfView = fov;
    }

    private void OnEnable()
    {
        if (resetRotation) cam.transform.localEulerAngles = Vector3.zero;

        Reload();

        recoilVel = Vector3.zero;
    }

    private void Awake()
    {
        Reload();
        cam.fieldOfView = fov;
        MouseLock = true;
    }

    private void LateUpdate()
    {
        if (resettingRotation)
        {
            resettingRotation = false;
            return;
        }

        float currentZ = Mathf.SmoothDampAngle(cam.transform.localEulerAngles.z, viewTilt.x + camTilt, ref cameraVel, viewRotationSmoothing);
        currentZ += recoil.z;

        recoil   = Vector3.SmoothDamp(recoil, Vector3.zero, ref recoilVel, recoilReturnSpeed);
        viewTilt = Vector2.zero;

        float x = cam.transform.localEulerAngles.x;
        if (x > 180) x -= 360;

        if (LockCamera)
        {
            float lockedYaw = cam.transform.localEulerAngles.y + recoil.y;
            Quaternion lockedRot = Quaternion.Euler(x + recoil.x, lockedYaw, currentZ);
            cam.transform.localRotation = lockedRot;
            return;
        }

        Vector3 dir = new(
            Mathf.Clamp(x - input.AlteredMouseDelta.y + recoil.x, -89.9f, 89.9f),
            cam.transform.localEulerAngles.y + input.AlteredMouseDelta.x + recoil.y,
            currentZ);

        if (!float.IsFinite(dir.x) || !float.IsFinite(dir.y) || !float.IsFinite(dir.z))
        {
            recoil = Vector3.zero;
            return;
        }

        Quaternion moveRot = Quaternion.Euler(dir);

        if (!QuaternionIsValid(moveRot))
        {
            recoil = Vector3.zero;
            return;
        }

        cam.transform.localRotation = moveRot;
    }

    public void ViewTilt(float increased = 1) => viewTilt.x = -input.Input.x * viewTiltAngle * increased;
    public void AddTilt(float value) => camTilt = value;

    private bool QuaternionIsValid(Quaternion q)
    {
        return float.IsFinite(q.x) && float.IsFinite(q.y) && float.IsFinite(q.z) && float.IsFinite(q.w);
    }

    public void Recoil(Vector3 recoilAmount)
    {
        Vector3 rand = Random.insideUnitSphere;
        Vector3 rec  = new(recoilAmount.x, rand.y * recoilAmount.y, recoilAmount.z);
        recoil += rec;
    }

    public void ResetRotation()
    {
        resettingRotation = true;
        cam.transform.localEulerAngles = Vector3.zero;
    }

    public void ResetFOV()
    {
        if (fovPulse != null) StopCoroutine(fovPulse);
        cam.fieldOfView = fov;
    }

    public void SetForward(Vector3 fwd)
    {
        resettingRotation = true;
        cam.transform.forward = fwd;
    }

    public void FOVPulse(float fovAddition)
    {
        if (fovAddition == 0) return;

        if (fovPulse != null) StopCoroutine(fovPulse);
        if (!gameObject.activeSelf) return;

        fovPulse = StartCoroutine(Pulse(fovAddition));
    }

    private IEnumerator Pulse(float addition)
    {
        float endFOV = cam.fieldOfView + addition;
        float vel = 0;

        while (Mathf.Abs(cam.fieldOfView - endFOV) > 1)
        {
            cam.fieldOfView = Mathf.SmoothDamp(Camera.main.fieldOfView, endFOV, ref vel, pulseStartInterpolate, Mathf.Infinity, Time.unscaledDeltaTime);
            yield return null;
        }

        cam.fieldOfView = endFOV;
        vel = 0;

        while (Mathf.Abs(cam.fieldOfView - fov) > 1)
        {
            cam.fieldOfView = Mathf.SmoothDamp(cam.fieldOfView, fov, ref vel, pulseEndInterpolate, Mathf.Infinity, Time.unscaledDeltaTime);
            yield return null;
        }

        cam.fieldOfView = fov;
    }
}
