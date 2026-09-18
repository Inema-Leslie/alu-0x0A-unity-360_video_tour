using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

/// <summary>
/// Ensures VR buttons can be clicked in ALL possible ways:
/// 1. Direct Physical Touch / Poke (controller or hand entering button volume)
/// 2. Controller Laser Pointer Raycast Click
/// 3. Head-Gaze Aim + Controller Trigger Press
/// 4. Head-Gaze Dwell (looking at button for 1.3 seconds auto-clicks)
/// </summary>
public class VRInteractiveButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Gaze Interaction")]
    public bool enableGaze = true;
    public float gazeDwellDuration = 1.3f;

    [Header("Touch Interaction")]
    public bool enableDirectTouch = true;
    public float touchDistanceThreshold = 0.20f;

    private Button button;
    private Hotspot hotspot;
    private RectTransform rectTransform;
    private BoxCollider boxCollider;
    private Transform mainCameraTransform;

    private bool isHovered = false;
    private float gazeTimer = 0f;
    private bool hasTriggered = false;

    private Transform leftController;
    private Transform rightController;

    void Awake()
    {
        button = GetComponent<Button>();
        hotspot = GetComponent<Hotspot>();
        rectTransform = GetComponent<RectTransform>();

        // Ensure BoxCollider exists for physical poke & raycast hits
        boxCollider = GetComponent<BoxCollider>();
        if (boxCollider == null)
        {
            boxCollider = gameObject.AddComponent<BoxCollider>();
        }
        boxCollider.isTrigger = true;
        UpdateColliderSize();
    }

    void Start()
    {
        Camera cam = Camera.main != null ? Camera.main : FindAnyObjectByType<Camera>();
        if (cam != null) mainCameraTransform = cam.transform;

        FindControllers();
    }

    private void FindControllers()
    {
        var allTransforms = FindObjectsByType<Transform>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (var t in allTransforms)
        {
            string n = t.name.ToLowerInvariant();
            if (n.Contains("left") && (n.Contains("controller") || n.Contains("hand")))
            {
                leftController = t;
            }
            else if (n.Contains("right") && (n.Contains("controller") || n.Contains("hand")))
            {
                rightController = t;
            }
        }
    }

    private void UpdateColliderSize()
    {
        if (boxCollider != null && rectTransform != null)
        {
            Vector2 size = rectTransform.rect.size;
            if (size.x <= 0 || size.y <= 0) size = new Vector2(300, 100);
            boxCollider.size = new Vector3(size.x, size.y, 40f);
            boxCollider.center = new Vector3(0, 0, 0);
        }
    }

    void Update()
    {
        if (hasTriggered) return;

        if (mainCameraTransform == null)
        {
            Camera cam = Camera.main != null ? Camera.main : FindAnyObjectByType<Camera>();
            if (cam != null) mainCameraTransform = cam.transform;
            if (mainCameraTransform == null) return;
        }

        // 1. Check Gaze (Ray from center eye)
        Ray gazeRay = new Ray(mainCameraTransform.position, mainCameraTransform.forward);
        bool isLookingAtMe = false;

        if (boxCollider != null && boxCollider.Raycast(gazeRay, out RaycastHit hit, 15f))
        {
            isLookingAtMe = true;
        }

        // 2. Check Physical Proximity of Controllers (Touch reach)
        bool isTouching = false;
        if (enableDirectTouch)
        {
            if (leftController != null && Vector3.Distance(transform.position, leftController.position) < touchDistanceThreshold)
            {
                isTouching = true;
            }
            if (rightController != null && Vector3.Distance(transform.position, rightController.position) < touchDistanceThreshold)
            {
                isTouching = true;
            }
        }

        bool activeHover = isLookingAtMe || isTouching || isHovered;

        if (activeHover)
        {
            // Check for instant trigger press while looking or touching
            if (IsAnyTriggerPressed())
            {
                TriggerClick();
                return;
            }

            // Gaze Dwell auto-click
            if (enableGaze)
            {
                gazeTimer += Time.deltaTime;
                if (gazeTimer >= gazeDwellDuration)
                {
                    TriggerClick();
                    return;
                }
            }
        }
        else
        {
            gazeTimer = 0f;
        }
    }

    private bool IsAnyTriggerPressed()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) return true;

        if (Gamepad.current != null)
        {
            if (Gamepad.current.buttonSouth.wasPressedThisFrame ||
                Gamepad.current.rightTrigger.wasPressedThisFrame ||
                Gamepad.current.leftTrigger.wasPressedThisFrame)
            {
                return true;
            }
        }

        foreach (var device in InputSystem.devices)
        {
            if (device is UnityEngine.InputSystem.XR.XRController controller)
            {
                var trigger = controller.TryGetChildControl<UnityEngine.InputSystem.Controls.ButtonControl>("triggerPressed") ??
                              controller.TryGetChildControl<UnityEngine.InputSystem.Controls.ButtonControl>("triggerButton");
                if (trigger != null && trigger.wasPressedThisFrame) return true;

                var pri = controller.TryGetChildControl<UnityEngine.InputSystem.Controls.ButtonControl>("primaryButton");
                if (pri != null && pri.wasPressedThisFrame) return true;

                var sec = controller.TryGetChildControl<UnityEngine.InputSystem.Controls.ButtonControl>("secondaryButton");
                if (sec != null && sec.wasPressedThisFrame) return true;
            }
        }

        return false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;
        string n = other.name.ToLowerInvariant();
        if (n.Contains("controller") || n.Contains("hand") || n.Contains("interactor") || n.Contains("poke"))
        {
            TriggerClick();
        }
    }

    public void TriggerClick()
    {
        if (hasTriggered) return;
        hasTriggered = true;

        if (button != null)
        {
            button.onClick?.Invoke();
        }

        if (hotspot != null)
        {
            hotspot.TriggerHotspot();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        gazeTimer = 0f;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        TriggerClick();
    }
}
