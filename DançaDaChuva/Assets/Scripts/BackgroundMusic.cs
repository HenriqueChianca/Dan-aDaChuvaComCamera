using UnityEngine;

public class BackgroundMusic : MonoBehaviour
{
    private static BackgroundMusic instance;

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
    }

    void Start()
    {
        AudioSource audio = GetComponent<AudioSource>();
        if (audio != null && !audio.isPlaying)
        {
            audio.Play();
        }
    }
}
