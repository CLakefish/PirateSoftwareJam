using UnityEngine;

public class MindEnterPortal : MonoBehaviour
{
    [SerializeField] private Camera camToOrient;
    [SerializeField] private Camera camVFX;
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

        targetCamera.fieldOfView = camVFX.fieldOfView;
        targetCamera.transform.rotation = camToOrient.transform.rotation;
        targetCamera.transform.position = camToOrient.transform.position;

        targetCamera.Render();
        renderer.enabled = true;
    }
}
