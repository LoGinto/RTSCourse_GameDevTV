using System;
using GameDevTV.RTS.EventBus;
using GameDevTV.RTS.Events;
using GameDevTV.RTS.Units;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;


namespace GameDevTV.RTS.Player{
public class PlayerInput : MonoBehaviour
{
    [SerializeField] private Rigidbody cameraTarget;
    [SerializeField] private CinemachineCamera cinemachineCamera;
    [SerializeField] LayerMask selectableUnitsLayers,groundLayer;
    [SerializeField] private new Camera camera;

    [SerializeField] CameraConfig cameraConfig;
    [SerializeField] RectTransform selectionBox;
    ISelectable selectedUnit;

    private RTSControls controls;
    private CinemachineFollow cinemachineFollow;

    private Vector3 startFlatOffset;
    Vector2 startingMousePosition; 
    private float yaw;
    private float currentHeight;
    private float targetHeight;
    private float startHeight;

    private void Awake()
    {
        controls = new RTSControls();
        if(camera == null)
            {
                camera = Camera.main;
            }
        if (cinemachineCamera == null)
        {
            cinemachineCamera = FindFirstObjectByType<CinemachineCamera>();
        }
        if (!cinemachineCamera.TryGetComponent(out cinemachineFollow))
        {
            Debug.LogError("No cinemachine follow");
            enabled = false;
            return;
        }

        Vector3 start = cinemachineFollow.FollowOffset;
        startFlatOffset = new Vector3(start.x, 0f, start.z);
        currentHeight = start.y;
        targetHeight = start.y;
        startHeight = start.y;
        Bus<UnitSelectedEvent>.OnEvent += HandleUnitSelected;
        Bus<UnitDeselectedEvent>.OnEvent += HandleUnitDeselected;
    }

        private void HandleUnitDeselected(UnitDeselectedEvent evt)
        {
            selectedUnit = null;
        }

        private void HandleUnitSelected(UnitSelectedEvent evt)
        {
            //handle selection
             if (selectedUnit != null)
            {
                selectedUnit.Deselect();
            }
             selectedUnit = evt.Unit;
        }

        private void OnEnable()
    {
        controls.Camera.Enable();
    }

    private void OnDisable()
    {
        controls.Camera.Disable();
    }
        void OnDestroy()
        {
            Bus<UnitSelectedEvent>.OnEvent -= HandleUnitSelected;
            Bus<UnitDeselectedEvent>.OnEvent -= HandleUnitDeselected;
        }

        private void Update()
    {
        HandleRotation();
        HandleZooming();
        HandlePanning();
        HandleLeftClick();
        HandleRightClick();
        HandleDragSelect();
        ApplyFollowOffset();
    }
    void HandleDragSelect()
    {
            if(selectionBox == null){return;}
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                //enable UI 
                //store start position
                selectionBox.gameObject.SetActive(true);
                startingMousePosition = Mouse.current.position.ReadValue();
            }
            else if(Mouse.current.leftButton.isPressed && !Mouse.current.leftButton.wasPressedThisFrame)
            {
                //resize selection box
                ResizeSelectionBox();
            }
            else if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                //select new units
                //deselect non-included units
                //disable Ui
                selectionBox.gameObject.SetActive(false);
            }
    }

        private void ResizeSelectionBox()
        {
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            float width = mousePosition.x - startingMousePosition.x;
            float height = mousePosition.y - startingMousePosition.y;
            selectionBox.anchoredPosition = startingMousePosition + new Vector2(width / 2, height / 2);
            selectionBox.sizeDelta = new Vector2(Mathf.Abs(width), Mathf.Abs(height));
        }

        void HandleRightClick()
    {
        if(selectedUnit == null || selectedUnit is not IMovable movable){return;}
        if (camera == null) { return; }
        if (!Mouse.current.rightButton.wasReleasedThisFrame) { return; }
        Ray cameraRay = camera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(cameraRay, out RaycastHit hit, float.MaxValue,groundLayer))
        {
            movable.MoveTo(hit.point);
        }
    }
