using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class ObjectDrag : MonoBehaviour
{
    [Header("Input Actions")]
    [SerializeField] private InputActionReference leftClickAction;
    [SerializeField] private InputActionReference mousePositionAction;
    private PlaceableObject placeableObject;
    private Camera mainCamera;
    private SpriteRenderer sr;
    private bool waitingConfirmation = false;
    private bool towerRegistered = false;
    private bool isInPreviewMode = true;

    void Start()
    {
        mainCamera = Camera.main;
        placeableObject = GetComponent<PlaceableObject>();
        sr = GetComponent<SpriteRenderer>();

        if (sr != null)
        {
            Color color = sr.color;
            sr.color = new Color(color.r, color.g, color.b, 0.6f);
        }

        EnableInputActions();
    }

    void OnDestroy()
    {
        DisableInputActions();
    }

    private void EnableInputActions()
    {
        if (leftClickAction != null)
        {
            leftClickAction.action.Enable();
            leftClickAction.action.performed += OnLeftClick;
        }

        if (mousePositionAction != null)
        {
            mousePositionAction.action.Enable();
        }
    }

    private void DisableInputActions()
    {
        if (leftClickAction != null)
        {
            leftClickAction.action.performed -= OnLeftClick;
            leftClickAction.action.Disable();
        }

        if (mousePositionAction != null)
        {
            mousePositionAction.action.Disable();
        }
    }

    void Update()
    {
        if (waitingConfirmation)
        {
            return;
        }

        UpdateTowerPosition();
        UpdateVisualFeedback();
    }

    private void UpdateTowerPosition()
    {
        Vector3 mousePos = GetMouseWorldPosition();

        Vector3Int cellPos = BuildingSystem.current.gridLayout.WorldToCell(mousePos);
        transform.position = BuildingSystem.current.gridLayout.CellToLocalInterpolated(cellPos);

        transform.position += new Vector3(BuildingSystem.current.gridLayout.cellSize.x / 2, BuildingSystem.current.gridLayout.cellSize.y / 2, 0);
    }

    private void UpdateVisualFeedback()
    {
        if (sr != null)
        {
            bool canPlace = placeableObject.CanBePlaced();
            bool withinTowerLimit = BuildingSystem.current.CanPlaceTower();
            bool canPlaceAndWithinLimit = canPlace && withinTowerLimit;

            sr.color = canPlaceAndWithinLimit ?
            new Color(1f, 1f, 1f, 0.6f) : new Color(1f, 0.5f, 0.5f, 0.6f);
        }
    }

    private Vector3 GetMouseWorldPosition()
    {
        if (mainCamera != null && mousePositionAction != null)
        {
            Vector2 mousePos = mousePositionAction.action.ReadValue<Vector2>();
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, mainCamera.nearClipPlane));
            worldPos.z = 0;
            return worldPos;
        }
        return Vector3.zero;
    }

    private void OnLeftClick(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        if (waitingConfirmation || !isInPreviewMode) return;

        bool canPlace = placeableObject.CanBePlaced();
        bool withinTowerLimit = BuildingSystem.current.CanPlaceTower();
        ShowConfirmationUI(canPlace && withinTowerLimit);
    }

    private void ShowConfirmationUI(bool canPlace)
    {
        waitingConfirmation = true;

        if (towerRegistered)
        {
            towerRegistered = false;
        }

        if (canPlace)
        {
            System.Action confirmAction = () =>
            {
                ConfirmPlacement();
            };
            System.Action cancelAction = () =>
            {
                CancelPlacement();
            };

            if (ConfirmationUi.instance != null)
            {
                ConfirmationUi.instance.ShowConfirmation(gameObject, transform.position, confirmAction, cancelAction);
            }
            else
            {
                confirmAction.Invoke();
            }
        }
        else
        {
            ShowCannotPlaceNotification();
            CancelPlacement();
        }
    }

    private void ConfirmPlacement()
    {
        placeableObject.Place();
        if (!towerRegistered)
        {
            BuildingSystem.current.RegisterPlacedTower(gameObject);
            towerRegistered = true;
        }

        if (sr != null)
        {
            Color color = sr.color;
            sr.color = new Color(color.r, color.g, color.b, 1f);
        }

        isInPreviewMode = false;
        Destroy(this);
        BuildingSystem.current.OnTowerPlacementFinished();
    }

    private void CancelPlacement()
    {
        if (!placeableObject.Placed)
        {
            Destroy(gameObject);
        }

        waitingConfirmation = false;
        BuildingSystem.current.OnTowerPlacementFinished();
    }

    /*private void LateUpdate()
    {
        bool mouseReleased = false;
        if (mouse != null)
        {
            mouseReleased = isMousePressed && !mouse.laftButton.isPressed;
            if (mouseReleased)
            {
                isMousePressed = false;
            }
        }
        else
        {
            mouseReleased = Input.GetMouseButtonUp(0);
        }
        if (mouseReleased && !waitingConfirmation)
        {
            bool canPlace = placeableObject.CanBePlaced();
            bool withinTowerLimit = BuildingSystem.current.CanPlaceTower();
            ShowConfirmationUI(canPlace && withinTowerLimit);
        }
    }*/

    private void ShowCannotPlaceNotification()
    {
        if (NotificationSystem.instance != null)
        {
            string reason = !BuildingSystem.current.CanPlaceTower() ?
                "Maximubn towers reached" :
                "Cannot place tower here";
            NotificationSystem.instance.ShowNotification(reason);
        }
    }
}
