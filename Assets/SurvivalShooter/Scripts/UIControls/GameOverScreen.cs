using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameOverScreen : MonoBehaviour
{
    public Text scoreValueLabel;
    public Text timeValueLabel; // TODO: we're not actually tracking time yet
    public Text highScoreHighlight;

    void OnEnable()
    {
        scoreValueLabel.text = ScoreManager.score.ToString();
        timeValueLabel.text = Time.timeSinceLevelLoad.ToString();
        if(ScoreManager.Instance.IsHighScoreNew)
        {
            highScoreHighlight.gameObject.SetActive(true);
        }
    }
}
