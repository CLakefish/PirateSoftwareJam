using System.Collections;
using TMPro;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [SerializeField] private TMP_Text dialogue;
    [SerializeField] private float dialogueCharacterTime;
    [SerializeField] private float dialogueEndingPause;
    private Coroutine dialogueCoroutine;

    private void Awake()
    {
        Instance = this;
    }

    public void DisplayDialogue(DialogueScriptableObject text)
    {
        if (text == null) return;

        if (dialogueCoroutine != null) StopCoroutine(dialogueCoroutine);
        dialogueCoroutine = StartCoroutine(RunDialogue(text));
    }

    public void ResetText()
    {
        if (dialogueCoroutine != null) StopCoroutine(dialogueCoroutine);
        dialogue.text = "";
    }

    private IEnumerator RunDialogue(DialogueScriptableObject text)
    {
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

        ResetText();
    }
}
