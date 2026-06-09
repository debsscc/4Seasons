//----------------------------------------------
// FEITO POR: DEBS CARVALHO
// DATA: 2026-06-09
// DESCRIÇÃO: Anima os pontos de loading com base no idioma atual.
//----------------------------------------------

using UnityEngine;

[RequireComponent(typeof(Animator))]
public class LoadingDotsAnimator : MonoBehaviour
{
    [Tooltip("Nome do state no Animator para PT-BR")]
    public string statePT = "LoadingDots_PT";

    [Tooltip("Nome do state no Animator para EN")]
    public string stateEN = "LoadingDots_ENG";

    private Animator _animator;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        string lang = LanguageManager.Instance != null
            ? LanguageManager.Instance.CurrentLanguage
            : LanguageManager.LANG_PT;

        string state = lang == LanguageManager.LANG_PT ? statePT : stateEN;
        _animator.Play(state, 0, 0f);
    }
}
