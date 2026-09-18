using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Ensures the Main Menu canvas is always clearly visible and interactable in VR on Meta Quest.
/// Dynamically anchors the menu at eye level in front of the VR camera,
/// and smoothly glides to face the player if they turn their head away.
/// </summary>
public class MainMenuVRSetup : MonoBehaviour
{
    [Header("VR Placement")]
    public float distance = 0.95f;
    public float verticalOffset = -0.05f;
    public float followSpeed = 3.5f;
    public float maxViewAngle = 28.0f;

    private Transform camTransform;
    private Canvas canvas;
    private bool isReady = false;

    void Awake()
    {
        canvas = GetComponent<Canvas>();
        if (canvas != null)
        {
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.planeDistance = distance;
            transform.localScale = Vector3.one * 0.0011f;
        }

        // Ensure raycasters exist
        if (GetComponent<GraphicRaycaster>() == null)
        {
            gameObject.AddComponent<GraphicRaycaster>();
        }

        var raycasterType = System.Type.GetType("UnityEngine.XR.Interaction.Toolkit.UI.TrackedDeviceGraphicRaycaster, Unity.XR.Interaction.Toolkit");
        if (raycasterType != null && GetComponent(raycasterType) == null)
        {
            gameObject.AddComponent(raycasterType);
        }
    }

    IEnumerator Start()
    {
        FindCamera();
        SnapToPlayerView();

        // Frame 1: Wait for OpenXR TrackedPoseDriver to acquire initial HMD pose
        yield return null;
        FindCamera();
        SnapToPlayerView();

        // Wait a few more frames to ensure tracking is stable
        yield return new WaitForSeconds(0.12f);
        FindCamera();
        SnapToPlayerView();
        isReady = true;
    }

    void LateUpdate()
    {
        if (camTransform == null)
        {
            FindCamera();
            if (camTransform == null) return;
        }

        Vector3 forwardFlat = camTransform.forward;
        forwardFlat.y = 0;
        if (forwardFlat.sqrMagnitude < 0.001f) forwardFlat = Vector3.forward;
        forwardFlat.Normalize();

        Vector3 targetPos = camTransform.position + forwardFlat * distance;
        targetPos.y = camTransform.position.y + verticalOffset;

        if (!isReady)
        {
            transform.position = targetPos;
            transform.rotation = Quaternion.LookRotation(forwardFlat, Vector3.up);
            return;
        }

        // Check if user turned their head away from the menu
        Vector3 toMenu = transform.position - camTransform.position;
        toMenu.y = 0;
        float angle = Vector3.Angle(forwardFlat, toMenu);

        // If user looks away more than maxViewAngle or drifts too far, smoothly bring menu in front
        if (angle > maxViewAngle || Vector3.Distance(transform.position, targetPos) > 0.8f)
        {
            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * followSpeed);
            Vector3 lookDir = transform.position - camTransform.position;
            if (lookDir.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDir, Vector3.up), Time.deltaTime * followSpeed);
            }
        }
    }

    public void SnapToPlayerView()
    {
        if (camTransform == null) FindCamera();
        if (camTransform == null) return;

        Vector3 forwardFlat = camTransform.forward;
        forwardFlat.y = 0;
        if (forwardFlat.sqrMagnitude < 0.001f) forwardFlat = Vector3.forward;
        forwardFlat.Normalize();

        transform.position = camTransform.position + forwardFlat * distance;
        transform.position = new Vector3(transform.position.x, camTransform.position.y + verticalOffset, transform.position.z);
        transform.rotation = Quaternion.LookRotation(forwardFlat, Vector3.up);

        if (canvas != null)
        {
            canvas.renderMode = RenderMode.WorldSpace;
            Camera c = camTransform.GetComponent<Camera>();
            if (c != null) canvas.worldCamera = c;
        }
    }

    private void FindCamera()
    {
        Camera cam = Camera.main != null ? Camera.main : FindAnyObjectByType<Camera>();
        if (cam != null)
        {
            camTransform = cam.transform;
            if (canvas != null && canvas.worldCamera == null)
            {
                canvas.worldCamera = cam;
            }
        }
    }
}
