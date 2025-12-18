using UnityEngine;

public class DVDItemHolder : MonoBehaviour, IItemHolder
{
    [SerializeField]
    private ItemsSO[] items;
    public ItemsSO[] Items => items;
}