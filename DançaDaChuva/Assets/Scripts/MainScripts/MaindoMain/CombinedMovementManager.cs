// ==============================
// CombinedMovementManager.cs
// ==============================
using UnityEngine;

public class CombinedMovementManager : MonoBehaviour
{
    [Header("Configurações de Inatividade")]
    public float inactivityThreshold = 2f;

    [Header("Referências")]
    public RainController rainController;
    public CameraMovementActivator cameraMovementActivator; // ✅ ADICIONADO

    private float inactivityTimer = 0f;
    private bool rainIsActive = false;

    void Update()
    {
        bool mouseMoved = Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0;
        bool cameraMoved = cameraMovementActivator != null && cameraMovementActivator.CameraIsMoving; // ✅ USO CORRETO

        if (mouseMoved || cameraMoved)
        {
            inactivityTimer = 0f;
            if (!rainIsActive)
            {
                rainController?.ActivateAll();
                rainIsActive = true;
                Debug.Log("[Combined] Ativando partículas (movimento detectado).");
            }
        }
        else
        {
            inactivityTimer += Time.deltaTime;
            if (inactivityTimer >= inactivityThreshold && rainIsActive)
            {
                rainController?.DeactivateAll();
                rainIsActive = false;
                Debug.Log("[Combined] Desativando partículas (inatividade detectada).");
            }
        }
    }
}
