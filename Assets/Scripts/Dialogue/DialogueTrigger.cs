using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [SerializeField] protected DialogueScriptableObject dialogue;

    public virtual void TriggerDialogue()
    {
        DialogueManager.Instance.DisplayDialogue(dialogue);
    }
}
