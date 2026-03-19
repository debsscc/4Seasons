using UnityEngine;
using System.Collections;
using System; // Necessário para Action

public class ScrollAnimation : MonoBehaviour
{
    public Action OnScrollFinished; 

    [Header("Timing")]
    [Tooltip("Delay (in seconds) before the scroll begins.")]
    public float startDelay = 5f;

    [Tooltip("How long the scroll should take (seconds). Used when scrollSpeed is 0.")]
    public float scrollDuration = 20f;

    [Tooltip("Scroll speed in units/sec. If > 0, it overrides scrollDuration.")]
    public float scrollSpeed = 0f;

    [Header("Movement")]
    [Tooltip("If true, the scroll starts from the object's current anchored position in the scene.")]
    public bool useSceneStartPosition = true;

    [Tooltip("If >0, overrides the automatic distance calculation.")]
    public float scrollDistance = 0f;

    [Tooltip("Optional RectTransform to move. If null, this component's RectTransform is used.")]
    public RectTransform targetRectTransform;

    [Tooltip("Enable debug logging (prints start/end positions and duration).")]
    public bool debugLog = false;

    private RectTransform rectTransform;

    void Awake()
    {
        rectTransform = targetRectTransform != null ? targetRectTransform : GetComponent<RectTransform>();
        if (rectTransform == null)
            Debug.LogError("ScrollAnimation requer um RectTransform (no mesmo GameObject ou atribuído em targetRectTransform).", this);
    }

    public void StartScroll()
    {
        gameObject.SetActive(true);
        StartCoroutine(ScrollCoroutine());
    }

    private IEnumerator ScrollCoroutine()
    {
        if (startDelay > 0f)
            yield return new WaitForSeconds(startDelay);

        float initialY = useSceneStartPosition ? rectTransform.anchoredPosition.y : -rectTransform.rect.height;

        // If requested, allow explicit scroll distance for cases where automatic calculation doesn't match the desired "travel".
        float travelDistance = scrollDistance > 0f 
            ? scrollDistance 
            : (Screen.height + rectTransform.rect.height * 2f); // keeps the same total travel distance as before

        float finalY = initialY + travelDistance;

        float timeElapsed = 0f;

        float duration = CalculateScrollDuration(travelDistance);

        if (debugLog)
            Debug.Log($"[ScrollAnimation] start={initialY:F1} end={finalY:F1} dist={travelDistance:F1} dur={duration:F1}", this);

        while (timeElapsed < duration)
        {
            timeElapsed += Time.deltaTime;
            
            float newY = Mathf.Lerp(initialY, finalY, timeElapsed / duration);
            rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, newY);

            yield return null; 
        }

        rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, finalY);

        if (OnScrollFinished != null)
        {
            OnScrollFinished.Invoke();
        }
        
        gameObject.SetActive(false); 
    }

    public float GetEstimatedScrollDuration()
    {
        if (rectTransform == null)
            return 0f;

        float travelDistance = scrollDistance > 0f
            ? scrollDistance
            : (Screen.height + rectTransform.rect.height * 2f);

        return CalculateScrollDuration(travelDistance);
    }

    private float CalculateScrollDuration(float travelDistance)
    {
        if (scrollSpeed > 0f)
            return Mathf.Max(0.01f, travelDistance / scrollSpeed);

        return Mathf.Max(0.01f, scrollDuration);
    }
}