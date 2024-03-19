using UnityEditor.Build.Content;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Class <c>Pellet</c> handles pellet logic.
/// This class references code from <see href="https://www.youtube.com/watch?v=TKt_VlMn_aA"/>.
/// </summary>
public class Pellet : MonoBehaviour
{
    public static readonly int POINTS = 10;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            Eat();
        }
    }

    protected virtual void Eat()
    {
        FindObjectOfType<Game>().PelletEaten(this); // potentially apply this to player instead!
    }
}
