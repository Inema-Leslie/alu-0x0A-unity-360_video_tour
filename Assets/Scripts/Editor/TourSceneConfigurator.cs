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
/// Editor utility to configure VR Tour scenes, navigation, and build settings.
/// Available via menu item: 'VR Tour/Configure All Scenes & Build Settings'.
/// </summary>
public static class TourSceneConfigurator
{
    private const string MAIN_MENU_SCENE_PATH = "Assets/Scenes/MainMenuScene.unity";
    private const string INTRANET_SCENE_PATH = "Assets/Scenes/IntranetTourScene.unity";
    private const string CAMPUS_SCENE_PATH = "Assets/Scenes/CustomCampusTourScene.unity";

    /// <summary>
    /// Configures all tour scenes and updates editor build settings.
    /// </summary>
    [MenuItem("VR Tour/Configure All Scenes & Build Settings", false, 1)]
    public static void ConfigureAll()
    {
        Debug.Log("=== Starting VR Tour Scene Configuration ===");

        EnsureScenesDirectory();
        CopyScenesIfMissing();

        ConfigureMainMenuScene();
        ConfigureIntranetScene();
        ConfigureCampusScene();

        UpdateBuildSettings();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("=== VR Tour Configuration Completed Successfully! ===");
    }

    private static void EnsureScenesDirectory()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
        {
            AssetDatabase.CreateFolder("Assets", "Scenes");
        }
    }

    private static void CopyScenesIfMissing()
    {
        // 1. MainMenuScene
        if (!File.Exists(MAIN_MENU_SCENE_PATH))
        {
            if (File.Exists("Assets/MainMenu scene.unity"))
            {
                AssetDatabase.CopyAsset("Assets/MainMenu scene.unity", MAIN_MENU_SCENE_PATH);
            }
            else if (File.Exists("Assets/MainMenuScene.unity"))
            {
                AssetDatabase.CopyAsset("Assets/MainMenuScene.unity", MAIN_MENU_SCENE_PATH);
            }
        }

        // 2. IntranetTourScene
        if (!File.Exists(INTRANET_SCENE_PATH))
        {
            if (File.Exists("Assets/360VideoTour.unity"))
            {
                AssetDatabase.CopyAsset("Assets/360VideoTour.unity", INTRANET_SCENE_PATH);
            }
        }

        // 3. CustomCampusTourScene
        if (!File.Exists(CAMPUS_SCENE_PATH))
        {
            if (File.Exists("Assets/CampusTour.unity"))
            {
                AssetDatabase.CopyAsset("Assets/CampusTour.unity", CAMPUS_SCENE_PATH);
            }
        }

        AssetDatabase.Refresh();
    }

    private static void ConfigureMainMenuScene()
    {
        var scene = EditorSceneManager.OpenScene(MAIN_MENU_SCENE_PATH, OpenSceneMode.Single);
        Debug.Log($"Configuring {scene.name}...");

        Camera cam = Camera.main;
        if (cam == null) cam = UnityEngine.Object.FindAnyObjectByType<Camera>();
        if (cam != null)
        {
            cam.transform.position = new Vector3(0, 1.4f, 0);
            cam.transform.rotation = Quaternion.identity;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.06f, 0.08f, 0.12f, 1f);

            if (cam.GetComponent<SimpleLook>() == null)
                cam.gameObject.AddComponent<SimpleLook>();

            if (cam.GetComponent<VRScreenFader>() == null)
                cam.gameObject.AddComponent<VRScreenFader>();
        }

        Canvas canvas = UnityEngine.Object.FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            var canvasObj = new GameObject("MainMenuCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
        }

        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = cam;
        canvas.transform.position = new Vector3(0, 1.4f, 2.6f);
        canvas.transform.rotation = Quaternion.identity;
        canvas.transform.localScale = new Vector3(0.0022f, 0.0022f, 0.0022f);

        RectTransform canvasRt = canvas.GetComponent<RectTransform>();
        canvasRt.sizeDelta = new Vector2(900, 620);

        if (canvas.GetComponent<GraphicRaycaster>() == null)
            canvas.gameObject.AddComponent<GraphicRaycaster>();

        Transform panelTransform = canvas.transform.Find("MenuPanel");
        GameObject panelObj;
        if (panelTransform == null)
        {
            panelObj = new GameObject("MenuPanel");
            panelObj.transform.SetParent(canvas.transform, false);
        }
        else
        {
            panelObj = panelTransform.gameObject;
        }

        RectTransform panelRt = panelObj.GetComponent<RectTransform>();
        if (panelRt == null) panelRt = panelObj.AddComponent<RectTransform>();
        panelRt.anchorMin = Vector2.zero;
        panelRt.anchorMax = Vector2.one;
        panelRt.sizeDelta = Vector2.zero;

        Image panelImg = panelObj.GetComponent<Image>();
        if (panelImg == null) panelImg = panelObj.AddComponent<Image>();
        panelImg.color = new Color(0.07f, 0.09f, 0.13f, 0.92f);

        Outline panelOutline = panelObj.GetComponent<Outline>();
        if (panelOutline == null) panelOutline = panelObj.AddComponent<Outline>();
        panelOutline.effectColor = new Color(0.2f, 0.75f, 1f, 0.4f);
        panelOutline.effectDistance = new Vector2(3, 3);

        for (int i = panelObj.transform.childCount - 1; i >= 0; i--)
        {
            UnityEngine.Object.DestroyImmediate(panelObj.transform.GetChild(i).gameObject);
        }

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
        titleRt.anchorMin = new Vector2(0.1f, 0.76f);
        titleRt.anchorMax = new Vector2(0.9f, 0.94f);
        titleRt.sizeDelta = Vector2.zero;
        TextMeshProUGUI titleTmp = titleObj.AddComponent<TextMeshProUGUI>();
        titleTmp.text = "Extended Immersive Tour";
        titleTmp.fontSize = 44;
        titleTmp.alignment = TextAlignmentOptions.Center;
        titleTmp.fontStyle = FontStyles.Bold;
        titleTmp.color = new Color(0.3f, 0.85f, 1f, 1f);

        // Subtitle
        GameObject subObj = new GameObject("Subtitle");
        subObj.transform.SetParent(panelObj.transform, false);
        RectTransform subRt = subObj.AddComponent<RectTransform>();
        subRt.anchorMin = new Vector2(0.1f, 0.64f);
        subRt.anchorMax = new Vector2(0.9f, 0.75f);
        subRt.sizeDelta = Vector2.zero;
        TextMeshProUGUI subTmp = subObj.AddComponent<TextMeshProUGUI>();
        subTmp.text = "ALU 360° Virtual Reality Experience\nSelect a tour to begin";
        subTmp.fontSize = 20;
        subTmp.alignment = TextAlignmentOptions.Center;
        subTmp.color = new Color(0.85f, 0.9f, 0.95f, 0.85f);

        // Button 1: Intranet Tour
        CreateMenuNavCard(
            panelObj.transform,
            "Btn_IntranetTour",
            new Vector2(0.12f, 0.36f),
            new Vector2(0.88f, 0.58f),
            "INTRANET TOUR",
            "Explore the 4 original 360° rooms: Living Room, Cantina, Cube & Mezzanine",
            "IntranetTourScene"
        );

        // Button 2: Custom Campus Tour
        CreateMenuNavCard(
            panelObj.transform,
            "Btn_CampusTour",
            new Vector2(0.12f, 0.10f),
            new Vector2(0.88f, 0.32f),
            "CUSTOM CAMPUS TOUR",
            "Explore the personalized 3-room campus experience: Room 1, Room 2 & Room 3",
            "CustomCampusTourScene"
        );

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("MainMenuScene successfully configured and saved.");
    }

    private static void CreateMenuNavCard(
        Transform parent,
        string goName,
        Vector2 anchorMin,
        Vector2 anchorMax,
        string title,
        string description,
        string targetScene)
    {
        GameObject btnObj = new GameObject(goName);
        btnObj.transform.SetParent(parent, false);

        RectTransform rt = btnObj.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.sizeDelta = Vector2.zero;

        Image img = btnObj.AddComponent<Image>();
        img.color = new Color(0.15f, 0.19f, 0.27f, 0.95f);

        Outline outline = btnObj.AddComponent<Outline>();
        outline.effectColor = new Color(0.3f, 0.8f, 1f, 0.4f);
        outline.effectDistance = new Vector2(2, 2);

        Button btn = btnObj.AddComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.normalColor = new Color(0.15f, 0.19f, 0.27f, 0.95f);
        cb.highlightedColor = new Color(0.2f, 0.55f, 0.85f, 1f);
        cb.pressedColor = new Color(0.12f, 0.75f, 0.5f, 1f);
        cb.selectedColor = cb.highlightedColor;
        btn.colors = cb;

        MenuSceneLoader loader = btnObj.AddComponent<MenuSceneLoader>();
        loader.sceneName = targetScene;

        btn.onClick.AddListener(loader.LoadTargetScene);

        GameObject titleObj = new GameObject("TitleText");
        titleObj.transform.SetParent(btnObj.transform, false);
        RectTransform tRt = titleObj.AddComponent<RectTransform>();
        tRt.anchorMin = new Vector2(0.05f, 0.48f);
        tRt.anchorMax = new Vector2(0.95f, 0.90f);
        tRt.sizeDelta = Vector2.zero;
        TextMeshProUGUI tTmp = titleObj.AddComponent<TextMeshProUGUI>();
        tTmp.text = title;
        tTmp.fontSize = 24;
        tTmp.fontStyle = FontStyles.Bold;
        tTmp.alignment = TextAlignmentOptions.Left;
        tTmp.color = Color.white;

        GameObject descObj = new GameObject("DescText");
        descObj.transform.SetParent(btnObj.transform, false);
        RectTransform dRt = descObj.AddComponent<RectTransform>();
        dRt.anchorMin = new Vector2(0.05f, 0.10f);
        dRt.anchorMax = new Vector2(0.95f, 0.48f);
        dRt.sizeDelta = Vector2.zero;
        TextMeshProUGUI dTmp = descObj.AddComponent<TextMeshProUGUI>();
        dTmp.text = description;
        dTmp.fontSize = 15;
        dTmp.alignment = TextAlignmentOptions.Left;
        dTmp.color = new Color(0.8f, 0.88f, 0.95f, 0.85f);
    }

    private static void ConfigureIntranetScene()
    {
        var scene = EditorSceneManager.OpenScene(INTRANET_SCENE_PATH, OpenSceneMode.Single);
        Debug.Log($"Configuring {scene.name}...");

        Camera cam = Camera.main;
        if (cam == null) cam = UnityEngine.Object.FindAnyObjectByType<Camera>();
        if (cam != null)
        {
            if (cam.GetComponent<SimpleLook>() == null)
                cam.gameObject.AddComponent<SimpleLook>();

            if (cam.GetComponent<VRScreenFader>() == null)
                cam.gameObject.AddComponent<VRScreenFader>();
        }

        TourManager tm = UnityEngine.Object.FindAnyObjectByType<TourManager>();
        if (tm != null)
        {
            var so = new SerializedObject(tm);
            so.FindProperty("defaultStartingRoom").stringValue = "LivingRoom";
            so.FindProperty("useFadeTransition").boolValue = false;
            so.ApplyModifiedProperties();
            tm.AutoDiscoverRooms();
        }

        // Clean up child Text (TMP) components under Hotspots/Buttons
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

        // Remove any unwanted BGM objects
        var bgmObj = GameObject.Find("BGM");
        if (bgmObj != null) UnityEngine.Object.DestroyImmediate(bgmObj);

        // Add VRNavHUD
        VRNavHUD hud = UnityEngine.Object.FindAnyObjectByType<VRNavHUD>();
        if (hud == null)
        {
            GameObject hudObj = new GameObject("VRNavHUD", typeof(VRNavHUD));
            hud = hudObj.GetComponent<VRNavHUD>();
            hud.mainMenuSceneName = "MainMenuScene";
            hud.campusSceneName = "CustomCampusTourScene";
            hud.intranetSceneName = "IntranetTourScene";
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("IntranetTourScene successfully configured and saved.");
    }

    private static void ConfigureCampusScene()
    {
        var scene = EditorSceneManager.OpenScene(CAMPUS_SCENE_PATH, OpenSceneMode.Single);
        Debug.Log($"Configuring {scene.name}...");

        Camera cam = Camera.main;
        if (cam == null) cam = UnityEngine.Object.FindAnyObjectByType<Camera>();
        if (cam != null)
        {
            if (cam.GetComponent<SimpleLook>() == null)
                cam.gameObject.AddComponent<SimpleLook>();

            if (cam.GetComponent<VRScreenFader>() == null)
                cam.gameObject.AddComponent<VRScreenFader>();
        }

        TourManager tm = UnityEngine.Object.FindAnyObjectByType<TourManager>();
        if (tm != null)
        {
            var so = new SerializedObject(tm);
            so.FindProperty("defaultStartingRoom").stringValue = "Room1";
            so.FindProperty("useFadeTransition").boolValue = false;
            so.ApplyModifiedProperties();
            tm.AutoDiscoverRooms();
        }

        // Remove any unwanted BGM objects
        var bgmObj = GameObject.Find("BGM");
        if (bgmObj != null) UnityEngine.Object.DestroyImmediate(bgmObj);

        // Add VRNavHUD
        VRNavHUD hud = UnityEngine.Object.FindAnyObjectByType<VRNavHUD>();
        if (hud == null)
        {
            GameObject hudObj = new GameObject("VRNavHUD", typeof(VRNavHUD));
            hud = hudObj.GetComponent<VRNavHUD>();
            hud.mainMenuSceneName = "MainMenuScene";
            hud.campusSceneName = "CustomCampusTourScene";
            hud.intranetSceneName = "IntranetTourScene";
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("CustomCampusTourScene successfully configured and saved.");
    }

    private static void UpdateBuildSettings()
    {
        var scenes = new List<EditorBuildSettingsScene>
        {
            new EditorBuildSettingsScene(MAIN_MENU_SCENE_PATH, true),
            new EditorBuildSettingsScene(INTRANET_SCENE_PATH, true),
            new EditorBuildSettingsScene(CAMPUS_SCENE_PATH, true)
        };

        EditorBuildSettings.scenes = scenes.ToArray();
        Debug.Log($"Build settings updated with {scenes.Count} scenes.");
    }
}
