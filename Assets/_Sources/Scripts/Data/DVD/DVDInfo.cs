using System.Collections.Generic;
using UnityEngine;

public class DVDInfo : MonoBehaviour, IItemHolder
{
    public ItemsSO[] dvdGenres;
    public ItemsSO[] Items => dvdGenres;

    public TraitsData traits;
}

public interface IItemHolder
{
    public ItemsSO[] Items { get; }
}
