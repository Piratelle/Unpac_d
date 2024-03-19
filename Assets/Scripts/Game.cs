using UnityEngine;

/// <summary>
/// Class <c>Game</c> handles shared game state logic.
/// This class references code from <see href="https://www.youtube.com/watch?v=TKt_VlMn_aA"/>.
/// </summary>
public class Game : MonoBehaviour
{
    public Transform pellets;

    private bool isGameOver;

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
        if (isGameOver && Input.anyKeyDown)
        {
            NewGame();
        }
    }

    // functions needed: start a new game, pellet consumption, score updaters?, possible state reset for player death (may be better in Player), player collisions?

    /// <summary>
    /// Method <c>NewGame</c> resets all games states, whether level-based or game-based.
    /// </summary>
    private void NewGame()
    {
        // also need to set scores etc.
        isGameOver = false;
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

        // also need to reset player states/positions
    }

    /// <summary>
    /// Method <c>GameOver</c> handles state cleanup when the game is over.
    /// </summary>
    private void GameOver()
    {
        // TBD - called when ???
        isGameOver = true;
    }
}
