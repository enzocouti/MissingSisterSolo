using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("Player SFX")]
    public AudioClip punch;
    public AudioClip punchAlt;
    public AudioClip dash;
    public AudioClip hurt;
    public AudioClip getUp;
    public AudioClip death;
    public AudioClip movement;

    [Header("Base Enemy SFX")]
    public AudioClip enemyPunch;
    public AudioClip enemyHurt;
    public AudioClip enemyGetUp;
    public AudioClip enemyDeath;

    [Header("Boss SFX")]
    public AudioClip bossPunch;
    public AudioClip bossSlam;    
    public AudioClip bossHurt;
    public AudioClip bossDeath;

    [Header("Audio Source")]
    public AudioSource sfxSource;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void PlaySFX(AudioClip clip, float vol = 1f)
    {
        if (clip && sfxSource)
            sfxSource.PlayOneShot(clip, vol);
    }

    
    public void PlayRandomPunch()
    {
        if (punch && punchAlt)
        {
            AudioClip clip = (Random.value < 0.5f) ? punch : punchAlt;
            PlaySFX(clip);
        }
        else if (punch) PlaySFX(punch);
        else if (punchAlt) PlaySFX(punchAlt);
    }
    public void PlayPunch() => PlaySFX(punch);
    public void PlayPunchAlt() => PlaySFX(punchAlt);
    public void PlayDash() => PlaySFX(dash);
    public void PlayHurt() => PlaySFX(hurt);
    public void PlayGetUp() => PlaySFX(getUp);
    public void PlayDeath() => PlaySFX(death);
    public void PlayMovement() => PlaySFX(movement);

    
    public void PlayEnemyPunch() => PlaySFX(enemyPunch);
    public void PlayEnemyHurt() => PlaySFX(enemyHurt);
    public void PlayEnemyGetUp() => PlaySFX(enemyGetUp);
    public void PlayEnemyDeath() => PlaySFX(enemyDeath);

    
    public void PlayBossPunch() => PlaySFX(bossPunch);
    public void PlayBossSlam() => PlaySFX(bossSlam);
    public void PlayBossHurt() => PlaySFX(bossHurt);
    public void PlayBossDeath() => PlaySFX(bossDeath);
}
