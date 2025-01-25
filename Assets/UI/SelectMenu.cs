using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectMenu : SubMenu
{
    [SerializeField] private Transform levelPanel;
    [SerializeField] private Transform buttonHolder;
    [SerializeField] private Button buttonPrefab;

    [Header("Levels")]
    [SerializeField] private List<LevelScriptableObject> levels;

    [Header("Interpolation")]
    [SerializeField] private float smoothingTime;
    [SerializeField] private float startSize;
    [SerializeField] private float endSize;

    private readonly List<Button> buttons = new();
    private Coroutine anim;

    private void Awake()
    {
        levelPanel.gameObject.SetActive(false);
        levelPanel.localScale = Vector3.one * endSize;
    }

    public override void OnEnter()
    {
        if (buttons.Count > 0)
        {
            foreach (var button in buttons) Destroy(button.gameObject);
        }

        buttons.Clear();

        for (int i = 0; i < levels.Count; ++i)
        {
            var level = levels[i];

            Button button = Instantiate(buttonPrefab, buttonHolder);
            button.onClick.AddListener(() => { level.Load(); });

            SelectMenuButton s = button.GetComponent<SelectMenuButton>();
            s.DisplayName        = level.DisplayName;
            s.DisplayDescription = level.Description;
        }

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
