using UnityEngine;
using UnityEngine.SceneManagement;

public class TestingReset : MonoBehaviour
{
    void Start()
    {
        Invoke(nameof(GoBack), 1);
    }

    private void GoBack()
    {
        SceneManager.LoadScene("Test");
    }
}
