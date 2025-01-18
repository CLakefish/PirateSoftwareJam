using TMPro;
using UnityEngine;

public class StartMenuButton : MonoBehaviour
{
    [SerializeField] private TMP_Text displayName;

    public string DisplayName
    {
        set
        {
            displayName.text = value;
        }
    }
}
