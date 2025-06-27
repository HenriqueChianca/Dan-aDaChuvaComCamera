using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraMovementActivator : MonoBehaviour
{
    [Header("Movimento da Câmera")]
    public int webcamIndex = 0;
    public RawImage rawImageDisplay;
    public float movementThreshold = 15f;
    public float inactivityThreshold = 3f;
    public int movementsNeeded = 10;

    [Header("Partículas")]
    public List<CameraParticleSystemData> particleSystemsData;

    private WebCamTexture webcamTexture;
    private Color32[] previousFrame;
    private Color32[] currentFrame;

    private int movementCount = 0;
    private float inactivityTimer = 0f;

    // ✅ NOVO: acessível publicamente para outros scripts
    public bool CameraIsMoving { get; private set; } = false;

    void Start()
    {
        if (WebCamTexture.devices.Length > 0)
        {
            WebCamDevice device = WebCamTexture.devices[webcamIndex];
            webcamTexture = new WebCamTexture(device.name);
            rawImageDisplay.texture = webcamTexture;
            rawImageDisplay.material.mainTexture = webcamTexture;
            webcamTexture.Play();
        }

        foreach (var data in particleSystemsData)
        {
            if (data.ps != null)
            {
                data.ps.Stop();
                var emission = data.ps.emission;
                emission.enabled = false;
                data.isActive = false;
                data.activationCoroutine = null;
            }
        }
    }

    void Update()
    {
        CameraIsMoving = false;

        if (webcamTexture == null || !webcamTexture.isPlaying || !webcamTexture.didUpdateThisFrame)
            return;

        if (currentFrame == null || currentFrame.Length != webcamTexture.width * webcamTexture.height)
        {
            currentFrame = new Color32[webcamTexture.width * webcamTexture.height];
            previousFrame = new Color32[webcamTexture.width * webcamTexture.height];
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

        if (totalDifference > movementThreshold * 1000f)
        {
            CameraIsMoving = true; // ✅ NOVO: marca que a câmera se moveu
            movementCount++;

            if (movementCount >= movementsNeeded)
            {
                foreach (var data in particleSystemsData)
                {
                    if (data.ps != null && !data.isActive && data.activationCoroutine == null)
                    {
                        data.activationCoroutine = StartCoroutine(ActivateAfterDelay(data));
                    }
                }
            }

            inactivityTimer = 0f;
        }
        else
        {
            inactivityTimer += Time.deltaTime;
            if (inactivityTimer >= inactivityThreshold)
            {
                DeactivateAll();
            }
        }

        currentFrame.CopyTo(previousFrame, 0);
    }

    IEnumerator ActivateAfterDelay(CameraParticleSystemData data)
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
        foreach (var data in particleSystemsData)
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

    public void RestartCamera()
    {
        if (webcamTexture != null)
        {
            webcamTexture.Stop();
            webcamTexture.Play();
            Debug.Log("WebCam reiniciada.");
        }
    }
}

[System.Serializable]
public class CameraParticleSystemData
{
    [Tooltip("Particle System a ser controlado.")]
    public ParticleSystem ps;

    [Tooltip("Delay individual de ativação (em segundos) para este Particle System.")]
    public float activationDelay;

    [HideInInspector] public bool isActive = false;
    [HideInInspector] public Coroutine activationCoroutine = null;
}
