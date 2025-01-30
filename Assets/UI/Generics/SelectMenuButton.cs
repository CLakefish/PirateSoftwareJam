using TMPro;
using UnityEngine;

public class SelectMenuButton : MonoBehaviour
{
    [SerializeField] private TMP_Text levelName;
    [SerializeField] private TMP_Text levelDescription;
    [SerializeField] private TMP_Text levelTime;

    public string DisplayName {
        set {
            levelName.text = value;
        }
    }

    public string DisplayTime {
        set {
            levelDescription.text = value;
        }
    }

    public string DisplayCompleted {
        set {
            levelTime.text = value;
        }
    }
}
