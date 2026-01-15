using Sirenix.OdinInspector;
using UnityEngine;

public class CharacterAnimatorRunner : MonoBehaviour
{
    [SerializeField] private CharacterIdentity characterIdentity;
    [SerializeField] private DialogueEmotionController dialogueEmotionController;
    [SerializeField] private Animator characterAnimator;

    void Start()
    {
        // dialogueEmotionController.AddAnimatorRunner(this);

        // if(!characterAnimator)
        // {
        //     characterAnimator = GetComponent<Animator>();
        // }

        // PlayAnimation(EmotionType.Normal);
    }

    public void PlayAnimation(EmotionType animationTrigger)
    {
        Debug.Log($"[CharacterAnimatorRunner] Playing animation '{animationTrigger}' for character '{characterIdentity.characterId}'");
        characterAnimator.SetTrigger(animationTrigger.ToString());
    }

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
