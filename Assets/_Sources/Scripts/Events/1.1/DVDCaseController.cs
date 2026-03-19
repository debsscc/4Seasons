using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class DVDCaseController : MonoBehaviour, IPointerClickHandler
{
    #region Serialized Fields

    [Header("Visual")]
    public Color hoverOutlineColor = Color.white;
    [Tooltip("Arraste aqui o Image exato que deve receber o outline (recomendado). Se vazio, o script tenta achar automaticamente nos filhos.")]
    [SerializeField] private Image outlineTargetImage;

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
    private UIOutline _outline;
    private UIOutlineHover _outlineHover;
    private RectTransform _rectTransform;
    private Vector2 _originalPosition;
    private Vector3 _originalScale;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
        _rectTransform = GetComponent<RectTransform>();
        _originalPosition = _rectTransform.anchoredPosition;
        _originalScale = _rectTransform.localScale;
        EnsureOutlineOnGraphic();
        HideDVD();
        if (backdropBlocker != null) backdropBlocker.SetActive(false);
    }

    #endregion

    #region Initialization

    private void EnsureOutlineOnGraphic()
    {
        var image = outlineTargetImage != null ? outlineTargetImage : GetComponentInChildren<Image>(true);
        if (image == null)
        {
            Debug.LogWarning($"[DVDCaseController] {name} e seus filhos não têm Image, outline não funcionará.");
            return;
        }

        image.raycastTarget = true;

        var targetObj = image.gameObject;

        _outline = targetObj.GetComponent<UIOutline>();
        if (_outline == null)
            _outline = targetObj.AddComponent<UIOutline>();

        _outline.enabled = true;
        var c = hoverOutlineColor;
        c.a = 0f;
        _outline.effectColor = c;

        _outlineHover = targetObj.GetComponent<UIOutlineHover>();
        if (_outlineHover == null)
            _outlineHover = targetObj.AddComponent<UIOutlineHover>();
    }

    private void HideDVD()
    {
        if (dvdDraggable != null)
            dvdDraggable.gameObject.SetActive(false);
    }

    #endregion

    #region Pointer Events

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

    private void SetOutlineAlpha(float alpha)
    {
        if (_outline == null) return;
        var c = _outline.effectColor;
        c.a = alpha;
        _outline.effectColor = c;
    }

    #endregion

    #region Focus / Unfocus

    private void FocusCase()
    {
        _state = State.Focused;
        if (_outlineHover != null) _outlineHover.enabled = false;
        SetOutlineAlpha(1f);

        if (backdropBlocker != null) backdropBlocker.SetActive(true);

        transform.SetAsLastSibling();

        _rectTransform.DOAnchorPos(Vector2.zero, focusDuration).SetEase(Ease.OutCubic);
        _rectTransform.DOScale(_originalScale * focusedScale, focusDuration).SetEase(Ease.OutBack);
    }

    private void UnfocusCase()
    {
        _state = State.Idle;

        if (backdropBlocker != null) backdropBlocker.SetActive(false);

        if (_outlineHover != null) _outlineHover.enabled = true;
        SetOutlineAlpha(0f);

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
            .SetEase(Ease.OutCubic)
            .OnComplete(() =>
            {
                if (_outlineHover != null) _outlineHover.enabled = true;
                SetOutlineAlpha(0f);
            });
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
