using UnityEngine;

public class TextWiggle : MonoBehaviour
{
    [SerializeField] private TMPro.TMP_Text text;
    [SerializeField] private Vector2 speed, amplitude;

    private void Update()
    {
        TextEffect.UpdateText(text, speed, amplitude);
    }
}
