using UnityEngine;

public class Hook : Latchable
{
    public override void Latch()
    {
        PlayerManager.Instance.Transitions.Grab();
    }
}
