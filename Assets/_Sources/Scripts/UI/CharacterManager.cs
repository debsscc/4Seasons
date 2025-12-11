using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class CharacterManager : MonoBehaviour
{
    [SerializeField] private CharacterData characterInstace;

    [Button]
    public void IncreaseRelationship(int valueToAdd)
    {
        characterInstace.RelationshipScore += valueToAdd;
    }
}