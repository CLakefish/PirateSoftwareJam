using UnityEngine;

public class Hook : Latchable
{
    public override void Latch(HomunculusController controller)
    {
        PlayerManager.Instance.Transitions.IdleSnap();
    }
}
