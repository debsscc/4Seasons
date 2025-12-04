using System.Collections.Generic;
using UnityEngine;

public class DrinksINFO : MonoBehaviour, IItemHolder
{
    public ItemsSO[] drinkTypes;
    public ItemsSO[] Items => drinkTypes;

    public TraitsData traits;

public interface IItemHolder
{
    public ItemsSO[] Items { get; }
}
}
