using UnityEngine;
using UnityEngine.EventSystems;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
#endif

public class InputSystemDebugger : MonoBehaviour{

#if ENABLE_INPUT_SYSTEM
    private InputAction debugAction;

    void Awake(){
        debugAction = new InputAction("Debug", InputActionType.Button, "<Mouse>/leftButton");

        debugAction.performed += OnDebugPerformed;

        debugAction.Enable();
    }

    void OnDestroy(){
        if(debugAction != null){
            debugAction.performed -=OnDebugPerformed;
            debugAction.Disable();
            debugAction.Dispose();
        }
    }

    private void OnDebugPerformed(InputAction.CallbackContext context){
        if(context.performed){
            ExecuteDebugChecks();
        }
    }

#else

    void Update(){
        if (Input.GetMouseButtonDown(0)){
            ExecuteDebugChecks();
        }
    }
#endif

    private void ExecuteDebugChecks(){
        Debug.Log("======= INPUT SYSTEM DEBUG =======");

            #if ENABLE_INPUT_SYSTEM
            Debug.Log("NEW INPUT SYSTEM DETECTED!");

            var mouse = Mouse.current;
            if(mouse != null){
                Debug.Log($"Mouse device found: {mouse.name}");
                Debug.Log($" -Left button pressed: {mouse.leftButton.isPressed}");
                Debug.Log($" -Mouse position: {mouse.position.ReadValue()}");
            }
            else{
                Debug.LogWarning("No mouse device found");
            }

            var keyboard = Keyboard.current;
        if (keyboard != null){
            Debug.Log($"Keyboard device found: {keyboard.name}");
        }
        
        var gamepad = Gamepad.current;
        if (gamepad != null){
            Debug.Log($"Gamepad device found: {gamepad.name}");
        }
            
            var inputModule = FindFirstObjectByType<InputSystemUIInputModule>();
            if (inputModule != null){
                Debug.Log($"InputSystemUIInputModule found: {inputModule.name}");
                Debug.Log($"  - Enabled: {inputModule.enabled}");
                Debug.Log($"  - Actions Asset: {inputModule.actionsAsset}");

                if(inputModule.actionsAsset != null){
                    Debug.Log($" - UI Action Asset: {inputModule.actionsAsset.name}");
                    var uiActions = inputModule.actionsAsset.FindActionMap("UI");
                    if(uiActions != null){
                        Debug.Log($" - UI Action Map found with {uiActions.actions.Count} actions");
                        foreach (var action in uiActions.actions){
                            Debug.Log ($" - Action: {action.name} (Enabled: {action.enabled})");
                        }
                    }
                }
            }
            else{
                Debug.LogError("InputSystemUIInputModule NOT FOUND!");
            }

            Debug.Log($"Input System initialized: {InputSystem.settings != null}");
            if (InputSystem.settings != null){
                Debug.Log($"  - Update mode: {InputSystem.settings.updateMode}");
                Debug.Log($"  - Compensate for screen orientation: {InputSystem.settings.compensateForScreenOrientation}");
            }
            
            Debug.Log($"Connected devices: {InputSystem.devices.Count}");
            foreach (var device in InputSystem.devices){
                Debug.Log($"  - Device: {device.name} ({device.GetType().Name}) - Enabled: {device.enabled}");
            }
        #endif

            var standaloneInputModule = FindFirstObjectByType<StandaloneInputModule>();
            if (standaloneInputModule != null){
                Debug.Log($"StandaloneInputModule found: {standaloneInputModule.name}");
                Debug.Log($"  - Enabled: {standaloneInputModule.enabled}");
            }
            else{
                Debug.LogWarning("StandaloneInputModule NOT FOUND!");
            }

        if (EventSystem.current != null){
            Debug.Log($"EventSystem current input module: {EventSystem.current.currentInputModule}");
            Debug.Log($"EventSystem current input module type: {EventSystem.current.currentInputModule?.GetType().Name}");
            Debug.Log($"EventSystem current input module enabled: {EventSystem.current.currentInputModule?.enabled}");

            if (EventSystem.current.currentInputModule != null){
                Debug.Log($"Input module is processing: {EventSystem.current.currentInputModule.IsModuleSupported()}");
            }

            Debug.Log($"EventSystem first selected: {EventSystem.current.firstSelectedGameObject}");
            Debug.Log($"EventSystem current selected: {EventSystem.current.currentSelectedGameObject}");
        }
        else{
            Debug.LogError("EventSystem not found");
        }

        Debug.Log("Project Settings:");
#if ENABLE_INPUT_SYSTEM && ENABLE_LEGACY_INPUT_MANAGER
                    Debug.Log("  - Input handling: Both (New and Old)");
#elif ENABLE_INPUT_SYSTEM
                    Debug.Log("  - Input handling: Input System Package (New)");
#elif ENABLE_LEGACY_INPUT_MANAGER
                    Debug.Log("  - Input handling: Input Manager (Old)");
#else
        Debug.Log(" -Input handling: Unknown configuration");
        #endif

        #if ENABLE_INPUT_SYSTEM
        Debug.Log("Additional Input System Info:");
        Debug.Log($"  - Input System version: {InputSystem.version}");
        Debug.Log($"  - Polling frequency: {InputSystem.pollingFrequency}");
        
        var activeActions = InputSystem.ListEnabledActions();
        Debug.Log($"  - Active actions count: {activeActions.Count}");
        if (activeActions.Count > 0){
            Debug.Log("  - Active actions:");
            foreach (var action in activeActions){
                Debug.Log($"    - {action.name} (Map: {action.actionMap?.name})");
            }
        }
#endif

        Debug.Log("======= END INPUT SYSTEM DEBUG =======");
    }
}
