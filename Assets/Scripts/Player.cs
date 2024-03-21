using System;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

/// <summary>
/// Class <c>Player</c> handles player logic.
/// This class references code from <see href="https://www.youtube.com/watch?v=TKt_VlMn_aA"/>, <see href="https://www.youtube.com/watch?v=hkaysu1Z-N8"/>, and <see href="https://www.youtube.com/watch?v=3yuBOB3VrCk"/>.
/// </summary>
public class Player : NetworkBehaviour
{
    #region Variables
    private static readonly Vector2[] DIRS = { Vector2.up, Vector2.left, Vector2.down, Vector2.right };
    private static readonly PlayerControls[] CTLS = { new(KeyCode.W, KeyCode.A, KeyCode.S, KeyCode.D)
            , new(KeyCode.UpArrow, KeyCode.LeftArrow, KeyCode.DownArrow, KeyCode.RightArrow)
            , new(KeyCode.I, KeyCode.J, KeyCode.K, KeyCode.L)
            , new(KeyCode.Keypad8, KeyCode.Keypad4, KeyCode.Keypad2, KeyCode.Keypad6)};

    [SerializeField] private int defaultPlayer;
    [SerializeField] private float baseSpeed = 4f;
    [SerializeField] private LayerMask obstacleLayer;

    public NetworkVariable<Vector2> Dir { get; private set; } = new(Vector2.zero, writePerm: NetworkVariableWritePermission.Owner);
    public NetworkVariable<Vector2> NextDir { get; private set; } = new(Vector2.zero, writePerm: NetworkVariableWritePermission.Owner);
    public NetworkVariable<float> SpeedMultiplier { get; private set; } = new(0f, writePerm: NetworkVariableWritePermission.Owner);
    public NetworkVariable<int> Score { get; private set; } = new(0, writePerm: NetworkVariableWritePermission.Owner);
    public NetworkVariable<int> BankedScore { get; private set; } = new(0, writePerm: NetworkVariableWritePermission.Owner);

    private Vector2 startDir;
    private Vector3 startPos;
    private Rigidbody2D rBody;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private PlayerControls controls;
    private Game game;
    private int currPlayer;
    #endregion

    #region Player Controls (Struct)
    /// <summary>
    /// Struct <c>PlayerControls</c> provides for each player to have their own key controls without changing the base class's behavior.
    /// </summary>
    [System.Serializable]
    public struct PlayerControls
    {
        public KeyCode Up { get; private set; }
        public KeyCode Left { get; private set; }
        public KeyCode Down { get; private set; }
        public KeyCode Right { get; private set; }

        public PlayerControls(KeyCode upKey, KeyCode leftKey, KeyCode downKey, KeyCode rightKey)
        {
            Up = upKey;
            Left = leftKey;
            Down = downKey;
            Right = rightKey;
        }
    }
    #endregion

    #region Initialization
    /// <summary>
    /// Method <c>Awake</c> sets up all necessary code-component links.
    /// </summary>
    private void Awake()
    {
        game = FindObjectOfType<Game>();
        rBody = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        startPos = transform.position;
    }

    /// <summary>
    /// Method <c>OnNetworkSpawn</c> sets up post-network-spawn links and player states.
    /// </summary>
    public override void OnNetworkSpawn()
    {
        Dir.OnValueChanged += DirChanged;
        Score.OnValueChanged += ScoreChanged;
        BankedScore.OnValueChanged += BankedChanged;
        game.IsGameOver.OnValueChanged += GameOverChanged;
        game.Level.OnValueChanged += LevelChanged;
        InitializeAs(defaultPlayer); // learned from prefab!
    }

