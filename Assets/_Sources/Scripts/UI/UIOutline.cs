using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("UI/UIOutline")]
public class UIOutline : MonoBehaviour
{
    [SerializeField] private Color _effectColor = Color.white;
    [SerializeField] private float _effectDistance = 2f;

    private Image _outlineImage;
    private Material _outlineMaterial;

    public Color effectColor
    {
        get => _effectColor;
        set
        {
            _effectColor = value;
            if (_outlineMaterial != null)
                _outlineMaterial.color = value;
        }
    }

    public new bool enabled
    {
        get => _outlineImage != null && _outlineImage.gameObject.activeSelf;
        set { if (_outlineImage != null) _outlineImage.gameObject.SetActive(value); }
    }

    private void Start()
    {
        var source = GetComponent<Image>();
        if (source == null)
        {
            Debug.LogError($"[UIOutline] {gameObject.name} não tem Image component!", this);
            return;
        }

        var shader = Shader.Find("Custom/UIColorFill");
        if (shader == null)
        {
            Debug.LogError("[UIOutline] Shader 'Custom/UIColorFill' não encontrado! Verifique se o arquivo .shader está no projeto.", this);
            return;
        }

        var go = new GameObject("_UIOutline");
        go.transform.SetParent(transform.parent, false);
        go.transform.SetSiblingIndex(transform.GetSiblingIndex());

        var parentRT = GetComponent<RectTransform>();
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = parentRT.anchorMin;
        rt.anchorMax = parentRT.anchorMax;
        rt.anchoredPosition = parentRT.anchoredPosition;
        rt.sizeDelta = parentRT.sizeDelta + Vector2.one * _effectDistance * 2f;
        rt.pivot = parentRT.pivot;
        rt.localScale = parentRT.localScale;

        _outlineImage = go.AddComponent<Image>();
        _outlineImage.sprite = source.sprite;
        _outlineImage.type = source.type;
        _outlineImage.raycastTarget = false;

        _outlineMaterial = new Material(shader);
        _outlineMaterial.color = _effectColor;
        _outlineImage.material = _outlineMaterial;

        go.SetActive(false);

        Debug.Log($"[UIOutline] Outline criado em '{gameObject.name}' com shader OK.", this);
    }

    private void OnDestroy()
    {
        if (_outlineMaterial != null)
            Destroy(_outlineMaterial);
    }
}
