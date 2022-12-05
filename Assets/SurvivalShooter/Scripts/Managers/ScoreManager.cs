using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Events;

public class ScoreManager : MonoBehaviour
{
    public static int score;        // The player's score.

    public int HighScore { get; private set; }
    public bool IsHighScoreNew { get; private set; }

    public static ScoreManager Instance { get; private set; }

    public UnityEvent<int> OnHighScoreUpdated { get; private set; } = new UnityEvent<int>();

    Text text;                      // Reference to the Text component.

    void Awake ()
    {
        Instance = this;

        // Set up the reference.
        text = GetComponent <Text> ();

        // Reset the score.
        score = 0;
    }

    private void OnEnable()
    {
        GameStateManager.Instance.OnGameStateChanged.AddListener(HandleGameEnded);
    }

    void Update ()
    {
        // Set the displayed text to be the word "Score" followed by the score value.
        text.text = score.ToString();
    }

    void HandleGameEnded(GameStateManager.GameState newState)
    {
        // do nothing if game isn't over
        if(newState != GameStateManager.GameState.End) { return; }

        if(score > HighScore)
        {
            HighScore = score;
            OnHighScoreUpdated.Invoke(HighScore);
            IsHighScoreNew = true;
        }
    }
}