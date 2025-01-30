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
    [SerializeField] private RectTransform openPosition;
    [SerializeField] private RectTransform closePosition;
    [SerializeField] private float smoothingTime;

    private readonly List<Button> buttons = new();
    private Coroutine anim;

    private void Awake()
    {
        levelPanel.position = closePosition.position;
        levelPanel.gameObject.SetActive(false);
    }

    public override void OnEnter()
    {
        if (buttons.Count > 0)
        {
            RunAnimation(true);
            return;
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

            buttons.Add(button);
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
        Vector3 posVel     = Vector3.zero;

        if (active) levelPanel.gameObject.SetActive(true);

        while (Vector3.Distance(levelPanel.position, desiredPos) > 0.01f)
        {
            levelPanel.position = Vector3.SmoothDamp(levelPanel.position, desiredPos, ref posVel, smoothingTime, Mathf.Infinity, Time.unscaledDeltaTime);
            yield return null;
        }

        levelPanel.position = desiredPos;

        if (!active) levelPanel.gameObject.SetActive(false);
    }
}
