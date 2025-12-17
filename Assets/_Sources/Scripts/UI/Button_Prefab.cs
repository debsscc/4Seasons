using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Sirenix.OdinInspector;
using System.Collections;

public class Button_Prefab : MonoBehaviour
{
    [FoldoutGroup("Visual"), Required, Tooltip("Imagem do botão na UI.")]
    public Image buttonImage;

    [FoldoutGroup("Visual"), PreviewField(60), Tooltip("Sprite padrão (normal).")]
    public Sprite defaultSprite;

    [FoldoutGroup("Visual"), PreviewField(60), Tooltip("Sprite ao passar o mouse (Hover).")]
    public Sprite hoverSprite;

    [FoldoutGroup("Visual"), PreviewField(60), Tooltip("Sprite selecionado ou pressionado.")]
    public Sprite selectedSprite;

    [FoldoutGroup("Audio"), Tooltip("Som reproduzido ao clicar no botão.")]
    public AudioClip clickSound;

    [FoldoutGroup("Audio"), Tooltip("Som reproduzido ao passar o mouse.")]
    public AudioClip hoverSound;

    [FoldoutGroup("Audio"), ShowIf("@this.clickSound != null"), Range(0f, 1f)]
    public float clickVolume = 1f;


    [FoldoutGroup("Animation"), Tooltip("Animator no botão ou em objeto filho.")]
    public Animator animator;

    [FoldoutGroup("Animation"), ShowIf("@this.animator != null"), Tooltip("Trigger do Animator para clique.")]
    public string clickTrigger = "Click";


    [FoldoutGroup("Navigation"), Tooltip("Modo especial: usa a seleção do MapSelectionManager ao invés de sceneToLoadName.")]
    public bool isMapConfirmButton = false;

    [FoldoutGroup("Navigation"), HideIf("isMapConfirmButton"), ValueDropdown("SceneDropdown")]
    public string sceneToLoadName;

    [FoldoutGroup("Navigation"), Tooltip("Eventos customizados disparados após o clique.")]
    public UnityEngine.Events.UnityEvent onClick;


    private Button _button;


    private void Awake()
    {
        _button = GetComponent<Button>();

        if (_button == null)
        {
            Debug.LogError("Button_Prefab: Nenhum Button encontrado no GameObject.");
            return;
        }

        _button.onClick.AddListener(HandleClick);

        if (buttonImage && defaultSprite)
            buttonImage.sprite = defaultSprite;
    }


    private void HandleClick()
    {
        if (clickSound)
            AudioSource.PlayClipAtPoint(clickSound, Vector3.zero, clickVolume);

        if (animator && !string.IsNullOrEmpty(clickTrigger))
            animator.SetTrigger(clickTrigger);

        if (isMapConfirmButton)
        {
            HandleMapConfirm();
        }
        else if (!string.IsNullOrEmpty(sceneToLoadName))
        {
            SceneTransition.Instance.ChangeScene(sceneToLoadName);
        }

        onClick?.Invoke();
    }

    private void HandleMapConfirm()
    {
        if (MapSelectionManager.Instance == null)
        {
            Debug.LogError("Button_Prefab: MapSelectionManager.Instance não encontrado!");
            return;
        }

        string selectedScene = MapSelectionManager.Instance.GetPendingSceneName();

        if (string.IsNullOrEmpty(selectedScene))
        {
            Debug.LogWarning("Button_Prefab: Nenhum mapa selecionado para confirmar.");
            return;
        }

        Debug.Log($"Button_Prefab: Confirmando seleção do mapa: {selectedScene}");
        MapSelectionManager.Instance.ConfirmSelection();
    }


    public void OnHover()
        => SetSprite(hoverSprite, hoverSound);

    public void OnSelected()
        => SetSprite(selectedSprite);

    public void OnExit()
        => SetSprite(defaultSprite);


    private void SetSprite(Sprite sprite, AudioClip sound = null)
    {
        if (buttonImage && sprite)
            buttonImage.sprite = sprite;

        if (sound)
            AudioSource.PlayClipAtPoint(sound, Vector3.zero);
    }

    private static IEnumerable SceneDropdown()
    {
        int sceneCount = SceneManager.sceneCountInBuildSettings;

        for (int i = 0; i < sceneCount; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            string name = System.IO.Path.GetFileNameWithoutExtension(path);
            yield return name;
        }
    }
}