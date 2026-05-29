using Sirenix.OdinInspector;
using UnityEngine;

public class CharacterAnimatorRunner : MonoBehaviour
{
    [SerializeField] private CharacterIdentity characterIdentity;
    [SerializeField] private DialogueEmotionController dialogueEmotionController;
    [SerializeField] private Animator characterAnimator;

    private EmotionType _currentEmotion = EmotionType.Normal;

    void Start()
    {
        dialogueEmotionController.AddAnimatorRunner(this);

        if(!characterAnimator)
        {
            characterAnimator = GetComponent<Animator>();
        }

        PlayAnimation(EmotionType.Normal);
    }

    /// <param name="force">Se true, aplica mesmo que a emoção já esteja ativa (previews, ForceApply, Update).
    /// Se false, pula se já está no mesmo estado (feedback de score — evita flash de re-entrada).</param>
    public void PlayAnimation(EmotionType animationTrigger, bool force = false)
    {
        if (!force && animationTrigger == _currentEmotion) return;

        // Reseta todos os triggers pendentes antes de ativar o novo,
        // prevenindo acúmulo durante navegação rápida entre opções.
        foreach (var param in characterAnimator.parameters)
            if (param.type == AnimatorControllerParameterType.Trigger)
                characterAnimator.ResetTrigger(param.name);

        characterAnimator.SetTrigger(animationTrigger.ToString());
        _currentEmotion = animationTrigger;
    }

    public void ResetCurrentEmotion()
    {
        _currentEmotion = EmotionType.Normal;
    }

    public Animator GetAnimator() => characterAnimator;

    public string GetCharacterId()
    {
        return characterIdentity.characterId;
    }

    [Button]
    public void TestPlayAnimation(EmotionType emontionType)
    {
        PlayAnimation(emontionType);
    }
}
