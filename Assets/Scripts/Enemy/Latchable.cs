using UnityEngine;

public abstract class Latchable : MonoBehaviour
{
    public bool slowTime = true;
    public abstract void Latch(HomunculusController controller);
}
