using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("UI/UIOutline")]
public class UIOutline : MonoBehaviour
{
    [SerializeField] private Color _effectColor = Color.white;
    [SerializeField] private float _outlineSize = 5f;
    [SerializeField] private float _outlineSoftness = 0.5f;

    private Image _image;
    private Material _material;

    private static readonly int OutlineColorProp = Shader.PropertyToID("_OutlineColor");
    private static readonly int OutlineSizeProp  = Shader.PropertyToID("_OutlineSize");
    private static readonly int OutlineSoftProp  = Shader.PropertyToID("_OutlineSoftness");

    public Color effectColor
    {
        get => _effectColor;
        set
        {
            _effectColor = value;
            if (_material != null)
                _material.SetColor(OutlineColorProp, value);
        }
    }

    public new bool enabled
    {
        get => _material != null && _material.IsKeywordEnabled("USE_OUTLINE");
        set
        {
            if (_material == null) return;
            if (value) _material.EnableKeyword("USE_OUTLINE");
            else       _material.DisableKeyword("USE_OUTLINE");
        }
    }

    private void Awake()
    {
        _image = GetComponent<Image>();
        if (_image == null)
        {
            Debug.LogError($"[UIOutline] {gameObject.name} não tem Image component!", this);
            return;
        }

        var shader = Shader.Find("Custom/UI/Outline");
        if (shader == null)
        {
            Debug.LogError("[UIOutline] Shader 'Custom/UI/Outline' não encontrado!", this);
            return;
        }

        _material = new Material(shader);
        _material.SetColor(OutlineColorProp, _effectColor);
        _material.SetFloat(OutlineSizeProp, _outlineSize);
        _material.SetFloat(OutlineSoftProp, _outlineSoftness);
        _material.DisableKeyword("USE_OUTLINE");

        _image.material = _material;
    }

    private void OnDestroy()
    {
        if (_material != null)
            Destroy(_material);
    }
}
