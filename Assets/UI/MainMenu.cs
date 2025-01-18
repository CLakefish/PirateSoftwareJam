using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public static MainMenu Instance { get; private set; }

    [Header("Sub Menus")]
    [SerializeField] private StartMenu     startMenu;
    [SerializeField] private List<SubMenu> subMenus = new();

    private SubMenu currentSubMenu;
    public SubMenu CurrentSubMenu => currentSubMenu;
    public List<SubMenu> SubMenus => subMenus;

    public void SetSubmenu(SubMenu next)
    {
        if (currentSubMenu != null)
        {
            if (currentSubMenu.DisplayName == next.DisplayName)
            {
                currentSubMenu.OnReEnter();
                return;
            }

            currentSubMenu.OnExit();
        }

        currentSubMenu = next;
        currentSubMenu.OnEnter();
    }

    private void OnEnable()
    {
        startMenu.Init(this);

        foreach (var subMenu in subMenus) subMenu.Init(this);
    }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
            return;
        }

        Instance = this;

        SetSubmenu(startMenu);
    }
}
