using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Video;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

/// <summary>
/// Comprehensive automated configurator that builds and validates all 3 VR scenes:
/// MainMenuScene, IntranetTourScene, and CustomCampusTourScene.
/// Accessible via Unity Editor menu: 'VR Tour/Run Complete Setup'.
/// </summary>
public static class CompleteSceneConfigurator
{
    private const string MAIN_MENU_PATH = "Assets/Scenes/MainMenuScene.unity";
    private const string INTRANET_PATH = "Assets/Scenes/IntranetTourScene.unity";
    private const string CAMPUS_PATH = "Assets/Scenes/CustomCampusTourScene.unity";

    /// <summary>
    /// Executes the full configuration and linking of all 3 tour scenes.
    /// </summary>
    [MenuItem("VR Tour/Run Complete Setup", false, 10)]
    public static void RunCompleteSetup()
    {
        Debug.Log("=== Running Complete VR Tour Setup ===");

        EnsureDirectories();
        SyncSourceScenes();

        SetupMainMenuScene();
        SetupIntranetTourScene();
        SetupCustomCampusTourScene();

        RegisterBuildSettings();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("=== All 3 Scenes Fully Configured & Connected Successfully! ===");
    }

    private static void EnsureDirectories()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
        {
            AssetDatabase.CreateFolder("Assets", "Scenes");
        }
    }

    private static void SyncSourceScenes()
    {
        // Ensure MainMenuScene exists
        if (!File.Exists(MAIN_MENU_PATH))
        {
            if (File.Exists("Assets/MainMenu scene.unity"))
                AssetDatabase.CopyAsset("Assets/MainMenu scene.unity", MAIN_MENU_PATH);
        }

        // Ensure IntranetTourScene exists
        if (!File.Exists(INTRANET_PATH))
        {
            if (File.Exists("Assets/360VideoTour.unity"))
                AssetDatabase.CopyAsset("Assets/360VideoTour.unity", INTRANET_PATH);
        }

        // Ensure CustomCampusTourScene exists
        if (!File.Exists(CAMPUS_PATH))
        {
            if (File.Exists("Assets/CampusTour.unity"))
                AssetDatabase.CopyAsset("Assets/CampusTour.unity", CAMPUS_PATH);
        }

        AssetDatabase.Refresh();
    }

    private const string XR_ORIGIN_PREFAB = "Assets/Samples/XR Interaction Toolkit/3.4.1/Starter Assets/Prefabs/XR Origin (XR Rig).prefab";

    private static void EnsureVROrigin(UnityEngine.SceneManagement.Scene scene)
    {
        GameObject rig = GameObject.Find("XR Origin (XR Rig)");
        if (rig == null)
        {
            foreach (var r in scene.GetRootGameObjects())
            {
                if (r.name.Contains("XR Origin") || r.name.Contains("XR Rig"))
                {
                    rig = r;
                    break;
                }
            }
        }

        if (rig == null)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(XR_ORIGIN_PREFAB);
            if (prefab != null)
            {
                foreach (var oc in UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                {
                    if (oc.transform.parent == null)
                    {
                        UnityEngine.Object.DestroyImmediate(oc.gameObject);
                    }
                }

                rig = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
                rig.name = "XR Origin (XR Rig)";
            }
        }

        if (rig != null)
        {
            rig.transform.position = Vector3.zero;
            rig.transform.rotation = Quaternion.identity;

            // Disable CharacterController and Gravity locomotion so the rig never falls
            var cc = rig.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            Transform locomotion = rig.transform.Find("Locomotion");
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

            if (rig.GetComponent<VRStationaryRigFix>() == null)
            {
                rig.AddComponent<VRStationaryRigFix>();
            }

            Camera cam = Camera.main;
            if (cam == null) cam = rig.GetComponentInChildren<Camera>(true);
            if (cam != null)
            {
                if (cam.GetComponent<SimpleLook>() == null)
                    cam.gameObject.AddComponent<SimpleLook>();
            }
        }
        else
        {
            Camera cam = Camera.main != null ? Camera.main : UnityEngine.Object.FindAnyObjectByType<Camera>();
            if (cam != null)
            {
                cam.transform.position = Vector3.zero;
                cam.transform.rotation = Quaternion.identity;
                var tpdType = Type.GetType("UnityEngine.InputSystem.XR.TrackedPoseDriver, Unity.InputSystem");
                if (tpdType != null && cam.GetComponent(tpdType) == null)
                    cam.gameObject.AddComponent(tpdType);
                if (cam.GetComponent<SimpleLook>() == null)
                    cam.gameObject.AddComponent<SimpleLook>();
            }
        }

        Camera mainCam = Camera.main;
        if (mainCam == null && rig != null) mainCam = rig.GetComponentInChildren<Camera>(true);

        Type raycasterType = Type.GetType("UnityEngine.XR.Interaction.Toolkit.UI.TrackedDeviceGraphicRaycaster, Unity.XR.Interaction.Toolkit");
        foreach (var c in UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (c.renderMode == RenderMode.WorldSpace)
            {
                if (mainCam != null) c.worldCamera = mainCam;

                if (c.GetComponent<GraphicRaycaster>() == null)
                    c.gameObject.AddComponent<GraphicRaycaster>();

                if (raycasterType != null && c.GetComponent(raycasterType) == null)
                {
                    c.gameObject.AddComponent(raycasterType);
                }
            }
        }

        // Ensure EventSystem and XRUIInputModule exist in scene
        var es = UnityEngine.Object.FindAnyObjectByType<EventSystem>();
        if (es == null)
        {
            var esGo = new GameObject("EventSystem", typeof(EventSystem));
            es = esGo.GetComponent<EventSystem>();
        }
        Type xruiType = Type.GetType("UnityEngine.XR.Interaction.Toolkit.UI.XRUIInputModule, Unity.XR.Interaction.Toolkit");
        if (xruiType != null && es.GetComponent(xruiType) == null)
        {
            es.gameObject.AddComponent(xruiType);
        }

        // Ensure controllers have laser pointers
        var allT = UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var t in allT)
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
    }

    private static void SetupMainMenuScene()
    {
        var scene = EditorSceneManager.OpenScene(MAIN_MENU_PATH, OpenSceneMode.Single);
        Debug.Log($"Configuring {scene.name}...");

        EnsureVROrigin(scene);

        Camera cam = Camera.main != null ? Camera.main : UnityEngine.Object.FindAnyObjectByType<Camera>();
        if (cam != null)
        {
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.04f, 0.07f, 0.12f, 1f);
            var fader = cam.GetComponent<VRScreenFader>();
            if (fader != null)
            {
                UnityEngine.Object.DestroyImmediate(fader);
            }
            var fadeCanvas = GameObject.Find("VRFadeCanvas");
            if (fadeCanvas != null) UnityEngine.Object.DestroyImmediate(fadeCanvas);
        }

        RenderSettings.skybox = null;

        // 1. Create 360 Photosphere background for Main Menu so there is never a dark abyss
        GameObject bgSphere = GameObject.Find("MenuBackgroundSphere");
        if (bgSphere == null)
        {
            bgSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bgSphere.name = "MenuBackgroundSphere";
            var col = bgSphere.GetComponent<Collider>();
            if (col != null) UnityEngine.Object.DestroyImmediate(col);
        }
        bgSphere.transform.position = Vector3.zero;
        bgSphere.transform.localScale = new Vector3(-50f, 50f, 50f);
        var bgRenderer = bgSphere.GetComponent<MeshRenderer>();
        if (bgRenderer != null)
        {
            Material mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Room1_Mat.mat");
            if (mat != null) bgRenderer.sharedMaterial = mat;
        }

        // Ensure EventSystem
        var es = UnityEngine.Object.FindAnyObjectByType<EventSystem>();
        if (es == null)
        {
            new GameObject("EventSystem", typeof(EventSystem), typeof(UnityEngine.InputSystem.UI.InputSystemUIInputModule));
        }

        // Find or create MainMenuCanvas
        Canvas canvas = UnityEngine.Object.FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            var cObj = new GameObject("MainMenuCanvas");
            canvas = cObj.AddComponent<Canvas>();
        }

        canvas.renderMode = RenderMode.WorldSpace;
        if (cam != null) canvas.worldCamera = cam;
        canvas.transform.position = new Vector3(0, 1.35f, 0.95f);
        canvas.transform.rotation = Quaternion.identity;
        canvas.transform.localScale = new Vector3(0.0011f, 0.0011f, 0.0011f);

        RectTransform canvasRt = canvas.GetComponent<RectTransform>();
        canvasRt.sizeDelta = new Vector2(920, 640);

        if (canvas.GetComponent<GraphicRaycaster>() == null)
            canvas.gameObject.AddComponent<GraphicRaycaster>();

        var raycasterType = Type.GetType("UnityEngine.XR.Interaction.Toolkit.UI.TrackedDeviceGraphicRaycaster, Unity.XR.Interaction.Toolkit");
        if (raycasterType != null && canvas.GetComponent(raycasterType) == null)
        {
            canvas.gameObject.AddComponent(raycasterType);
        }

        if (canvas.GetComponent<MainMenuVRSetup>() == null)
        {
            canvas.gameObject.AddComponent<MainMenuVRSetup>();
        }

        TMP_FontAsset fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");

        // Find or create MenuPanel
        Transform panelT = canvas.transform.Find("MenuPanel");
        GameObject panelObj;
        if (panelT == null)
        {
            panelObj = new GameObject("MenuPanel");
            panelObj.transform.SetParent(canvas.transform, false);
        }
        else
        {
            panelObj = panelT.gameObject;
        }

        RectTransform panelRt = panelObj.GetComponent<RectTransform>();
        if (panelRt == null) panelRt = panelObj.AddComponent<RectTransform>();
        panelRt.anchorMin = Vector2.zero;
        panelRt.anchorMax = Vector2.one;
        panelRt.sizeDelta = Vector2.zero;

        Image panelImg = panelObj.GetComponent<Image>();
        if (panelImg == null) panelImg = panelObj.AddComponent<Image>();
        panelImg.color = new Color(0.07f, 0.10f, 0.16f, 0.96f);

        Outline panelOutline = panelObj.GetComponent<Outline>();
        if (panelOutline == null) panelOutline = panelObj.AddComponent<Outline>();
        panelOutline.effectColor = new Color(0.25f, 0.85f, 1f, 0.75f);
        panelOutline.effectDistance = new Vector2(3, 3);

        // Clear existing children inside panel
        for (int i = panelObj.transform.childCount - 1; i >= 0; i--)
        {
            UnityEngine.Object.DestroyImmediate(panelObj.transform.GetChild(i).gameObject);
        }

        // Disable any legacy panels
        foreach (Transform child in canvas.transform)
        {
            if (child != panelObj.transform)
            {
                child.gameObject.SetActive(false);
            }
        }

        // Title
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(panelObj.transform, false);
        RectTransform titleRt = titleObj.AddComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0.05f, 0.76f);
        titleRt.anchorMax = new Vector2(0.95f, 0.94f);
        titleRt.sizeDelta = Vector2.zero;
        TextMeshProUGUI titleTmp = titleObj.AddComponent<TextMeshProUGUI>();
        if (fontAsset != null) titleTmp.font = fontAsset;
        titleTmp.text = "Extended Immersive Tour";
        titleTmp.fontSize = 44;
        titleTmp.alignment = TextAlignmentOptions.Center;
        titleTmp.fontStyle = FontStyles.Bold;
        titleTmp.color = new Color(0.35f, 0.88f, 1f, 1f);
        titleTmp.raycastTarget = false;

        // Subtitle
        GameObject subObj = new GameObject("Subtitle");
        subObj.transform.SetParent(panelObj.transform, false);
        RectTransform subRt = subObj.AddComponent<RectTransform>();
        subRt.anchorMin = new Vector2(0.05f, 0.63f);
        subRt.anchorMax = new Vector2(0.95f, 0.74f);
        subRt.sizeDelta = Vector2.zero;
        TextMeshProUGUI subTmp = subObj.AddComponent<TextMeshProUGUI>();
        if (fontAsset != null) subTmp.font = fontAsset;
        subTmp.text = "ALU 360° Virtual Reality Experience\nSelect a tour to begin";
        subTmp.fontSize = 20;
        subTmp.alignment = TextAlignmentOptions.Center;
        subTmp.color = new Color(0.85f, 0.92f, 0.98f, 0.90f);
        subTmp.raycastTarget = false;

        // Button 1: Intranet Tour
        CreateMenuNavCard(
            panelObj.transform,
            "Btn_IntranetTour",
            new Vector2(0.10f, 0.35f),
            new Vector2(0.90f, 0.58f),
            "INTRANET TOUR",
            "Explore the 4 original 360° rooms: Living Room, Cantina, Cube & Mezzanine",
            "IntranetTourScene",
            fontAsset
        );

        // Button 2: Custom Campus Tour
        CreateMenuNavCard(
            panelObj.transform,
            "Btn_CampusTour",
            new Vector2(0.10f, 0.08f),
            new Vector2(0.90f, 0.31f),
            "CUSTOM CAMPUS TOUR",
            "Explore the personalized 3-room campus experience: Room 1, Room 2 & Room 3",
            "CustomCampusTourScene",
            fontAsset
        );

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        // Keep root copy in sync
        if (File.Exists("Assets/MainMenu scene.unity"))
        {
            AssetDatabase.CopyAsset(MAIN_MENU_PATH, "Assets/MainMenu scene.unity");
        }
        Debug.Log("MainMenuScene configured with 360 photo background and synchronized.");
    }

    private static void CreateMenuNavCard(
        Transform parent,
        string goName,
        Vector2 anchorMin,
        Vector2 anchorMax,
        string title,
        string description,
        string targetScene,
        TMP_FontAsset fontAsset = null)
    {
        GameObject btnObj = new GameObject(goName);
        btnObj.transform.SetParent(parent, false);

        RectTransform rt = btnObj.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.sizeDelta = Vector2.zero;

        Image img = btnObj.AddComponent<Image>();
        img.color = new Color(0.13f, 0.18f, 0.26f, 0.95f);

        Outline outline = btnObj.AddComponent<Outline>();
        outline.effectColor = new Color(0.28f, 0.78f, 1f, 0.45f);
        outline.effectDistance = new Vector2(2, 2);

        Button btn = btnObj.AddComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.normalColor = new Color(0.13f, 0.18f, 0.26f, 0.95f);
        cb.highlightedColor = new Color(0.22f, 0.58f, 0.88f, 1f);
        cb.pressedColor = new Color(0.14f, 0.78f, 0.52f, 1f);
        cb.selectedColor = cb.highlightedColor;
        btn.colors = cb;

        MenuSceneLoader loader = btnObj.AddComponent<MenuSceneLoader>();
        loader.sceneName = targetScene;

        UnityEditor.Events.UnityEventTools.AddPersistentListener(btn.onClick, loader.LoadTargetScene);

        Hotspot hotspot = btnObj.AddComponent<Hotspot>();
        hotspot.hotspotIcon = img;
        if (hotspot.onTrigger == null) hotspot.onTrigger = new UnityEngine.Events.UnityEvent();
        UnityEditor.Events.UnityEventTools.AddPersistentListener(hotspot.onTrigger, loader.LoadTargetScene);

        btnObj.AddComponent<VRInteractiveButton>();

        // Title text
        GameObject titleObj = new GameObject("TitleText");
        titleObj.transform.SetParent(btnObj.transform, false);
        RectTransform tRt = titleObj.AddComponent<RectTransform>();
        tRt.anchorMin = new Vector2(0.04f, 0.50f);
        tRt.anchorMax = new Vector2(0.96f, 0.92f);
        tRt.sizeDelta = Vector2.zero;
        TextMeshProUGUI tTmp = titleObj.AddComponent<TextMeshProUGUI>();
        if (fontAsset != null) tTmp.font = fontAsset;
        tTmp.text = title;
        tTmp.fontSize = 25;
        tTmp.fontStyle = FontStyles.Bold;
        tTmp.alignment = TextAlignmentOptions.Left;
        tTmp.color = Color.white;
        tTmp.raycastTarget = false;

        // Description text
        GameObject descObj = new GameObject("DescText");
        descObj.transform.SetParent(btnObj.transform, false);
        RectTransform dRt = descObj.AddComponent<RectTransform>();
        dRt.anchorMin = new Vector2(0.04f, 0.08f);
        dRt.anchorMax = new Vector2(0.96f, 0.50f);
        dRt.sizeDelta = Vector2.zero;
        TextMeshProUGUI dTmp = descObj.AddComponent<TextMeshProUGUI>();
        if (fontAsset != null) dTmp.font = fontAsset;
        dTmp.text = description;
        dTmp.fontSize = 16;
        dTmp.alignment = TextAlignmentOptions.Left;
        dTmp.color = new Color(0.82f, 0.90f, 0.98f, 0.85f);
        dTmp.raycastTarget = false;
    }

    private static void SetupIntranetTourScene()
    {
        var scene = EditorSceneManager.OpenScene(INTRANET_PATH, OpenSceneMode.Single);
        Debug.Log($"Configuring {scene.name}...");

        EnsureVROrigin(scene);

        TourManager tm = UnityEngine.Object.FindAnyObjectByType<TourManager>();
        if (tm != null)
        {
            var so = new SerializedObject(tm);
            so.FindProperty("defaultStartingRoom").stringValue = "LivingRoom";
            so.FindProperty("useFadeTransition").boolValue = false;
            so.FindProperty("mainMenuSceneName").stringValue = "MainMenuScene";
            so.ApplyModifiedProperties();

            tm.AutoDiscoverRooms();

            // Populate allInfoCanvases
            var allToggles = UnityEngine.Object.FindObjectsByType<UIToggle>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            var canvasList = new List<GameObject>();
            foreach (var ut in allToggles)
            {
                canvasList.Add(ut.gameObject);
            }
            tm.allInfoCanvases = canvasList.ToArray();
            EditorUtility.SetDirty(tm);
            tm.ShowLivingRoom();
        }

        // Clean up child Text (TMP) objects under Hotspots/Buttons
        var tmpList = UnityEngine.Object.FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var tmp in tmpList)
        {
            var parentBtn = tmp.GetComponentInParent<Button>(true);
            var parentHotspot = tmp.GetComponentInParent<Hotspot>(true);
            if (parentBtn != null || parentHotspot != null)
            {
                var h = tmp.GetComponent<Hotspot>();
                if (h != null) UnityEngine.Object.DestroyImmediate(h, true);
                tmp.raycastTarget = false;
            }
        }

        // Destroy any leftover BGM audio object
        var bgmObj = GameObject.Find("BGM");
        if (bgmObj != null) UnityEngine.Object.DestroyImmediate(bgmObj);

        // Add or configure VRNavHUD
        VRNavHUD hud = UnityEngine.Object.FindAnyObjectByType<VRNavHUD>();
        if (hud == null)
        {
            GameObject hudObj = new GameObject("VRNavHUD", typeof(VRNavHUD));
            hud = hudObj.GetComponent<VRNavHUD>();
        }
        hud.mainMenuSceneName = "MainMenuScene";
        hud.campusSceneName = "CustomCampusTourScene";
        hud.intranetSceneName = "IntranetTourScene";

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        // Keep 360VideoTour.unity synchronized
        if (File.Exists("Assets/360VideoTour.unity"))
        {
            AssetDatabase.CopyAsset(INTRANET_PATH, "Assets/360VideoTour.unity");
        }
        Debug.Log("IntranetTourScene configured and synchronized.");
    }

    private static void SetupCustomCampusTourScene()
    {
        var scene = EditorSceneManager.OpenScene(CAMPUS_PATH, OpenSceneMode.Single);
        Debug.Log($"Configuring {scene.name}...");

        EnsureVROrigin(scene);

        TourManager tm = UnityEngine.Object.FindAnyObjectByType<TourManager>();
        if (tm == null)
        {
            var tmObj = new GameObject("TourManager", typeof(TourManager));
            tm = tmObj.GetComponent<TourManager>();
        }

        // Discover and configure rooms
        var rootObjs = scene.GetRootGameObjects();
        var allT = new List<Transform>();
        foreach (var r in rootObjs)
        {
            allT.AddRange(r.GetComponentsInChildren<Transform>(true));
        }

        Transform room1T = allT.Find(t => t.name.Equals("Room1", StringComparison.OrdinalIgnoreCase));
        Transform room2T = allT.Find(t => t.name.Equals("Room2", StringComparison.OrdinalIgnoreCase));
        Transform room3T = allT.Find(t => t.name.Equals("Room3", StringComparison.OrdinalIgnoreCase));

        Transform ui1 = allT.Find(t => t.name.Equals("Hotspot_ToRoom2", StringComparison.OrdinalIgnoreCase));
        Transform ui2 = allT.Find(t => t.name.Equals("Hotspot_ToRoom3&1", StringComparison.OrdinalIgnoreCase));
        Transform ui3 = allT.Find(t => t.name.StartsWith("GoToRoom2Button", StringComparison.OrdinalIgnoreCase) && t.parent != null && t.parent.name.Equals("Room3", StringComparison.OrdinalIgnoreCase));

        // Load 360 photo textures
        Texture pic1 = AssetDatabase.LoadAssetAtPath<Texture>("Assets/Pictures/Picture1.jpg");
        Texture pic2 = AssetDatabase.LoadAssetAtPath<Texture>("Assets/Pictures/Picture2.jpg");
        Texture pic3 = AssetDatabase.LoadAssetAtPath<Texture>("Assets/Pictures/Picture3.jpg");

        // Remove any VideoPlayer components on campus rooms since pictures are now used
        if (room1T != null) { var vp = room1T.GetComponent<VideoPlayer>(); if (vp != null) UnityEngine.Object.DestroyImmediate(vp, true); }
        if (room2T != null) { var vp = room2T.GetComponent<VideoPlayer>(); if (vp != null) UnityEngine.Object.DestroyImmediate(vp, true); }
        if (room3T != null) { var vp = room3T.GetComponent<VideoPlayer>(); if (vp != null) UnityEngine.Object.DestroyImmediate(vp, true); }

        var roomsList = new List<TourManager.CampusRoom>();

        if (room1T != null)
        {
            roomsList.Add(new TourManager.CampusRoom
            {
                roomID = "Room1",
                displayName = "Room 1 (Campus)",
                roomSphere = room1T.gameObject,
                videoPlayer = null,
                roomHotspotsCanvas = ui1 != null ? ui1.gameObject : null,
                videoFileName = "",
                roomPicture = pic1
            });
        }

        if (room2T != null)
        {
            roomsList.Add(new TourManager.CampusRoom
            {
                roomID = "Room2",
                displayName = "Room 2 (Campus)",
                roomSphere = room2T.gameObject,
                videoPlayer = null,
                roomHotspotsCanvas = ui2 != null ? ui2.gameObject : null,
                videoFileName = "",
                roomPicture = pic2
            });
        }

        if (room3T != null)
        {
            roomsList.Add(new TourManager.CampusRoom
            {
                roomID = "Room3",
                displayName = "Room 3 (Campus)",
                roomSphere = room3T.gameObject,
                videoPlayer = null,
                roomHotspotsCanvas = ui3 != null ? ui3.gameObject : null,
                videoFileName = "",
                roomPicture = pic3
            });
        }

        var tmSo = new SerializedObject(tm);
        tmSo.FindProperty("defaultStartingRoom").stringValue = "Room1";
        tmSo.FindProperty("useFadeTransition").boolValue = false;
        tmSo.FindProperty("mainMenuSceneName").stringValue = "MainMenuScene";
        tmSo.ApplyModifiedProperties();

        // Assign rooms via reflection/serialized properties
        var fRooms = typeof(TourManager).GetField("campusRooms", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        fRooms?.SetValue(tm, roomsList);

        EditorUtility.SetDirty(tm);

        // Ensure buttons have proper listeners wired
        Button[] buttons = UnityEngine.Object.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var btn in buttons)
        {
            // Disable child TMP raycasts
            var tmps = btn.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var tmp in tmps)
            {
                tmp.raycastTarget = false;
                var childH = tmp.GetComponent<Hotspot>();
                if (childH != null) UnityEngine.Object.DestroyImmediate(childH, true);
            }

            // Wire onClick if matching navigation pattern
            string bName = btn.name.ToLowerInvariant();
            if (bName.Contains("room1"))
            {
                if (btn.onClick.GetPersistentEventCount() == 0)
                {
                    UnityEditor.Events.UnityEventTools.AddPersistentListener(btn.onClick, tm.ShowRoom1);
                }
            }
            else if (bName.Contains("room2"))
            {
                if (btn.onClick.GetPersistentEventCount() == 0)
                {
                    UnityEditor.Events.UnityEventTools.AddPersistentListener(btn.onClick, tm.ShowRoom2);
                }
            }
            else if (bName.Contains("room3"))
            {
                if (btn.onClick.GetPersistentEventCount() == 0)
                {
                    UnityEditor.Events.UnityEventTools.AddPersistentListener(btn.onClick, tm.ShowRoom3);
                }
            }

            // Add Hotspot visual polish without audio
            var h = btn.GetComponent<Hotspot>();
            if (h == null)
            {
                h = btn.gameObject.AddComponent<Hotspot>();
                h.hotspotIcon = btn.GetComponent<Image>();
            }
        }

        // Destroy any leftover BGM audio object
        var bgmObj = GameObject.Find("BGM");
        if (bgmObj != null) UnityEngine.Object.DestroyImmediate(bgmObj);

        // Add or configure VRNavHUD
        VRNavHUD hud = UnityEngine.Object.FindAnyObjectByType<VRNavHUD>();
        if (hud == null)
        {
            GameObject hudObj = new GameObject("VRNavHUD", typeof(VRNavHUD));
            hud = hudObj.GetComponent<VRNavHUD>();
        }
        hud.mainMenuSceneName = "MainMenuScene";
        hud.campusSceneName = "CustomCampusTourScene";
        hud.intranetSceneName = "IntranetTourScene";

        // Initialize to Room 1
        tm.ShowRoom1();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        // Keep CampusTour.unity synchronized
        if (File.Exists("Assets/CampusTour.unity"))
        {
            AssetDatabase.CopyAsset(CAMPUS_PATH, "Assets/CampusTour.unity");
        }
        Debug.Log("CustomCampusTourScene configured and synchronized.");
    }

    private static void RegisterBuildSettings()
    {
        var scenes = new List<EditorBuildSettingsScene>
        {
            new EditorBuildSettingsScene(MAIN_MENU_PATH, true),
            new EditorBuildSettingsScene(INTRANET_PATH, true),
            new EditorBuildSettingsScene(CAMPUS_PATH, true)
        };

        EditorBuildSettings.scenes = scenes.ToArray();
        Debug.Log($"EditorBuildSettings updated with 3 scenes: MainMenuScene (0), IntranetTourScene (1), CustomCampusTourScene (2).");
    }
}
