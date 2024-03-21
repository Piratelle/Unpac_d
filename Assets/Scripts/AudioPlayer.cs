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
    [SerializeField] private AudioSource stealAudioSrc;
    [SerializeField] private AudioSource enemyAudioSrc;
    [SerializeField] private AudioSource gameOverAudioSrc;

    /// <summary>
    /// Method <c>PlayBtnClick</c> plays the designated button click sound.
    /// </summary>
    public void PlayBtnClick()
    {
        btnClkAudioSrc.Play();
    }

    /// <summary>
    /// Method <c>PlayBtnHover</c> plays the designated button hover sound.
    /// </summary>
    public void PlayBtnHover()
    {
        btnHovAudioSrc.Play();
    }

    /// <summary>
    /// Method <c>PlayEat</c> plays the designated eating sound.
    /// </summary>
    public void PlayEat()
    {
        eatAudioSrc.Play();
    }

    /// <summary>
    /// Method <c>PlaySteal</c> plays the designated stealing sound.
    /// </summary>
    public void PlaySteal()
    {
        stealAudioSrc.Play();
    }

    /// <summary>
    /// Method <c>StartEnemy</c> plays the looping enemy sound for a certain duration.
    /// </summary>
    /// <param name="duration">the duration to play the enemy sound.</param>
    public void StartEnemy(float duration)
    {
        enemyAudioSrc.Play();
        CancelInvoke();
        Invoke(nameof(StopEnemy), duration);
    }

    /// <summary>
    /// Method <c>StopEnemy</c> stops the looping enemy sound.
    /// </summary>
    public void StopEnemy()
    {
        enemyAudioSrc.Stop();
    }

    /// <summary>
    /// Method <c>PlayGameOver</c> plays the designated game over sound.
    /// </summary>
    public void PlayGameOver()
    {
        CancelInvoke();
        StopEnemy();
        gameOverAudioSrc.Play();
    }
}
