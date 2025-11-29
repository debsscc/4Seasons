using UnityEngine;
using UnityEngine.UI;

public class SeatButton : MonoBehaviour
{
    public ItemOpcao assento;
    public bool isFree;

    public Button uiButton;

    private void Awake()
    {
        if (uiButton == null)
            uiButton = GetComponent<Button>();
    }

    public void AtualizarVisual()
    {
        uiButton.interactable = isFree;
        uiButton.image.color = isFree ? Color.green : Color.red;
    }
}
