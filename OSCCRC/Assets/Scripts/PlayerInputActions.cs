// GENERATED AUTOMATICALLY FROM 'Assets/PlayerInputActions.inputactions'

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class @PlayerInputActions : IInputActionCollection, IDisposable
{
    public InputActionAsset asset { get; }
    public @PlayerInputActions()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""PlayerInputActions"",
    ""maps"": [
        {
            ""name"": ""Game"",
            ""id"": ""c52a3ef3-0fab-42d0-bfe3-223f71aa1b46"",
            ""actions"": [
                {
                    ""name"": ""CursorMovement"",
                    ""type"": ""PassThrough"",
                    ""id"": ""66a633e2-2b57-4bd1-aff0-5d4b833fa612"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Select"",
                    ""type"": ""Button"",
                    ""id"": ""2bd8f7ef-b79f-470d-b0e9-1709d5184521"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": ""Press""
                },
                {
                    ""name"": ""DirectionalPlacement"",
                    ""type"": ""Button"",
                    ""id"": ""439f2da8-54df-4c83-be61-877bacb22db8"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Pause"",
                    ""type"": ""Button"",
                    ""id"": ""f5d9d535-7369-4485-93e2-9a4bac852a81"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": ""Press""
                },
                {
                    ""name"": ""Reset"",
                    ""type"": ""Button"",
                    ""id"": ""91a40072-e898-481c-8e89-ee2c1d18a82f"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": ""Press""
                },
                {
                    ""name"": ""Menu"",
                    ""type"": ""Button"",
                    ""id"": ""7802d923-d4e9-4aa2-9bb5-7d5c5d9dbd75"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": ""Press""
                },
                {
                    ""name"": ""FramerateDisplayToggle"",
                    ""type"": ""Button"",
                    ""id"": ""b905ebf5-d20e-480b-b568-4ae424947a42"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": ""Press""
                },
                {
                    ""name"": ""SpeedToggle"",
                    ""type"": ""Button"",
                    ""id"": ""f8279fd6-02e3-4dcf-9795-df61828067a4"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": ""Press""
                },
                {
                    ""name"": ""MoonwalkToggle"",
                    ""type"": ""Button"",
                    ""id"": ""3f2448a8-d32a-4b54-b6ec-acf62f2c5d77"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": ""Press""
                },
                {
                    ""name"": ""FirstPersonToggle"",
                    ""type"": ""Button"",
                    ""id"": ""0a888bc1-0a5c-4b32-bda9-659730af0f61"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": ""Press""
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""c48f0150-d842-43ca-a4fa-37ec481dd115"",
                    ""path"": ""<Gamepad>/leftStick"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Controller"",
                    ""action"": ""CursorMovement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""e3ec4e1e-0257-4736-a101-be92ed2c98ef"",
                    ""path"": ""<Gamepad>/rightStick"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Controller"",
                    ""action"": ""CursorMovement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""ff1a32e9-30bb-4578-85c7-558a49176f4b"",
                    ""path"": ""<Mouse>/delta"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Mouse+Keyboard"",
                    ""action"": ""CursorMovement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""ARROWS"",
                    ""id"": ""bc1b560f-8d4c-44c6-9bae-dadbf3d74fdf"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Mouse+Keyboard"",
                    ""action"": ""DirectionalPlacement"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""d7b350dd-8040-4170-a243-2805d56caedb"",
                    ""path"": ""<Keyboard>/upArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Mouse+Keyboard"",
                    ""action"": ""DirectionalPlacement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""86c02e8c-3143-497c-b107-0f535455c923"",
                    ""path"": ""<Keyboard>/downArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Mouse+Keyboard"",
                    ""action"": ""DirectionalPlacement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""b01c0d12-37b4-49d9-a5d5-3e6d62ee0d61"",
                    ""path"": ""<Keyboard>/leftArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Mouse+Keyboard"",
                    ""action"": ""DirectionalPlacement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""b3dfa2b2-bbcb-430f-afcd-15dc8ffd42ae"",
                    ""path"": ""<Keyboard>/rightArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Mouse+Keyboard"",
                    ""action"": ""DirectionalPlacement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""WASD"",
                    ""id"": ""e11958e2-66df-4dac-af48-efe2e1e6cfcf"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Mouse+Keyboard"",
                    ""action"": ""DirectionalPlacement"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""bb71a011-ec7d-4342-ac10-c5c7807d1530"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Mouse+Keyboard"",
                    ""action"": ""DirectionalPlacement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""2cd6cd47-c68f-4f6e-85ff-16e04d33a5c1"",
                    ""path"": ""<Keyboard>/s"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Mouse+Keyboard"",
                    ""action"": ""DirectionalPlacement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""323f8124-de18-4407-bb5b-4a61253f7c65"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Mouse+Keyboard"",
                    ""action"": ""DirectionalPlacement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""79710dd3-dc1a-4ca2-9732-ad23995090f4"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Mouse+Keyboard"",
                    ""action"": ""DirectionalPlacement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""ActionButtons"",
                    ""id"": ""33590d8b-0b6a-4295-a164-a163fa66437d"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""DirectionalPlacement"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""fd42766e-cefb-4079-b367-ffb7a3fe63c3"",
                    ""path"": ""<Gamepad>/buttonNorth"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Controller"",
                    ""action"": ""DirectionalPlacement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""b39ab87b-b7e4-4c9f-8fb6-6033d00330c0"",
                    ""path"": ""<Gamepad>/buttonSouth"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Controller"",
                    ""action"": ""DirectionalPlacement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""dcb1d350-9dab-4c34-ae72-008238905f1a"",
                    ""path"": ""<Gamepad>/buttonWest"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Controller"",
                    ""action"": ""DirectionalPlacement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""5d72bb36-8ed0-4f3a-8e85-68d5b1b167ad"",
                    ""path"": ""<Gamepad>/buttonEast"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Controller"",
                    ""action"": ""DirectionalPlacement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""a39d1427-1144-43e3-816e-0100a5ac9b40"",
                    ""path"": ""<Keyboard>/space"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Mouse+Keyboard"",
                    ""action"": ""Pause"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""4940b7dd-d638-49d2-8c5f-dbb0afc2438d"",
                    ""path"": ""<Gamepad>/rightTrigger"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Controller"",
                    ""action"": ""Pause"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""81cd59e8-d78f-4a36-8c0d-61546aa57b81"",
                    ""path"": ""<Keyboard>/r"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Mouse+Keyboard"",
                    ""action"": ""Reset"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""86fbdef0-5992-415b-9f02-3cb4b434bafe"",
                    ""path"": ""<Gamepad>/rightShoulder"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Controller"",
                    ""action"": ""Reset"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""17871ca7-8ad1-452e-9b4c-d072e6302d0e"",
                    ""path"": ""<Keyboard>/escape"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Mouse+Keyboard"",
                    ""action"": ""Menu"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""bb3d97ff-ad1e-4df7-a2a1-ed0b1f5a82f2"",
                    ""path"": ""<Gamepad>/start"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Controller"",
                    ""action"": ""Menu"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""d79db0ec-38ca-421e-a7cd-a7126e082fd5"",
                    ""path"": ""<Keyboard>/f5"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Mouse+Keyboard"",
                    ""action"": ""FramerateDisplayToggle"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""6e391fae-a46a-4bb0-80e0-b3ab4e9c8097"",
                    ""path"": ""<Gamepad>/select"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Controller"",
                    ""action"": ""FramerateDisplayToggle"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""9a6a6827-ec70-4924-b407-a4619c3c228a"",
                    ""path"": ""<Keyboard>/leftShift"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Mouse+Keyboard"",
                    ""action"": ""SpeedToggle"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""785ce032-b99b-4ddb-b723-684f83050a32"",
                    ""path"": ""<Keyboard>/rightShift"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Mouse+Keyboard"",
                    ""action"": ""SpeedToggle"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""f2f52e2e-ed4a-442d-b54f-96317bdf23f3"",
                    ""path"": ""<Gamepad>/leftTrigger"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Controller"",
                    ""action"": ""SpeedToggle"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""25bf4840-9475-484e-a580-37ccb39801f7"",
                    ""path"": ""<Gamepad>/leftShoulder"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Controller"",
                    ""action"": ""SpeedToggle"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""d02dd0a4-919f-438a-8d43-60369019cbf8"",
                    ""path"": ""<Keyboard>/m"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Mouse+Keyboard"",
                    ""action"": ""MoonwalkToggle"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""b158cd89-4269-4885-8f21-897c7a555395"",
                    ""path"": ""<Gamepad>/dpad/down"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Controller"",
                    ""action"": ""MoonwalkToggle"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""eb1806b5-444f-4924-ba47-e22947d0d296"",
                    ""path"": ""<Keyboard>/n"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Mouse+Keyboard"",
                    ""action"": ""FirstPersonToggle"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""25e435f2-9458-4f74-b598-8127ee0dd639"",
                    ""path"": ""<Gamepad>/dpad/up"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Controller"",
                    ""action"": ""FirstPersonToggle"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""80a5d517-9ded-4d97-9fae-e0ed24d637a7"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Mouse+Keyboard"",
                    ""action"": ""Select"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""3471a769-dee9-4bd7-897b-ca9b40bd079e"",
                    ""path"": ""<Gamepad>/buttonSouth"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Controller"",
                    ""action"": ""Select"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": [
        {
            ""name"": ""Mouse+Keyboard"",
            ""bindingGroup"": ""Mouse+Keyboard"",
            ""devices"": [
                {
                    ""devicePath"": ""<Mouse>"",
                    ""isOptional"": false,
                    ""isOR"": false
                },
                {
                    ""devicePath"": ""<Keyboard>"",
                    ""isOptional"": false,
                    ""isOR"": false
                }
            ]
        },
        {
            ""name"": ""Controller"",
            ""bindingGroup"": ""Controller"",
            ""devices"": [
                {
                    ""devicePath"": ""<Gamepad>"",
                    ""isOptional"": false,
                    ""isOR"": false
                }
            ]
        }
    ]
}");
        // Game
        m_Game = asset.FindActionMap("Game", throwIfNotFound: true);
        m_Game_CursorMovement = m_Game.FindAction("CursorMovement", throwIfNotFound: true);
        m_Game_Select = m_Game.FindAction("Select", throwIfNotFound: true);
        m_Game_DirectionalPlacement = m_Game.FindAction("DirectionalPlacement", throwIfNotFound: true);
        m_Game_Pause = m_Game.FindAction("Pause", throwIfNotFound: true);
        m_Game_Reset = m_Game.FindAction("Reset", throwIfNotFound: true);
        m_Game_Menu = m_Game.FindAction("Menu", throwIfNotFound: true);
        m_Game_FramerateDisplayToggle = m_Game.FindAction("FramerateDisplayToggle", throwIfNotFound: true);
        m_Game_SpeedToggle = m_Game.FindAction("SpeedToggle", throwIfNotFound: true);
        m_Game_MoonwalkToggle = m_Game.FindAction("MoonwalkToggle", throwIfNotFound: true);
        m_Game_FirstPersonToggle = m_Game.FindAction("FirstPersonToggle", throwIfNotFound: true);
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }

    // Game
    private readonly InputActionMap m_Game;
    private IGameActions m_GameActionsCallbackInterface;
    private readonly InputAction m_Game_CursorMovement;
    private readonly InputAction m_Game_Select;
    private readonly InputAction m_Game_DirectionalPlacement;
    private readonly InputAction m_Game_Pause;
    private readonly InputAction m_Game_Reset;
    private readonly InputAction m_Game_Menu;
    private readonly InputAction m_Game_FramerateDisplayToggle;
    private readonly InputAction m_Game_SpeedToggle;
    private readonly InputAction m_Game_MoonwalkToggle;
    private readonly InputAction m_Game_FirstPersonToggle;
    public struct GameActions
    {
        private @PlayerInputActions m_Wrapper;
        public GameActions(@PlayerInputActions wrapper) { m_Wrapper = wrapper; }
        public InputAction @CursorMovement => m_Wrapper.m_Game_CursorMovement;
        public InputAction @Select => m_Wrapper.m_Game_Select;
        public InputAction @DirectionalPlacement => m_Wrapper.m_Game_DirectionalPlacement;
        public InputAction @Pause => m_Wrapper.m_Game_Pause;
        public InputAction @Reset => m_Wrapper.m_Game_Reset;
        public InputAction @Menu => m_Wrapper.m_Game_Menu;
        public InputAction @FramerateDisplayToggle => m_Wrapper.m_Game_FramerateDisplayToggle;
        public InputAction @SpeedToggle => m_Wrapper.m_Game_SpeedToggle;
        public InputAction @MoonwalkToggle => m_Wrapper.m_Game_MoonwalkToggle;
        public InputAction @FirstPersonToggle => m_Wrapper.m_Game_FirstPersonToggle;
        public InputActionMap Get() { return m_Wrapper.m_Game; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(GameActions set) { return set.Get(); }
        public void SetCallbacks(IGameActions instance)
        {
            if (m_Wrapper.m_GameActionsCallbackInterface != null)
            {
                @CursorMovement.started -= m_Wrapper.m_GameActionsCallbackInterface.OnCursorMovement;
                @CursorMovement.performed -= m_Wrapper.m_GameActionsCallbackInterface.OnCursorMovement;
                @CursorMovement.canceled -= m_Wrapper.m_GameActionsCallbackInterface.OnCursorMovement;
                @Select.started -= m_Wrapper.m_GameActionsCallbackInterface.OnSelect;
                @Select.performed -= m_Wrapper.m_GameActionsCallbackInterface.OnSelect;
                @Select.canceled -= m_Wrapper.m_GameActionsCallbackInterface.OnSelect;
                @DirectionalPlacement.started -= m_Wrapper.m_GameActionsCallbackInterface.OnDirectionalPlacement;
                @DirectionalPlacement.performed -= m_Wrapper.m_GameActionsCallbackInterface.OnDirectionalPlacement;
                @DirectionalPlacement.canceled -= m_Wrapper.m_GameActionsCallbackInterface.OnDirectionalPlacement;
                @Pause.started -= m_Wrapper.m_GameActionsCallbackInterface.OnPause;
                @Pause.performed -= m_Wrapper.m_GameActionsCallbackInterface.OnPause;
                @Pause.canceled -= m_Wrapper.m_GameActionsCallbackInterface.OnPause;
                @Reset.started -= m_Wrapper.m_GameActionsCallbackInterface.OnReset;
                @Reset.performed -= m_Wrapper.m_GameActionsCallbackInterface.OnReset;
                @Reset.canceled -= m_Wrapper.m_GameActionsCallbackInterface.OnReset;
                @Menu.started -= m_Wrapper.m_GameActionsCallbackInterface.OnMenu;
                @Menu.performed -= m_Wrapper.m_GameActionsCallbackInterface.OnMenu;
                @Menu.canceled -= m_Wrapper.m_GameActionsCallbackInterface.OnMenu;
                @FramerateDisplayToggle.started -= m_Wrapper.m_GameActionsCallbackInterface.OnFramerateDisplayToggle;
                @FramerateDisplayToggle.performed -= m_Wrapper.m_GameActionsCallbackInterface.OnFramerateDisplayToggle;
                @FramerateDisplayToggle.canceled -= m_Wrapper.m_GameActionsCallbackInterface.OnFramerateDisplayToggle;
                @SpeedToggle.started -= m_Wrapper.m_GameActionsCallbackInterface.OnSpeedToggle;
                @SpeedToggle.performed -= m_Wrapper.m_GameActionsCallbackInterface.OnSpeedToggle;
                @SpeedToggle.canceled -= m_Wrapper.m_GameActionsCallbackInterface.OnSpeedToggle;
                @MoonwalkToggle.started -= m_Wrapper.m_GameActionsCallbackInterface.OnMoonwalkToggle;
                @MoonwalkToggle.performed -= m_Wrapper.m_GameActionsCallbackInterface.OnMoonwalkToggle;
                @MoonwalkToggle.canceled -= m_Wrapper.m_GameActionsCallbackInterface.OnMoonwalkToggle;
                @FirstPersonToggle.started -= m_Wrapper.m_GameActionsCallbackInterface.OnFirstPersonToggle;
                @FirstPersonToggle.performed -= m_Wrapper.m_GameActionsCallbackInterface.OnFirstPersonToggle;
                @FirstPersonToggle.canceled -= m_Wrapper.m_GameActionsCallbackInterface.OnFirstPersonToggle;
            }
            m_Wrapper.m_GameActionsCallbackInterface = instance;
            if (instance != null)
            {
                @CursorMovement.started += instance.OnCursorMovement;
                @CursorMovement.performed += instance.OnCursorMovement;
                @CursorMovement.canceled += instance.OnCursorMovement;
                @Select.started += instance.OnSelect;
                @Select.performed += instance.OnSelect;
                @Select.canceled += instance.OnSelect;
                @DirectionalPlacement.started += instance.OnDirectionalPlacement;
                @DirectionalPlacement.performed += instance.OnDirectionalPlacement;
                @DirectionalPlacement.canceled += instance.OnDirectionalPlacement;
                @Pause.started += instance.OnPause;
                @Pause.performed += instance.OnPause;
                @Pause.canceled += instance.OnPause;
                @Reset.started += instance.OnReset;
                @Reset.performed += instance.OnReset;
                @Reset.canceled += instance.OnReset;
                @Menu.started += instance.OnMenu;
                @Menu.performed += instance.OnMenu;
                @Menu.canceled += instance.OnMenu;
                @FramerateDisplayToggle.started += instance.OnFramerateDisplayToggle;
                @FramerateDisplayToggle.performed += instance.OnFramerateDisplayToggle;
                @FramerateDisplayToggle.canceled += instance.OnFramerateDisplayToggle;
                @SpeedToggle.started += instance.OnSpeedToggle;
                @SpeedToggle.performed += instance.OnSpeedToggle;
                @SpeedToggle.canceled += instance.OnSpeedToggle;
                @MoonwalkToggle.started += instance.OnMoonwalkToggle;
                @MoonwalkToggle.performed += instance.OnMoonwalkToggle;
                @MoonwalkToggle.canceled += instance.OnMoonwalkToggle;
                @FirstPersonToggle.started += instance.OnFirstPersonToggle;
                @FirstPersonToggle.performed += instance.OnFirstPersonToggle;
                @FirstPersonToggle.canceled += instance.OnFirstPersonToggle;
            }
        }
    }
    public GameActions @Game => new GameActions(this);
    private int m_MouseKeyboardSchemeIndex = -1;
    public InputControlScheme MouseKeyboardScheme
    {
        get
        {
            if (m_MouseKeyboardSchemeIndex == -1) m_MouseKeyboardSchemeIndex = asset.FindControlSchemeIndex("Mouse+Keyboard");
            return asset.controlSchemes[m_MouseKeyboardSchemeIndex];
        }
    }
    private int m_ControllerSchemeIndex = -1;
    public InputControlScheme ControllerScheme
    {
        get
        {
            if (m_ControllerSchemeIndex == -1) m_ControllerSchemeIndex = asset.FindControlSchemeIndex("Controller");
            return asset.controlSchemes[m_ControllerSchemeIndex];
        }
    }
    public interface IGameActions
    {
        void OnCursorMovement(InputAction.CallbackContext context);
        void OnSelect(InputAction.CallbackContext context);
        void OnDirectionalPlacement(InputAction.CallbackContext context);
        void OnPause(InputAction.CallbackContext context);
        void OnReset(InputAction.CallbackContext context);
        void OnMenu(InputAction.CallbackContext context);
        void OnFramerateDisplayToggle(InputAction.CallbackContext context);
        void OnSpeedToggle(InputAction.CallbackContext context);
        void OnMoonwalkToggle(InputAction.CallbackContext context);
        void OnFirstPersonToggle(InputAction.CallbackContext context);
    }
}
