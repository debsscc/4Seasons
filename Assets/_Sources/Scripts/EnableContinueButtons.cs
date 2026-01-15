using UnityEngine;
using UnityEngine.UI;

public class EnableContinueButtons : MonoBehaviour
{
    [SerializeField] Button[] buttons;

    void Update()
    {
        foreach (var button in buttons)
        {
            if (!button.enabled || !button.interactable)
            {
                button.enabled = true;
                button.interactable = true;
            }
        }
    }
}
