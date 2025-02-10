using UnityEngine;

public class MindEnterPortal : MonoBehaviour
{
    [SerializeField] private Camera camToFollow;
    [SerializeField] private Camera targetCamera;
    private MeshRenderer renderer;
    private RenderTexture texture;

    private void OnEnable()
    {
        renderer = GetComponentInChildren<MeshRenderer>();

        if (texture != null) Destroy(texture);

        texture = new(Screen.width, Screen.height, 0);
        texture.depth = 16;
        targetCamera.targetTexture = texture;

        renderer.material.SetTexture("_MainTex", texture);

        targetCamera.depthTextureMode = DepthTextureMode.Depth;

        targetCamera.gameObject.SetActive(false);
    }

    private void LateUpdate()
    {
        renderer.enabled = false;

        targetCamera.fieldOfView = camToFollow.fieldOfView;
        targetCamera.transform.forward = camToFollow.transform.forward;
        targetCamera.transform.position = camToFollow.transform.position;

        targetCamera.Render();
        renderer.enabled = true;
    }
}
