using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using TMPro;

public enum LoseReason
{
    ReachedCheese,
    TimerExpired
}

public class BotController : MonoBehaviour
{
    [Header("References")]
    public NavMeshAgent agent;
    public GameObject markerPrefab;
    public GameObject loseMenuUI; 
    public TMP_Text loseMessageText;
    [Tooltip("UI icon shown while the player can place the cheese.")]
    public GameObject cheeseHeldIcon;
    public MonoBehaviour cheeseTimer;
    
    [Header("Layers")]
    public LayerMask groundLayer;
    public LayerMask baitLayer;

    [Header("Settings")]
    public float clickCooldown = 0.5f;
    public float maxPathDistance = 20f; 
    public float arrivalThreshold = 0.5f; 

    private GameObject currentMarker;
    private float nextClickTime = 0f;

    void Start()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        ConfigureCheeseIcon();
        SetCheeseHeldIconVisible(true);
        ResetCheeseTimer();
    }

    public void BeginMazeRun()
    {
        SetCheeseHeldIconVisible(true);
        ResetCheeseTimer();
        StartCheeseTimer();
    }

    void Update()
    {
        
        if (Time.timeScale == 0) return;

        
        if (Input.GetMouseButtonDown(0) && Time.unscaledTime >= nextClickTime)
        {
            if (!EventSystem.current.IsPointerOverGameObject()) 
            {
                HandleClick();
                nextClickTime = Time.unscaledTime + clickCooldown;
            }
        }

        CheckIfBotReachedBait();
    }

    void HandleClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100f, baitLayer))
        {
            if (currentMarker != null && hit.collider.gameObject == currentMarker)
            {
                ClearBait();
                SetCheeseHeldIconVisible(true);
                Debug.Log("Picked up the cheese");
                return;
            }
        }

        if (Physics.Raycast(ray, out hit, 100f, groundLayer))
        {
            if (currentMarker != null) return;

            NavMeshPath path = new NavMeshPath();
            if (agent.CalculatePath(hit.point, path))
            {
                if (GetPathLength(path) > maxPathDistance) 
                return; 

                currentMarker = Instantiate(markerPrefab, hit.point, Quaternion.identity);
                currentMarker.layer = Mathf.RoundToInt(Mathf.Log(baitLayer.value, 2));
                SetCheeseHeldIconVisible(false);

                agent.isStopped = false;
                agent.SetDestination(hit.point);
                Debug.Log("Placed the cheese");
            }
        }
    }

    void CheckIfBotReachedBait()
    {
        if (currentMarker != null)
        {
            if (Vector3.Distance(transform.position, currentMarker.transform.position) <= arrivalThreshold)
            {
                TriggerLose(LoseReason.ReachedCheese);
            }
        }
    }

    public void TriggerLose(LoseReason reason)
    {
        ClearBait();
        SetCheeseHeldIconVisible(false);
        StopCheeseTimer();

        if (loseMessageText != null)
        {
            loseMessageText.text = reason == LoseReason.TimerExpired
                ? "Timer over"
                : "The rat got the cheese\n\nYou lost";
        }

        if (loseMenuUI != null)
        {
            loseMenuUI.SetActive(true);
        }

        Time.timeScale = 0f;
    }

  
    public void ResetBot(Vector3 startPos)
    {
       
        ClearBait();
        SetCheeseHeldIconVisible(true);
        ResetCheeseTimer();
        nextClickTime = 0;

       
        agent.enabled = false; 
        transform.position = startPos;
        agent.enabled = true;
        agent.ResetPath();
        
       
        Time.timeScale = 1f;
    }

    public void ClearBait()
    {
        if (currentMarker != null)
        {
            Destroy(currentMarker);
            currentMarker = null; 
        }

        if (agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }
    }

    private void SetCheeseHeldIconVisible(bool isVisible)
    {
        if (cheeseHeldIcon != null)
        {
            cheeseHeldIcon.SetActive(isVisible);
        }
    }

    private void ConfigureCheeseIcon()
    {
        if (cheeseHeldIcon == null) return;

        Canvas iconCanvas = cheeseHeldIcon.GetComponent<Canvas>();
        if (iconCanvas != null)
        {
            iconCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            iconCanvas.worldCamera = null;
            iconCanvas.overrideSorting = true;
            iconCanvas.sortingOrder = -10;
        }

        RectTransform iconTransform = cheeseHeldIcon.GetComponent<RectTransform>();
        if (iconTransform == null) return;

        iconTransform.anchorMin = new Vector2(1f, 0f);
        iconTransform.anchorMax = new Vector2(1f, 0f);
        iconTransform.pivot = new Vector2(1f, 0f);
        iconTransform.sizeDelta = new Vector2(48f, 48f);
        iconTransform.anchoredPosition = new Vector2(-24f, 24f);
        iconTransform.localRotation = Quaternion.identity;
        iconTransform.localScale = Vector3.one;
    }

    private void StartCheeseTimer()
    {
        if (cheeseTimer != null) cheeseTimer.SendMessage("StartTimer", SendMessageOptions.RequireReceiver);
    }

    private void ResetCheeseTimer()
    {
        if (cheeseTimer != null) cheeseTimer.SendMessage("ResetTimer", SendMessageOptions.RequireReceiver);
    }

    private void StopCheeseTimer()
    {
        if (cheeseTimer != null) cheeseTimer.SendMessage("StopTimer", SendMessageOptions.RequireReceiver);
    }

    public void StopMazeTimer()
    {
        StopCheeseTimer();
    }

    private float GetPathLength(NavMeshPath path)
    {
        float length = 0.0f;
        if (path.corners.Length < 2) return 0f;
        for (int i = 1; i < path.corners.Length; i++)
            length += Vector3.Distance(path.corners[i - 1], path.corners[i]);
        return length;
    }
}