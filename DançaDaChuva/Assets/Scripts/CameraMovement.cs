using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CameraMovement : MonoBehaviour
{
    public RawImage rawImage; // Arraste no Inspector se estiver na cena Game.
    public string preferredDeviceName /*= ""*/; // Nome opcional da câmera.
    private WebCamTexture webcamTexture;

    void Awake()
    {
        print("eu existo");
        DontDestroyOnLoad(gameObject); // Se quer manter entre cenas.
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        print("OnEnable chamado, registrando OnSceneLoaded.");
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    //void Start()
    {
        if (scene.name == "Game")
        {
            print("tentando inicializar a camera");
            StartCamera(preferredDeviceName);
        }
           
        
        else
        {
            StopCamera();
            //Destroy(gameObject); // Destroi o objeto se não for mais necessário.
        }
    }

    public void StartCamera(string deviceName)
    {
        if (webcamTexture != null && webcamTexture.isPlaying)
            return; // Já está rodando.

        WebCamDevice[] devices = WebCamTexture.devices;
        print(webcamTexture);
        if (devices.Length == 0)
        {
            Debug.LogWarning("Nenhuma câmera detectada!");
            return;
        }

        string selectedDevice = deviceName;
        if (string.IsNullOrEmpty(deviceName))
        {
            selectedDevice = devices[0].name; // Pega a primeira disponível.
        }

        webcamTexture = new WebCamTexture(selectedDevice);
        webcamTexture.Play();

        /*if (rawImage != null)*/
        print("tentando pegar a rawImage");
        rawImage.texture = webcamTexture;
        
        /*else
        {
            Debug.LogWarning("RawImage não atribuído na cena atual.");
        }*/
    }

    public void StopCamera()
    {
        if (webcamTexture != null)
        {
            if (webcamTexture.isPlaying)
                webcamTexture.Stop();

            webcamTexture = null;
        }
    }


}
