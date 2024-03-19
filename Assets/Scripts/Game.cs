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

    // functions needed: score updaters, player collisions (handle in Player?)

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

    /// <summary>
    /// Method <c>PelletEaten</c> handles state changes when a pellet is eaten.
    /// </summary>
    /// <param name="pellet"></param>
    public void PelletEaten(Pellet pellet)
    {
        pellet.gameObject.SetActive(false);

        if (pellet is PowerPellet)
        {
            // do more stuff
        }

        // update score

        if (!HasPellets())
        {
            // turn off player objects
            // new round? game over? consider: Invoke(nameOf(NewLevel), 3.0f);
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
