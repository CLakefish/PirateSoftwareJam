using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Area : MonoBehaviour
{
    [SerializeField] private Transform spawnPosition;
    [SerializeField] private AreaEnd   endPosition;
    [SerializeField] private MindExitPortal exit;
    [SerializeField] private List<Transform> rotated;
    [SerializeField] private List<AudioClip> triggerSFX;

    [Header("Displays")]
    [SerializeField] private DialogueScriptableObject dialogue;
    [SerializeField] private MonologueScriptableObject monologue;
    [SerializeField] private DialogueScriptableObject onComplete;

    public EnemyController EnemyController { get; private set; }
    public Transform SpawnPosition => spawnPosition;
    public AreaEnd   EndPosition => endPosition;
    public MindExitPortal Exit => exit;

    private readonly List<RotationSpace> rotations = new();
    private bool hasTriggered = false;
    private PlayerManager playerManager;

    private void Start()
    {
        endPosition.SetParent(this);

        rotations.AddRange(GetComponentsInChildren<RotationSpace>(true));

        var e = GetComponentInChildren<MindExitPortal>();
        if (e != null) exit = e;

        TurnOff();
    }

    private void OnDisable()
    {
        DialogueManager.Instance.ResetText();
        MonologueManager.Instance.ClearText();
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

        hasTriggered = false;
        playerManager.Transitions.ToPlayer(this);

        DialogueManager.Instance.DisplayDialogue(dialogue);
        MonologueManager.Instance.SetText(monologue);
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

        var mindPortal = GetComponentInChildren<MindExitPortal>();

        if (mindPortal)
        {
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

        foreach (var clip in triggerSFX)
        {
            AudioManager.Instance.PlaySFX(clip);
        }

        DialogueManager.Instance.DisplayDialogue(onComplete);
        playerManager.Transitions.Snap();
    }

    public void Reset()
    {
        hasTriggered = false;
        TurnOff();

        var latches = GetComponentsInChildren<MovingHook>(true);

        foreach (var latch in latches) {
            latch.ResetLatch();
        }

        if (EnemyController.HasTriggered) EnemyController.ClearTrigger();

        foreach (var r in rotations) r.ClearAll();
        foreach (var t in rotated)   t.localEulerAngles = Vector3.zero;

        playerManager.PlatformerController.Camera.ResetRotation();
        playerManager.PlatformerController.ResetVelocity();
        playerManager.SetPlayerPosition(SpawnPosition);
    }

    public void TurnOff()
    {
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
