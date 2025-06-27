using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MouseAndCameraMovement : MonoBehaviour
{
    [System.Serializable]
    public class ParticleSystemData
    {
        public ParticleSystem ps;
        public float activationDelay;
        [HideInInspector] public bool isActive = false;
        [HideInInspector] public Coroutine activationCoroutine = null;
    }

    [Header("Particle Systems controlados por movimento")]
    public List<ParticleSystemData> particleSystemsData;

    [Header("Configurações de movimento")]
    public int movementsNeeded = 10;
    public float inactivityThreshold = 3f;

    [Header("Mouse")]
    public float mouseMovementThreshold = 0.05f;

    [Header("Câmera USB")]
    public float cameraThreshold = 15f;
    public RawImage rawImageDisplay;

    // Mouse
    private Vector2 lastMousePosition;
    private int movementCount = 0;

    // Câmera
    private WebCamTexture webcamTexture;
    private Color32[] previousFrame;
    private Color32[] currentFrame;

    // Tempo
    private float inactivityTimer = 0f;

    void Start()
    {
        // Inicializa partículas
        foreach (ParticleSystemData data in particleSystemsData)
        {
            if (data.ps != null)
            {
                data.ps.Stop();
                var emission = data.ps.emission;
                emission.enabled = false;
            }
        }

        // Mouse
        lastMousePosition = Input.mousePosition;

        // Câmera
        if (WebCamTexture.devices.Length > 0)
        {
            WebCamDevice device = WebCamTexture.devices[0];
            webcamTexture = new WebCamTexture(device.name);
            webcamTexture.Play();

            if (rawImageDisplay != null)
            {
                rawImageDisplay.texture = webcamTexture;
                rawImageDisplay.material.mainTexture = webcamTexture;
            }
        }
    }

    void Update()
    {
        bool moved = DetectMouseMovement() | DetectCameraMovement(); // usa OR bit a bit para garantir ambos

        if (moved)
        {
            movementCount++;
            inactivityTimer = 0f;

            if (movementCount >= movementsNeeded)
            {
                foreach (ParticleSystemData data in particleSystemsData)
                {
                    if (data.ps != null && !data.isActive && data.activationCoroutine == null)
                    {
                        data.activationCoroutine = StartCoroutine(ActivateAfterDelay(data));
                    }
                }
            }
        }
        else
        {
            bool anyActive = false;
            foreach (ParticleSystemData data in particleSystemsData)
            {
                if (data.isActive)
                {
                    anyActive = true;
                    break;
                }
            }

            if (anyActive)
            {
                inactivityTimer += Time.deltaTime;
                if (inactivityTimer >= inactivityThreshold)
                {
                    DeactivateAll();
                }
            }
        }
    }

    bool DetectMouseMovement()
    {
        Vector2 currentMousePosition = Input.mousePosition;
        float distance = Vector2.Distance(currentMousePosition, lastMousePosition);
        lastMousePosition = currentMousePosition;
        return distance > mouseMovementThreshold;
    }

    bool DetectCameraMovement()
    {
        if (webcamTexture == null || !webcamTexture.isPlaying || !webcamTexture.didUpdateThisFrame)
            return false;

        if (currentFrame == null || currentFrame.Length != webcamTexture.width * webcamTexture.height)
        {
            currentFrame = new Color32[webcamTexture.width * webcamTexture.height];
            previousFrame = new Color32[webcamTexture.width * webcamTexture.height];
            webcamTexture.GetPixels32(previousFrame);
        }

        webcamTexture.GetPixels32(currentFrame);

        float totalDifference = 0f;
        for (int i = 0; i < currentFrame.Length; i += 10)
        {
            Color32 current = currentFrame[i];
            Color32 previous = previousFrame[i];
            float diff = Mathf.Abs(current.r - previous.r) + Mathf.Abs(current.g - previous.g) + Mathf.Abs(current.b - previous.b);
            totalDifference += diff / 3f;
        }

        currentFrame.CopyTo(previousFrame, 0);

        return totalDifference > cameraThreshold * 1000f;
    }

    IEnumerator ActivateAfterDelay(ParticleSystemData data)
    {
        yield return new WaitForSeconds(data.activationDelay);
        if (!data.isActive)
        {
            data.isActive = true;
            if (data.ps != null)
            {
                var emission = data.ps.emission;
                emission.enabled = true;
                data.ps.Play();
            }
        }
        data.activationCoroutine = null;
    }

    void DeactivateAll()
    {
        movementCount = 0;
        foreach (ParticleSystemData data in particleSystemsData)
        {
            if (data.ps != null)
            {
                var emission = data.ps.emission;
                emission.enabled = false;
                data.ps.Stop();
                data.isActive = false;
                if (data.activationCoroutine != null)
                {
                    StopCoroutine(data.activationCoroutine);
                    data.activationCoroutine = null;
                }
            }
        }
        inactivityTimer = 0f;
    }
}
