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
        if (cursorTexture != null)
        {
            Cursor.SetCursor(cursorTexture, hotspot, CursorMode.Auto);
        }
    }

    public void OnExitCursor()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }
}