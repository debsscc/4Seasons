using UnityEngine;
using Sirenix.OdinInspector; 

public class UICursorChange : MonoBehaviour
{
    [TitleGroup("Cursor Settings")]
    [Tooltip("A textura personalizada para o cursor.")]
    [PreviewField(Alignment = ObjectFieldAlignment.Left, Height = 60)]
    public Texture2D cursorTexture;

    [TitleGroup("Cursor Settings")]
    [Tooltip("O ponto de clique (hotspot) da textura. Ex: (0, 0) para ponta da seta.")]
    public Vector2 hotspot = Vector2.zero;
    
    [TitleGroup("Drag Cursor Settings")]
    [Tooltip("A textura do cursor enquanto estiver arrastando.")]
    [PreviewField(Alignment = ObjectFieldAlignment.Left, Height = 60)]
    public Texture2D dragCursorTexture;
    [Tooltip("Hotspot do cursor de arrasto.")]
    public Vector2 dragHotspot = Vector2.zero;
    private bool _isDragging;

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (cursorTexture == null)
        {
            Debug.LogWarning("UICursorChange: Nenhuma textura de cursor atribuída. Certifique-se de configurar a textura e o Event Trigger.");
        }
    }

    public void OnHoverCursor()
    {
        if (_isDragging) return;
        if (cursorTexture != null)
        {
            Cursor.SetCursor(cursorTexture, hotspot, CursorMode.Auto);
        }
    }

    public void OnBeginDragCursor()
    {
        _isDragging = true;

        var tex = dragCursorTexture != null ? dragCursorTexture : cursorTexture;
        var hot = dragCursorTexture != null ? dragHotspot : hotspot;

        if (tex != null)
            Cursor.SetCursor(tex, hot, CursorMode.Auto);
    }

    public void OnEndDragCursor()
    {
        _isDragging = false;

        // Ao soltar, volta ao padrão (ou você pode chamar OnHoverCursor se quiser manter hover)
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }

    public void OnExitCursor()
    {
        if (_isDragging) return;
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }
}