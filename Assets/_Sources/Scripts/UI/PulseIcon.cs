using UnityEngine;

public class PulseIcon : MonoBehaviour
{
    public float speed = 2;
    private CanvasGroup cg;

    void Awake()
    {
        cg = GetComponent<CanvasGroup>();
        if (cg == null) cg = gameObject.AddComponent<CanvasGroup>();
    }

    void Update()
    {
       cg.alpha = (Mathf.Sin(Time.time * speed) + 1) / 2f;
    }
}