void HandleLeftClick()
{
    if (camera == null) { return; }
    if (!Mouse.current.leftButton.wasReleasedThisFrame) { return; }

    Ray cameraRay = camera.ScreenPointToRay(Mouse.current.position.ReadValue());

    if (Physics.Raycast(cameraRay, out RaycastHit hit, float.MaxValue, selectableUnitsLayers)
        && hit.collider.GetComponentInParent<ISelectable>() is ISelectable clicked)
    {
        // Clicked the already-selected unit: toggle it off
        if (selectedUnit == clicked)
        {
            selectedUnit.Deselect();
            return;
        }

        // Clicked a different unit: swap selection
        selectedUnit?.Deselect();
        clicked.Select();
        return;
    }

    // Clicked nothing selectable: clear selection
    if (selectedUnit != null)
    {
        selectedUnit.Deselect();
        selectedUnit = null;
    }
}

    private void HandleRotation()
    {
        float rotateAmt = controls.Camera.Rotate.ReadValue<float>();
        yaw = Mathf.Repeat(yaw + rotateAmt * cameraConfig.RotationSpeed* Time.deltaTime, 360f);
    }

    private void HandleZooming()
    {
        float zoomAmt = Mathf.Clamp(controls.Camera.Zoom.ReadValue<Vector2>().y, -1f, 1f);

        if (Mathf.Abs(zoomAmt) > 0.01f)
        {
            targetHeight = Mathf.Clamp(targetHeight - zoomAmt * cameraConfig.ZoomStep, cameraConfig.MaxZoomIn, cameraConfig.MaxZoomOut);
        }

        currentHeight = Mathf.Lerp(currentHeight, targetHeight, 1f - Mathf.Exp(-cameraConfig.ZoomSpeed * Time.deltaTime));
    }

private void HandlePanning()
{
    Vector2 input = controls.Camera.Move.ReadValue<Vector2>();
    Vector2 mouseMove = GetMouseMoveAmount();

    Vector3 move = new Vector3(
        input.x * cameraConfig.KeyboardRightPanSpeed + mouseMove.x,
        0f,
        input.y * cameraConfig.KeyboardForwardPanSpeed + mouseMove.y);

    move = Quaternion.Euler(0f, yaw, 0f) * move;
    cameraTarget.linearVelocity = move;
}

 private void ApplyFollowOffset()
{
    Vector3 flat = Quaternion.Euler(0f, yaw, 0f) * startFlatOffset;
    float scale = currentHeight / startHeight;
    cinemachineFollow.FollowOffset = new Vector3(flat.x * scale, currentHeight, flat.z * scale);
}
    Vector2 GetMouseMoveAmount()
    {
        Vector2 moveAmt = Vector2.zero;

        // Edge panning can be switched off in the config
        if (!cameraConfig.EnableEdgePan) { return moveAmt; }

        // Mouse position in pixels; (0,0) is the bottom-left corner of the screen
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        int screenWidth = Screen.width;
        int screenHeight = Screen.height;

        // Left edge: close to x = 0
        if (mousePosition.x <= cameraConfig.EdgePanSize)
        {
            moveAmt.x -= cameraConfig.MousePanSpeed;
        }

        // Right edge: close to x = screen width
        if (mousePosition.x >= screenWidth - cameraConfig.EdgePanSize)
        {
            moveAmt.x += cameraConfig.MousePanSpeed;
        }

        // Top edge: close to y = screen height
        if (mousePosition.y >= screenHeight - cameraConfig.EdgePanSize)
        {
            moveAmt.y += cameraConfig.MousePanSpeed;
        }

        // Bottom edge: close to y = 0
        if (mousePosition.y <= cameraConfig.EdgePanSize)
        {
            moveAmt.y -= cameraConfig.MousePanSpeed;
        }

        return moveAmt;
    }
}
}