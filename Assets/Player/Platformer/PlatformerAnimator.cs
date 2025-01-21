using UnityEngine;

public class PlatformerAnimator : MonoBehaviour
{
    [SerializeField] private Animator hand;

    public void HandAnim(string name) => hand.SetTrigger(name);
}
