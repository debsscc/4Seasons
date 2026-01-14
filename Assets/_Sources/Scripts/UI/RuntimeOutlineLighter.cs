using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class RuntimeOutlineLighter : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Color hoverColor = Color.white;

    private Outline outline;

    void Awake()
    {
        // Garantir Graphic
        var graphic = GetComponent<Graphic>();
        if (graphic == null)
        {
            Debug.LogError($"[RuntimeOutlineHover] {name} sem Graphic!");
            return;
        }

        graphic.raycastTarget = true;

        // Garantir Outline
        outline = GetComponent<Outline>();
        if (outline == null)
            outline = gameObject.AddComponent<Outline>();

        outline.effectColor = hoverColor;
        outline.effectDistance = new Vector2(4, -4);

        Color c = outline.effectColor;
        c.a = 0f;
        outline.effectColor = c;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        FadeOutline(1f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        FadeOutline(0f);
    }

    void FadeOutline(float alpha)
    {
        if (outline == null) return;

        Color target = hoverColor;
        target.a = alpha;

        DOTween.To(
            () => outline.effectColor,
            x => outline.effectColor = x,
            target,
            0.2f
        );
    }
}