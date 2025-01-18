using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }

    [SerializeField] private GameObject homunculus;
    [SerializeField] private GameObject platformer;

    public bool Homunculus {
        set {
            homunculus.SetActive(value);
            platformer.SetActive(!value);
        } 
    }

    public void SetPlayer(Transform pos) {
        platformer.transform.position = pos.position;
        platformer.transform.forward  = pos.forward;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            return;
        }
    }
}
