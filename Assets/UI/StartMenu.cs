using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartMenu : SubMenu
{
    [Header("Button")]
    [SerializeField] private Button    buttonPrefab;
    [SerializeField] private Transform buttonHolder;
    private readonly HashSet<Button>   buttons = new();

    public void Start() {
        foreach (var menu in context.SubMenus) {
            Button menuButton = Instantiate(buttonPrefab, buttonHolder);
            menuButton.GetComponent<StartMenuButton>().DisplayName = menu.DisplayName;
            menuButton.onClick.AddListener(() => { context.SetSubmenu(menu); });
            buttons.Add(menuButton);
        }

        Button quitButton = Instantiate(buttonPrefab, buttonHolder);
        quitButton.GetComponent<StartMenuButton>().DisplayName = "Quit";
        quitButton.onClick.AddListener(() => { Application.Quit(); });
        buttons.Add(quitButton);
    }

    public override void OnEnter()
    {

    }

    public override void OnExit() {
        
    }
}
