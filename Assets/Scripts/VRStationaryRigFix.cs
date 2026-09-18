using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Ensures the XR Rig stays locked at world origin (0, 0, 0) in 360 tour scenes.
/// Completely disables physics gravity, character controllers, and accidental locomotion falling.
/// Also auto-initializes all scenes at runtime to ensure VR UI raycasting and camera stability.
/// </summary>
[DefaultExecutionOrder(-100)]
public class VRStationaryRigFix : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void OnRuntimeInit()
    {
        FixActiveScene();
        SceneManager.sceneLoaded += (scene, mode) => FixActiveScene();
    }

    public static void FixActiveScene()
    {
        // 1. Disable all CharacterControllers to eliminate physics gravity falling
        var controllers = FindObjectsByType<CharacterController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var cc in controllers)
        {
            cc.enabled = false;
        }

        // 2. Disable Locomotion Gravity / Continuous Movement providers
        var allTransforms = FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var t in allTransforms)
        {
            string n = t.name.ToLowerInvariant();
            if (n == "gravity" || n == "climb" || n == "jump" || n == "move")
            {
                if (t.parent != null && t.parent.name.ToLowerInvariant().Contains("locomotion"))
                {
                    t.gameObject.SetActive(false);
                }
            }
        }

        // 3. Find and lock XR Origin
        GameObject rig = GameObject.Find("XR Origin (XR Rig)");
        if (rig == null)
        {
            foreach (var t in allTransforms)
            {
                if (t.name.Contains("XR Origin") || t.name.Contains("XR Rig"))
                {
                    rig = t.gameObject;
                    break;
                }
            }
        }

        if (rig != null)
        {
            rig.transform.position = Vector3.zero;
            if (rig.GetComponent<VRStationaryRigFix>() == null)
            {
                rig.AddComponent<VRStationaryRigFix>();
            }
        }

        // 4. Ensure EventSystem has XRUIInputModule
        var es = FindAnyObjectByType<EventSystem>();
        if (es != null)
        {
            Type xruiType = Type.GetType("UnityEngine.XR.Interaction.Toolkit.UI.XRUIInputModule, Unity.XR.Interaction.Toolkit");
            if (xruiType != null && es.GetComponent(xruiType) == null)
            {
                es.gameObject.AddComponent(xruiType);
            }
        }

        // 5. Ensure all WorldSpace canvases have GraphicRaycaster and TrackedDeviceGraphicRaycaster
        Type raycasterType = Type.GetType("UnityEngine.XR.Interaction.Toolkit.UI.TrackedDeviceGraphicRaycaster, Unity.XR.Interaction.Toolkit");
        var canvases = FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        Camera mainCam = Camera.main != null ? Camera.main : FindAnyObjectByType<Camera>();

        foreach (var c in canvases)
        {
            if (c.renderMode == RenderMode.WorldSpace)
            {
                if (mainCam != null && c.worldCamera == null)
                {
                    c.worldCamera = mainCam;
                }
                if (c.GetComponent<GraphicRaycaster>() == null)
                {
                    c.gameObject.AddComponent<GraphicRaycaster>();
                }
                if (raycasterType != null && c.GetComponent(raycasterType) == null)
                {
                    c.gameObject.AddComponent(raycasterType);
                }
            }
        }

        // 6. If in MainMenuScene, ensure MainMenuCanvas has MainMenuVRSetup
        string sceneName = SceneManager.GetActiveScene().name.ToLowerInvariant();
        if (sceneName.Contains("mainmenu"))
        {
            var menuCanvas = GameObject.Find("MainMenuCanvas");
            if (menuCanvas != null)
            {
                if (menuCanvas.GetComponent<MainMenuVRSetup>() == null)
                {
                    menuCanvas.AddComponent<MainMenuVRSetup>();
                }
            }
        }

        // 7. Ensure Left and Right Controllers have VRControllerLaserPointer attached
        foreach (var t in allTransforms)
        {
            string n = t.name.ToLowerInvariant();
            if ((n.Contains("left") || n.Contains("right")) && (n.Contains("controller") || n.Contains("hand")))
            {
                if (t.GetComponent<VRControllerLaserPointer>() == null)
                {
                    t.gameObject.AddComponent<VRControllerLaserPointer>();
                }
            }
        }

        // 8. Ensure all Buttons in the scene have VRInteractiveButton and BoxCollider
        var buttons = FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var btn in buttons)
        {
            if (btn.GetComponent<VRInteractiveButton>() == null)
            {
                btn.gameObject.AddComponent<VRInteractiveButton>();
            }
        }
    }

    void Awake()
    {
        CharacterController cc = GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        Transform locomotion = transform.Find("Locomotion");
        if (locomotion != null)
        {
            Transform gravityObj = locomotion.Find("Gravity");
            if (gravityObj != null) gravityObj.gameObject.SetActive(false);

            Transform moveObj = locomotion.Find("Move");
            if (moveObj != null) moveObj.gameObject.SetActive(false);

            Transform climbObj = locomotion.Find("Climb");
            if (climbObj != null) climbObj.gameObject.SetActive(false);

            Transform jumpObj = locomotion.Find("Jump");
            if (jumpObj != null) jumpObj.gameObject.SetActive(false);
        }

        transform.position = Vector3.zero;
    }

    void Update()
    {
        if (transform.position != Vector3.zero)
        {
            transform.position = Vector3.zero;
        }
    }

    void LateUpdate()
    {
        if (transform.position != Vector3.zero)
        {
            transform.position = Vector3.zero;
        }
    }
}
