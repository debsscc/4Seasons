using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Sirenix.OdinInspector;

public class UIButtonOdin : MonoBehaviour
{

    [FoldoutGroup("Visual"), Required, Tooltip("Imagem do botão na UI.")]
    public Image buttonImage;

    [FoldoutGroup("Visual"), PreviewField(60)]
    public Sprite defaultSprite;

    [FoldoutGroup("Visual"), PreviewField(60)]
    public Sprite hoverSprite;

    [FoldoutGroup("Visual"), PreviewField(60)]
    public Sprite selectedSprite;

    [FoldoutGroup("Audio")]
    public AudioClip clickSound;

    [FoldoutGroup("Audio")]
    public AudioClip hoverSound;

    [FoldoutGroup("Audio"), ShowIf("clickSound"), Range(0f, 1f)]
    public float clickVolume = 1f;


    [FoldoutGroup("Animation")]
    public Animator animator;

    [FoldoutGroup("Animation"), ShowIf("animator")]
    public string clickTrigger = "Click";

    [FoldoutGroup("Navigation"), Tooltip("Deixe vazio para não trocar de cena.")]
    public string sceneToLoad;

    [FoldoutGroup("Navigation"), Tooltip("Ou use um evento customizado.")]
    public UnityEngine.Events.UnityEvent onClick;


    private Button _button;

    private void Awake()
    {
        _button = GetComponent<Button>();

        if (_button == null)
        {
            Debug.LogError("UIButtonOdin: Nenhum Button encontrado no GameObject.");
            return;
        }

        _button.onClick.AddListener(HandleClick);
    }

    private void HandleClick()
    {
        if (clickSound)
            AudioSource.PlayClipAtPoint(clickSound, Vector3.zero, clickVolume);

        if (animator && !string.IsNullOrEmpty(clickTrigger))
            animator.SetTrigger(clickTrigger);

        if (!string.IsNullOrEmpty(sceneToLoad))
            SceneManager.LoadScene(sceneToLoad);

        onClick?.Invoke();
    }


    public void OnHover() => SetSprite(hoverSprite, hoverSound);

    public void OnSelected()
    {
        onClick?.Invoke();
        SetSprite(selectedSprite);
    }

    public void OnExit() => SetSprite(defaultSprite);

    private void SetSprite(Sprite sprite, AudioClip sound = null)
    {
        if (sprite && buttonImage)
            buttonImage.sprite = sprite;

        if (sound)
            AudioSource.PlayClipAtPoint(sound, Vector3.zero, 1f);
    }

    [FoldoutGroup("Debug"), InfoBox("Faltando referência buttonImage!", InfoMessageType.Error, "IsMissingButtonImage")]
    public bool showWarnings;

    private bool IsMissingButtonImage()
    {
        return buttonImage == null;
    }
}
    