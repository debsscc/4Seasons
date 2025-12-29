using System;
using UnityEngine;

[Serializable]
public class DialogueLine
{
    public CharacterData speaker;
    public string text;
    public Sprite expression;
    public AudioClip blipSound;
}

