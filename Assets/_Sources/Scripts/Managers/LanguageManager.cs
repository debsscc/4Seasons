using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;
using Yarn.Unity;

// Gerencia o idioma do jogo.
//   Unity Localization (UI) → BCP-47: "pt-BR", "en"
//   YarnSpinner (diálogos)  → ISO 639-1: "pt", "en"
public class LanguageManager : Singleton<LanguageManager>
{
    private const string PREFS_KEY = "SelectedLanguage";

    // Identificadores internos de idioma (usados nos botões e no PlayerPrefs)
    public const string LANG_PT = "pt";
    public const string LANG_EN = "en";

    public string CurrentLanguage { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        ApplySavedOrDetectedLanguage();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyDialogueLanguage(CurrentLanguage);
    }

    private void ApplySavedOrDetectedLanguage()
    {
        string saved = PlayerPrefs.GetString(PREFS_KEY, string.Empty);

        string lang = !string.IsNullOrEmpty(saved)
            ? saved
            : DetectSystemLanguage();

        ApplyLanguage(lang, save: string.IsNullOrEmpty(saved));
    }

    private static string DetectSystemLanguage()
    {
        return Application.systemLanguage == SystemLanguage.Portuguese
            ? LANG_PT
            : LANG_EN;
    }

    // SetLanguage("pt") ou SetLanguage("en")
    public void SetLanguage(string lang)
    {
        ApplyLanguage(lang, save: true);
    }

    private void ApplyLanguage(string lang, bool save)
    {
        CurrentLanguage = lang;

        ApplyUILanguage(lang);
        ApplyDialogueLanguage(lang);

        if (save)
            PlayerPrefs.SetString(PREFS_KEY, lang);
    }

    // --- Unity Localization (textos de UI) ---
    private static void ApplyUILanguage(string lang)
    {
        string bcp47 = lang == LANG_PT ? "pt-BR" : "en";

        var locale = LocalizationSettings.AvailableLocales.GetLocale(bcp47);
        if (locale != null)
            LocalizationSettings.SelectedLocale = locale;
        else
            Debug.LogWarning($"[LanguageManager] UI locale '{bcp47}' não encontrado nas configurações de Localization.");
    }

    // --- YarnSpinner (diálogos) ---
    private static void ApplyDialogueLanguage(string lang)
    {
        // Inclui inativos: runners de feedback ficam desativados até o minigame terminar.
        var runners = FindObjectsByType<DialogueRunner>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var runner in runners)
        {
            // Ignora prefabs que estão em memória mas não carregados em cena.
            if (!runner.gameObject.scene.isLoaded) continue;

            if (runner.LineProvider is LineProviderBehaviour lineProvider)
                lineProvider.LocaleCode = lang;
            else
                Debug.LogWarning($"[LanguageManager] '{runner.name}' sem LineProviderBehaviour configurado.");
        }
    }
}
