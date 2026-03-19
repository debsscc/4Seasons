using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using DG.Tweening;

public class MiniGame2Scoring : MonoBehaviour, IMiniGameScoring
{
    [Serializable]
    public class FavoriteDrinkConfig
    {
        [Tooltip("Personagem correspondente (usado para determinar o characterId automaticamente)")]
        public CharacterData character;

        [Tooltip("Opcional: ID do personagem (use apenas se não quiser depender de CharacterData.name)")]
        public string characterId;

        [Tooltip("Lista de drinks favoritos (verifica também pelo nome do ItemsSO)")]
        public List<ItemsSO> favoriteItems = new List<ItemsSO>();

        public string GetCharacterId()
        {
            if (character != null)
                return character.name;
            return characterId;
        }
    }

    [Header("Money and Hands")]
    public GameObject money1;
    public GameObject money2;
    public GameObject handIcon;

    [Header("Actions Buttons")]
    public Button confirmButton;
    public Button stealButton;

    [Header("Feedback Modals")]
    public GameObject modalRobbed;
    public GameObject modalPurchased;

    [Header("Config")]
    [Tooltip("Timer to open the modal")]
    public float delayBeforeModal = 3f;

    [Header("Basket and Drinks")]
    public SlotDraggable basketSlot;
    public List<DrinksINFO> drinkItems = new List<DrinksINFO>();

    [Header("Characters")]
    public List<CharacterData> eventNpcs = new List<CharacterData>();
    public CharacterData playerCharacter;

    [Header("Favorite Drinks (Heart Feedback)")]
    [Tooltip("Configurações de drinks favoritos para cada personagem (usado apenas para coração de feedback)")]
    public List<FavoriteDrinkConfig> favoriteDrinkConfigs = new List<FavoriteDrinkConfig>();

    private bool _isStealing = false;
    private int _drinksInBasket = 0;

    private void Start()
    {
        if (confirmButton != null)
        {
            confirmButton.gameObject.SetActive(false);
            confirmButton.onClick.AddListener(OnActionButtonClicked);
        }

        if (stealButton != null)
        {
            stealButton.gameObject.SetActive(false);
            stealButton.onClick.AddListener(OnActionButtonClicked);
        }

        if (modalPurchased) modalPurchased.SetActive(false);
        if (modalRobbed) modalRobbed.SetActive(false);

        if (drinkItems.Count == 0)
        {
            var allDrinks = FindObjectsByType<DrinksINFO>(FindObjectsSortMode.None);
            drinkItems.AddRange(allDrinks);
        }

        if (eventNpcs.Count == 0 && CharactersManager.Instance != null)
        {
            eventNpcs = CharactersManager.Instance.npcs;
        }

        if (playerCharacter == null && CharactersManager.Instance != null)
        {
            playerCharacter = CharactersManager.Instance.playerCharacter;
        }
    }

    public void OnItemRemovedFromSlot()
    {
        Debug.Log("[MiniGame2] Item removido do slot.");
        RecalculateBasketState();
    }

    public void OnObjectDropped(SlotDraggable slot, ItemsSO[] items)
    {
        if (slot != basketSlot)
            return;

        DrinksINFO drink = null;
        if (items != null && items.Length > 0)
        {
            foreach (var d in drinkItems)
            {
                if (d==null) continue;
                foreach (var t in d.drinkTypes)
                {
                    if (t == null) continue;
                    if (System.Array.IndexOf(items,t) > 0)
                    {
                        drink = d;
                        break;
                    }
                }
                if (drink != null) break;
            }
        }
        if (drink == null)
        {
            drink = GetDrinkFromSlot(slot);
        }
        if (drink == null)
        {
            Debug.LogWarning("[MiniGame2] Bebida nula ao adicionar à cesta.");
            return;
        }

        drink.isInBasket = true;
        Debug.Log($"[MiniGame2] Bebida '{drink.name}' adicionada à cesta.");
        RecalculateBasketState();

        if (MiniGameFeedbackManager.Instance != null)
        MiniGameFeedbackManager.Instance.ApplyPreview(items);
        Debug.Log($"[MiniGame2] Aplicando preview para {items.Length} itens.");

    }

