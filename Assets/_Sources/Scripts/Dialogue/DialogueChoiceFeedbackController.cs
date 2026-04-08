using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DialogueChoiceFeedbackController : MonoBehaviour
{
    [SerializeField] private float emotionDuration = 3f;
    [SerializeField] private EmotionType gainEmotion = EmotionType.Happy;
    [SerializeField] private EmotionType loseEmotion = EmotionType.Sad;
    [SerializeField] private EmotionType loseFallbackEmotion = EmotionType.Angry;

    private class RunnerBinding
    {
        public CharacterAnimatorRunner runner;
        public CharacterData data;
        public int previousScore;
        public Coroutine resetCoroutine;
        public Action<int> handler;
    }

    private readonly List<RunnerBinding> _bindings = new();

    private void Awake()
    {
        Debug.Log("[DialogueChoiceFeedbackController] Awake OK.");
    }

    private void Start()
    {
        var scoreDialogue = FindFirstObjectByType<ScoreRulesDialogue>();
        if (scoreDialogue == null)
        {
            return;
        }

        // Constrói dicionário characterId → CharacterData a partir dos bindings já configurados no ScoreRulesDialogue
        var idToData = new Dictionary<string, CharacterData>(StringComparer.OrdinalIgnoreCase);
        foreach (var b in scoreDialogue.characterBindings)
        {
            if (b == null || b.characterData == null) continue;
            var key = b.characterId?.Trim();
            if (string.IsNullOrEmpty(key)) continue;
            if (!idToData.ContainsKey(key))
                idToData[key] = b.characterData;
        }
        var runners = FindObjectsByType<CharacterAnimatorRunner>(FindObjectsSortMode.None);

        foreach (var runner in runners)
        {
            var characterId = runner.GetCharacterId()?.Trim();
            if (!idToData.TryGetValue(characterId, out var data) || data == null)
            {
                Debug.LogWarning($"[DialogueChoiceFeedbackController] CharacterData não encontrado para '{characterId}' — verifique o characterId no CharacterIdentity e os bindings do ScoreRulesDialogue.");
                continue;
            }

            var binding = new RunnerBinding
            {
                runner = runner,
                data = data,
                previousScore = data.RelationshipScore
            };

            binding.handler = newScore => OnScoreChanged(binding, newScore);
            data.OnRelationshipChanged += binding.handler;
            _bindings.Add(binding);

            Debug.Log($"[DialogueChoiceFeedbackController] Inscrito em '{characterId}' (score inicial: {binding.previousScore}).");
        }
    }

    private void OnScoreChanged(RunnerBinding binding, int newScore)
    {
        bool gained = newScore > binding.previousScore;
        binding.previousScore = newScore;

        if (binding.resetCoroutine != null)
            StopCoroutine(binding.resetCoroutine);

        binding.resetCoroutine = StartCoroutine(ApplyFeedbackDelayed(binding, gained));
    }

    private IEnumerator ApplyFeedbackDelayed(RunnerBinding binding, bool gained)
    {
        // Aguarda 1 frame para o DialogueEmotionController.ForceApplyCurrentEmotion rodar antes e n bugar
        yield return null;

        if (gained)
            PlayWithFallback(binding.runner, gainEmotion, EmotionType.Normal);
        else
            PlayWithFallback(binding.runner, loseEmotion, loseFallbackEmotion);

        yield return new WaitForSeconds(emotionDuration);

        if (binding.runner != null)
            binding.runner.PlayAnimation(EmotionType.Normal);

        binding.resetCoroutine = null;
    }

    private void PlayWithFallback(CharacterAnimatorRunner runner, EmotionType first, EmotionType fallback)
    {
        if (HasTrigger(runner, first))
        {
            Debug.Log($"[DialogueChoiceFeedbackController] Tocando '{first}' em '{runner.GetCharacterId()}'");
            runner.PlayAnimation(first);
        }
        else if (HasTrigger(runner, fallback))
        {
            Debug.Log($"[DialogueChoiceFeedbackController] Fallback: tocando '{fallback}' em '{runner.GetCharacterId()}'");
            runner.PlayAnimation(fallback);
        }
    }

    // mapeando p n dar problema (EmotionType → nome do estado DEFAULT no AnimatorController base)
    private static readonly Dictionary<EmotionType, string> _emotionToStateName = new()
    {
        { EmotionType.Normal,     "DEFAULT_NORMAL"     },
        { EmotionType.Happy,      "Default_FELIZ"      },
        { EmotionType.Angry,      "DEFAULT_RAIVA"      },
        { EmotionType.Sad,        "DEFAULT_TRISTE"     },
        { EmotionType.Surprised,  "DEFAULT_SURPRESA"   },
        { EmotionType.Excited,    "DEFAULT_ANIMADO"    },
        { EmotionType.Flirty,     "DEFAULT_PROVOCANDO" },
        { EmotionType.Worried,    "DEFAULT_PREOCUPADO" },
        { EmotionType.Shy,        "DEFAULT_VERGONHA"   },
        { EmotionType.Thoughtful, "DEFAULT_PENSATIVA"  },
    };

    private bool HasTrigger(CharacterAnimatorRunner runner, EmotionType emotion)
    {
        var animator = runner.GetAnimator();
        if (animator == null) return false;

        // Se o animator usa OverrideController, verifica se o clip daquele estado ta posto ali ou n
        if (animator.runtimeAnimatorController is AnimatorOverrideController overrideCtrl)
        {
            if (!_emotionToStateName.TryGetValue(emotion, out var stateName))
                return false;

            var overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
            overrideCtrl.GetOverrides(overrides);

            foreach (var pair in overrides)
            {
                if (pair.Key != null && pair.Key.name == stateName)
                    return pair.Value != null;
            }
            // Estado não encontrado na lista de overrides = sem clip sobrescrito
            return false;
        }

        // verifica só se o trigger existe como parâmetro
        string triggerName = emotion.ToString();
        foreach (var param in animator.parameters)
        {
            if (param.type == AnimatorControllerParameterType.Trigger && param.name == triggerName)
                return true;
        }
        return false;
    }


    private void OnDestroy()
    {
        foreach (var binding in _bindings)
        {
            if (binding.data != null && binding.handler != null)
                binding.data.OnRelationshipChanged -= binding.handler;
        }
        _bindings.Clear();
    }
}
