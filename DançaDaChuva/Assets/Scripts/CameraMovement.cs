using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CameraMovement : MonoBehaviour
{
    public RawImage rawImageDisplay;
    private WebCamTexture webcamTexture;

    void Start()
    {
        StartCoroutine(FindAndStartCamera());
    }

    IEnumerator FindAndStartCamera()
    {
        WebCamDevice[] devices = WebCamTexture.devices;

        if (devices.Length == 0)
        {
            Debug.LogError("Nenhuma câmera encontrada.");
            yield break;
        }

        // Primeiro: tenta encontrar e iniciar a PS3 Eye
        foreach (var device in devices)
        {
            Debug.Log("Câmera encontrada: " + device.name);

            if (device.name.ToLower().Contains("ps3"))
            {
                Debug.Log("Tentando iniciar PS3 Eye: " + device.name);

                if (TryStartCamera(device.name))
                {
                    yield break; // sucesso, então sai
                }
            }
        }

        Debug.LogWarning("Nenhuma PS3 Eye encontrada. Procurando outra câmera USB...");

        // Segundo: tenta qualquer outra câmera que não seja virtual
        foreach (var device in devices)
        {
            string lowerName = device.name.ToLower();

            if (lowerName.Contains("virtual") || lowerName.Contains("droidcam") || lowerName.Contains("obs"))
            {
                Debug.Log("Ignorando câmera virtual: " + device.name);
                continue; // ignora câmeras virtuais
            }

            Debug.Log("Tentando iniciar outra câmera USB: " + device.name);

            if (TryStartCamera(device.name))
            {
                yield break; // sucesso, então sai
            }
        }

        Debug.LogError("Nenhuma câmera válida foi encontrada e iniciada.");
    }

    bool TryStartCamera(string deviceName)
    {
        webcamTexture = new WebCamTexture(deviceName);
        webcamTexture.Play();

        // Dá um tempinho para inicializar
        float timeout = 2f;
        float timer = 0f;

        while (timer < timeout)
        {
            if (webcamTexture.width > 16 && webcamTexture.height > 16 && webcamTexture.didUpdateThisFrame)
            {
                Debug.Log("Câmera iniciada com sucesso: " + deviceName);

                if (rawImageDisplay == null)
                {
                    rawImageDisplay = FindObjectOfType<RawImage>();
                    if (rawImageDisplay == null)
                    {
                        Debug.LogError("Nenhuma RawImage encontrada para mostrar a câmera.");
                        return false;
                    }
                }

                rawImageDisplay.texture = webcamTexture;
                rawImageDisplay.material.mainTexture = webcamTexture;
                return true;
            }

            timer += Time.deltaTime;
        }

        // Se não conseguir inicializar direito
        Debug.LogWarning("Falha ao iniciar a câmera: " + deviceName);
        webcamTexture.Stop();
        return false;
    }

    void OnDisable()
    {
        if (webcamTexture != null && webcamTexture.isPlaying)
        {
            webcamTexture.Stop();
        }
    }

    public void StopCamera()
    {
        if (webcamTexture != null && webcamTexture.isPlaying)
        {
            webcamTexture.Stop();
            Debug.Log("Câmera parada antes de reiniciar a cena.");
        }
    }

}
