using UnityEngine;

public class CameraInitializer : MonoBehaviour
{
    void Start()
    {
        CameraMovement cameraMovement = FindObjectOfType<CameraMovement>();
        if (cameraMovement != null)
        {
            cameraMovement.ForceReconnectCamera();
            Debug.Log("Reconexão de câmera forçada na carga da cena.");
        }
        else
        {
            Debug.LogWarning("Nenhum CameraMovement encontrado na cena.");
        }
    }
}
