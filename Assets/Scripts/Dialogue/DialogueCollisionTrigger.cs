using UnityEngine;

public class DialogueCollisionTrigger : DialogueTrigger
{
    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;
        hasTriggered = true;
        TriggerDialogue();
    }

    private void OnTriggerStay(Collider other)
    {
        if (hasTriggered) return;
        hasTriggered = true;
        TriggerDialogue();
    }
}
