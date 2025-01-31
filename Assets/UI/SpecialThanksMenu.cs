using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SpecialThanksMenu : SubMenu
{
    [SerializeField] private Transform settingsPanel;
    [SerializeField] private TMP_Text text;
    [SerializeField] private List<string> names = new();

    [Header("Interpolation")]
    [SerializeField] private RectTransform openPosition;
    [SerializeField] private RectTransform closePosition;
    [SerializeField] private float smoothingTime;

    private Coroutine anim;

    private void Awake()
    {
        settingsPanel.position = closePosition.position;
        settingsPanel.gameObject.SetActive(false);
    }

    public override void OnEnter()
    {
        text.text = "";

        foreach (var name in names)
        {
            text.text += name + "\n";
        }

        RunAnimation(true);
    }


    public override void OnExit()
    {
        RunAnimation(false);
    }

    private void RunAnimation(bool active)
    {
        if (anim != null) StopCoroutine(anim);
        anim = StartCoroutine(OpenAnimation(active));
    }

    private IEnumerator OpenAnimation(bool active)
    {
        Vector3 desiredPos = active ? openPosition.position : closePosition.position;
        Vector3 posVel = Vector3.zero;

        if (active) settingsPanel.gameObject.SetActive(true);

        while (Vector3.Distance(settingsPanel.position, desiredPos) > 0.01f)
        {
            settingsPanel.position = Vector3.SmoothDamp(settingsPanel.position, desiredPos, ref posVel, smoothingTime, Mathf.Infinity, Time.unscaledDeltaTime);
            yield return null;
        }

        settingsPanel.position = desiredPos;

        if (!active) settingsPanel.gameObject.SetActive(false);
    }
}
