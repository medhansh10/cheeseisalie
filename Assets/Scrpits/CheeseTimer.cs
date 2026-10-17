using UnityEngine;
using UnityEngine.UI;

public class CheeseTimer : MonoBehaviour
{
    public Image timerFill;
    public BotController botController;
    public float duration = 10f;

    private float remainingTime;
    private bool isRunning;
    private bool hasTriggeredLose;

    private void Awake()
    {
        if (botController == null) botController = GetComponent<BotController>();
        ConfigureTimer();
        ResetTimer();
    }

    private void Update()
    {
        if (!isRunning || hasTriggeredLose) return;

        remainingTime -= Time.deltaTime;
        UpdateFill();

        if (remainingTime <= 0f)
        {
            remainingTime = 0f;
            isRunning = false;
            hasTriggeredLose = true;
            UpdateFill();

            if (botController != null)
            {
                botController.TriggerLose(LoseReason.TimerExpired);
            }
            else
            {
                Debug.LogError("CheeseTimer cannot trigger lose because BotController is not assigned.", this);
            }
        }
    }

    public void StartTimer()
    {
        remainingTime = Mathf.Max(0f, duration);
        hasTriggeredLose = false;
        isRunning = true;
        if (timerFill != null) timerFill.gameObject.SetActive(true);
        UpdateFill();
    }

    public void ResetTimer()
    {
        remainingTime = Mathf.Max(0f, duration);
        hasTriggeredLose = false;
        isRunning = false;
        if (timerFill != null) timerFill.gameObject.SetActive(false);
        UpdateFill();
    }

    public void StopTimer()
    {
        isRunning = false;
        if (timerFill != null) timerFill.gameObject.SetActive(false);
        UpdateFill();
    }

    private void UpdateFill()
    {
        if (timerFill != null)
        {
            timerFill.fillAmount = duration > 0f ? remainingTime / duration : 0f;
        }
    }

    private void ConfigureTimer()
    {
        Canvas timerCanvas = GetComponent<Canvas>();
        if (timerCanvas != null)
        {
            if (timerCanvas.transform.parent != null)
            {
                timerCanvas.transform.SetParent(null, false);
            }

            timerCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            timerCanvas.worldCamera = null;
            timerCanvas.overrideSorting = true;
            timerCanvas.sortingOrder = -5;
        }

        RectTransform timerTransform = GetComponent<RectTransform>();
        if (timerTransform == null) return;

        timerTransform.anchorMin = new Vector2(1f, 0f);
        timerTransform.anchorMax = new Vector2(1f, 0f);
        timerTransform.pivot = new Vector2(1f, 0f);
        timerTransform.sizeDelta = new Vector2(48f, 48f);
        timerTransform.anchoredPosition = new Vector2(-24f, 100f);
        timerTransform.localScale = Vector3.one;
    }
}
