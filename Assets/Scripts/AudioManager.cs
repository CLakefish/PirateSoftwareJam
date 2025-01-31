using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private AudioMixer mixer;
    [SerializeField] private AudioSource music;
    [SerializeField] private AudioSource sfx;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        ReloadVolumes();
        DontDestroyOnLoad(gameObject);
    }

    public void ReloadVolumes()
    {
        mixer.SetFloat("Master", PlayerPrefs.GetFloat("Master Volume"));
        mixer.SetFloat("Music", PlayerPrefs.GetFloat("Music Volume"));
        mixer.SetFloat("SFX", PlayerPrefs.GetFloat("SFX Volume"));
    }

    public void PlayMusic(AudioClip clip)
    {
        music.clip = clip;
        music.loop = true;
        music.Play();
    }

    public void PlaySFX(AudioClip clip)
    {
        sfx.PlayOneShot(clip);
    }
}
