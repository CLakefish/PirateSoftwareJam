using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MindExitPortal : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    [SerializeField] private float  portalViewScaling = 0.25f;
/*    [SerializeField] private float  portalOpenPause = 0.75f;*/
    [SerializeField] private float  portalOpenInterpolation = 1;

    private readonly List<MeshRenderer> renderers = new();

    private RenderTexture texture;
    private PlayerManager playerManager;
    private Transform     homunculus;
    private Camera        platformerCamera;

    public Vector3 StartPosition  { get; set; }
    public Vector3 Forward
    {
        get
        {
            return targetCamera.transform.forward;
        }
    }

    private bool isActive = false;
    public bool IsActive
    {
        get
        {
            return isActive;
        }
        set
        {
            isActive = value;
            targetCamera.gameObject.SetActive(isActive);
        }
    }

    private void OnEnable()
    {
        renderers.AddRange(GetComponentsInChildren<MeshRenderer>());

        if (texture != null) Destroy(texture);

        texture = new(Screen.width, Screen.height, 0);
        texture.depth = 24;
        targetCamera.targetTexture = texture;

        foreach (var r in renderers)
        {
            r.material.SetTexture("_MainTex", texture);
        }

        targetCamera.depthTextureMode = DepthTextureMode.Depth;

        ClearTrigger();
    }

    private void LateUpdate()
    {
        if (!IsActive) return;

        StartPosition = playerManager.HomunculusController.Rigidbody.transform.position;

        foreach (var rend in renderers)
        {
            RenderCam(rend);
            break;
        }
    }

    private void RenderCam(Renderer rend)
    {
        rend.enabled = false;

        targetCamera.projectionMatrix = platformerCamera.projectionMatrix;
        targetCamera.fieldOfView      = platformerCamera.fieldOfView;

        Matrix4x4 mat = homunculus.localToWorldMatrix * transform.worldToLocalMatrix * platformerCamera.transform.localToWorldMatrix;

        Vector3 pos = (Vector3)mat.GetColumn(3);
        Vector3 dir = (StartPosition - pos) * portalViewScaling;
        Vector3 newPos = StartPosition - new Vector3(0, dir.y, 0);

        targetCamera.transform.position = newPos;
        targetCamera.transform.rotation = Quaternion.Euler(new Vector3(0, 180, 0)) * mat.rotation;

        targetCamera.Render();
        rend.enabled = true;
    }

    private bool Visible(Renderer renderer, Camera camera)
    {
        Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(camera);
        return GeometryUtility.TestPlanesAABB(frustumPlanes, renderer.bounds);
    }

    public void Set(PlayerManager manager)
    {
        playerManager = manager;
        homunculus       = playerManager.HomunculusController.Rigidbody.transform;
        platformerCamera = playerManager.PlatformerController.Camera.CamComponent;
    }

    public void OpenAnim()
    {
        StartCoroutine(OpenAnimCoroutine());
    }

    public void ClearTrigger()
    {
        targetCamera.gameObject.SetActive(false);
        isActive = false;

        foreach (var renderer in renderers)
        {
            renderer.transform.localScale = Vector3.zero;
        }
    }
    
    private IEnumerator OpenAnimCoroutine()
    {
        Vector3 scale = Vector3.zero;
        Vector3 vel   = Vector3.zero;

        isActive = true;

        while (scale != Vector3.one)
        {
            scale = Vector3.SmoothDamp(scale, Vector3.one, ref vel, portalOpenInterpolation, Mathf.Infinity, Time.unscaledDeltaTime);

            foreach (var renderer in renderers)
            {
                renderer.transform.localScale = scale;
            }

            yield return null;
        }
    }
}
