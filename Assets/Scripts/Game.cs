using TMPro;
using UnityEngine;

/// <summary>
/// Class <c>Game</c> handles shared game state logic.
/// This class references code from <see href="https://www.youtube.com/watch?v=TKt_VlMn_aA"/>.
/// </summary>
public class Game : MonoBehaviour
{
    [SerializeField] private Transform pellets;
    [SerializeField] private GameObject[] playerPanels;

    public bool IsGameOver { get; private set; } = true; // default to true for purposes of allowing new player connections

    /// <summary>
    /// Method <c>Start</c> initializes the object before starting a new game.
    /// </summary>
    private void Start()
    {
        NewGame();
    }

    /// <summary>
    /// Method <c>Update</c> scans for user input that can affect game states.
    /// </summary>
    private void Update()
    {
        // potentially change this depending on UI!!
        if (IsGameOver && Input.anyKeyDown)
        {
            NewGame();
        }
    }

    /// <summary>
    /// Method <c>Activate</c> activates the UI for the provided player number.
    /// </summary>
    /// <param name="playerNum">the player whose UI should be activated.</param>
    public void Activate(int playerNum)
    {
        if (playerNum <= playerPanels.Length && playerNum >= 0)
        {
            playerPanels[playerNum].SetActive(true);
        }
    }

    public void UpdatePlayerScore(int playerNum, int score)
    {
        UpdatePlayerText(playerNum, "Score", score);
    }

    public void UpdatePlayerBanked(int playerNum, int banked)
    {
        UpdatePlayerText(playerNum, "Bank", banked);
    }

    private void UpdatePlayerText(int playerNum, string name, int value)
    {
        if (playerNum <= playerPanels.Length && playerNum >= 0)
        {
            GameObject playerPanel = playerPanels[playerNum];
            foreach (TMP_Text text in playerPanel.GetComponentsInChildren<TMP_Text>())
            {
                if (text.name == name) {
                    text.text = value.ToString();
                    return;
                }
            }
        }
    }

    /// <summary>
    /// Method <c>NewGame</c> resets all games states, whether level-based or game-based.
    /// </summary>
    private void NewGame()
    {
        IsGameOver = false;
        NewLevel(true);
    }

    /// <summary>
    /// Method <c>NewLevel</c> resets all level-based game states.
    /// </summary>
    private void NewLevel(bool isNewGame = false)
    {
        // handle pellet re-population
        foreach (Transform pellet in pellets)
        {
            pellet.gameObject.SetActive(true);
        }

        // reset players... how??
    }

    /// <summary>
    /// Method <c>GameOver</c> handles state cleanup when the game is over.
    /// </summary>
    private void GameOver()
    {
        // TBD - called when ???
        IsGameOver = true;
    }

    /// <summary>
    /// Method <c>PelletEaten</c> handles game state changes when a pellet is eaten.
    /// </summary>
    public void PelletEaten()
    {
        if (!HasPellets())
        {
            // turn off player object(s) to make sure they don't get eaten after level/game is over
            // new round? game over? consider: Invoke(nameof(NewLevel), 3.0f);
        }
    }

    /// <summary>
    /// Method <c>HasPellets</c> verifies if any uneaten pellets remain on the board.
    /// </summary>
    /// <returns>True if there are uneaten pellet on the board, False otherwise.</returns>
    private bool HasPellets()
    {
        foreach (Transform pellet in pellets)
        {
            if (pellet.gameObject.activeSelf) return true;
        }
        return false;
    }
}
