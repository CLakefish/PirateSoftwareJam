using TMPro;
using UnityEngine;

public class SelectMenuButton : MonoBehaviour
{
    [SerializeField] private TMP_Text levelName;
    [SerializeField] private TMP_Text levelDescription;

    public string DisplayName {
        set {
            levelName.text = value;
        }
    }

    public string DisplayDescription {
        set {
            levelDescription.text = value;
        }
    }
}
