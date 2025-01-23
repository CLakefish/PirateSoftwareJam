using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Dialogue")]
public class DialogueScriptableObject : ScriptableObject
{
    [SerializeField] private List<string> dialogues = new();
    [SerializeField] private Color color;

    public List<string> Dialogues => dialogues;
    public Color Color => color;
}
