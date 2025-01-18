using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonHighlight : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private float selectSize;
    [SerializeField] private float standardSize;
    [SerializeField] private float smoothingTime;

    [Header("Colors")]
    [SerializeField] private Color deselectColorButton;
    [SerializeField] private Color deselectColorText;
    [SerializeField] private Color selectColorButton;
    [SerializeField] private Color selectColorText;
    private Coroutine anim;

    private List<TMP_Text> text = new();
    private Image image;

    private void Awake()
    {
        image = GetComponent<Image>();
        text  = transform.GetComponentsInChildren<TMP_Text>().ToList();

        image.color = deselectColorButton;
        foreach (var t in text) t.color = deselectColorText;
    }

    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        image.color = selectColorButton;
        foreach (var t in text) t.color = selectColorText;

        RunAnimation(selectSize);
    }

    public virtual void OnPointerExit(PointerEventData eventData)
    {
        image.color = deselectColorButton;
        foreach (var t in text) t.color = deselectColorText;

        RunAnimation(standardSize);
    }

    private void RunAnimation(float size)
    {
        if (anim != null) StopCoroutine(anim);
        anim = StartCoroutine(OpenAnimation(size));
    }

    private IEnumerator OpenAnimation(float size)
    {
        Vector3 desiredSize = Vector3.one * size;
        Vector3 scaleVel = Vector3.zero;

        while (Vector3.Distance(transform.localScale, desiredSize) > 0.01f)
        {
            transform.localScale = Vector3.SmoothDamp(transform.localScale, desiredSize, ref scaleVel, smoothingTime);
            yield return null;
        }

        transform.localScale = desiredSize;
    }
}
