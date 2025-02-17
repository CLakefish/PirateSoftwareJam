using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Area : MonoBehaviour
{
    [SerializeField] private Transform spawnPosition;
    [SerializeField] private AreaEnd   endPosition;
    [SerializeField] private List<Transform> rotated;
    [SerializeField] private List<AudioClip> triggerSFX;

    [Header("Displays")]
    [SerializeField] private DialogueScriptableObject dialogue;
    [SerializeField] private MonologueScriptableObject monologue;

    public EnemyController EnemyController { get; private set; }
    public Transform SpawnPosition => spawnPosition;
    public AreaEnd   EndPosition => endPosition;

    private bool hasTriggered = false;
    private PlayerManager playerManager;


    private readonly List<RotationSpace> rotations = new();

    private void Awake()
    {
        endPosition.SetParent(this);

        rotations.AddRange(GetComponentsInChildren<RotationSpace>());

        TurnOff();
    }

    public void Trigger(EnemyController controller)
    {
        playerManager = PlayerManager.Instance;
        gameObject.SetActive(true);

        foreach (var r in rotated)
        {
            r.gameObject.SetActive(true);
        }

        EnemyController = controller;
        EndPosition.FollowPos = playerManager.PlatformerController.Camera.CamComponent.transform;

        var mindPortal = GetComponentInChildren<MindExitPortal>();

        if (mindPortal) {
            mindPortal.IsActive = true;
            mindPortal.Set(playerManager);
            mindPortal.OpenAnim();

            var triggers = mindPortal.GetComponentsInChildren<MindPortalTrigger>();
            foreach (var t in triggers)
            {
                t.OnTrigger += () => { 
                    PlayerManager.Instance.Transitions.ToHomunculus(this); 
                    EnemyController.OnExit(); 
                };
            }
        }

        hasTriggered = false;
        DialogueManager.Instance.DisplayDialogue(dialogue);
        MonologueManager.Instance.SetText(monologue);
        playerManager.Transitions.ToPlayer(this);
    }

    public void SetEnemyRenderer(bool enabled)
    {
        EnemyController.SetRendering(enabled);
    }

    public void BrainTrigger()
    {
        if (hasTriggered) return;
        hasTriggered = true;

        if (EnemyController.HasTriggered) EnemyController.BrainTrigger();

        foreach (var clip in triggerSFX)
        {
            AudioManager.Instance.PlaySFX(clip);
        }

        DialogueManager.Instance.ResetText();
        MonologueManager.Instance.ClearText();
        playerManager.Transitions.Snap();
    }

    public void SetPosition()
    {
        foreach (var r in rotations) r.ClearAll();
        foreach (var t in rotated)   t.localEulerAngles = Vector3.zero;
        playerManager.PlatformerController.Camera.ResetRotation();
        playerManager.PlatformerController.ResetVelocity();
        playerManager.SetPlayerPosition(SpawnPosition);
    }

    public void TurnOff()
    {
        foreach (var r in rotated)
        {
            r.gameObject.SetActive(false);
        }

        var mindPortal = GetComponentInChildren<MindExitPortal>();
        if (mindPortal)
        {
            mindPortal.IsActive = false;
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
