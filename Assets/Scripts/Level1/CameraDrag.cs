using UnityEngine;
using System;
using UnityEngine.InputSystem;

public class CameraDrag : MonoBehaviour{
   private Vector3 origin;
   private Vector3 difference;

   private Camera mainCamera;
   private bool isDragging;

   private Bounds cameraBounds;
   private Vector3 targetPosition;

   [SerializeField] private float smoothTime = 0.15f;
   private Vector3 currentVelocity = Vector3.zero;
   private Vector3 smoothedPosition;

   private void Awake() => mainCamera = Camera.main;

   private void Start(){
       UpdateCameraBounds();
       smoothedPosition = transform.position;
   }

   public void UpdateCameraBounds(){
        var height = mainCamera.orthographicSize;
        var width = height * mainCamera.aspect;

        if(Globals.WorldBounds.size == Vector3.zero){
            Debug.Log("WorldBounds is not set.");
            return;
        }

        var minX = Globals.WorldBounds.min.x + width;
        var maxX = Globals.WorldBounds.max.x - width;

        var minY = Globals.WorldBounds.min.y + height;
        var maxY = Globals.WorldBounds.max.y - height;

        if(minX > maxX){
            float midX = (Globals.WorldBounds.min.x + Globals.WorldBounds.max.x) / 2f;
            minX = maxX = midX;
        }

        if(minY > maxY){
            float midY = (Globals.WorldBounds.min.y + Globals.WorldBounds.max.y) / 2f;
            minY = maxY = midY;
        }

        cameraBounds = new Bounds();
        cameraBounds.SetMinMax(new Vector3(minX, minY, 0f), new Vector3(maxX, maxY, 0f));
   }

   public void OnDrag(InputAction.CallbackContext context){
        if(context.started) origin = GetMousePosition;
        isDragging = context.started || context.performed;
   }

   private void LateUpdate(){
        if(!isDragging) return;
        difference = GetMousePosition - origin;
        
        targetPosition = transform.position - difference;
        targetPosition = GetCameraBounds();

        origin = GetMousePosition;

        smoothedPosition = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, smoothTime);

        transform.position = targetPosition;
   }

   private Vector3 GetCameraBounds(){
        return new Vector3(
            Mathf.Clamp(targetPosition.x, cameraBounds.min.x, cameraBounds.max.x),
            Mathf.Clamp(targetPosition.y, cameraBounds.min.y, cameraBounds.max.y),
            transform.position.z
        );
   }

   private Vector3 GetMousePosition => mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
}
