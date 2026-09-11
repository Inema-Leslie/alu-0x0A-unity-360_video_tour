using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class Hotspot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Visual Feedback")]
    public Image hotspotIcon;
    public Color normalColor = Color.white;
    public Color hoverColor = new Color(0.2f, 0.8f, 1f, 1f);
    public Color clickColor = Color.green;

    [Header("Gaze Settings")]
    public float gazeDuration = 2.0f;
    private float gazeTimer = 0f;
    private bool isGazing = false;

    [Header("Action")]
    public UnityEvent onTrigger;

    void Start()
    {
        if (hotspotIcon) hotspotIcon.color = normalColor;
    }

    void Update()
    {
        if (isGazing)
        {
            gazeTimer += Time.deltaTime;

            if (gazeTimer >= gazeDuration)
            {
                TriggerHotspot();
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isGazing = true;
        gazeTimer = 0f;
        if (hotspotIcon) hotspotIcon.color = hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ResetHotspot();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        TriggerHotspot();
    }

    private void TriggerHotspot()
    {
        if (hotspotIcon) hotspotIcon.color = clickColor;
        isGazing = false;
        onTrigger?.Invoke();
        ResetHotspot();
    }

    private void ResetHotspot()
    {
        isGazing = false;
        gazeTimer = 0f;
        if (hotspotIcon) hotspotIcon.color = normalColor;
    }
}