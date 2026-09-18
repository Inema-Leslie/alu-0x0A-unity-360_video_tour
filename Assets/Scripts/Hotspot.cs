using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;


public class Hotspot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    
    [Header("Visual Feedback")]
    public Image hotspotIcon;

    
    public Color normalColor = Color.white;

    
    public Color hoverColor = new Color(0.25f, 0.85f, 1f, 1f);

    
    public Color clickColor = new Color(0.2f, 1f, 0.4f, 1f);

    
    [Header("Gaze Settings")]
    public float gazeDuration = 2.0f;

   
    [Header("Action")]
    public UnityEvent onTrigger = new UnityEvent();

    
    [Header("Animation & Polish")]
    [SerializeField] private bool enableHoverScale = true;
    [SerializeField] private float hoverScaleFactor = 1.18f;

    
    [SerializeField] private bool enableIdlePulse = true;
    [SerializeField] private float pulseFrequency = 1.5f;
    [SerializeField] private float pulseAmplitude = 0.05f;

    
    private Vector3 initialScale;
    
    private float gazeTimer = 0f;
    
    private bool isGazing = false;
    
    private bool isHovered = false;

    
    void Awake()
    {
        initialScale = transform.localScale;

        var btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.AddListener(TriggerHotspot);
        }
    }

    
    void Start()
    {
        if (hotspotIcon != null) hotspotIcon.color = normalColor;
    }

    
    void Update()
    {
        AnimateVisuals();

        if (isGazing)
        {
            gazeTimer += Time.deltaTime;
            if (gazeTimer >= gazeDuration)
            {
                TriggerHotspot();
            }
        }
    }

    
    private void AnimateVisuals()
    {
        Vector3 targetScale = initialScale;

        if (isHovered && enableHoverScale)
        {
            targetScale = initialScale * hoverScaleFactor;
        }
        else if (enableIdlePulse)
        {
            float pulse = Mathf.Sin(Time.time * pulseFrequency * Mathf.PI * 2f) * pulseAmplitude;
            targetScale = initialScale * (1f + pulse);
        }

        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * 10f);

        if (hotspotIcon != null)
        {
            Color targetColor = isHovered ? hoverColor : normalColor;
            hotspotIcon.color = Color.Lerp(hotspotIcon.color, targetColor, Time.deltaTime * 12f);
        }
    }

    
    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        isGazing = true;
        gazeTimer = 0f;
    }

    
    public void OnPointerExit(PointerEventData eventData)
    {
        ResetHotspot();
    }

    
    public void OnPointerClick(PointerEventData eventData)
    {
        TriggerHotspot();
    }

    public void TriggerHotspot()
    {
        isGazing = false;
        isHovered = false;

        if (hotspotIcon != null) hotspotIcon.color = clickColor;

        onTrigger?.Invoke();
    }

    
    private void ResetHotspot()
    {
        isHovered = false;
        isGazing = false;
        gazeTimer = 0f;
    }
}