    /// <summary>
    /// Method <c>OnNetworkDespawn</c> handles game state clean-up when a player disconnects.
    /// </summary>
    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        game.Deactivate(currPlayer);
    }

    /// <summary>
    /// Method <c>InitializeAs</c> initializes the player's number-specific state.
    /// </summary>
    /// <param name="playerNum"></param>
    public void InitializeAs(int playerNum)
    {
        currPlayer = playerNum;
        SetControls(playerNum);
        startDir = DIRS[2 * (playerNum % 2) + 1];
        game.Activate(currPlayer);
    }

    /// <summary>
    /// Method <c>SetControls</c> sets the KeyCodes which will be accepted to direct this player based on stored presets.
    /// </summary>
    /// <param name="ctrlIndex">the index of the preset controls to be used for this player.</param>
    private void SetControls(int ctrlIndex)
    {
        if (ctrlIndex < CTLS.Length)
        {
            controls = CTLS[ctrlIndex];
        }
    }
    #endregion

    #region State Changes
    /// <summary>
    /// Method <c>ResetState</c> resets the player state to initial values.
    /// </summary>
    /// <param name="isFullReset">a boolean indicating if we should also reset states that persist between levels.</param>
    public void ResetState(bool isFullReset = false)
    {
        if (!IsOwner) return;
        SetSpeed(1f);
        SetDirection(startDir, true);
        TeleportTo(startPos);
        ResetMode();
        if (isFullReset)
        {
            SetBanked(0);
            SetScore(0);
        }
        else
        {
            // bank score between levels
            BankScore();
        }
    }

    /// <summary>
    /// Method <c>GameOverChanged</c> handles player state updates when a game launches or terminates.
    /// </summary>
    /// <param name="previousValue">the previous state of game over-ness.</param>
    /// <param name="newValue">the new state of game over-ness.</param>
    private void GameOverChanged(bool previousValue, bool newValue)
    {
        if (newValue)
        {
            // Game Over!
            EnterDeadMode();
            if (IsOwner) SetSpeed(0f);
            BankScore();
        } else
        {
            // New Game!
            ResetState(true);
        }
    }

    /// <summary>
    /// Method <c>LevelChanged</c> handles player state updates when a new level is reached.
    /// </summary>
    /// <param name="previousValue">the previous level.</param>
    /// <param name="newValue">the new level.</param>
    private void LevelChanged(int previousValue, int newValue)
    {
        if (newValue > 0) ResetState(); // don't need to re-do work handled by GameOver state change
    }
    #endregion

    #region Movement
    /// <summary>
    /// Method <c>Update</c> handles non-physics player movement.
    /// </summary>
    private void Update()
    {
        if (!IsOwner) return;
        // first check for new user input
        if (Input.GetKeyDown(controls.Up))
        {
            SetDirection(Vector2.up);
        }
        else if (Input.GetKeyDown(controls.Left))
        {
            SetDirection(Vector2.left);
        } else if (Input.GetKeyDown(controls.Down))
        {
            SetDirection(Vector2.down);
        } else if (Input.GetKeyDown(controls.Right))
        {
            SetDirection(Vector2.right);
        }

        // then handle pre-existing movement queue
        if (NextDir.Value != Vector2.zero)
        {
            SetDirection(NextDir.Value);
        }
    }

    /// <summary>
    /// Method <c>FixedUpdate</c> handles all player physics interactions.
    /// </summary>
    private void FixedUpdate()
    {
        if (!IsOwner) return;
        Vector2 pos = rBody.position;
        Vector2 move = Dir.Value * baseSpeed * SpeedMultiplier.Value * Time.fixedDeltaTime;
        rBody.MovePosition(pos + move);
    }

    /// <summary>
    /// Method <c>SetDir</c> attempts to change the player's direction but checks for obstacles first.
    /// </summary>
    /// <param name="newDir">the direction the player wants to go.</param>
    /// <param name="forceSet">a boolean indicating whether or not the presence of obstacles should be ignored.</param>
    private void SetDirection(Vector2 direction, bool forceSet = false)
    {
        if (forceSet || !IsBlocked(direction))
        {
            Dir.Value = direction;
            NextDir.Value = Vector2.zero;
        } else
        {
            // queue the direction so we keep trying
            NextDir.Value = direction;
        }
    }

    /// <summary>
    /// Method <c>DirChanged</c> handles the animation updates resulting from a change in direction.
    /// </summary>
    /// <param name="previousValue">the previous direction.</param>
    /// <param name="newValue">the new direction.</param>
    private void DirChanged(Vector2 previousValue, Vector2 newValue)
    {
        // handle animation updates
        animator.SetInteger("Dir", Array.FindIndex(DIRS, d => d.Equals(newValue)));
    }

    /// <summary>
    /// Method <c>SetSpeed</c> sets the player's speed multiplier.
    /// </summary>
    /// <param name="speed">the player's new speed.</param>
    private void SetSpeed(float speed)
    {
        if (!IsOwner) return;
        SpeedMultiplier.Value = speed;
        animator.SetFloat("Speed", speed);
    }

    /// <summary>
    /// Method <c>IsBlocked</c> checks a potential direction to see if the player can move that way.
    /// </summary>
    /// <param name="direction">the direction to be checked.</param>
    /// <returns>True if the player is blocked from moving in that direction, False otherwise.</returns>
    private bool IsBlocked(Vector2 direction)
    {
        RaycastHit2D hit = Physics2D.BoxCast(transform.position, Vector2.one * 0.9f, 0f, direction, 0.75f, obstacleLayer);
        return hit.collider != null;
    }

    /// <summary>
    /// Method <c>TeleportTo</c> handles non-interpolated player motion.
    /// </summary>
    /// <param name="newPosition">the position where the player will end up.</param>
    public void TeleportTo(Vector3 newPosition)
    {
        if (IsOwner) gameObject.GetComponent<NetworkTransform>().Teleport(newPosition, transform.rotation, transform.localScale);
    }
    #endregion

    #region Scoring
    /// <summary>
    /// Method <c>SetScore</c> is a mutator for this player's internal score value.
    /// </summary>
    /// <param name="newScore">the new score value.</param>
    private void SetScore(int newScore)
    {
        if (!IsOwner) return;
        Score.Value = newScore;
    }

    /// <summary>
    /// Method <c>IncScore</c> increments this player's score by the given value.
    /// </summary>
    /// <param name="plusScore">the increment value for the score. <see cref="SetScore"/></param>
    private void IncScore(int plusScore)
    {
        SetScore(Score.Value + plusScore);
    }

    /// <summary>
    /// Method <c>ScoreChanged</c> notifies the game whenever a player's score is changed.
    /// </summary>
    /// <param name="previousValue">the previous score.</param>
    /// <param name="newValue">the new score.</param>
    private void ScoreChanged(int previousValue, int newValue)
    {
        game.UpdatePlayerScore(currPlayer, Score.Value);
    }

    /// <summary>
    /// Method <c>SetBanked</c> is a mutator for this player's internal banked score value.
    /// </summary>
    /// <param name="newBanked">the new banked score value.</param>
    private void SetBanked(int newBanked)
    {
        if (!IsOwner) return;
        BankedScore.Value = newBanked;
    }

    /// <summary>
    /// Method <c>IncBanked</c> increments this player's banked score by the given value.
    /// </summary>
    /// <param name="plusBanked">the increment value for the banked score. <see cref="SetBanked"/></param>
    private void IncBanked(int plusBanked)
    {
        SetBanked(BankedScore.Value + plusBanked);
    }

    /// <summary>
    /// Method <c>BankScore</c> transfers this player's non-banked score into their banked score.
    /// </summary>
    private void BankScore()
    {
        IncBanked(Score.Value);
        SetScore(0);
    }

    /// <summary>
    /// Method <c>BankedChanged</c> notifies the game whenever a player's banked score is changed.
    /// </summary>
    /// <param name="previousValue">the previous banked score.</param>
    /// <param name="newValue">the new banked score.</param>
    private void BankedChanged(int previousValue, int newValue)
    {
        game.UpdatePlayerBanked(currPlayer, BankedScore.Value);
    }
    #endregion

    #region Eating & Modes
    /// <summary>
    /// Method <c>OnCollisionEnter2D</c> handles state changes from collisions with other players.
    /// </summary>
    /// <param name="collision">the GameObject colliding with this player.</param>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            Player enemy = collision.gameObject.GetComponent<Player>();
            enemy.EatPlayer(this);
            EnterDeadMode(2f);
        }
    }

    /// <summary>
    /// Method <c>EatPellet</c> handles state changes when this player eats a pellet.
    /// </summary>
    /// <param name="pellet">the pellet eaten by this player.</param>
    public void EatPellet(Pellet pellet)
    {
        if (IsOwner) EatPelletServerRpc();
        pellet.gameObject.SetActive(false);
        if (pellet is PowerPellet)
        {
            EnterEnemyMode(PowerPellet.DURATION);
        }
        IncScore(pellet.Points);
        game.PelletEaten();
    }

    /// <summary>
    /// Method <c>EatPelletServerRpc</c> handles the server response to eating a pellet.
    /// </summary>
    [ServerRpc]
    private void EatPelletServerRpc()
    {
        AudioPlayer.Instance.PlayEat();
    }

    /// <summary>
    /// Method <c>EatPlayer</c> handles state changes when this player eats another.
    /// </summary>
    /// <param name="player">the player eaten by this player.</param>
    public void EatPlayer(Player player)
    {
        if (IsOwner) EatPlayerServerRpc();
        IncScore(player.Score.Value);
    }

    /// <summary>
    /// Method <c>EatPlayerServerRpc</c> handles the server response to eating another player.
    /// </summary>
    [ServerRpc]
    private void EatPlayerServerRpc()
    {
        AudioPlayer.Instance.PlaySteal();
    }

    /// <summary>
    /// Method <c>ResetMode</c> resets this player the default player mode.
    /// </summary>
    private void ResetMode()
    {
        gameObject.layer = LayerMask.NameToLayer("Player");
        spriteRenderer.color = Color.white;
    }

    /// <summary>
    /// Method <c>EnterEnemyMode</c> activates this player's ability to PvP.
    /// </summary>
    /// <param name="duration">the duration before the mode resets.</param>
    private void EnterEnemyMode(float duration)
    {
        gameObject.layer = LayerMask.NameToLayer("Enemy");
        spriteRenderer.color = Color.red;
        Invoke(nameof(ResetMode), duration);
        if (IsOwner) EnemyModeServerRpc(duration);
    }

    /// <summary>
    /// Method <c>EnemyModeServerRpc</c> handles the server response to Enemy Mode.
    /// </summary>
    /// <param name="duration">the duration before the mode resets.</param>
    [ServerRpc]
    private void EnemyModeServerRpc(float duration)
    {
        AudioPlayer.Instance.StartEnemy(duration);
    }

    /// <summary>
    /// Method <c>EnterDeadMode</c> sends this player to the graveyard.
    /// </summary>
    /// <param name="duration">the duration before the mode resets.</param>
    private void EnterDeadMode(float duration = 0f)
    {
        gameObject.layer = LayerMask.NameToLayer("Ghost");
        spriteRenderer.color = Color.gray;
        if (duration > 0) Invoke(nameof(ResetMode), duration);
    }
    #endregion
}
