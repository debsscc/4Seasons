using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

/// <summary>
/// Controla o comportamento de um DVD Case no MiniGame 1.1
/// Estados: Idle → Focused → Opened
/// </summary>
public class DVDCaseController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    #region Serialized Fields
    
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
    
    #endregion

    #region Private Fields
    
    private AudioSource audioSource;
    private Image caseImage;
    private Outline outline;

    private CaseState currentState = CaseState.Idle;
    private Vector3 originalPosition;
    private Vector3 originalScale;

    // Controle global: apenas 1 case pode estar focado por vez
    private static DVDCaseController currentlyFocusedCase = null;
    
    #endregion

    #region State Enum
    
    private enum CaseState
    {
        Idle,    // Estado inicial, pode ser clicado para focar
        Focused, // Centralizado na tela, pode ser clicado para abrir
        Opened   // Aberto, DVD revelado
    }
    
    #endregion

    #region Unity Lifecycle
    
    private void Awake()
    {
        InitializeComponents();
        StoreOriginalTransform();
        SetupOutline();
        HideDVD();
    }
    
    #endregion

    #region Initialization
    
    private void InitializeComponents()
    {
        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
        caseImage = GetComponent<Image>();
        outline = GetComponent<Outline>();
    }

    private void StoreOriginalTransform()
    {
        originalPosition = transform.position;
        originalScale = transform.localScale;
    }

    private void SetupOutline()
    {
        if (outline == null) return;

        outline.effectColor = hoverOutlineColor;
        Color startColor = outline.effectColor;
        startColor.a = 0f;
        outline.effectColor = startColor;
    }

    private void HideDVD()
    {
        if (dvdDraggable != null)
            dvdDraggable.gameObject.SetActive(false);
    }
    
    #endregion

    #region Pointer Events
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        // Mostra outline apenas se estiver Idle ou Focused
        if (currentState == CaseState.Idle || currentState == CaseState.Focused)
        {
            ShowOutline(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Esconde outline apenas se estiver Idle ou Focused
        if (currentState == CaseState.Idle || currentState == CaseState.Focused)
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
    
    #endregion

    #region Visual Feedback
    
    private void ShowOutline(bool show)
    {
        if (outline == null) return;

        Color targetColor = hoverOutlineColor;
        targetColor.a = show ? 1f : 0f;

        DOTween.To(() => outline.effectColor, x => outline.effectColor = x, targetColor, 0.2f);
    }
    
    #endregion

    #region State Transitions
    
    /// <summary>
    /// Idle → Focused: Move o case para o centro da tela e aumenta a escala
    /// </summary>
    private void FocusCase()
    {
        Debug.Log($"Focando no DVD Case: {name}");

        // Se já existe outro case focado, retorna ele ao estado Idle
        if (currentlyFocusedCase != null && currentlyFocusedCase != this)
        {
            currentlyFocusedCase.ReturnToIdle();
        }

        currentState = CaseState.Focused;
        currentlyFocusedCase = this;

        // Calcula posição central da tela
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

        // Anima movimento e escala
        transform.DOMove(worldCenter + focusScreenOffset, focusMoveDuration);
        transform.DOScale(originalScale * focusScale, focusMoveDuration)
            .SetEase(Ease.OutBack);
    }

    /// <summary>
    /// Focused → Idle: Retorna o case à posição e escala originais
    /// </summary>
    private void ReturnToIdle()
    {
        Debug.Log($"Retornando o DVD Case para o estado Idle: {name}");

        currentState = CaseState.Idle;

        // Limpa a referência global se for este case
        if (currentlyFocusedCase == this)
        {
            currentlyFocusedCase = null;
        }

        // Anima retorno
        transform.DOMove(originalPosition, 0.25f);
        transform.DOScale(originalScale, 0.25f)
            .SetEase(Ease.InOutQuad);

        ShowOutline(false);
    }

    /// <summary>
    /// Focused → Opened: Abre o case e revela o DVD
    /// </summary>
    private void OpenCase()
    {
        Debug.Log($"Abrindo o DVD Case: {name}");

        currentState = CaseState.Opened;
        ShowOutline(false);

        // Limpa a referência global
        if (currentlyFocusedCase == this)
        {
            currentlyFocusedCase = null;
        }

        // Toca som de abertura
        if (openCaseSfx)
            audioSource.PlayOneShot(openCaseSfx);

        // Usa animação do Animator se disponível, senão usa fallback DOTween
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
    
    #endregion

    #region Animation
    
    private System.Collections.IEnumerator WaitForAnimationAndReveal(float duration)
    {
        yield return new WaitForSeconds(duration);
        RevealDVD();
    }

    private void OpenCaseFallback()
    {
        Sequence openSequence = DOTween.Sequence();

        // Pequeno "bounce" de abertura
        openSequence.Append(caseTransform.DOScale(originalScale * (focusScale + 0.1f), 0.15f));
        openSequence.Append(caseTransform.DOScale(originalScale * focusScale, 0.15f));
        openSequence.Join(caseImage.DOFade(0.4f, 0.3f));

        openSequence.OnComplete(RevealDVD);
    }
    
    #endregion

    #region DVD Reveal
    
    /// <summary>
    /// Ativa o DVD e o torna draggable
    /// </summary>
    private void RevealDVD()
    {
        if (dvdDraggable == null) return;

        // Ativa o DVD
        dvdDraggable.gameObject.SetActive(true);

        // Posiciona o DVD
        if (dvdSpawnPosition)
            dvdDraggable.transform.position = dvdSpawnPosition.position;

        // Conecta o DVD ao MiniGameController
        if (miniGameController != null)
        {
            dvdDraggable.MiniGameController = miniGameController;
            dvdDraggable.TargetSlots = miniGameController.targetSlots;
        }

        // Anima aparição do DVD
        dvdDraggable.transform.localScale = Vector3.zero;
        dvdDraggable.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
    }
    
    #endregion
}