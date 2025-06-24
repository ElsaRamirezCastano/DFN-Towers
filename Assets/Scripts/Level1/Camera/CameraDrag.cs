using UnityEngine;
using UnityEngine.InputSystem;

public class CameraDrag : MonoBehaviour
{

    [Header("Input Actions")]
    [SerializeField] private InputActionReference dragAction;
    [SerializeField] private InputActionReference mousePositionAction;

    [Header("Camera Settings")]
    [SerializeField] private float smoothTime = 0.15f;
    [SerializeField] private bool useSmoothing = true;

    private Vector3 origin;
    private Vector3 difference;
    private Camera mainCamera;
    private bool isDragging;

    private Bounds cameraBounds;
    private Vector3 targetPosition;
    private Vector3 currentVelocity = Vector3.zero;
    private Vector3 smoothedPosition;

    private void Awake(){
        mainCamera = Camera.main;
        if (mainCamera == null){
            mainCamera = FindFirstObjectByType<Camera>();
        }
    }

    private void OnEnable(){
        EnableInputActions();
    }

    private void OnDisable(){
        DisableInputActions();
    }

    private void EnableInputActions(){
        if (dragAction != null){
            dragAction.action.Enable();
            dragAction.action.started += OnDragStarted;
            dragAction.action.performed += OnDragPerformed;
            dragAction.action.canceled += OnDragCanceled;
        }

        if (mousePositionAction != null){
            mousePositionAction.action.Enable();
        }
    }

    private void DisableInputActions(){
        if (dragAction != null){
            dragAction.action.started -= OnDragStarted;
            dragAction.action.performed -= OnDragPerformed;
            dragAction.action.canceled -= OnDragCanceled;
            dragAction.action.Disable();
        }

        if (mousePositionAction != null){
            mousePositionAction.action.Disable();
        }
    }

    private void Start(){
        UpdateCameraBounds();
        smoothedPosition = transform.position;
    }

    public void UpdateCameraBounds(){

        if (mainCamera == null) return;
        var height = mainCamera.orthographicSize;
        var width = height * mainCamera.aspect;

        if (Globals.WorldBounds.size == Vector3.zero){
            Debug.Log("WorldBounds is not set.");
            return;
        }

        var minX = Globals.WorldBounds.min.x + width;
        var maxX = Globals.WorldBounds.max.x - width;

        var minY = Globals.WorldBounds.min.y + height;
        var maxY = Globals.WorldBounds.max.y - height;

        if (minX > maxX){
            float midX = (Globals.WorldBounds.min.x + Globals.WorldBounds.max.x) / 2f;
            minX = maxX = midX;
        }

        if (minY > maxY){
            float midY = (Globals.WorldBounds.min.y + Globals.WorldBounds.max.y) / 2f;
            minY = maxY = midY;
        }

        cameraBounds = new Bounds();
        cameraBounds.SetMinMax(new Vector3(minX, minY, 0f), new Vector3(maxX, maxY, 0f));
    }

    public void OnDragStarted(InputAction.CallbackContext context){
        origin = GetMouseWorldPosition();
        isDragging = true;
    }

    private void OnDragPerformed(InputAction.CallbackContext context){
        isDragging = true;
    }

    private void OnDragCanceled(InputAction.CallbackContext context){
        isDragging = false;
    }

    private void LateUpdate(){
        if (!isDragging) return;

        Vector3 currentMousePos = GetMouseWorldPosition();
        difference = currentMousePos - origin;

        targetPosition = transform.position - difference;
        targetPosition = ApplyCameraBounds(targetPosition);

        origin = currentMousePos;

        if (useSmoothing){
            smoothedPosition = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, smoothTime);
            transform.position = smoothedPosition;
        }
        else{
            transform.position = targetPosition;
        }
    }

    private Vector3 ApplyCameraBounds(Vector3 position){
        if (Globals.WorldBounds.size == Vector3.zero){
            return position;
        }

        return new Vector3(
            Mathf.Clamp(position.x, cameraBounds.min.x, cameraBounds.max.x),
            Mathf.Clamp(position.y, cameraBounds.min.y, cameraBounds.max.y),
            transform.position.z
        );
    }

    private Vector3 GetMouseWorldPosition(){
        if (mousePositionAction != null && mainCamera != null){
            Vector2 screenPos = mousePositionAction.action.ReadValue<Vector2>();
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(screenPos);
            worldPos.z = 0;
            return worldPos;
        }
        return Vector3.zero;
    }
}
