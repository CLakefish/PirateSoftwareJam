using UnityEngine;

public class Hook : Latchable
{
    public override void Latch(HomunculusController controller)
    {
        PlayerManager.Instance.Transitions.IdleSnap();
        controller.Rebound();
        return;
        //throw new System.NotImplementedException();
    }
}
