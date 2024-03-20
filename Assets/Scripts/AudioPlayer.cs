using UnityEngine;

/// <summary>
/// Class <c>AudioPlayer</c> controls audio via a Singleton to avoid noisy noise.
/// This class references code from <see href="https://www.youtube.com/watch?v=3yuBOB3VrCk"/>.
/// </summary>
public class AudioPlayer : Singleton<AudioPlayer>
{
    [SerializeField] private AudioSource btnClkAudioSrc;
    [SerializeField] private AudioSource btnHovAudioSrc;
    [SerializeField] private AudioSource eatAudioSrc;
    [SerializeField] private AudioSource enemyAudioSrc;
    [SerializeField] private AudioSource gameOverAudioSrc;

    public void PlayBtnClick()
    {
        btnClkAudioSrc.Play();
    }

    public void PlayBtnHover()
    {
        btnHovAudioSrc.Play();
    }

    public void PlayEat()
    {
        eatAudioSrc.Play();
    }

    public void PlayGameOver()
    {
        gameOverAudioSrc.Play();
    }
}
