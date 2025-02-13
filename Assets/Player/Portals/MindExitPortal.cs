using System.Collections.Generic;
using UnityEngine;

public class MindExitPortal : MonoBehaviour
{
    [SerializeField] private Transform homunculus;
    [SerializeField] private Camera targetCamera;
    [SerializeField] private Camera platformerCamera;
    [SerializeField] private float scaling = 0.25f;
    private List<MeshRenderer> renderers = new();
    private RenderTexture texture;

    public Vector3 StartPosition  { get; set; }
    public Vector3 Forward
    {
        get
        {
            return targetCamera.transform.forward;
        }
    }

    private bool isActive = true;
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
        texture.depth = 16;
        targetCamera.targetTexture = texture;

        foreach (var r in renderers)
        {
            r.material.SetTexture("_MainTex", texture);
        }

        targetCamera.depthTextureMode = DepthTextureMode.Depth;

        targetCamera.gameObject.SetActive(false);
    }

    private void Start()
    {
        homunculus = PlayerManager.Instance.HomunculusController.Rigidbody.transform;
        platformerCamera = PlayerManager.Instance.PlatformerController.Camera.CamComponent;
    }

    private void LateUpdate()
    {
        if (!IsActive) return;

        StartPosition = PlayerManager.Instance.HomunculusController.Rigidbody.transform.position;

        bool rendered = false;

        foreach (var rend in renderers)
        {
            if (!Visible(rend, platformerCamera))
            {
                targetCamera.gameObject.SetActive(false);
                continue;
            }

            if (rendered) break;
            rendered = true;

            targetCamera.gameObject.SetActive(true);

            rend.enabled = false;

            targetCamera.projectionMatrix = platformerCamera.projectionMatrix;
            targetCamera.fieldOfView = platformerCamera.fieldOfView;

            Matrix4x4 mat = homunculus.localToWorldMatrix * transform.worldToLocalMatrix * platformerCamera.transform.localToWorldMatrix;

            Vector3 pos = (Vector3)mat.GetColumn(3);
            Vector3 dir = (StartPosition - pos) * scaling;
            Vector3 newPos = StartPosition - new Vector3(0, dir.y, 0);

            targetCamera.transform.position = newPos;
            targetCamera.transform.rotation = Quaternion.Euler(new Vector3(0, 180, 0)) * mat.rotation;

            targetCamera.Render();
            rend.enabled = true;
        }
    }

    public static bool Visible(Renderer renderer, Camera camera)
    {
        Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(camera);
        return GeometryUtility.TestPlanesAABB(frustumPlanes, renderer.bounds);
    }
}
