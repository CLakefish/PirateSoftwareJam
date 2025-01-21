using System.Collections;
using TMPro;
using UnityEngine;

public class PlayerRespawnMenu : PlayerManager.PlayerController
{
    [Header("Respawn")]
    [SerializeField] private GameObject respawnMenu;
    [SerializeField] private TMP_Text title;
    [SerializeField] private TMP_Text respawn;
    [SerializeField] private float titlePause;
    [SerializeField] private float textPause;

    public void Respawn()
    {
        TimeManager.Instance.StopTime();

        HomunculusController.Camera.MouseLock  = false;
        HomunculusController.Camera.LockCamera = true;
        HomunculusController.enabled = false;

        respawnMenu.SetActive(true);

        StartCoroutine(RespawnMenuEffects());
    }

    private void Update()
    {
        if (!respawnMenu.activeSelf) return;

        if (PlayerInputs.Jump)
        {
            TimeManager.Instance.ResumeTime();
            LevelManager.Instance.ReloadScene();
        }
    }

    private IEnumerator RespawnMenuEffects()
    {
        string titleText = "PERISHED";
        string respawnText = "PRESS SPACE TO RESPAWN";

        title.text   = "";
        respawn.text = "";

        for (int i = 0; i < titleText.Length; i++)
        {
            title.text += titleText[i];
            yield return new WaitForSecondsRealtime(textPause);
        }

        yield return new WaitForSecondsRealtime(titlePause);

        for (int i = 0; i < respawnText.Length; i++)
        {
            respawn.text += respawnText[i];
            yield return new WaitForSecondsRealtime(textPause);
        }
    }
}
