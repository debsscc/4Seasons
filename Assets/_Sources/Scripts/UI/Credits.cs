using UnityEngine;

public class Credits : MonoBehaviour
{
   public float scrollSpeed = 20f;
   private RectTransform rectTransform;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }
    void Update()
    {
        rectTransform.anchoredPosition += new Vector2(0, scrollSpeed * Time.deltaTime);
    }
}
