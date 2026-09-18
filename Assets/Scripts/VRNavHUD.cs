using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using TMPro;


public class VRNavHUD : MonoBehaviour
{
    [Header("Target Scenes")]
    public string mainMenuSceneName = "MainMenuScene";

    
    public string intranetSceneName = "IntranetTourScene";

   
    public string campusSceneName = "CustomCampusTourScene";

    
    [Header("HUD Positioning")]
    [Tooltip("If true, lazily follows the camera rotation at a comfortable distance")]
    public bool followCamera = true;

    
    public float followSpeed = 2.5f;

    
    public float distance = 3.2f;

    
    public float verticalOffset = -0.7f;

    // Reference to the active camera transform
    private Transform camTransform;

    // Initializes camera transform reference
    void Awake()
    {
    }

    
    void Start()
    {
        Camera cam = Camera.main != null ? Camera.main : FindAnyObjectByType<Camera>();
        if (cam != null) camTransform = cam.transform;

        if (transform.childCount == 0)
        {
            BuildRuntimeHUD();
        }

        RepositionInFrontOfCamera();
    }

    
    void LateUpdate()
    {
        if (followCamera && camTransform != null)
        {
            Vector3 targetPos = camTransform.position + (camTransform.forward * distance);
            targetPos.y = camTransform.position.y + verticalOffset;

            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * followSpeed);

            Vector3 lookDir = transform.position - camTransform.position;
            if (lookDir.sqrMagnitude > 0.001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(lookDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * followSpeed);
            }
        }
    }

    
    public void RepositionInFrontOfCamera()
    {
        if (camTransform == null)
        {
            Camera cam = Camera.main != null ? Camera.main : FindAnyObjectByType<Camera>();
            if (cam != null) camTransform = cam.transform;
        }

        if (camTransform != null)
        {
            Vector3 forwardFlat = camTransform.forward;
            forwardFlat.y = 0;
            forwardFlat.Normalize();
            if (forwardFlat.sqrMagnitude < 0.01f) forwardFlat = Vector3.forward;

            transform.position = camTransform.position + (forwardFlat * distance) + new Vector3(0, verticalOffset, 0);
            transform.rotation = Quaternion.LookRotation(transform.position - camTransform.position);
        }
    }

    
    private void BuildRuntimeHUD()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        bool isIntranet = currentScene.Contains("Intranet") || currentScene.Contains("360Video");

        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 100;

        RectTransform rt = GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(460, 60);
        transform.localScale = Vector3.one * 0.002f;

        gameObject.AddComponent<CanvasGroup>();
        gameObject.AddComponent<GraphicRaycaster>();

        var raycasterType = System.Type.GetType("UnityEngine.XR.Interaction.Toolkit.UI.TrackedDeviceGraphicRaycaster, Unity.XR.Interaction.Toolkit");
        if (raycasterType != null && GetComponent(raycasterType) == null)
        {
            gameObject.AddComponent(raycasterType);
        }

        GameObject bgObj = new GameObject("HUD_Background");
        bgObj.transform.SetParent(transform, false);
        Image bgImage = bgObj.AddComponent<Image>();
        bgImage.color = new Color(0.08f, 0.1f, 0.14f, 0.85f);
        RectTransform bgRt = bgObj.GetComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.sizeDelta = Vector2.zero;

        Outline outline = bgObj.AddComponent<Outline>();
        outline.effectColor = new Color(0.2f, 0.7f, 1f, 0.35f);
        outline.effectDistance = new Vector2(2, 2);

        HorizontalLayoutGroup layout = bgObj.AddComponent<HorizontalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.spacing = 15;
        layout.padding = new RectOffset(15, 15, 8, 8);
        layout.childControlWidth = true;
        layout.childControlHeight = true;

        CreateHUDButton(bgObj.transform, "Main Menu", () => GoToScene(mainMenuSceneName));

        string switchName = isIntranet ? "Campus Tour" : "Intranet Tour";
        string switchScene = isIntranet ? campusSceneName : intranetSceneName;
        CreateHUDButton(bgObj.transform, switchName, () => GoToScene(switchScene));
    }

    
    private GameObject CreateHUDButton(Transform parent, string title, UnityEngine.Events.UnityAction onClick)
    {
        GameObject btnObj = new GameObject("Btn_" + title.Replace(" ", ""));
        btnObj.transform.SetParent(parent, false);

        Image img = btnObj.AddComponent<Image>();
        img.color = new Color(0.18f, 0.22f, 0.3f, 0.95f);

        Button btn = btnObj.AddComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.normalColor = new Color(0.18f, 0.22f, 0.3f, 0.95f);
        cb.highlightedColor = new Color(0.2f, 0.6f, 0.95f, 1f);
        cb.pressedColor = new Color(0.1f, 0.4f, 0.7f, 1f);
        cb.selectedColor = cb.highlightedColor;
        btn.colors = cb;

        btn.onClick.AddListener(onClick);

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);
        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = title;
        tmp.fontSize = 20;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.fontStyle = FontStyles.Bold;
        tmp.raycastTarget = false;

        RectTransform textRt = textObj.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.sizeDelta = Vector2.zero;

        return btnObj;
    }

    
    public void GoToScene(string targetScene)
    {
        if (VRScreenFader.Instance != null)
        {
            VRScreenFader.Instance.FadeOutAndLoad(targetScene, 0.5f);
        }
        else
        {
            SceneManager.LoadScene(targetScene);
        }
    }
}
