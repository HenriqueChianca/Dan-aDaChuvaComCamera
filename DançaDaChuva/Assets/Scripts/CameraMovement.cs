using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraMovement : MonoBehaviour
{

    [System.Serializable]
    public class ParticleSystemData
    {
        [Tooltip("Particle System a ser controlado.")]
        public ParticleSystem ps;

        [Tooltip("Delay individual de ativação (em segundos) para este Particle System.")]
        public float activationDelay;

        [HideInInspector]
        public bool isActive = false;

        [HideInInspector]
        public Coroutine activationCoroutine = null;
    }


    [Tooltip("Lista de Particle Systems (com delay individual) a serem controlados pelo movimento detectado pela câmera.")]
    public List<ParticleSystemData> particleSystemsData;

    [Tooltip("RawImage para exibir a imagem da câmera na interface.")]
    public RawImage displayImage;

    [Tooltip("Tempo em segundos para desativar as partículas após inatividade.")]
    public float inactivityThreshold = 3f;

    [Tooltip("Quantidade mínima de pixels alterados para ativar os Particle Systems.")]
    public int movementThreshold = 1000;

    private WebCamTexture webCamTexture;
    private Color32[] previousFrame;
    private float inactivityTimer = 0f;

    void Start()
    {
        StartCoroutine(InitializeCamera());

        foreach (ParticleSystemData data in particleSystemsData)
        {
            if (data.ps != null)
            {
                data.ps.Stop();
                var emission = data.ps.emission;
                emission.enabled = false;
            }
        }
    }

    IEnumerator InitializeCamera()
    {
        if (WebCamTexture.devices.Length > 0)
        {
            webCamTexture = new WebCamTexture(WebCamTexture.devices[0].name);
            webCamTexture.Play();
            yield return new WaitUntil(() => webCamTexture.width > 100);
            Debug.Log("Câmera inicializada!");
            previousFrame = new Color32[webCamTexture.width * webCamTexture.height];

            if (displayImage != null)
            {
                displayImage.texture = webCamTexture;
                displayImage.material.mainTexture = webCamTexture;
            }
        }
        else
        {
            Debug.LogError("Nenhuma câmera detectada!");
        }
    }

    void Update()
    {
        if (webCamTexture == null || !webCamTexture.isPlaying)
            return;

        DetectCameraMovement();

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

    void DetectCameraMovement()
    {
        if (!webCamTexture.didUpdateThisFrame)
            return;

        Color32[] currentFrame = webCamTexture.GetPixels32();
        int diffCount = 0;

        if (previousFrame != null && previousFrame.Length == currentFrame.Length)
        {
            for (int i = 0; i < currentFrame.Length; i += 10)
            {
                if (Mathf.Abs(currentFrame[i].r - previousFrame[i].r) > 20 ||
                    Mathf.Abs(currentFrame[i].g - previousFrame[i].g) > 20 ||
                    Mathf.Abs(currentFrame[i].b - previousFrame[i].b) > 20)
                {
                    diffCount++;
                }
            }
        }

        previousFrame = (Color32[])currentFrame.Clone();
        Debug.Log("Pixels alterados: " + diffCount);

        if (diffCount > movementThreshold)
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
