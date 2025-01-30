using System.Collections;
using TMPro;
using UnityEngine;

public class PlayerCompleteMenu : PlayerManager.PlayerController
{
    [Header("Respawn")]
    [SerializeField] private GameObject winMenu;
    [SerializeField] private TMP_Text title;
    [SerializeField] private TMP_Text respawn;
    [SerializeField] private float titlePause;
    [SerializeField] private float textPause;

    public void Activate()
    {
        TimeManager.Instance.StopTime();

        HomunculusController.Camera.MouseLock = false;
        HomunculusController.Camera.LockCamera = true;
        HomunculusController.enabled = false;

        winMenu.SetActive(true);

        StartCoroutine(RespawnMenuEffects());
    }

    private void Update()
    {
        if (!winMenu.activeSelf) return;

        if (PlayerInputs.Jump)
        {
            LevelManager.Instance.NextScene();
        }
    }

    private IEnumerator RespawnMenuEffects()
    {
        string titleText = "FLAMES REIGNITED";
        string respawnText = "PRESS SPACE TO EMERGE ANEW";

        title.text = "";
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
