using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class BackgroundMusic : MonoBehaviour
{
    private static BackgroundMusic instance;

    [Header("Áudio de Fundo")]
    public AudioClip musicClip;

    private AudioSource audioSource;

    void Awake()
    {
        // Garante que só haja uma instância deste objeto
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        // Pega ou adiciona o AudioSource
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.loop = true;
    }

    void Start()
    {
        if (musicClip != null)
        {
            audioSource.clip = musicClip;
            if (!audioSource.isPlaying)
                audioSource.Play();
        }
        else
        {
            Debug.LogWarning("Nenhuma música atribuída no Inspector.");
        }
    }
}
