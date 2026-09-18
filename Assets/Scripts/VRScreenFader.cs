using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class VRScreenFader : MonoBehaviour
{
    private static VRScreenFader _instance;

    public static VRScreenFader Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindAnyObjectByType<VRScreenFader>();
                if (_instance == null)
                {
                    Camera cam = Camera.main;
                    if (cam == null) cam = FindAnyObjectByType<Camera>();
                    if (cam != null)
                    {
                        _instance = cam.gameObject.AddComponent<VRScreenFader>();
                    }
                    else
                    {
                        GameObject faderObj = new GameObject("VRScreenFader");
                        _instance = faderObj.AddComponent<VRScreenFader>();
                    }
                }
            }
            return _instance;
        }
    }

    [Header("Fade Settings")]
    [SerializeField] private float defaultFadeDuration = 0.5f;
    [SerializeField] private Color fadeColor = Color.black;
    [SerializeField] private bool fadeInOnStart = false;

    private Canvas fadeCanvas;
    private CanvasGroup canvasGroup;
    private Image fadeImage;
    private Coroutine currentFadeRoutine;

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else if (_instance != this)
        {
            Destroy(this);
            return;
        }

        SetupFadeOverlay();
        SetAlpha(0f);
    }

    void Start()
    {
        if (fadeInOnStart)
        {
            SetAlpha(1f);
            FadeIn(defaultFadeDuration);
        }
        else
        {
            SetAlpha(0f);
        }
    }

    private void SetupFadeOverlay()
    {
        if (fadeCanvas != null) return;

        GameObject canvasObj = new GameObject("VRFadeCanvas");
        canvasObj.transform.SetParent(transform, false);
        canvasObj.transform.localPosition = new Vector3(0, 0, 0.35f);
        canvasObj.transform.localRotation = Quaternion.identity;
        canvasObj.transform.localScale = Vector3.one * 0.001f;

        fadeCanvas = canvasObj.AddComponent<Canvas>();
        fadeCanvas.renderMode = RenderMode.WorldSpace;
        fadeCanvas.sortingOrder = 32767;

        canvasGroup = canvasObj.AddComponent<CanvasGroup>();
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
        canvasGroup.alpha = 0f;

        RectTransform rt = canvasObj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(2000, 2000);

        GameObject imageObj = new GameObject("FadeImage");
        imageObj.transform.SetParent(canvasObj.transform, false);

        fadeImage = imageObj.AddComponent<Image>();
        fadeImage.color = fadeColor;
        fadeImage.raycastTarget = false;

        RectTransform imgRt = imageObj.GetComponent<RectTransform>();
        imgRt.anchorMin = Vector2.zero;
        imgRt.anchorMax = Vector2.one;
        imgRt.sizeDelta = Vector2.zero;
        imgRt.anchoredPosition = Vector2.zero;

        // Ensure canvas is disabled by default
        canvasObj.SetActive(false);
    }

    public void SetAlpha(float alpha)
    {
        if (canvasGroup != null)
        {
            float clamped = Mathf.Clamp01(alpha);
            canvasGroup.alpha = clamped;
            canvasGroup.blocksRaycasts = clamped > 0.01f;
            if (fadeCanvas != null)
            {
                fadeCanvas.gameObject.SetActive(clamped > 0.001f);
            }
        }
    }

    public void FadeIn(float duration = -1f, Action onComplete = null)
    {
        if (duration < 0) duration = defaultFadeDuration;
        StartFade(1f, 0f, duration, onComplete);
    }

    public void FadeOut(float duration = -1f, Action onComplete = null)
    {
        if (duration < 0) duration = defaultFadeDuration;
        StartFade(0f, 1f, duration, onComplete);
    }

    public void FadeTransition(Action onBlackout, float fadeDuration = -1f, Action onComplete = null)
    {
        if (fadeDuration < 0) fadeDuration = defaultFadeDuration;
        float halfDuration = fadeDuration * 0.5f;

        if (currentFadeRoutine != null) StopCoroutine(currentFadeRoutine);
        currentFadeRoutine = StartCoroutine(TransitionRoutine(halfDuration, onBlackout, onComplete));
    }

    public void FadeOutAndLoad(string sceneName, float duration = -1f)
    {
        if (duration < 0) duration = defaultFadeDuration;
        if (currentFadeRoutine != null) StopCoroutine(currentFadeRoutine);
        currentFadeRoutine = StartCoroutine(FadeAndLoadRoutine(sceneName, duration));
    }

    private void StartFade(float fromAlpha, float toAlpha, float duration, Action onComplete)
    {
        if (fadeCanvas == null || canvasGroup == null) SetupFadeOverlay();

        if (fadeCanvas != null)
        {
            fadeCanvas.gameObject.SetActive(true);
        }

        if (currentFadeRoutine != null) StopCoroutine(currentFadeRoutine);
        currentFadeRoutine = StartCoroutine(FadeRoutine(fromAlpha, toAlpha, duration, onComplete));
    }

    private IEnumerator FadeRoutine(float fromAlpha, float toAlpha, float duration, Action onComplete)
    {
        if (canvasGroup == null) SetupFadeOverlay();

        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(timer / duration);
            SetAlpha(Mathf.Lerp(fromAlpha, toAlpha, t));
            yield return null;
        }

        SetAlpha(toAlpha);
        if (toAlpha <= 0.001f && fadeCanvas != null)
        {
            fadeCanvas.gameObject.SetActive(false);
        }
        currentFadeRoutine = null;
        onComplete?.Invoke();
    }

    private IEnumerator TransitionRoutine(float halfDuration, Action onBlackout, Action onComplete)
    {
        yield return FadeRoutine(canvasGroup != null ? canvasGroup.alpha : 0f, 1f, halfDuration, null);

        onBlackout?.Invoke();

        yield return new WaitForSecondsRealtime(0.08f);

        yield return FadeRoutine(1f, 0f, halfDuration, onComplete);
    }

    private IEnumerator FadeAndLoadRoutine(string sceneName, float duration)
    {
        yield return FadeRoutine(canvasGroup != null ? canvasGroup.alpha : 0f, 1f, duration, null);
        SceneManager.LoadScene(sceneName);
    }
}
