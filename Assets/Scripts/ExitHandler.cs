using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Class <c>ExitHandler</c> handles movement overflow for a particular maze exit.
/// This class heavily references code from <see href="https://www.youtube.com/watch?v=TKt_VlMn_aA"/>.
/// </summary>
public class ExitHandler : MonoBehaviour
{
    [SerializeField] private Transform target;

    /// <summary>
    /// Method <c>OnTriggerEnter2D</c> moves any colliding object to this exit's target.
    /// </summary>
    /// <param name="collision">the colliding object</param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Vector3 position = collision.transform.position;
        position.x = target.position.x;
        position.y = target.position.y;

        if (collision.TryGetComponent<Player>(out Player player))
        {
            player.TeleportTo(position);
        } else
        {
            collision.transform.position = position; // nothing else should be colliding, but just in case
        }
    }
}
