using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour
{
    public class PlayerController : MonoBehaviour
    {
        protected PlayerManager player;

        public void SetPlayer(PlayerManager player) => this.player = player;

        public HomunculusController HomunculusController => player.homunculus;
        public PlatformerController PlatformerController => player.platformer;
        public PlayerLatching       PlayerLatching       => player.playerLatching;
        public PlayerInputManager   PlayerInputs         => player.playerInputs;


        public PlayerRespawnMenu     PlayerRespawn     => player.playerRespawn;
        public PlayerCompleteMenu    PlayerComplete    => player.playerComplete;
        public PlayerTransitions     PlayerTransitions => player.playerTransitions;
    }

    public static PlayerManager Instance { get; private set; }

    public PlayerTransitions    Transitions          => playerTransitions;
    public HomunculusController HomunculusController => homunculus;
    public PlatformerController PlatformerController => platformer;
    public PlayerInputManager   PlayerInputs         => playerInputs;
    public PlayerLatching       PlayerLatching       => playerLatching;


    [SerializeField] private HomunculusController homunculus;
    [SerializeField] private PlatformerController platformer;
    [SerializeField] private PlayerLatching       playerLatching;
    [SerializeField] private PlayerInputManager   playerInputs;

    [Header("VFX")]
    [SerializeField] private PlayerRespawnMenu  playerRespawn;
    [SerializeField] private PlayerCompleteMenu playerComplete;
    [SerializeField] private PlayerTransitions  playerTransitions;

    [Header("Menu")]
    [SerializeField] private GameObject PauseMenu;

    private void OnEnable()
    {
        homunculus.SetPlayer(this);
        platformer.SetPlayer(this);
        playerRespawn.SetPlayer(this);
        playerComplete.SetPlayer(this);
        playerTransitions.SetPlayer(this);

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

            homunculus.Camera.MouseLock = active;

            homunculus.Camera.enabled = platformer.Camera.enabled = playerTransitions.enabled = active;

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
        homunculus.Camera.MouseLock = homunculus.Camera.enabled = platformer.Camera.enabled = playerTransitions.enabled = true;

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
}
