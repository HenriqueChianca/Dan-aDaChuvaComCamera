using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class CameraMovement : MonoBehaviour
{
    public static CameraMovement Instance;


    private WebCamTexture webCamTexture;
    public RawImage rawImage; // Arraste a RawImage aqui no Inspector
    private string preferredCameraName = "PS3 Eye Universal"; // Nome exato da PS3 Eye
    private bool tryingToReconnect = false;

    void Start()
    {
        LogAvailableCameras(); // Apenas para debug
        InitializeCamera();
    }


    void Awake()
    {
        // Singleton e persistência
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Reconecta RawImage sempre que a cena carregar
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Se a cena foi recarregada, encontra o novo RawImage e reaplica a textura
        if (rawImage == null)
            rawImage = FindObjectOfType<RawImage>();

        if (rawImage != null && webCamTexture != null)
        {
            rawImage.texture = webCamTexture;
            rawImage.material.mainTexture = webCamTexture;
        }
    }



    void LogAvailableCameras()
    {
        WebCamDevice[] devices = WebCamTexture.devices;
        if (devices.Length == 0)
        {
            Debug.LogWarning("Nenhuma câmera detectada no sistema.");
        }
        else
        {
            for (int i = 0; i < devices.Length; i++)
            {
                Debug.Log($"[CAMERA DETECTADA {i}] Nome: {devices[i].name}");
            }
        }
    }

    void InitializeCamera()
    {
        WebCamDevice[] devices = WebCamTexture.devices;

        if (devices.Length == 0)
        {
            Debug.LogError("Nenhuma câmera foi encontrada!");
            return;
        }

        // Procura pela câmera preferida primeiro
        foreach (var device in devices)
        {
            if (device.name == preferredCameraName)
            {
                Debug.Log("Usando câmera preferida: " + device.name);
                StartCamera(device.name);
                return;
            }
        }

        // Se não encontrou a câmera preferida, usa a primeira disponível
        Debug.LogWarning("Câmera preferida não encontrada. Usando a primeira disponível: " + devices[0].name);
        StartCamera(devices[0].name);
    }

    void StartCamera(string deviceName)
    {
        // Para e limpa a anterior, se houver
        if (webCamTexture != null)
        {
            if (webCamTexture.isPlaying)
                webCamTexture.Stop();

            Destroy(webCamTexture);
            webCamTexture = null;
        }

        webCamTexture = new WebCamTexture(deviceName);
        rawImage.texture = webCamTexture;
        rawImage.material.mainTexture = webCamTexture;

        webCamTexture.Play();

        Debug.Log("Iniciada câmera: " + deviceName);

        if (!tryingToReconnect)
        {
            StartCoroutine(CheckCameraConnection());
        }
    }

    IEnumerator CheckCameraConnection()
    {
        tryingToReconnect = true;

        while (true)
        {
            yield return new WaitForSeconds(2f);

            if (webCamTexture == null || !webCamTexture.isPlaying || webCamTexture.width <= 16)
            {
                Debug.LogWarning("Câmera travou ou não iniciou. Tentando reiniciar...");
                InitializeCamera(); // Tenta reiniciar
                break;
            }
        }

        tryingToReconnect = false;
    }

    public void ForceReconnectCamera()
    {
        Debug.Log("Forçando reconexão da câmera...");
        InitializeCamera();
    }

    public void StopCamera()
    {
        if (webCamTexture != null && webCamTexture.isPlaying)
        {
            webCamTexture.Stop();
            Debug.Log("Câmera parada manualmente.");
        }
    }
}
