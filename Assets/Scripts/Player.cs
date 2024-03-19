using System;
using UnityEngine;

/// <summary>
/// Class <c>Player</c> handles player logic.
/// This class references code from <see href="https://www.youtube.com/watch?v=TKt_VlMn_aA"/> and <see href="https://www.youtube.com/watch?v=hkaysu1Z-N8"/>.
/// </summary>
public class Player : MonoBehaviour
{
    private static readonly Vector2[] DIRS = { Vector2.up, Vector2.left, Vector2.down, Vector2.right };

    [SerializeField] private float baseSpeed = 4f;
    [SerializeField] private float speedMultiplier = 1f;
    [SerializeField] private Vector2 startDir;// = Vector2.zero;
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private PlayerControls controls;

    private Rigidbody2D rBody;
    private Animator animator;

    public Vector2 dir { get; private set; }
    public Vector2 nextDir { get; private set; } // next direction is queued if we cannot move that way yet
    public Vector3 startPos { get; private set; }

    /// <summary>
    /// Struct <c>PlayerControls</c> provides for each player to have their own key controls without changing the base class's behavior.
    /// </summary>
    [System.Serializable]
    public struct PlayerControls
    {
        public KeyCode up { get; private set; }
        public KeyCode left { get; private set; }
        public KeyCode down { get; private set; }
        public KeyCode right { get; private set; }

        public PlayerControls(KeyCode upKey, KeyCode leftKey, KeyCode downKey, KeyCode rightKey)
        {
            up = upKey;
            left = leftKey;
            down = downKey;
            right = rightKey;
        }
    }

    /// <summary>
    /// Method <c>Awake</c> sets up all necessary code-component links.
    /// </summary>
    private void Awake()
    {
        rBody = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        startPos = transform.position;
    }

    /// <summary>
    /// Method <c>Start</c> initializes the player.
    /// </summary>
    private void Start()
    {
        SetControls(KeyCode.W, KeyCode.A, KeyCode.S, KeyCode.D);
        Reset();
    }

    /// <summary>
    /// Method <c>Update</c> handles non-physics player movement.
    /// </summary>
    private void Update()
    {
        // first check for new user input
        if (Input.GetKeyDown(controls.up))
        {
            SetDirection(Vector2.up);
        }
        else if (Input.GetKeyDown(controls.left))
        {
            SetDirection(Vector2.left);
        } else if (Input.GetKeyDown(controls.down))
        {
            SetDirection(Vector2.down);
        } else if (Input.GetKeyDown(controls.right))
        {
            SetDirection(Vector2.right);
        }

        // then handle pre-existing movement queue
        if (nextDir != Vector2.zero)
        {
            SetDirection(nextDir);
        }
    }

    /// <summary>
    /// Method <c>FixedUpdate</c> handles all player physics interactions.
    /// </summary>
    private void FixedUpdate()
    {
        Vector2 pos = rBody.position;
        Vector2 move = dir * baseSpeed * speedMultiplier * Time.fixedDeltaTime;
        rBody.MovePosition(pos + move);
    }

    /// <summary>
    /// Method <c>SetControls</c> sets the KeyCodes which will be accepted to direct this player.
    /// </summary>
    /// <param name="up">the KeyCode which will indicate "up".</param>
    /// <param name="left">the KeyCode which will indicate "left".</param>
    /// <param name="right">the KeyCode which will indicate "right".</param>
    /// <param name="down">the KeyCode which will indicate "down".</param>
    private void SetControls(KeyCode up, KeyCode left, KeyCode right, KeyCode down)
    {
        controls = new PlayerControls(up, left, right, down);
    }

    /// <summary>
    /// Method <c>Reset</c> resets the player state to initial values.
    /// </summary>
    private void Reset()
    {
        SetSpeed(1f);
        SetDirection(startDir, true);
        nextDir = Vector2.zero;
        transform.position = startPos;
        rBody.isKinematic = false;
        enabled = true;
    }

    /// <summary>
    /// Method <c>SetDir</c> attempts to change the player's direction but checks for obstacles first.
    /// </summary>
    /// <param name="newDir"></param>
    /// <param name="forceSet"></param>
    private void SetDirection(Vector2 direction, bool forceSet = false)
    {
        if (forceSet || !IsBlocked(direction))
        {
            dir = direction;
            nextDir = Vector2.zero;

            // handle animation updates
            animator.SetInteger("Dir", Array.FindIndex(DIRS, d => d.Equals(direction)));
        } else
        {
            // queue the direction so we keep trying
            nextDir = direction;
        }
    }

    /// <summary>
    /// Method <c>SetSpeed</c> sets the player's speed multiplier.
    /// </summary>
    /// <param name="speed">the player's new speed.</param>
    private void SetSpeed(float speed)
    {
        speedMultiplier = speed;
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
}
