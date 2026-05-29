using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// Controla os botões de troca de idioma no Settings.
/// Conecte os dois botões e as cores no Inspector.
public class LanguageToggleUI : MonoBehaviour
{
    [Header("Botões")]
    public Button buttonPT;
    public Button buttonEN;

    [Header("Visual: idioma ativo")]
    public Color activeColor = Color.black;
    public Color inactiveColor = new Color(1f, 1f, 1f, 0.4f);

    private void OnEnable()
    {
        // Atualiza o visual quando o Settings é aberto
        RefreshButtons(LanguageManager.Instance != null
            ? LanguageManager.Instance.CurrentLanguage
            : LanguageManager.LANG_PT);
    }

    public void OnClickPT()
    {
        LanguageManager.Instance.SetLanguage(LanguageManager.LANG_PT);
        RefreshButtons(LanguageManager.LANG_PT);
    }

    public void OnClickEN()
    {
        LanguageManager.Instance.SetLanguage(LanguageManager.LANG_EN);
        RefreshButtons(LanguageManager.LANG_EN);
    }

    private void RefreshButtons(string currentLang)
    {
        SetButtonActive(buttonPT, currentLang == LanguageManager.LANG_PT);
        SetButtonActive(buttonEN, currentLang == LanguageManager.LANG_EN);
    }

    private void SetButtonActive(Button btn, bool active)
    {
        if (btn == null) return;


        // Muda a cor da imagem de fundo (opcional)
        var img = btn.GetComponent<Image>();
        if (img != null)
            img.color = active ? activeColor : inactiveColor;
    }
}
