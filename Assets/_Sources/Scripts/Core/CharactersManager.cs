using System.Collections.Generic;

public class CharactersManager : Singleton<CharactersManager>
{
   public List<CharacterData> npcs = new List<CharacterData>();
   public CharacterData playerCharacter;
   public CharacterData motherCharacter;

   public int positiveBaseScore = 2;
   public int negativeBaseScore = 1;

   public void ApplyPointsByTrait(params ItemsSO[] items)
   {
      foreach (var npc in npcs)
      {
         foreach (var item in items)
         {
            if (npc.favoriteItems.Contains(item))
               npc.RelationshipScore += positiveBaseScore;
            else
               npc.RelationshipScore -= negativeBaseScore;
         }
      }
   }
}