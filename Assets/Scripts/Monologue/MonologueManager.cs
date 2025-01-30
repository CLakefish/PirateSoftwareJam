using System.Collections;
using TMPro;
using UnityEngine;

public class MonologueManager : MonoBehaviour
{
    public static MonologueManager Instance { get; private set; }

    [SerializeField] private TMP_Text monologueDisplay;
    [SerializeField] private float monologuePauseTime;
    [SerializeField] private float monologueLetterTime;
    [SerializeField] private float startPause = 0.5f;
    private Coroutine textCoroutine;

    private void Awake()
    {
        if (Instance != null) return;
        Instance = this;
    }

    public void SetText(MonologueScriptableObject text)
    {
        if (text == null) return;

        if (textCoroutine != null) StopCoroutine(textCoroutine);
        textCoroutine = StartCoroutine(TextEffect(text));
    }

    public void ClearText()
    {
        if (textCoroutine != null) StopCoroutine(textCoroutine);
        textCoroutine = null;
        monologueDisplay.text = "";
    }

    private IEnumerator TextEffect(MonologueScriptableObject text)
    {
        string current = "";

        yield return new WaitForSeconds(startPause);

        while (true) {
            string temp = text.GetMonologue();
            while (current == temp)
            {
                temp = text.GetMonologue();
                yield return null;
            }

            current = temp;

            monologueDisplay.text = "";

            for (int i = 0; i < current.Length; ++i)
            {
                monologueDisplay.text += current[i];
                yield return new WaitForSeconds(monologueLetterTime);
            }

            yield return new WaitForSeconds(monologuePauseTime);
        }
    }
}
