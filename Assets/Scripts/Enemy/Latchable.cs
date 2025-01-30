using UnityEngine;

public abstract class Latchable : MonoBehaviour
{
    public Vector3 offset = Vector3.zero;
    public bool slowTime = true;
    public abstract void Latch(HomunculusController controller);
}
