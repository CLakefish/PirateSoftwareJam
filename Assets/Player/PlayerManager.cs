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
        public PlayerInputManager   PlayerInputs         => player.playerInputs;


        public PlayerRespawn     PlayerRespawn     => player.playerRespawn;
        public PlayerTransitions PlayerTransitions => player.playerTransitions;
    }

    public static PlayerManager Instance { get; private set; }

    public PlayerTransitions Transitions => playerTransitions;


    [SerializeField] private HomunculusController homunculus;
    [SerializeField] private PlatformerController platformer;
    [SerializeField] private PlayerInputManager   playerInputs;

    [Header("VFX")]
    [SerializeField] private PlayerRespawn     playerRespawn;
    [SerializeField] private PlayerTransitions playerTransitions;

    private void OnEnable()
    {
        homunculus.SetPlayer(this);
        platformer.SetPlayer(this);
        playerRespawn.SetPlayer(this);
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
}
