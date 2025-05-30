using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputManager : MonoBehaviour
{
    public static PlayerInputManager Instance { get; private set; }

    [Header("Map")]
    [SerializeField] private PlayerInput map;

    [Header("Movement")]
    [SerializeField] private Vector2Input moveInput;
    [SerializeField] private Vector2Input lookInput;
    [SerializeField] private BoolInput jump;
    [SerializeField] private BoolInput slide;
    [SerializeField] private BoolInput menu;

    [Header("Variables")]
    [SerializeField] private Vector2 sensitivity;
    [SerializeField] private bool invertX;
    [SerializeField] private bool invertY;

    [Header("Save Data")]
    [SerializeField] private ToggleSettingScriptableObject invertXData;
    [SerializeField] private ToggleSettingScriptableObject invertYData;
    [SerializeField] private SliderSettingScriptableObject sensitivityDataX;
    [SerializeField] private SliderSettingScriptableObject sensitivityDataY;

    private HashSet<InputScriptableObject> inputSet;

    public Vector2 Input
    {
        get
        {
            return moveInput.Value;
        }
    }
    public Vector2 NormalizedInput
    {
        get
        {
            return moveInput.NormalizedValue;
        }
    }

    public Vector2 MouseDelta
    {
        get
        {
            return lookInput.Value;
        }
    }

    public Vector2 AlteredMouseDelta
    {
        get
        {
            return new Vector2(MouseDelta.x * (invertX ? -1 : 1) * sensitivity.x, MouseDelta.y * (invertY ? -1 : 1) * sensitivity.y);
        }
    }

    public bool IsInputting
    {
        get
        {
            return moveInput.Active;
        }
    }

    public bool Jump
    {
        get
        {
            return jump.Pressed;
        }
    }

    public bool Slide
    {
        get
        {
            return slide.Held;
        }
    }

    public bool Menu
    {
        get
        {
            return menu.Pressed;
        }
    }

    public void Reload()
    {
        invertX = invertXData.Load() == 1;
        invertY = invertYData.Load() == 1;
        sensitivity.x = sensitivityDataX.Load();
        sensitivity.y = sensitivityDataY.Load();
    }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        Reload();
    }

    private void OnEnable()
    {
        inputSet = new() { jump, moveInput, lookInput, slide, menu };

        foreach (var action in inputSet)
        {
            action.action.Enable();
            action.Initialize(map);
        }
    }

    private void Update()
    {
        foreach (var action in inputSet) action.Update();
    }
}
