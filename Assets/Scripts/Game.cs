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
    [SerializeField] private TMP_Text playersLabel;

    public bool IsGameOver { get; private set; } = true; // default to true for purposes of allowing new player connections

    private int playerCount = 0;

    /// <summary>
    /// Method <c>Activate</c> activates the UI for the provided player number.
    /// </summary>
    /// <param name="playerNum">the player whose UI should be activated.</param>
    public void Activate(int playerNum)
    {
        SetPlayer(playerNum, true);
    }

    /// <summary>
    /// Method <c>Deactivate</c> deactivates the UI for the provided player number.
    /// </summary>
    /// <param name="playerNum">the player whose UI should be deactivated.</param>
    public void Deactivate(int playerNum)
    {
        SetPlayer(playerNum, false);
    }

    /// <summary>
    /// Method <c>SetPlayer</c> handles the generic player activation update logic.
    /// </summary>
    /// <param name="playerNum"></param>
    /// <param name="isActive">a boolean indicating if the player should be activated or deactivated.</param>
    private void SetPlayer(int playerNum, bool isActive)
    {
        if (playerNum <= playerPanels.Length && playerNum >= 0)
        {
            playerPanels[playerNum].SetActive(true);
            playerCount += isActive ? 1 : -1;
            playersLabel.text = playerCount.ToString() + "/4";
        }
    }

    /// <summary>
    /// Method <c>UpdatePlayerScore</c> handles the UI component of a score update.
    /// </summary>
    /// <param name="playerNum">the player whose score will be updated.</param>
    /// <param name="score">the new score value.</param>
    public void UpdatePlayerScore(int playerNum, int score)
    {
        UpdatePlayerText(playerNum, "Score", score);
    }

    /// <summary>
    /// Method <c>UpdatePlayerBanked</c> handles the UI component of a banked score update.
    /// </summary>
    /// <param name="playerNum">the player whose banked score will be updated.</param>
    /// <param name="banked">the new banked score value.</param>
    public void UpdatePlayerBanked(int playerNum, int banked)
    {
        UpdatePlayerText(playerNum, "Bank", banked);
    }

    /// <summary>
    /// Method <c>UpdatePlayerText</c> handles the generic player UI text update logic.
    /// </summary>
    /// <param name="playerNum">the player whose UI will be updated.</param>
    /// <param name="name">the name of the text component that will be updated.</param>
    /// <param name="value">the new value.</param>
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
    public void NewGame()
    {
        IsGameOver = false;
        NewLevel();
    }

    /// <summary>
    /// Method <c>NewLevel</c> resets all level-based game states.
    /// </summary>
    private void NewLevel()
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
