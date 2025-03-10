using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour
{
    public class PlayerController : MonoBehaviour
    {
        protected PlayerManager player;

        public void SetPlayer(PlayerManager player) => this.player = player;

        protected HomunculusController HomunculusController => player.homunculus;
        protected PlatformerController PlatformerController => player.platformer;
        protected PlayerLatching       PlayerLatching       => player.playerLatching;
        protected PlayerInputManager   PlayerInputs         => player.playerInputs;


        protected PlayerRespawnMenu     PlayerRespawn     => player.playerRespawn;
        protected PlayerCompleteMenu    PlayerComplete    => player.playerComplete;
        protected PlayerTransitions     PlayerTransitions => player.playerTransitions;
        protected PlatformerAnimator    PlatformerHand    => player.hand;
    }

    public static PlayerManager Instance { get; private set; }

    public PlayerTransitions    Transitions          => playerTransitions;
    public HomunculusController HomunculusController => homunculus;
    public PlatformerController PlatformerController => platformer;
    public PlayerInputManager   PlayerInputs         => playerInputs;


    [SerializeField] private HomunculusController homunculus;
    [SerializeField] private PlatformerController platformer;
    [SerializeField] private PlayerLatching       playerLatching;
    [SerializeField] private PlayerInputManager   playerInputs;

    [Header("VFX")]
    [SerializeField] private PlayerRespawnMenu  playerRespawn;
    [SerializeField] private PlayerCompleteMenu playerComplete;
    [SerializeField] private PlayerTransitions  playerTransitions;
    [SerializeField] private PlatformerAnimator hand;

    [Header("Menu")]
    [SerializeField] private GameObject PauseMenu;
    public bool ActiveMenu
    {
        get { return PauseMenu.activeSelf; }
    }

    private void OnEnable()
    {
        homunculus.SetPlayer(this);
        platformer.SetPlayer(this);
        playerRespawn.SetPlayer(this);
        playerComplete.SetPlayer(this);
        playerTransitions.SetPlayer(this);
        hand.SetPlayer(this);

        Instance = this;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            return;
        }

        Destroy(gameObject);
    }

    private void Start()
    {
        ReloadSaveData();
    }

    private void Update()
    {
        if (playerInputs.Menu)
        {
            PauseMenu.SetActive(!PauseMenu.activeSelf);
            bool active = !PauseMenu.activeSelf;

            homunculus.Camera.MouseLock = platformer.Camera.MouseLock = active;
            homunculus.Camera.LockCamera = !active;
            homunculus.Camera.enabled    = platformer.Camera.enabled = playerTransitions.enabled = active;

            LevelManager.Instance.AllowTimeIncrement = active;

            if (PauseMenu.activeSelf) {
                TimeManager.Instance.StopTime();
            }
            else {
                ReloadSaveData();
                TimeManager.Instance.ResumeTime();
            }
        }
    }

    public void CloseMenu()
    {
        PauseMenu.SetActive(false);
        homunculus.Camera.MouseLock = false;
        homunculus.Camera.enabled = platformer.Camera.enabled = playerTransitions.enabled = true;

        ReloadSaveData();
        TimeManager.Instance.ResumeTime();
    }

    public void ReloadSaveData()
    {
        playerInputs.Reload();
        HomunculusController.Camera.Reload();
        PlatformerController.Camera.Reload();
        AudioManager.Instance.ReloadVolumes();
    }

    public void SetPlayerPosition(Transform pos)
    {
        PlatformerController.transform.position = pos.position;
        PlatformerController.transform.forward  = pos.forward;
    }
}
