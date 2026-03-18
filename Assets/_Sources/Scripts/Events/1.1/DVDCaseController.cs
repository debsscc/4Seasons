using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class DVDCaseController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    #region Serialized Fields

    [Header("Visual")]
    public Color hoverOutlineColor = Color.white;

    [Header("DVD Interno")]
    public DraggablePrefab dvdDraggable;
    public Transform dvdSpawnPosition;

    [Header("Animação de Abertura")]
    public Animator caseAnimator;
    public string openAnimationTrigger = "Open";
    public float openAnimationDuration = 0.5f;
    public AudioClip openCaseSfx;

    [Header("Zoom ao Centro")]
    [Tooltip("Fator de escala ao focar (ex: 1.5 = 50% maior)")]
    public float focusedScale = 1.5f;
    [Tooltip("Duração da animação de zoom e retorno")]
    public float focusDuration = 0.35f;
    [Tooltip("Painel fullscreen transparente. O Button dele deve chamar OnBackdropClicked().")]
    public GameObject backdropBlocker;

    [Header("MiniGame")]
    public MiniGameController miniGameController;

    #endregion

    #region Private Fields

    private enum State { Idle, Focused, Opened }
    private State _state = State.Idle;

    private AudioSource audioSource;
    private Outline outline;
    private RectTransform _rectTransform;
    private Vector2 _originalPosition;
    private Vector3 _originalScale;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
        outline = GetComponent<Outline>();
        _rectTransform = GetComponent<RectTransform>();
        _originalPosition = _rectTransform.anchoredPosition;
        _originalScale = _rectTransform.localScale;
        SetupOutline();
        HideDVD();
        if (backdropBlocker != null) backdropBlocker.SetActive(false);
    }

    #endregion

    #region Initialization

    private void SetupOutline()
    {
        if (outline == null) return;
        Color c = hoverOutlineColor;
        c.a = 0f;
        outline.effectColor = c;
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
        if (_state == State.Idle) ShowOutline(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_state == State.Idle) ShowOutline(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_state == State.Idle)
            FocusCase();
        else if (_state == State.Focused)
            OpenCase();
    }

    public void OnBackdropClicked()
    {
        if (_state == State.Focused)
            UnfocusCase();
    }

    #endregion

    #region Visual Feedback

    private void ShowOutline(bool show)
    {
        if (outline == null) return;
        Color target = hoverOutlineColor;
        target.a = show ? 1f : 0f;
        DOTween.To(() => outline.effectColor, x => outline.effectColor = x, target, 0.2f);
    }

    #endregion

    #region Focus / Unfocus

    private void FocusCase()
    {
        _state = State.Focused;
        ShowOutline(false);

        if (backdropBlocker != null) backdropBlocker.SetActive(true);

        transform.SetAsLastSibling();

        _rectTransform.DOAnchorPos(Vector2.zero, focusDuration).SetEase(Ease.OutCubic);
        _rectTransform.DOScale(_originalScale * focusedScale, focusDuration).SetEase(Ease.OutBack);
    }

    private void UnfocusCase()
    {
        _state = State.Idle;

        if (backdropBlocker != null) backdropBlocker.SetActive(false);

        _rectTransform.DOAnchorPos(_originalPosition, focusDuration).SetEase(Ease.OutCubic);
        _rectTransform.DOScale(_originalScale, focusDuration).SetEase(Ease.OutCubic);
    }

    #endregion

    #region Open

    private void OpenCase()
    {
        _state = State.Opened;

        if (backdropBlocker != null) backdropBlocker.SetActive(false);

        if (openCaseSfx)
            audioSource.PlayOneShot(openCaseSfx);

        if (caseAnimator != null)
        {
            caseAnimator.SetTrigger(openAnimationTrigger);
            StartCoroutine(WaitAndReveal(openAnimationDuration));
        }
        else
        {
            RevealDVD();
        }

        _rectTransform.DOAnchorPos(_originalPosition, focusDuration)
            .SetDelay(openAnimationDuration)
            .SetEase(Ease.OutCubic);
        _rectTransform.DOScale(_originalScale, focusDuration)
            .SetDelay(openAnimationDuration)
            .SetEase(Ease.OutCubic);
    }

    private System.Collections.IEnumerator WaitAndReveal(float duration)
    {
        yield return new WaitForSeconds(duration);
        RevealDVD();
    }

    private void RevealDVD()
    {
        if (dvdDraggable == null) return;

        dvdDraggable.gameObject.SetActive(true);

        if (dvdSpawnPosition != null)
            dvdDraggable.transform.position = dvdSpawnPosition.position;

        if (miniGameController != null)
        {
            dvdDraggable.MiniGameController = miniGameController;
            dvdDraggable.TargetSlots = miniGameController.targetSlots;
        }

        dvdDraggable.transform.localScale = Vector3.zero;
        dvdDraggable.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
    }

    #endregion
}
