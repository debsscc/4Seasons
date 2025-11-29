using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public DialogueUI ui;
    private DialogueSequence currentSequence;
    private int index;


    public void StartDialogue(DialogueSequence seq)
    {
        currentSequence = seq;
        index = 0;
        ShowLine();
    }

    public void NextLine()
    {
        index++;
        if (index >= currentSequence.lines.Count)
        {
            EndDialogue();
            return;
        }
        ShowLine();
    }

    private void ShowLine()
    {
        var line = currentSequence.lines[index];
        //ui.DisplayLine(line);
    }
    private void EndDialogue()
    {
        ui.Hide();
        //TODO: send trigger or event for dialogue end
    }


}
