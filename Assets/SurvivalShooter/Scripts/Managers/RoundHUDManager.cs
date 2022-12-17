using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoundHUDManager : MonoBehaviour
{
    public Text roundLabel;
    public void SetRoundNumber(int roundNumber)
    {
        roundLabel.text = $"Round {roundNumber}";
    }
    public void SetRoundNumberAsString(string roundName)
    {
        roundLabel.text = roundName;
    }
}
