using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Area : MonoBehaviour
{
    [SerializeField] private Transform spawnPosition;
    [SerializeField] private AreaEnd   endPosition;
    [SerializeField] private List<Transform> rotated;
    [SerializeField] private DialogueScriptableObject dialogue;

    public EnemyController EnemyController { get; private set; }
    public Transform SpawnPosition => spawnPosition;
    public AreaEnd EndPosition => endPosition;

    private bool hasTriggered = false;


    private readonly List<RotationSpace> rotations = new List<RotationSpace>();

    private void Awake()
    {
        endPosition.SetParent(this);

        rotations.AddRange(GetComponentsInChildren<RotationSpace>());

        TurnOff();
    }

    public void Trigger(EnemyController controller)
    {
        foreach (var r in rotated)
        {
            r.gameObject.SetActive(true);
        }

        EnemyController = controller;

        hasTriggered = false;
        DialogueManager.Instance.DisplayDialogue(dialogue);
        PlayerManager.Instance.Transitions.ToPlayer(this);
    }

    public void EndTrigger()
    {
        if (hasTriggered) return;
        hasTriggered = true;
        DialogueManager.Instance.ResetText();
        PlayerManager.Instance.Transitions.ToHomunculus(this);
    }

    public void SetPosition()
    {
        foreach (var r in rotations) r.ClearAll();
        foreach (var t in rotated)   t.localEulerAngles = Vector3.zero;
        PlayerManager.Instance.Transitions.SetPlayer(SpawnPosition);
    }

    public void TurnOff()
    {
        foreach (var r in rotated)
        {
            r.gameObject.SetActive(false);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(spawnPosition.position, 1);

        Gizmos.color = Color.red;
        Gizmos.DrawRay(spawnPosition.position, spawnPosition.forward * 2);
    }
}
