using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using DG.Tweening;

public class MiniGame2Scoring : MonoBehaviour, IMiniGameScoring
{
    [Header("Money and Hands")]
    public GameObject money1;
    public GameObject money2;
    public GameObject handIcon;

    [Header("Actions Buttons")]
    public Button actionButton;
    public TextMeshProUGUI actionButtonText;

    [Header("Feedback Modals")]
    public GameObject modalRobbed;
    public GameObject modalPurchased;

    [Header("Config")]
    [Tooltip("Timer to open the modal")]
    public float delayBeforeModal = 3f;

    [Header("Outline Basket hover")]
    public Outline basketOutline;
    public Color basketHoverColor = Color.yellow;

    [Header("Basket and Drinks")]
    public SlotDraggable basketSlot;
    public List<DrinksINFO> drinkItems = new List<DrinksINFO>();

    [Header("Characters")]
    public List<CharacterData> eventNpcs = new List<CharacterData>();
    public CharacterData playerCharacter;

    private bool _isStealing = false;
    private int _drinksInBasket = 0;

    private void Start()
    {
        if (actionButton != null)
        {
            actionButton.gameObject.SetActive(false);
            actionButton.onClick.AddListener(OnActionButtonClicked);
        }
        if (modalPurchased) modalPurchased.SetActive(false);
        if (modalRobbed) modalRobbed.SetActive(false);

        if (basketOutline != null)
        {
            basketOutline.enabled = false;
        }

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
    }

    public void OnDragOverSlot(DraggablePrefab draggable, SlotDraggable nearSlot)
    {
        if (basketOutline == null || basketSlot == null) return;
        
        if (nearSlot == basketSlot)
        {
            basketOutline.enabled = true;
            basketOutline.effectColor = basketHoverColor;
        }
        else
        {
            basketOutline.enabled = false;
        }
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
        slot.OnSuccessfulDrop();
        RecalculateBasketState();
    }

    public void OnDrinkRemovedFromBasket(DrinksINFO drink)
    {
        if (drink == null) return;

        drink.isInBasket = false;

        var t = drink.transform;
        if (t != null)
        {
            t.DOKill(); //function dotween that kills others dots
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

        if (actionButton != null)
        {
            if (_drinksInBasket == 0)
            {
                actionButton.gameObject.SetActive(false);
            }
            else
            {
                actionButton.gameObject.SetActive(true);
                if (actionButtonText != null)
                {
                    actionButtonText.text = _isStealing ? "ROUBAR" : "COMPRAR";
                }
                else
                {
                }
            }
        }
    }

    private void OnActionButtonClicked()
    {
        ApplyScoring();
        StartCoroutine(ShowModalAfterDelay());
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