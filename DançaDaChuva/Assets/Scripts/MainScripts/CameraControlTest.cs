using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CameraControlTest : MonoBehaviour
{
    private WebCamTexture webcamTexture;
    public RawImage rawImageDisplay; // Atribua no Inspector o seu RawImage
    public string targetSceneName = "Game"; // Nome da cena onde o script deve funcionar
    public string preferredCameraName = "PS3 Eye Universal"; // Nome preferencial da câmera
    public string rawImageObjectName = "RawImage";

    [Header("Configurações da Câmera")]
    public int requestedWidth = 640;
    public int requestedHeight = 480;
    public int requestedFPS = 60;

    [Header("Fallback Settings")]
    public bool useAnyAvailableCamera = true; // Usar qualquer câmera se a preferida não for encontrada
    public bool showAvailableDevices = true; // Mostrar dispositivos disponíveis no console

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == targetSceneName)
        {
            FindAndAssignRawImage();
            InitializeCamera();
        }
        else
        {
            StopCamera();
        }
    }

    private void InitializeCamera()
    {
        // Verifica se a câmera já está inicializada
        if (webcamTexture != null && webcamTexture.isPlaying)
        {
            Debug.Log("Câmera já está em funcionamento.");
            return;
        }

        // Obtém todos os dispositivos de câmera disponíveis
        WebCamDevice[] devices = WebCamTexture.devices;

        if (devices.Length == 0)
        {
            Debug.LogError("Nenhuma câmera encontrada no sistema!");
            return;
        }

        // Mostra dispositivos disponíveis no console
        if (showAvailableDevices)
        {
            Debug.Log("Câmeras disponíveis:");
            foreach (var device in devices)
            {
                Debug.Log($"- {device.name} (FrontFacing: {device.isFrontFacing})");
            }
        }

        // Tenta encontrar a câmera preferida
        WebCamDevice selectedDevice = default;
        bool cameraFound = false;

        foreach (var device in devices)
        {
            if (device.name.Contains(preferredCameraName))
            {
                selectedDevice = device;
                cameraFound = true;
                Debug.Log($"Câmera preferencial encontrada: {device.name}");
                break;
            }
        }

        // Fallback: usa a primeira câmera disponível se a preferida não for encontrada
        if (!cameraFound && useAnyAvailableCamera && devices.Length > 0)
        {
            selectedDevice = devices[0];
            cameraFound = true;
            Debug.Log($"Usando câmera alternativa: {selectedDevice.name}");
        }

        if (!cameraFound)
        {
            Debug.LogError($"Câmera preferencial '{preferredCameraName}' não encontrada e fallback desabilitado.");
            return;
        }

        // Inicializa a textura da webcam com configurações desejadas
        try
        {
            webcamTexture = new WebCamTexture(selectedDevice.name, requestedWidth, requestedHeight, requestedFPS);

            // Atribui a textura ao RawImage
            if (rawImageDisplay != null)
            {
                rawImageDisplay.texture = webcamTexture;
                rawImageDisplay.material.mainTexture = webcamTexture;
                print(rawImageDisplay.material.mainTexture);
            }
            else
            {
                Debug.LogError("RawImage não atribuído no Inspector!");
            }

            // Inicia a câmera
            webcamTexture.Play();
            Debug.Log($"Câmera {selectedDevice.name} inicializada com sucesso (Resolução: {webcamTexture.width}x{webcamTexture.height}, FPS: {webcamTexture.requestedFPS})");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Erro ao inicializar câmera: {e.Message}");

            // Tentativa fallback com configurações padrão
            try
            {
                Debug.Log("Tentando fallback com configurações padrão...");
                webcamTexture = new WebCamTexture(selectedDevice.name);
                if (rawImageDisplay != null)
                {
                    rawImageDisplay.texture = webcamTexture;
                    rawImageDisplay.material.mainTexture = webcamTexture;
                }
                webcamTexture.Play();
                Debug.Log($"Câmera inicializada em resolução padrão: {webcamTexture.width}x{webcamTexture.height}");
            }
            catch (System.Exception fallbackException)
            {
                Debug.LogError($"Fallback também falhou: {fallbackException.Message}");
            }
        }
    }

    private void StopCamera()
    {
        if (webcamTexture != null && webcamTexture.isPlaying)
        {
            webcamTexture.Stop();
            Debug.Log("Câmera parada.");
        }
    }

    void OnDestroy()
    {
        StopCamera();
    }

    // Método público para reiniciar a câmera manualmente se necessário
    public void RestartCamera()
    {
        StopCamera();
        InitializeCamera();
    }

    private void FindAndAssignRawImage()
    {
        // Se já tiver uma referência, não precisa procurar novamente
        if (rawImageDisplay != null) return;

        // Procura o RawImage na hierarquia
        GameObject rawImageObj = GameObject.Find(rawImageObjectName);

        if (rawImageObj != null)
        {
            rawImageDisplay = rawImageObj.GetComponent<RawImage>();

            if (rawImageDisplay == null)
            {
                Debug.LogError($"O objeto '{rawImageObjectName}' encontrado não possui componente RawImage!");
            }
            else
            {
                Debug.Log($"RawImage '{rawImageObjectName}' atribuído automaticamente.");
            }
        }
        else
        {
            Debug.LogError($"Nenhum objeto chamado '{rawImageObjectName}' encontrado na hierarquia!");
        }
    }
}