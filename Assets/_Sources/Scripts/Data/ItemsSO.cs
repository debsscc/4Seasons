using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "New Object/Item")]
public class ItemsSO : ScriptableObject
{
   [SerializeField] private string orderDescription;
   public string OrderDescription => orderDescription;
}