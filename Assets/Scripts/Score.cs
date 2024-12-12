using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Score", menuName = "ScriptableObjects/Score", order = 1)]
public class Score : ScriptableObject
{
    public static int score;
    public static int highScore;
}
