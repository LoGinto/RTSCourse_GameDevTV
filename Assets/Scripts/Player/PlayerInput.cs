using System.Collections.Generic;
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
    List<ISelectable> selectedUnits = new(12);
    HashSet<AbstractUnit> aliveUnits  = new (100);
    HashSet<AbstractUnit> addedUnits = new(25);
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
        Bus<UnitpSpawnedEvent>.OnEvent += HandleUnitSpawned;
    }

        private void HandleUnitSpawned(UnitpSpawnedEvent evt)
        {
            aliveUnits.Add(evt.Unit);
        }

        private void HandleUnitDeselected(UnitDeselectedEvent evt)
        {
            selectedUnits.Remove(evt.Unit);
        }

        private void HandleUnitSelected(UnitSelectedEvent evt)
        {
            //handle selection
            selectedUnits.Add(evt.Unit);
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
            Bus<UnitpSpawnedEvent>.OnEvent  -= HandleUnitSpawned;
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
                HandleMouseDown();
            }
            else if(Mouse.current.leftButton.isPressed && !Mouse.current.leftButton.wasPressedThisFrame)
            {
                //resize selection box
              HandleMouseDrag();
            }
            else if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                HandleMouseUp();
            }
        }

        private void HandleMouseUp()
        {
            if (controls.Camera.MultiSelect.IsPressed())    
            {
                ShiftSelect();
            }
            else
            {
                DeselectAllUnits();
                HandleLeftClick();
                foreach (AbstractUnit unit in addedUnits)
                {
                    unit.Select();
                }
            }
            selectionBox.gameObject.SetActive(false);
        }

        private void HandleMouseDrag()
        {
            Bounds selectionBoxBounds = ResizeSelectionBox();
            foreach (AbstractUnit unit in aliveUnits)
            {
                Vector2 unitPosition = camera.WorldToScreenPoint(unit.transform.position);
                if (selectionBoxBounds.Contains(unitPosition))
                {
                    addedUnits.Add(unit);
                }
            }
        }

        private void HandleMouseDown()
        {
            selectionBox.sizeDelta = Vector2.zero;
            selectionBox.gameObject.SetActive(true);
            startingMousePosition = Mouse.current.position.ReadValue();
            addedUnits.Clear();
        }

        void ShiftSelect()
        {
             foreach (AbstractUnit unit in addedUnits)
                {
                    if (selectedUnits.Contains(unit))
                        unit.Deselect();
                    else
                        unit.Select();
                }
        }
    private void DeselectAllUnits()
        {
            ISelectable[] currentlySelectedUnits = selectedUnits.ToArray();
            foreach(ISelectable selectable in currentlySelectedUnits)
            {
                selectable.Deselect();
            }
        }

        private Bounds ResizeSelectionBox()
        {
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            float width = mousePosition.x - startingMousePosition.x;
            float height = mousePosition.y - startingMousePosition.y;
            selectionBox.anchoredPosition = startingMousePosition + new Vector2(width / 2, height / 2);
            selectionBox.sizeDelta = new Vector2(Mathf.Abs(width), Mathf.Abs(height));
            return new Bounds(selectionBox.anchoredPosition,selectionBox.sizeDelta);
        }

        void HandleRightClick()
    {
        if(selectedUnits.Count == 0 ){return;}
        if (camera == null) { return; }
        if (!Mouse.current.rightButton.wasReleasedThisFrame) { return; }
        Ray cameraRay = camera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(cameraRay, out RaycastHit hit, float.MaxValue,groundLayer))
        {
            List<AbstractUnit> abstractUnits = new List<AbstractUnit>(selectedUnits.Count);
            foreach(ISelectable selectable in selectedUnits)
                {
                    if(selectable is AbstractUnit unit)
                    {
                        abstractUnits.Add(unit);
                    }
                }
            int unitsOnLayer = 0;
            int maxUnitsOnLayer = 1;
            float circleRadius = 0;
            float radialOffset = 0;
            foreach(AbstractUnit unit in abstractUnits)
                {
                    Vector3 targetPosition = new Vector3(
                        hit.point.x + circleRadius*Mathf.Cos(radialOffset * unitsOnLayer), 
                        hit.point.y,
                        hit.point.z + circleRadius * Mathf.Sin(radialOffset * unitsOnLayer)
                    );
                    unit.MoveTo(targetPosition);
                    unitsOnLayer++;
                    if (unitsOnLayer >= maxUnitsOnLayer)
                    {
                        unitsOnLayer = 0;
                        circleRadius  += unit.AgentRadius * 3.5f;
                        maxUnitsOnLayer = Mathf.FloorToInt(2*Mathf.PI*circleRadius/(unit.AgentRadius * 2));
                        radialOffset = 2*Mathf.PI / maxUnitsOnLayer;
                    }
                }
            // foreach(ISelectable selectable in selectedUnits)
            //     {
            //         if(selectable is IMovable movable){
            //             movable.MoveTo(hit.point);
            //         }
            //     }
        }
    }
    void HandleLeftClick()
    {
        if (camera == null) { return; }

        // Big box means it was a drag, not a click
        if (selectionBox.sizeDelta.sqrMagnitude > 100f) { return; }

        Ray cameraRay = camera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(cameraRay, out RaycastHit hit, float.MaxValue, selectableUnitsLayers))
        {
            AbstractUnit unit = hit.collider.GetComponentInParent<AbstractUnit>();
            if (unit != null)
            {
                addedUnits.Add(unit);
            }
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