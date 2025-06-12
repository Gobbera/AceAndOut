using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip cardSlapClip;
    [SerializeField] private AudioClip turnNotificationClip;
    [SerializeField] private AudioClip trucoNotificationClip;
    // Adicione outros efeitos que quiser

    void Awake()
    {
        // Garante que só exista um SoundManager
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // opcional, se quiser manter entre cenas
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void TrucoNotification()
    {
        if (sfxSource != null && trucoNotificationClip != null)
        {
            sfxSource.PlayOneShot(trucoNotificationClip, 0.422f);
        }
    }
    public void TurnNotification()
    {
        if (sfxSource != null && turnNotificationClip != null)
        {
            sfxSource.PlayOneShot(turnNotificationClip, 0.4f);
        }
    }
    public void PlayCardSlap()
    {
        if (sfxSource != null && cardSlapClip != null)
        {
            sfxSource.pitch = Random.Range(0.30f, 2.30f); // variação sutil no pitch
            sfxSource.PlayOneShot(cardSlapClip, 0.4f);
            sfxSource.pitch = 1f; // reseta o pitch para não afetar outros sons
        }
    }

    public void PlaySound(AudioClip clip, float volume = 1f)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.PlayOneShot(clip, volume);
        }
    }
}
