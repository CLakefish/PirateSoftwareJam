using UnityEngine;

public abstract class SubMenu : MonoBehaviour
{
    [SerializeField] private string displayName;

    public string DisplayName => displayName;
    public bool IsOpen;

    protected MainMenu context;

    public void Init(MainMenu context) => this.context = context;

    public abstract void OnEnter();
    public abstract void OnExit();

    public virtual void OnFocusUpdate() { }
    public virtual void OnFocusFixed()  { }
}
