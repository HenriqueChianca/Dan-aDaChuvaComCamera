// ============================
// UnifiedMovementDetector.cs
// ============================
using System.Collections.Generic;
using UnityEngine;

public class UnifiedMovementDetector : MonoBehaviour
{
    [Header("Sensibilidade e Tempo")]
    public float minMovementThreshold = 0.01f;
    public float inactivityDuration = 2f;

    [Header("Referências")]
    public RainController rainController; // Script que controla o sistema de partículas
    public Camera cameraToTrack;

    private Vector3 lastMousePosition;
    private Vector3 lastCameraPosition;
    private float inactivityTimer = 0f;
    private bool isActive = false;

    void Start()
    {
        lastMousePosition = Input.mousePosition;
        if (cameraToTrack != null)
            lastCameraPosition = cameraToTrack.transform.position;
    }

    void Update()
    {
        bool mouseMoved = Vector3.Distance(Input.mousePosition, lastMousePosition) > minMovementThreshold;
        bool cameraMoved = false;

        if (cameraToTrack != null)
        {
            cameraMoved = Vector3.Distance(cameraToTrack.transform.position, lastCameraPosition) > minMovementThreshold;
        }

        lastMousePosition = Input.mousePosition;
        if (cameraToTrack != null)
            lastCameraPosition = cameraToTrack.transform.position;

        if (mouseMoved || cameraMoved)
        {
            inactivityTimer = 0f;
            if (!isActive)
            {
                ActivateRain();
            }
        }
        else
        {
            inactivityTimer += Time.deltaTime;
            if (inactivityTimer >= inactivityDuration && isActive)
            {
                DeactivateRain();
            }
        }
    }

    void ActivateRain()
    {
        isActive = true;
        rainController?.ActivateAll();
        Debug.Log("[UnifiedMovementDetector] Ativando partículas (movimento detectado).");
    }

    void DeactivateRain()
    {
        isActive = false;
        rainController?.DeactivateAll();
        Debug.Log("[UnifiedMovementDetector] Desativando partículas (inatividade detectada).");
    }
}
