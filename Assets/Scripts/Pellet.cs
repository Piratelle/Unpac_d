using UnityEngine;

/// <summary>
/// Class <c>Pellet</c> handles pellet logic.
/// This class references code from <see href="https://www.youtube.com/watch?v=TKt_VlMn_aA"/>.
/// </summary>
public class Pellet : MonoBehaviour
{
    public readonly int Points = 10;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player") || 
            collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            this.gameObject.SetActive(false);
            collision.GetComponent<Player>().EatPellet(this);
        }
    }
}
