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
    public Animator caseAnimator;
    public string openAnimationTrigger = "Open";
    public float openAnimationDuration = 0.5f;
    public AudioClip openCaseSfx;

    [Header("MiniGame")]
    public MiniGameController miniGameController;   

    [Header("Focus / Zoom")]
    public float focusScale = 1.6f;
    public float focusMoveDuration = 0.35f;
    public Vector3 focusScreenOffset = Vector3.zero;

    private AudioSource audioSource;
    private Image caseImage;
    private Outline outline;

    private enum CaseState
    {
        Idle,
        Focused,
        Opened 
    }
    private CaseState currentState = CaseState.Idle;

    private Vector3 originalPosition;
    private Vector3 originalScale;

    // ✅ Controle global para evitar múltiplos DVDs focados
    private static DVDCaseController currentlyFocusedCase = null;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
        caseImage = GetComponent<Image>();
        outline = GetComponent<Outline>();

        originalPosition = transform.position;
        originalScale = transform.localScale;

        if (outline)
        {
            outline.effectColor = hoverOutlineColor;
            Color startColor = outline.effectColor;
            startColor.a = 0f;
            outline.effectColor = startColor;
        }

        if (dvdDraggable)
            dvdDraggable.gameObject.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (currentState == CaseState.Idle)
        {
            ShowOutline(true);
        }
        // ✅ Se estiver Focused, mantém o outline visível
        else if (currentState == CaseState.Focused)
        {
            ShowOutline(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // ✅ NÃO volta pro Idle se estiver Focused (evita bug de movimento)
        if (currentState == CaseState.Idle)
        {
            ShowOutline(false);
        }
        // ✅ Se estiver Focused, só esconde o outline mas mantém o estado
        else if (currentState == CaseState.Focused)
        {
            ShowOutline(false);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        switch (currentState)
        {
            case CaseState.Idle:
                FocusCase();
                break;

            case CaseState.Focused:
                OpenCase();
                break;

            case CaseState.Opened:
                // Já aberto, não faz nada
                break;
        }
    }

    private void ShowOutline(bool show)
    {
        if (outline == null) return;

        Color targetColor = hoverOutlineColor;
        targetColor.a = show ? 1f : 0f;

        DOTween.To(() => outline.effectColor, x => outline.effectColor = x, targetColor, 0.2f);
    }

    private void FocusCase()
    {
        Debug.Log($"Focando no DVD Case: {name}");

        // ✅ Se já existe outro DVD focado, volta ele pro Idle primeiro
        if (currentlyFocusedCase != null && currentlyFocusedCase != this)
        {
            currentlyFocusedCase.ReturnToIdle();
        }

        currentState = CaseState.Focused;
        currentlyFocusedCase = this;

        Vector3 screenCenter = new Vector3(
            Screen.width / 2f,
            Screen.height / 2f,
            0f
        );

        RectTransformUtility.ScreenPointToWorldPointInRectangle(
            caseTransform,
            screenCenter,
            null,
            out Vector3 worldCenter
        );

        transform.DOMove(worldCenter + focusScreenOffset, focusMoveDuration);
        transform.DOScale(originalScale * focusScale, focusMoveDuration)
            .SetEase(Ease.OutBack);
    }

    private void ReturnToIdle()
    {
        Debug.Log($"Retornando o DVD Case para o estado Idle: {name}");
        
        currentState = CaseState.Idle;

        // ✅ Limpa a referência global se for este DVD
        if (currentlyFocusedCase == this)
        {
            currentlyFocusedCase = null;
        }

        transform.DOMove(originalPosition, 0.25f);
        transform.DOScale(originalScale, 0.25f)
            .SetEase(Ease.InOutQuad);
        
        ShowOutline(false);
    }

    private void OpenCase()
    {
        Debug.Log($"Abrindo o DVD Case: {name}");
        
        currentState = CaseState.Opened;
        ShowOutline(false);

        // ✅ Limpa a referência global
        if (currentlyFocusedCase == this)
        {
            currentlyFocusedCase = null;
        }

        if (openCaseSfx) 
            audioSource.PlayOneShot(openCaseSfx);

        if (caseAnimator != null)
        {
            caseAnimator.SetTrigger(openAnimationTrigger);
            StartCoroutine(WaitForAnimationAndReveal(openAnimationDuration));
            Debug.Log("Abrindo DVD Case com animação do Animator.");
        }
        else
        {
            Debug.Log("Abrindo DVD Case com fallback DOTween.");
            OpenCaseFallback();
        }
    }

    private System.Collections.IEnumerator WaitForAnimationAndReveal(float duration)
    {
        yield return new WaitForSeconds(duration);
        RevealDVD();
    }

    private void OpenCaseFallback()
    {
        Sequence openSequence = DOTween.Sequence();
        
        openSequence.Append(caseTransform.DOScale(originalScale * (focusScale + 0.1f), 0.15f));
        openSequence.Append(caseTransform.DOScale(originalScale * focusScale, 0.15f));
        openSequence.Join(caseImage.DOFade(0.4f, 0.3f));

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