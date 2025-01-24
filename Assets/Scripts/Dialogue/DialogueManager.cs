using System.Collections;
using TMPro;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [SerializeField] private Canvas canvas;
    [SerializeField] private TMP_Text dialogue;
    [SerializeField] private float dialogueCharacterTime;
    [SerializeField] private float dialogueEndingPause;
    private Coroutine dialogueCoroutine;
    private PlayerManager player;
    private TimeManager timeManager;
    private bool cutscenePlaying;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        player      = PlayerManager.Instance;
        timeManager = TimeManager.Instance;
    }

    private void Update()
    {
        if (cutscenePlaying && player.PlayerInputs.Jump)
        {
            if (dialogueCoroutine != null) StopCoroutine(dialogueCoroutine);
            timeManager.SetScale(1);
            timeManager.Interpolate = true;
            ResetText();
        }
    }

    public void DisplayDialogue(DialogueScriptableObject text)
    {
        if (text == null) return;

        cutscenePlaying = true;

        if (dialogueCoroutine != null) StopCoroutine(dialogueCoroutine);
        dialogueCoroutine = StartCoroutine(RunDialogue(text));
    }

    public void ResetText()
    {
        if (dialogueCoroutine != null) StopCoroutine(dialogueCoroutine);
        dialogue.text = "";

        cutscenePlaying = false;
    }

    private IEnumerator RunDialogue(DialogueScriptableObject text)
    {
        if (text.ForceInput)
        {
            timeManager.Interpolate = false;
            timeManager.SetScale(0.01f);
        }

        yield return new WaitForSecondsRealtime(dialogueEndingPause);

        dialogue.color = text.Color;
        dialogue.text = "";

        for (int i = 0; i < text.Dialogues.Count; i++)
        {
            var d = text.Dialogues[i];
            string c = "";

            for (int j = 0; j < d.Length; j++)
            {
                c += d[j];
                dialogue.text = c;
                yield return new WaitForSecondsRealtime(dialogueCharacterTime);
            }

            yield return new WaitForSecondsRealtime(dialogueEndingPause);
        }

        if (text.ForceInput)
        {
            while (!player.PlayerInputs.Jump)
            {
                player.HomunculusController.Rigidbody.linearVelocity = Vector3.zero;

                yield return null;
            }

            timeManager.Interpolate = true;
        }

        ResetText();
    }
}
