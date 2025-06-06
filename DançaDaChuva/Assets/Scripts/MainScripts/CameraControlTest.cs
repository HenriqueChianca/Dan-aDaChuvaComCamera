using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CameraControlTest : MonoBehaviour
{
    private WebCamTexture webcamTexture;
    public RawImage rawImageDisplay; // Atribua no Inspector o seu RawImage
    public string targetSceneName = "Game"; // Nome da cena onde o script deve funcionar
    public string preferredCameraName = "PS3 Eye Universal"; // Nome preferencial da c�mera
    public string rawImageObjectName = "RawImage";

    [Header("Configura��es da C�mera")]
    public int requestedWidth = 640;
    public int requestedHeight = 480;
    public int requestedFPS = 60;

    [Header("Fallback Settings")]
    public bool useAnyAvailableCamera = true; // Usar qualquer c�mera se a preferida n�o for encontrada
    public bool showAvailableDevices = true; // Mostrar dispositivos dispon�veis no console

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
        // Verifica se a c�mera j� est� inicializada
        if (webcamTexture != null && webcamTexture.isPlaying)
        {
            Debug.Log("C�mera j� est� em funcionamento.");
            return;
        }

        // Obt�m todos os dispositivos de c�mera dispon�veis
        WebCamDevice[] devices = WebCamTexture.devices;

        if (devices.Length == 0)
        {
            Debug.LogError("Nenhuma c�mera encontrada no sistema!");
            return;
        }

        // Mostra dispositivos dispon�veis no console
        if (showAvailableDevices)
        {
            Debug.Log("C�meras dispon�veis:");
            foreach (var device in devices)
            {
                Debug.Log($"- {device.name} (FrontFacing: {device.isFrontFacing})");
            }
        }

        // Tenta encontrar a c�mera preferida
        WebCamDevice selectedDevice = default;
        bool cameraFound = false;

        foreach (var device in devices)
        {
            if (device.name.Contains(preferredCameraName))
            {
                selectedDevice = device;
                cameraFound = true;
                Debug.Log($"C�mera preferencial encontrada: {device.name}");
                break;
            }
        }

        // Fallback: usa a primeira c�mera dispon�vel se a preferida n�o for encontrada
        if (!cameraFound && useAnyAvailableCamera && devices.Length > 0)
        {
            selectedDevice = devices[0];
            cameraFound = true;
            Debug.Log($"Usando c�mera alternativa: {selectedDevice.name}");
        }

        if (!cameraFound)
        {
            Debug.LogError($"C�mera preferencial '{preferredCameraName}' n�o encontrada e fallback desabilitado.");
            return;
        }

        // Inicializa a textura da webcam com configura��es desejadas
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
                Debug.LogError("RawImage n�o atribu�do no Inspector!");
            }

            // Inicia a c�mera
            webcamTexture.Play();
            Debug.Log($"C�mera {selectedDevice.name} inicializada com sucesso (Resolu��o: {webcamTexture.width}x{webcamTexture.height}, FPS: {webcamTexture.requestedFPS})");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Erro ao inicializar c�mera: {e.Message}");

            // Tentativa fallback com configura��es padr�o
            try
            {
                Debug.Log("Tentando fallback com configura��es padr�o...");
                webcamTexture = new WebCamTexture(selectedDevice.name);
                if (rawImageDisplay != null)
                {
                    rawImageDisplay.texture = webcamTexture;
                    rawImageDisplay.material.mainTexture = webcamTexture;
                }
                webcamTexture.Play();
                Debug.Log($"C�mera inicializada em resolu��o padr�o: {webcamTexture.width}x{webcamTexture.height}");
            }
            catch (System.Exception fallbackException)
            {
                Debug.LogError($"Fallback tamb�m falhou: {fallbackException.Message}");
            }
        }
    }

    private void StopCamera()
    {
        if (webcamTexture != null && webcamTexture.isPlaying)
        {
            webcamTexture.Stop();
            Debug.Log("C�mera parada.");
        }
    }

    void OnDestroy()
    {
        StopCamera();
    }

    // M�todo p�blico para reiniciar a c�mera manualmente se necess�rio
    public void RestartCamera()
    {
        StopCamera();
        InitializeCamera();
    }

    private void FindAndAssignRawImage()
    {
        // Se j� tiver uma refer�ncia, n�o precisa procurar novamente
        if (rawImageDisplay != null) return;

        // Procura o RawImage na hierarquia
        GameObject rawImageObj = GameObject.Find(rawImageObjectName);

        if (rawImageObj != null)
        {
            rawImageDisplay = rawImageObj.GetComponent<RawImage>();

            if (rawImageDisplay == null)
            {
                Debug.LogError($"O objeto '{rawImageObjectName}' encontrado n�o possui componente RawImage!");
            }
            else
            {
                Debug.Log($"RawImage '{rawImageObjectName}' atribu�do automaticamente.");
            }
        }
        else
        {
            Debug.LogError($"Nenhum objeto chamado '{rawImageObjectName}' encontrado na hierarquia!");
        }
    }
}