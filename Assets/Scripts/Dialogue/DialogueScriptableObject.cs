using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Dialogue")]
public class DialogueScriptableObject : ScriptableObject
{
    [System.Serializable]
    public class Dialogue {
        public string dialogue;
        public AudioClip audio;
        public Color color;
        public bool skippable = true;
    }

    [SerializeField] private List<Dialogue> dialogues = new();
    [SerializeField] private bool forceInput;
    [SerializeField] private bool hold;

    public List<Dialogue> Dialogues  => dialogues;
    public bool           ForceInput => forceInput;
    public bool           Hold       => hold;
}
