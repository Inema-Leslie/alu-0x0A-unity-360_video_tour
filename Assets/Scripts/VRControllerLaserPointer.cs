using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class VRControllerLaserPointer : MonoBehaviour
{
    [Header("Raycast Settings")]
    public float maxDistance = 30f;
    public LayerMask hitLayers = ~0;

    [Header("Visuals")]
    public Color laserColor = new Color(0.2f, 0.75f, 1f, 0.65f);
    public Color hitColor = new Color(0.35f, 1f, 0.7f, 0.95f);
    public float lineWidth = 0.006f;

    private LineRenderer line;
    private GameObject dot;
    private Button currentHoveredButton;
    private Hotspot currentHoveredHotspot;

    void Awake()
    {
        SetupVisuals();
    }

    private void SetupVisuals()
    {
        line = GetComponent<LineRenderer>();
        if (line == null) line = gameObject.AddComponent<LineRenderer>();

        line.useWorldSpace = true;
        line.startWidth = lineWidth;
        line.endWidth = lineWidth * 0.5f;
        line.positionCount = 2;

        Shader spriteShader = Shader.Find("Sprites/Default") ?? Shader.Find("Unlit/Color");
        Material lineMat = new Material(spriteShader);
        line.material = lineMat;
        line.startColor = laserColor;
        line.endColor = new Color(laserColor.r, laserColor.g, laserColor.b, 0.05f);

        if (dot == null)
        {
            dot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            dot.name = "LaserPointerDot";
            var col = dot.GetComponent<Collider>();
            if (col != null) Destroy(col);
            dot.transform.localScale = Vector3.one * 0.025f;
            var dotRenderer = dot.GetComponent<MeshRenderer>();
            if (dotRenderer != null)
            {
                dotRenderer.material = lineMat;
                dotRenderer.material.color = hitColor;
            }
            dot.SetActive(false);
        }
    }

    void Update()
    {
        Vector3 origin = transform.position;
        Vector3 direction = transform.forward;
        Ray ray = new Ray(origin, direction);

        bool hasHit = false;
        Vector3 hitPoint = origin + direction * maxDistance;
        float closestDistance = maxDistance;

        Button hitButton = null;
        Hotspot hitHotspot = null;

        if (Physics.Raycast(ray, out RaycastHit physicsHit, maxDistance, hitLayers))
        {
            hasHit = true;
            closestDistance = physicsHit.distance;
            hitPoint = physicsHit.point;
            hitButton = physicsHit.collider.GetComponentInParent<Button>();
            hitHotspot = physicsHit.collider.GetComponentInParent<Hotspot>();
        }

        var canvases = FindObjectsByType<Canvas>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (var canvas in canvases)
        {
            if (canvas.renderMode != RenderMode.WorldSpace) continue;

            Plane canvasPlane = new Plane(canvas.transform.forward, canvas.transform.position);

            if (canvasPlane.Raycast(ray, out float enterDistance) && enterDistance < closestDistance)
            {
                Vector3 planeIntersection = ray.GetPoint(enterDistance);
                var buttonsInCanvas = canvas.GetComponentsInChildren<Button>();

                foreach (var btn in buttonsInCanvas)
                {
                    if (!btn.interactable || !btn.gameObject.activeInHierarchy) continue;

                    RectTransform rectTransform = btn.GetComponent<RectTransform>();
                    if (rectTransform != null)
                    {
                        Vector3 localPoint = rectTransform.InverseTransformPoint(planeIntersection);
                        if (rectTransform.rect.Contains(localPoint))
                        {
                            closestDistance = enterDistance;
                            hitPoint = planeIntersection;
                            hitButton = btn;
                            hitHotspot = btn.GetComponentInParent<Hotspot>();
                            hasHit = true;
                            break;
                        }
                    }
                }
            }
        }

        if (line != null)
        {
            line.SetPosition(0, origin);
            line.SetPosition(1, hitPoint);

            if (hasHit)
            {
                line.startColor = hitColor;
                line.endColor = hitColor;
                if (dot != null)
                {
                    dot.SetActive(true);
                    dot.transform.position = hitPoint;
                }
            }
            else
            {
                line.startColor = laserColor;
                line.endColor = new Color(laserColor.r, laserColor.g, laserColor.b, 0.05f);
                if (dot != null) dot.SetActive(false);
            }
        }

        if (hitButton != currentHoveredButton)
        {
            if (currentHoveredButton != null && EventSystem.current != null)
            {
                ExecuteEvents.Execute(currentHoveredButton.gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.pointerExitHandler);
            }
            if (hitButton != null && EventSystem.current != null)
            {
                ExecuteEvents.Execute(hitButton.gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.pointerEnterHandler);
            }
            currentHoveredButton = hitButton;
        }

        if (hitHotspot != currentHoveredHotspot)
        {
            if (currentHoveredHotspot != null && EventSystem.current != null)
            {
                currentHoveredHotspot.OnPointerExit(new PointerEventData(EventSystem.current));
            }
            if (hitHotspot != null && EventSystem.current != null)
            {
                hitHotspot.OnPointerEnter(new PointerEventData(EventSystem.current));
            }
            currentHoveredHotspot = hitHotspot;
        }

        if (IsClickTriggered())
        {
            if (currentHoveredButton != null)
            {
                currentHoveredButton.onClick?.Invoke();
            }
            if (currentHoveredHotspot != null)
            {
                currentHoveredHotspot.TriggerHotspot();
            }
        }
    }

    private bool IsClickTriggered()
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
                              controller.TryGetChildControl<UnityEngine.InputSystem.Controls.ButtonControl>("trigger") ??
                              controller.TryGetChildControl<UnityEngine.InputSystem.Controls.ButtonControl>("primaryButton");

                if (trigger != null && trigger.wasPressedThisFrame)
                {
                    return true;
                }
            }
        }

        return false;
    }
}