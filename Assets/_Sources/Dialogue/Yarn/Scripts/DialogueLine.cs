using System;
using UnityEditor.U2D.Animation;
using UnityEngine;

[Serializable]
public class DialogueLine
{
    public CharacterData speaker;
    public string text;
    public Sprite expression;
    public AudioClip blipSound;
}

