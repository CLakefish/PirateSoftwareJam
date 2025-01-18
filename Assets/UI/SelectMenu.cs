using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SelectMenu : SubMenu
{
    [SerializeField] private Transform levelPanel;

    [Header("Interpolation")]
    [SerializeField] private float smoothingTime;
    [SerializeField] private float startSize;
    [SerializeField] private float endSize;

    private Coroutine anim;

    private void Awake()
    {
        levelPanel.gameObject.SetActive(false);
        levelPanel.localScale = Vector3.one * endSize;
    }

    public override void OnEnter()
    {
        levelPanel.gameObject.SetActive(true);
        RunAnimation(startSize, true);
    }

    public override void OnExit()
    {
        RunAnimation(endSize, false);
    }

    private void RunAnimation(float size, bool active)
    {
        if (anim != null) StopCoroutine(anim);
        anim = StartCoroutine(OpenAnimation(size, active));
    }

    private IEnumerator OpenAnimation(float size, bool active)
    {
        Vector3 desiredSize = Vector3.one * size;
        Vector3 scaleVel    = Vector3.zero;

        while (Vector3.Distance(levelPanel.localScale, desiredSize) > 0.01f)
        {
            levelPanel.localScale = Vector3.SmoothDamp(levelPanel.localScale, desiredSize, ref scaleVel, smoothingTime);
            yield return null;
        }

        levelPanel.localScale = desiredSize;

        levelPanel.gameObject.SetActive(active);
    }
}