    public void OnDrinkRemovedFromBasket(DrinksINFO drink)
    {
        if (drink == null) return;

        drink.isInBasket = false;

        var t = drink.transform;
        if (t != null)
        {
            t.DOKill();
            Vector3 originalScale = Vector3.one;

            t.localScale = originalScale;
            t.DOScale(originalScale * 1.05f, 0.08f)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                t.DOScale(originalScale, 0.08f).SetEase(Ease.InQuad);
            });
        }
        RecalculateBasketState();
    }

    private void RecalculateBasketState()
    {
        _drinksInBasket = 0;
        foreach (var drink in drinkItems)
        {
            if (drink != null && drink.isInBasket)
                _drinksInBasket++;
        }

        Debug.Log($"[MiniGame2] Bebidas na cesta: {_drinksInBasket}");
        UpdateMoneyAndButton();
    }

    public int GetDrinksInBasketCount()
    {
        return _drinksInBasket;
    }

    private void UpdateMoneyAndButton()
    {
        _isStealing = _drinksInBasket > 2;

        if (money1) money1.SetActive(_drinksInBasket == 0 || _drinksInBasket == 1);
        if (money2) money2.SetActive(_drinksInBasket == 0);

        if (handIcon)
        {
            handIcon.SetActive(_isStealing);
        }

        if (confirmButton != null)
        {
            if (_drinksInBasket == 0)
            {
                confirmButton.gameObject.SetActive(false);
                stealButton.gameObject.SetActive(false);
            }
            else
            {
                stealButton.gameObject.SetActive(_isStealing);
                confirmButton.gameObject.SetActive(!_isStealing);
            }
        }
    }

    private void OnActionButtonClicked()
    {
        ApplyScoring();
        ApplyHeartFeedback();
        StartCoroutine(ShowModalAfterDelay());
    }

    private FavoriteDrinkConfig GetFavoriteConfig(string characterId)
    {
        if (string.IsNullOrEmpty(characterId)) return null;
        return favoriteDrinkConfigs.Find(x => x.GetCharacterId() == characterId);
    }

    private bool IsFavoriteDrinkForCharacter(string characterId, ItemsSO drinkItem)
    {
        if (drinkItem == null) return false;

        var config = GetFavoriteConfig(characterId);
        if (config != null && config.favoriteItems != null && config.favoriteItems.Count > 0)
        {
            if (config.favoriteItems.Contains(drinkItem))
                return true;

            foreach (var fav in config.favoriteItems)
            {
                if (fav != null && fav.name == drinkItem.name)
                    return true;
            }

            return false;
        }

        // Fallback para os favoritos do CharacterData (se existir)
        var character = eventNpcs.Find(c => c != null && c.name == characterId);
        if (character == null && playerCharacter != null && playerCharacter.name == characterId)
            character = playerCharacter;

        if (character != null)
            return character.LikesItem(drinkItem);

        return false;
    }

    private void ApplyHeartFeedback()
    {
        if (MiniGameFeedbackManager.Instance == null) return;

        var chosenDrinks = new List<DrinksINFO>();
        foreach (var drink in drinkItems)
        {
            if (drink != null && drink.isInBasket)
                chosenDrinks.Add(drink);
        }

        void ShowForCharacterId(string characterId)
        {
            if (string.IsNullOrEmpty(characterId)) return;

            bool choseFavorite = false;
            foreach (var drink in chosenDrinks)
            {
                foreach (var drinkItemSO in drink.drinkTypes)
                {
                    if (drinkItemSO == null) continue;
                    if (IsFavoriteDrinkForCharacter(characterId, drinkItemSO))
                    {
                        choseFavorite = true;
                        break;
                    }
                }

                if (choseFavorite) break;
            }

            MiniGameFeedbackManager.Instance.ShowHeart(characterId, choseFavorite);
        }

        void ShowForConfig(FavoriteDrinkConfig config)
        {
            if (config == null) return;
            string id = config.GetCharacterId();
            if (string.IsNullOrEmpty(id)) return;

            bool choseFavorite = false;
            foreach (var drink in chosenDrinks)
            {
                foreach (var drinkItemSO in drink.drinkTypes)
                {
                    if (drinkItemSO == null) continue;
                    if (IsFavoriteDrinkForCharacter(id, drinkItemSO))
                    {
                        choseFavorite = true;
                        break;
                    }
                }

                if (choseFavorite) break;
            }

            MiniGameFeedbackManager.Instance.ShowHeart(id, choseFavorite);
        }

        // Primeiro, aplique configurações explícitas do Inspector
        foreach (var config in favoriteDrinkConfigs)
            ShowForConfig(config);

        // Depois, aplique para os NPCs que não foram cobertos pelo config
        foreach (var npc in eventNpcs)
        {
            if (npc == null) continue;
            if (GetFavoriteConfig(npc.name) != null) continue;
            ShowForCharacterId(npc.name);
        }

        if (playerCharacter != null && GetFavoriteConfig(playerCharacter.name) == null)
            ShowForCharacterId(playerCharacter.name);
    }

    private IEnumerator ShowModalAfterDelay()
    {
        yield return new WaitForSeconds(delayBeforeModal);

        if (_isStealing)
        {
            if (modalRobbed) modalRobbed.SetActive(true);
        }
        else
        {
            if (modalPurchased) modalPurchased.SetActive(true);
        }
    }

    private void ApplyScoring()
    {
        List<DrinksINFO> chosenDrinks = new List<DrinksINFO>();
        foreach (var drink in drinkItems)
        {
            if (drink != null && drink.isInBasket)
                chosenDrinks.Add(drink);
        }

        foreach (var npc in eventNpcs)
        {
            if (npc == null) continue;

            int delta = 0;

            bool choseNpcFavorite = false;
            foreach (var drink in chosenDrinks)
            {
                foreach (var drinkItemSO in drink.drinkTypes)
                {
                    if (npc.favoriteItems.Contains(drinkItemSO))
                    {
                        choseNpcFavorite = true;
                        break;
                    }
                }
                if (choseNpcFavorite) break;
            }

            if (choseNpcFavorite)
            {
                delta += 2;
            }
            else
            {
                delta -= 1;
            }
            foreach (var drink in chosenDrinks)
            {
                bool drinkMatchesTrait = false;

                if (npc.traits.isRebel && drink.traits.isRebel)
                    drinkMatchesTrait = true;
                else if (npc.traits.isCorrect && drink.traits.isCorrect)
                    drinkMatchesTrait = true;

                if (drinkMatchesTrait)
                    delta += 1;
                else
                    delta -= 1;
            }
            if (_isStealing)
            {
                if (npc.traits.isRebel)
                    delta += 3;
                else if (npc.traits.isCorrect)
                    delta -= 2;
            }
            else
            {
                if (npc.traits.isCorrect)
                    delta += 3;
                else if (npc.traits.isRebel)
                    delta -= 2;
            }

            int before = npc.RelationshipScore;
            npc.RelationshipScore += delta;
        }
        if (playerCharacter != null)
        {
            int selfDelta = 0;

            bool chosePlayerFavorite = false;
            foreach (var drink in chosenDrinks)
            {
                foreach (var drinkItemSO in drink.drinkTypes)
                {
                    if (playerCharacter.favoriteItems.Contains(drinkItemSO))
                    {
                        chosePlayerFavorite = true;
                        break;
                    }
                }
                if (chosePlayerFavorite) break;
            }

            if (chosePlayerFavorite)
                selfDelta += 2;
            else
                selfDelta -= 1;

            foreach (var drink in chosenDrinks)
            {
                bool drinkMatchesTrait = false;

                if (playerCharacter.traits.isRebel && drink.traits.isRebel)
                    drinkMatchesTrait = true;
                else if (playerCharacter.traits.isCorrect && drink.traits.isCorrect)
                    drinkMatchesTrait = true;

                if (drinkMatchesTrait)
                    selfDelta += 1;
                else
                    selfDelta -= 1;
            }

            if (_isStealing)
            {
                if (playerCharacter.traits.isRebel)
                    selfDelta += 2;
                else if (playerCharacter.traits.isCorrect)
                    selfDelta -= 2;
            }

            int beforeSelf = playerCharacter.RelationshipScore;
            playerCharacter.RelationshipScore += selfDelta;
        }
    }

    public void RegisterDrink(DrinksINFO drink)
    {
        if (drink != null && !drinkItems.Contains(drink))
        {
            drinkItems.Add(drink);
        }
    }

    private DrinksINFO GetDrinkFromSlot(SlotDraggable slot)
    {
        if (slot == null) return null;
        if (slot.lastDroppedObject != null)
        {
            var drink = slot.lastDroppedObject.GetComponent<DrinksINFO>()
            ??slot.lastDroppedObject.GetComponentInChildren<DrinksINFO>();
            if (drink != null) return drink;
        }
        return null;
    }
}
