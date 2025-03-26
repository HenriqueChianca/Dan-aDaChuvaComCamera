using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CameraMovement : MonoBehaviour
{
    public ParticleSystem particleSystem;
    public RawImage displayImage; // Exibe a imagem da câmera na interface
    private WebCamTexture webCamTexture;
    private Color32[] previousFrame;
    private bool isActive = false;
    private float inactivityTimer = 0f;

    [Tooltip("Tempo (em segundos) antes de desativar as partículas em caso de inatividade")]
    public float inactivityThreshold = 3f;

    [Tooltip("Quantidade mínima de pixels alterados para ativar as partículas")]
    public int movementThreshold = 1000;

    void Start()
    {
        StartCoroutine(InitializeCamera());
        if (particleSystem != null)
        {
            // Garante que as partículas estejam desativadas no início
            var emission = particleSystem.emission;
            emission.enabled = false;
            particleSystem.Stop();
        }
    }

    IEnumerator InitializeCamera()
    {
        if (WebCamTexture.devices.Length > 0)
        {
            // Seleciona a primeira câmera disponível
            webCamTexture = new WebCamTexture(WebCamTexture.devices[0].name);
            webCamTexture.Play();
            yield return new WaitUntil(() => webCamTexture.width > 100);

            Debug.Log("✅ Câmera inicializada!");
            previousFrame = new Color32[webCamTexture.width * webCamTexture.height];

            if (displayImage != null)
            {
                displayImage.texture = webCamTexture;
                displayImage.material.mainTexture = webCamTexture;
            }
        }
        else
        {
            Debug.LogError("🚨 Nenhuma câmera detectada!");
        }
    }

    void Update()
    {
        if (webCamTexture == null || !webCamTexture.isPlaying)
            return;

        DetectCameraMovement();

        if (isActive)
        {
            inactivityTimer += Time.deltaTime;
            if (inactivityTimer >= inactivityThreshold)
            {
                DeactivateParticles();
            }
        }
    }

    void DetectCameraMovement()
    {
        if (!webCamTexture.didUpdateThisFrame) return;

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

        Debug.Log("📸 Pixels alterados: " + diffCount);

        if (diffCount > movementThreshold)
        {
            ActivateParticles();
        }
    }

    void ActivateParticles()
    {
        if (!isActive)
        {
            isActive = true;
            inactivityTimer = 0f;
            if (particleSystem != null)
            {
                var emission = particleSystem.emission;
                emission.enabled = true;
                particleSystem.Play();
            }
        }
    }

    void DeactivateParticles()
    {
        if (isActive)
        {
            isActive = false;
            if (particleSystem != null)
            {
                var emission = particleSystem.emission;
                emission.enabled = false;
                particleSystem.Stop();
            }
        }
    }
}
