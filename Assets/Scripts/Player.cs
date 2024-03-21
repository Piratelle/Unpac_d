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
    [SerializeField] private NetworkVariable<float> speedMultiplier = new(1f, writePerm: NetworkVariableWritePermission.Owner);
    [SerializeField] private LayerMask obstacleLayer;

    private Vector2 startDir;
    private Rigidbody2D rBody;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private PlayerControls controls;
    private Game game;
    private int currPlayer;

    public NetworkVariable<Vector2> Dir { get; private set; } = new(Vector2.zero, writePerm: NetworkVariableWritePermission.Owner);
    public Vector2 NextDir { get; private set; } // next direction is queued if we cannot move that way yet
    public Vector3 StartPos { get; private set; }
    public int Score { get; private set; }
    public int BankedScore {  get; private set; }
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
        StartPos = transform.position;
        InitializeAs(defaultPlayer); // learned from prefab!
    }

    /// <summary>
    /// Method <c>Start</c> initializes the player's generic state.
    /// </summary>
    private void Start()
    {
        ResetState();
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
        game.Activate(playerNum);
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

    /// <summary>
    /// Method <c>ResetState</c> resets the player state to initial values.
    /// </summary>
    /// <param name="isFullReset">a boolean indicating if we should also reset states that persist between levels.</param>
    public void ResetState(bool isFullReset = false)
    {
        SetSpeed(1f);
        SetDirection(startDir, true);
        NextDir = Vector2.zero;
        transform.position = StartPos;
        rBody.isKinematic = false;
        enabled = true;
        ResetMode();
        if (isFullReset)
        {
            SetBanked(0);
        }
        else
        {
            IncBanked(Score);
        }
        SetScore(0);
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
        if (NextDir != Vector2.zero)
        {
            SetDirection(NextDir);
        }
    }

    /// <summary>
    /// Method <c>FixedUpdate</c> handles all player physics interactions.
    /// </summary>
    private void FixedUpdate()
    {
        if (!IsOwner) return;
        Vector2 pos = rBody.position;
        Vector2 move = Dir.Value * baseSpeed * speedMultiplier.Value * Time.fixedDeltaTime;
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
            NextDir = Vector2.zero;

            // handle animation updates
            animator.SetInteger("Dir", Array.FindIndex(DIRS, d => d.Equals(direction)));
        } else
        {
            // queue the direction so we keep trying
            NextDir = direction;
        }
    }

    /// <summary>
    /// Method <c>SetSpeed</c> sets the player's speed multiplier.
    /// </summary>
    /// <param name="speed">the player's new speed.</param>
    private void SetSpeed(float speed)
    {
        speedMultiplier.Value = speed;
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
        if (IsOwner) TeleportToServerRpc(newPosition);
    }

    /// <summary>
    /// Method <c>TeleportToServerRpc</c> handles server-based non-interpolated player motion.
    /// </summary>
    /// <param name="newPosition">the position where the player will end up.</param>
    [ServerRpc]
    private void TeleportToServerRpc(Vector3 newPosition)
    {
        gameObject.GetComponent<NetworkTransform>().Teleport(newPosition, transform.rotation, transform.localScale);
    }
    #endregion

    #region Scoring
    /// <summary>
    /// Method <c>SetScore</c> is a mutator for this player's internal score value.
    /// </summary>
    /// <param name="newScore">the new score value.</param>
    private void SetScore(int newScore)
    {
        Score = newScore;
        game.UpdatePlayerScore(currPlayer, Score);
    }

    /// <summary>
    /// Method <c>IncScore</c> increments this player's score by the given value.
    /// </summary>
    /// <param name="plusScore">the increment value for the score. <see cref="SetScore"/></param>
    private void IncScore(int plusScore)
    {
        SetScore(Score + plusScore);
    }

    /// <summary>
    /// Method <c>SetBanked</c> is a mutator for this player's internal banked score value.
    /// </summary>
    /// <param name="newBanked">the new banked score value.</param>
    private void SetBanked(int newBanked)
    {
        BankedScore = newBanked;
        game.UpdatePlayerBanked(currPlayer, BankedScore);
    }

    /// <summary>
    /// Method <c>IncBanked</c> increments this player's banked score by the given value.
    /// </summary>
    /// <param name="plusBanked">the increment value for the banked score. <see cref="SetBanked"/></param>
    private void IncBanked(int plusBanked)
    {
        SetBanked(BankedScore + plusBanked);
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
        pellet.gameObject.SetActive(false);
        if (pellet is PowerPellet)
        {
            EnterEnemyMode(PowerPellet.DURATION);
        }
        IncScore(pellet.Points);
        game.PelletEaten();
    }

    /// <summary>
    /// Method <c>EatPlayer</c> handles state changes when this player eats another.
    /// </summary>
    /// <param name="player">the player eaten by this player.</param>
    public void EatPlayer(Player player)
    {
        IncScore(player.Score);
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
    }

    /// <summary>
    /// Method <c>EnterDeadMode</c> sends this player to the graveyard.
    /// </summary>
    /// <param name="duration">the duration before the mode resets.</param>
    private void EnterDeadMode(float duration)
    {
        gameObject.layer = LayerMask.NameToLayer("Ghost");
        spriteRenderer.color = Color.gray;
        Invoke(nameof(ResetMode), duration);
    }
    #endregion
}
