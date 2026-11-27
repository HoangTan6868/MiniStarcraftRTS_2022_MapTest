using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI duy nhat cho ca Single Player va Multiplayer.
/// Prefab duoc tao boi Tools > StartCraft > Generate Bright Loading UI.
/// </summary>
public class StartCraftLoadingUI : MonoBehaviour
{
    [Header("Assigned by editor generator")]
    public Canvas canvas;
    public CanvasGroup canvasGroup;
    public Image progressFill;
    public TMP_Text headingText;
    public TMP_Text statusText;
    public TMP_Text percentageText;
    public TMP_Text hintText;

    public float minimumVisibleSeconds = 1.2f;
    public float fadeSeconds = 0.25f;

    private static StartCraftLoadingUI instance;
    private float desiredProgress;
    private float displayedProgress;
    private float shownAt;
    private float fadeSpeed = 1f;
    private bool isShowing;
    private bool indeterminate;
    private Coroutine hideCoroutine;

    public static bool IsVisible => instance != null && instance.isShowing;

    public static StartCraftLoadingUI Ensure()
    {
        if (instance != null)
            return instance;

        StartCraftLoadingUI prefab = Resources.Load<StartCraftLoadingUI>("StartCraftLoadingCanvas");
        if (prefab == null)
        {
            Debug.LogError("[StartCraft] Missing Resources/StartCraftLoadingCanvas.prefab. " +
                "Run Tools > StartCraft > Generate Bright Loading UI in Unity Editor.");
            return null;
        }

        instance = Instantiate(prefab);
        instance.name = "StartCraftLoadingCanvas (Runtime)";
        DontDestroyOnLoad(instance.gameObject);
        instance.ResetVisuals();
        return instance;
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
        fadeSpeed = 1f / Mathf.Max(0.05f, fadeSeconds);
        ResetVisuals();
    }

    private void ResetVisuals()
    {
        desiredProgress = 0f;
        displayedProgress = 0f;
        isShowing = false;
        indeterminate = false;
        if (progressFill != null) progressFill.fillAmount = 0f;
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }
        if (canvas != null) canvas.enabled = false;
    }

    public static void Show(string heading, string status, bool networkMode = false)
    {
        StartCraftLoadingUI ui = Ensure();
        if (ui == null) return;
        ui.ShowInternal(heading, status, networkMode);
    }

    private void ShowInternal(string heading, string status, bool networkMode)
    {
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }
        isShowing = true;
        indeterminate = networkMode;
        shownAt = Time.realtimeSinceStartup;
        desiredProgress = 0f;
        displayedProgress = 0f;
        if (canvas != null) canvas.enabled = true;
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
        }
        if (headingText != null) headingText.text = heading;
        if (statusText != null) statusText.text = status;
        if (hintText != null)
            hintText.text = networkMode
                ? "TIP: Hay cho dong doi cung san sang!"
                : "TIP: Dung quan sat ban do de chon vi tri can cu.";
        if (percentageText != null) percentageText.text = networkMode ? "DANG DONG BO..." : "0%";
        if (progressFill != null) progressFill.fillAmount = 0f;
    }

    public static void SetProgress(float progress, string status = null)
    {
        if (instance == null || !instance.isShowing) return;
        instance.indeterminate = false;
        instance.desiredProgress = Mathf.Clamp01(progress);
        if (!string.IsNullOrEmpty(status) && instance.statusText != null)
            instance.statusText.text = status;
    }

    public static void SetStatus(string status)
    {
        if (instance != null && instance.isShowing && instance.statusText != null)
            instance.statusText.text = status;
    }

    public static void Hide()
    {
        if (instance == null || !instance.isShowing) return;
        if (instance.hideCoroutine != null)
            instance.StopCoroutine(instance.hideCoroutine);
        instance.hideCoroutine = instance.StartCoroutine(instance.HideRoutine());
    }

    private void Update()
    {
        if (!isShowing) return;
        if (indeterminate)
        {
            // Unity NGO khong cong bo phan tram dong bo end-to-end tin cay.
            // Hien thi thanh dang xu ly, KHONG gia % hoan tat.
            float t = Mathf.PingPong(Time.unscaledTime * 0.7f, 1f);
            displayedProgress = Mathf.Lerp(0.22f, 0.82f, t);
            if (percentageText != null) percentageText.text = "DANG DONG BO...";
        }
        else
        {
            displayedProgress = Mathf.MoveTowards(
                displayedProgress, desiredProgress, 0.72f * Time.unscaledDeltaTime);
            if (percentageText != null)
                percentageText.text = Mathf.RoundToInt(displayedProgress * 100f) + "%";
        }
        if (progressFill != null) progressFill.fillAmount = displayedProgress;
    }

    private IEnumerator HideRoutine()
    {
        float wait = Mathf.Max(0, minimumVisibleSeconds - (Time.realtimeSinceStartup - shownAt));
        if (wait > 0f) yield return new WaitForSecondsRealtime(wait);

        // Chuyen sang 100% chi khi hoan tat, truoc khi tat giao dien.
        indeterminate = false;
        desiredProgress = 1f;
        displayedProgress = 1f;
        if (progressFill != null) progressFill.fillAmount = 1f;
        if (percentageText != null) percentageText.text = "100%";
        if (statusText != null) statusText.text = "DA SAN SANG!";
        yield return new WaitForSecondsRealtime(0.18f);
        while (canvasGroup != null && canvasGroup.alpha > 0f)
        {
            canvasGroup.alpha = Mathf.MoveTowards(
                canvasGroup.alpha, 0f, fadeSpeed * Time.unscaledDeltaTime);
            yield return null;
        }
        isShowing = false;
        if (canvasGroup != null) canvasGroup.blocksRaycasts = false;
        if (canvas != null) canvas.enabled = false;
        hideCoroutine = null;
    }
}
