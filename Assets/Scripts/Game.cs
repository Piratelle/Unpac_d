using TMPro;
using Unity.Netcode;
using UnityEditor.U2D.Path.GUIFramework;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

/// <summary>
/// Class <c>Game</c> handles shared game state logic.
/// This class references code from <see href="https://www.youtube.com/watch?v=TKt_VlMn_aA"/>.
/// </summary>
public class Game : NetworkBehaviour
{
    #region Variables
    [SerializeField] private Transform pellets;
    [SerializeField] private GameObject[] playerPanels;
    [SerializeField] private GameObject[] playerFinals;
    [SerializeField] private TMP_Text playersLabel;
    [SerializeField] private TMP_Text winnerLabel;
    [SerializeField] private Canvas newGameCanvas;
    [SerializeField] private Canvas gameOverCanvas;

    public NetworkVariable<bool> IsGameOver { get; private set; } = new(true); // default to true for purposes of allowing new player connections
    public NetworkVariable<int> Level { get; private set; } = new(0);

    private int playerCount = 0;
    private int winningScore = 0;
    private int winningPlayer = 0;
    #endregion

    #region Player UI
    /// <summary>
    /// Method <c>Activate</c> activates the UI for the provided player number. <see cref="SetPlayer"/>.
    /// </summary>
    /// <param name="playerNum">the player whose UI should be activated.</param>
    public void Activate(int playerNum, bool isMe)
    {
        SetPlayer(playerNum, true, isMe);
    }

    /// <summary>
    /// Method <c>Deactivate</c> deactivates the UI for the provided player number. <see cref="SetPlayer"/>.
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
    private void SetPlayer(int playerNum, bool isActive, bool isMe = false)
    {
        if (playerNum <= playerPanels.Length && playerNum >= 0)
        {
            playerPanels[playerNum].SetActive(isActive);
            playerFinals[playerNum].SetActive(isActive);
            playerCount += isActive ? 1 : -1;
            playersLabel.text = playerCount.ToString() + "/" + ClientConnectionHandler.Instance.MaxPlayers();
            foreach (Transform child in playerPanels[playerNum].GetComponentsInChildren<Transform>(true))
            {
                if (child.name == "IsMe")
                {
                    child.gameObject.SetActive(isMe && isActive);
                    return;
                }
            }
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
        UpdatePlayerFinalScore(playerNum, banked);
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
    /// Method <c>UpdatePlayerFinalScore</c> handles the UI component of a final score update.
    /// </summary>
    /// <param name="playerNum">the player whose final score will be updated.</param>
    /// <param name="score">the new final score value.</param>
    public void UpdatePlayerFinalScore(int playerNum, int score)
    {
        if (score > winningScore)
        {
            winningScore = score;
            winningPlayer = playerNum + 1;
            winnerLabel.text = "WINNER: Player " + (playerNum + 1) + "!";
        }
        if (playerNum <= playerFinals.Length && playerNum >= 0)
        {
            GameObject playerPanel = playerFinals[playerNum];
            foreach (TMP_Text text in playerPanel.GetComponentsInChildren<TMP_Text>())
            {
                if (text.name == "Score")
                {
                    text.text = score.ToString();
                    return;
                }
            }
        }
    }
    #endregion

    #region New Game/Level
    /// <summary>
    /// Method <c>NewGame</c> resets all games states, whether level-based or game-based.
    /// </summary>
    public void NewGame()
    {
        NewGameServerRpc();
    }

    /// <summary>
    /// Method <c>NewGameServerRpc</c> handles server-based game launch updates.
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    private void NewGameServerRpc()
    {
        Level.Value = 0;
        IsGameOver.Value = false;
        NewGameClientRpc();
    }

    /// <summary>
    /// Method <c>NewGameClientRpc</c> handles post-server update client-based game launch updates.
    /// </summary>
    [ClientRpc]
    private void NewGameClientRpc()
    {
        NewLevel();
        newGameCanvas.gameObject.SetActive(false);
        gameOverCanvas.gameObject.SetActive(false);
        winningPlayer = 0;
        winningScore = 0;
    }

    /// <summary>
    /// Method <c>NewLevel</c> resets all level-based game states.
    /// </summary>
    private void NewLevel()
    {
        if (IsServer) Level.Value++;

        // handle pellet re-population
        foreach (Transform pellet in pellets)
        {
            pellet.gameObject.SetActive(true);
        }
    }
    #endregion

    #region Game Over
    /// <summary>
    /// Method <c>Update</c> scans for any user input that might impact game state.
    /// </summary>
    private void Update()
    {
        if (IsGameOver.Value && gameOverCanvas.gameObject.activeSelf)
        {
            if (Input.anyKeyDown)
            {
                HideGameOverServerRpc();
            }
        }
    }

    /// <summary>
    /// Method <c>GameOver</c> handles state cleanup when the game is over.
    /// </summary>
    private void GameOver()
    {
        GameOverServerRpc();
    }

    /// <summary>
    /// Method <c>GameOverServerRpc</c> handles server-based game end updates.
    /// </summary>
    [ServerRpc]
    private void GameOverServerRpc()
    {
        AudioPlayer.Instance.PlayGameOver();
        IsGameOver.Value = true;
        GameOverClientRpc();
    }

    /// <summary>
    /// Method <c>GameOverClientRpc</c> handles post-server update client-based game end updates.
    /// </summary>
    [ClientRpc]
    private void GameOverClientRpc()
    {
        gameOverCanvas.gameObject.SetActive(true);
        newGameCanvas.gameObject.SetActive(true);
    }

    /// <summary>
    /// Method <c>HideGameOverServerRpc</c> tells the server to hide the Game Over screen.
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    private void HideGameOverServerRpc()
    {
        HideGameOverClientRpc();
    }

    /// <summary>
    /// Method <c>HideGameOverClientRpc</c> tells the clients to hide the Game Over screen.
    /// </summary>
    [ClientRpc]
    private void HideGameOverClientRpc()
    {
        gameOverCanvas.gameObject.SetActive(false);
    }
    #endregion

    #region Pellets
    /// <summary>
    /// Method <c>PelletEaten</c> handles game state changes when a pellet is eaten.
    /// </summary>
    public void PelletEaten()
    {
        if (!HasPellets())
        {
            // potentially increment level here if we think of more fun logic
            GameOver();
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
    #endregion
}
