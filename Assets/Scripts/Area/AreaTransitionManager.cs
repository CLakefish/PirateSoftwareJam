using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class AreaTransitionManager : MonoBehaviour
{
    public static AreaTransitionManager Instance { get; private set; }

    [SerializeField] private RawImage homunculusView;
    [SerializeField] private RawImage platformerView;
    [SerializeField] private float interpolateSpeed;

    private Transform spawnPos;
    private Coroutine scale;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            return;
        }

        Destroy(gameObject);
    }

    public void TransitionTo(Area to)
    {
        PlayerManager.Instance.SetPlayer(to.SpawnPosition);
        PlayerManager.Instance.Homunculus = false;
        RunAnimation(false);
    }

    public void TransitionFrom(Area from)
    {
        RunAnimation(true);
    }

    private void RunAnimation(bool homunculus)
    {
        if (scale != null) StopCoroutine(scale);
        scale = StartCoroutine(TransitionAnimation(homunculus));
    }

    private IEnumerator TransitionAnimation(bool homunculus)
    {
        RawImage image   = platformerView;
        Vector3 scaleVel = Vector3.zero;
        Vector3 endScale = homunculus ? Vector3.zero : Vector3.one;

        image.transform.localScale = homunculus ? Vector3.one : Vector3.zero;

        image.transform.SetAsLastSibling();

        while (Vector3.Distance(image.transform.localScale, endScale) > 0.01f)
        {
            image.transform.localScale = Vector3.SmoothDamp(image.transform.localScale, endScale, ref scaleVel, interpolateSpeed);
            yield return null;
        }

        image.transform.localScale = endScale;

        PlayerManager.Instance.Homunculus = homunculus;
    }
}
