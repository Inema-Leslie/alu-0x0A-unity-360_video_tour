using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using TMPro;

[ExecuteAlways]
public class Hotspot3D : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("360 Spherical Position")]
    [Range(-180f, 180f)]
    public float yaw = 0f; 
    public float pitch = 0f; 
    public float distance = 4.5f;

    [Header("Destination & Action")]
    [Tooltip("If set, will instruct TourManager to switch to this room ID on click/gaze")]
    public string targetRoomID;
    [Tooltip("If set, will toggle this UIToggle panel on click/gaze")]
    public UIToggle targetInfoToggle;
    public UnityEvent onTrigger;

    [Header("Label & Content")]
    public string labelText = "Next Room";
    [SerializeField] private TMP_Text labelTmp;
    [SerializeField] private CanvasGroup labelCanvasGroup;

    [Header("Visual Components")]
    public Image mainIcon;
    public Image pulseRing;
    public Image progressRing;

    [Header("Color Palette")]
    public Color normalColor = new Color(1f, 1f, 1f, 0.95f);
    public Color hoverColor = new Color(0.2f, 0.85f, 1f, 1f);
    public Color pulseColor = new Color(0.2f, 0.85f, 1f, 0.5f);

    [Header("Gaze & Interaction")]
    public bool enableGaze = true;
    public float gazeDuration = 1.5f;

    [Header("Audio Feedback")]
    public AudioClip hoverSound;
    public AudioClip clickSound;

    private float gazeTimer = 0f;
    private bool isHovered = false;
    private AudioSource audioSource;
    private Camera targetCam;
    private Vector3 initialScale;

    void Awake()
    {
        initialScale = transform.localScale;
        if (Application.isPlaying)
        {
            audioSource = gameObject.GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 0f;
            }
        }
    }

    void Start()
    {
        UpdatePosition();
        UpdateLabel();
        if (progressRing != null) progressRing.fillAmount = 0f;
        if (labelCanvasGroup != null) labelCanvasGroup.alpha = 0f;
    }

    void OnValidate()
    {
        UpdatePosition();
        UpdateLabel();
    }

    void Update()
    {
        UpdateBillboard();

        if (Application.isPlaying)
        {
            AnimatePulse();
            AnimateLabelAndHover();
            HandleGaze();
        }
    }

    public void UpdatePosition()
    {
        float radYaw = yaw * Mathf.Deg2Rad;
        float radPitch = pitch * Mathf.Deg2Rad;

        Vector3 dir = new Vector3(
            Mathf.Sin(radYaw) * Mathf.Cos(radPitch),
            Mathf.Sin(radPitch),
            Mathf.Cos(radYaw) * Mathf.Cos(radPitch)
        );

        transform.localPosition = dir * distance;
    }

    private void UpdateBillboard()
    {
        if (targetCam == null)
        {
            targetCam = Camera.main;
            if (targetCam == null) return;
        }

        Vector3 toCam = transform.position - targetCam.transform.position;
        if (toCam.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(toCam);
        }
    }

    public void UpdateLabel()
    {
        if (labelTmp != null && !string.IsNullOrEmpty(labelText))
        {
            labelTmp.text = labelText;
        }
    }

    private void AnimatePulse()
    {
        if (pulseRing == null) return;

        float cycle = (Time.time * 1.4f) % 1f;
        float scale = Mathf.Lerp(1.0f, 1.45f, cycle);
        float alpha = Mathf.Lerp(pulseColor.a, 0f, cycle * cycle);

        pulseRing.transform.localScale = new Vector3(scale, scale, 1f);
        Color c = pulseColor;
        c.a = alpha;
        pulseRing.color = c;
    }

    private void AnimateLabelAndHover()
    {
        float targetAlpha = isHovered ? 1f : 0f;
        if (labelCanvasGroup != null)
        {
            labelCanvasGroup.alpha = Mathf.MoveTowards(labelCanvasGroup.alpha, targetAlpha, Time.deltaTime * 6f);
        }

        Vector3 targetScale = isHovered ? initialScale * 1.12f : initialScale;
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * 10f);

        if (mainIcon != null)
        {
            mainIcon.color = Color.Lerp(mainIcon.color, isHovered ? hoverColor : normalColor, Time.deltaTime * 8f);
        }
    }

    private void HandleGaze()
    {
        if (!enableGaze || !isHovered) return;

        gazeTimer += Time.deltaTime;
        if (progressRing != null)
        {
            progressRing.fillAmount = Mathf.Clamp01(gazeTimer / gazeDuration);
        }

        if (gazeTimer >= gazeDuration)
        {
            Trigger();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        gazeTimer = 0f;
        PlaySound(hoverSound);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        gazeTimer = 0f;
        if (progressRing != null) progressRing.fillAmount = 0f;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Trigger();
    }

    public void Trigger()
    {
        isHovered = false;
        gazeTimer = 0f;
        if (progressRing != null) progressRing.fillAmount = 0f;
        PlaySound(clickSound);

        if (!string.IsNullOrEmpty(targetRoomID))
        {
            var tm = FindAnyObjectByType<TourManager>();
            if (tm != null)
            {
                tm.GoToRoomByName(targetRoomID);
            }
            else
            {
                Debug.LogWarning("[Hotspot3D] No TourManager found in scene!");
            }
        }

        if (targetInfoToggle != null)
        {
            targetInfoToggle.TogglePanel();
        }

        onTrigger?.Invoke();
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    [ContextMenu("Align to Current Camera View")]
    public void AlignToCurrentCameraView()
    {
        Camera cam = Camera.main;
        #if UNITY_EDITOR
        if (!Application.isPlaying && UnityEditor.SceneView.lastActiveSceneView != null)
        {
            cam = UnityEditor.SceneView.lastActiveSceneView.camera;
        }
        #endif

        if (cam == null)
        {
            Debug.LogWarning("No Camera found to align with!");
            return;
        }

        Vector3 forward = cam.transform.forward.normalized;
        yaw = Mathf.Atan2(forward.x, forward.z) * Mathf.Rad2Deg;
        pitch = Mathf.Asin(Mathf.Clamp(forward.y, -1f, 1f)) * Mathf.Rad2Deg;
        UpdatePosition();
        Debug.Log($"[Hotspot3D] Aligned '{gameObject.name}' to view: Yaw={yaw:F1}°, Pitch={pitch:F1}°");
    }
}
