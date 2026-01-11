using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class DVDCaseController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Visual")]
    public Color hoverOutlineColor = Color.white;

    [Header("DVD Interno")]
    public DraggablePrefab dvdDraggable;   
    public Transform dvdSpawnPosition;

    [Header("Animação")]
    public RectTransform caseTransform;
    public float openAnimationDuration = 0.5f;
    public AudioClip openCaseSfx;

    [Header("MiniGame")]
    public MiniGameController miniGameController;   

    private bool isOpen = false;
    private AudioSource audioSource;
    private Image caseImage;
    private Outline outline;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
        caseImage = GetComponent<Image>();
        
        // Pega o componente Outline 
        outline = GetComponent<Outline>();
        
        if (outline)
        {
            outline.effectColor = hoverOutlineColor;
            Color startColor = outline.effectColor;
            startColor.a = 0f;
            outline.effectColor = startColor;
        }

        if (dvdDraggable)
        {
            dvdDraggable.gameObject.SetActive(false);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isOpen) return;
        ShowOutline(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isOpen) return;
        ShowOutline(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isOpen) return;
        OpenCase();
    }

    private void ShowOutline(bool show)
    {
        if (outline == null) return;

        Color targetColor = hoverOutlineColor;
        targetColor.a = show ? 1f : 0f;

        DOTween.To(() => outline.effectColor, x => outline.effectColor = x, targetColor, 0.2f);
    }

    private void OpenCase()
    {
        isOpen = true;
        ShowOutline(false);

        if (openCaseSfx) audioSource.PlayOneShot(openCaseSfx);

        Sequence openSequence = DOTween.Sequence();
        openSequence.Append(caseTransform.DORotate(new Vector3(0, 0, -15f), openAnimationDuration * 0.5f));
        openSequence.Append(caseTransform.DORotate(Vector3.zero, openAnimationDuration * 0.5f));
        openSequence.Join(caseImage.DOFade(0.3f, openAnimationDuration));

        openSequence.OnComplete(RevealDVD);
    }

    private void RevealDVD()
    {
        if (dvdDraggable == null) return;

        dvdDraggable.gameObject.SetActive(true);

        if (dvdSpawnPosition)
            dvdDraggable.transform.position = dvdSpawnPosition.position;

        if (miniGameController != null)
        {
            dvdDraggable.MiniGameController = miniGameController;
            dvdDraggable.TargetSlots = miniGameController.targetSlots;
        }

        dvdDraggable.transform.localScale = Vector3.zero;
        dvdDraggable.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
    }
}