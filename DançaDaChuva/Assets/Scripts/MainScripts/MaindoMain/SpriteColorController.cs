using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpriteColorController : MonoBehaviour
{
    [System.Serializable]
    public class ColorPhase
    {
        public int collisionThreshold;
        public Color targetColor;
        public float newScaleY = 1f;
    }

    [System.Serializable]
    public class SpriteColorInfo
    {
        public SpriteRenderer spriteRenderer;
        public List<ColorPhase> colorPhases = new List<ColorPhase>();
    }

    [Header("Sprites a serem modificados")]
    public List<SpriteColorInfo> spriteColorList = new List<SpriteColorInfo>();

    [Header("Configurações de Regresso e Vitória")]
    public float regressionDelay = 5f;
    public float winDelay = 5f;
    public float loseDelay = 5f;
    public float minAdvanceCooldown = 1f;

    [Header("Controle de partículas por movimento")]
    public ParticleSystem targetParticleSystem;
    public float activationDelay = 0f;
    public int movementsNeeded = 10;
    public float inactivityThreshold = 3f;

    [Header("Sensibilidade de Movimento")]
    public float mouseMovementThreshold = 0.05f;
    public float cameraThreshold = 15f;

    [Header("RawImage (opcional) para câmera")]
    public RawImage rawImageDisplay;

    // Internos: sprites
    private Dictionary<SpriteRenderer, int> collisionCounts = new();
    private Dictionary<SpriteRenderer, int> currentPhaseIndex = new();
    private Dictionary<SpriteRenderer, Vector3> originalPositions = new();
    private Dictionary<SpriteRenderer, float> lastAdvanceTime = new();

    // Internos: controle de tempo e estado
    private float regressionTimer = 0f;
    private float winTimer = 0f;
    private float loseTimer = 0f;
    private bool hasCollided = false;
    private bool isPaused = false;

    // Internos: movimento e câmera
    private Vector2 lastMousePosition;
    private int movementCount = 0;
    private float inactivityTimer = 0f;
    private bool isEmitting = false;
    private Coroutine activationCoroutine = null;

    private WebCamTexture webcamTexture;
    private Color32[] previousFrame;
    private Color32[] currentFrame;

    void Start()
    {
        // Inicialização dos sprites
        foreach (var spriteInfo in spriteColorList)
        {
            if (spriteInfo?.spriteRenderer == null) continue;

            var sr = spriteInfo.spriteRenderer;
            collisionCounts[sr] = 0;
            currentPhaseIndex[sr] = 0;
            originalPositions[sr] = sr.transform.position;
            lastAdvanceTime[sr] = Time.time;

            if (spriteInfo.colorPhases.Count > 0)
                sr.color = spriteInfo.colorPhases[0].targetColor;
        }

        // Inicializa mouse
        lastMousePosition = Input.mousePosition;

        // Inicializa câmera
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

        // Inicializa partículas
        if (targetParticleSystem != null)
        {
            var emission = targetParticleSystem.emission;
            emission.enabled = false;
            targetParticleSystem.Stop();
        }
    }

    void Update()
    {
        if (isPaused) return;

        bool moved = DetectMouseMovement() | DetectCameraMovement();

        if (moved)
        {
            movementCount++;
            inactivityTimer = 0f;

            if (movementCount >= movementsNeeded && !isEmitting && activationCoroutine == null)
            {
                activationCoroutine = StartCoroutine(ActivateAfterDelay());
            }
        }
        else
        {
            if (isEmitting)
            {
                inactivityTimer += Time.deltaTime;
                if (inactivityTimer >= inactivityThreshold)
                {
                    DeactivateParticleSystem();
                }
            }
        }

        CheckSpriteStates();
    }

    void CheckSpriteStates()
    {
        bool allAtFinal = true;
        bool allAtInitial = true;

        foreach (var spriteInfo in spriteColorList)
        {
            if (spriteInfo?.spriteRenderer == null) continue;
            var sr = spriteInfo.spriteRenderer;

            int index = currentPhaseIndex[sr];
            if (index < spriteInfo.colorPhases.Count)
                allAtFinal = false;

            if (index > 0)
                allAtInitial = false;
        }

        // Vitória
        if (allAtFinal)
        {
            winTimer += Time.deltaTime;
            if (winTimer >= winDelay)
            {
                Debug.Log("[Unified] Vitória!");
                GameStateManager.Instance.OnWinCondition();
            }
        }
        else winTimer = 0f;

        // Derrota
        if (allAtInitial && hasCollided)
        {
            loseTimer += Time.deltaTime;
            if (loseTimer >= loseDelay)
            {
                Debug.Log("[Unified] Derrota!");
                GameStateManager.Instance.OnLoseCondition();
            }
        }
        else loseTimer = 0f;

        // Regressão
        if (!allAtFinal)
        {
            regressionTimer += Time.deltaTime;
            if (regressionTimer >= regressionDelay)
            {
                foreach (var spriteInfo in spriteColorList)
                {
                    if (spriteInfo?.spriteRenderer == null) continue;
                    var sr = spriteInfo.spriteRenderer;
                    int currentIndex = currentPhaseIndex[sr];

                    if (currentIndex > 0)
                    {
                        currentPhaseIndex[sr]--;
                        var phase = spriteInfo.colorPhases[currentPhaseIndex[sr]];
                        sr.color = phase.targetColor;
                        sr.transform.localScale = new Vector3(sr.transform.localScale.x, phase.newScaleY, sr.transform.localScale.z);
                        sr.transform.position = originalPositions[sr];
                        collisionCounts[sr] = 0;

                        Debug.Log($"[Unified] Regressão sprite {sr.name} -> fase {currentPhaseIndex[sr]}");
                    }
                }
                regressionTimer = 0f;
            }
        }
        else regressionTimer = 0f;
    }

    void OnParticleCollision(GameObject other)
    {
        if (isPaused) return;

        hasCollided = true;

        foreach (var spriteInfo in spriteColorList)
        {
            if (spriteInfo?.spriteRenderer == null) continue;
            var sr = spriteInfo.spriteRenderer;

            collisionCounts[sr]++;
            int index = currentPhaseIndex[sr];

            if (index < spriteInfo.colorPhases.Count &&
                Time.time - lastAdvanceTime[sr] >= minAdvanceCooldown &&
                collisionCounts[sr] >= spriteInfo.colorPhases[index].collisionThreshold)
            {
                var phase = spriteInfo.colorPhases[index];
                sr.color = phase.targetColor;
                sr.transform.localScale = new Vector3(sr.transform.localScale.x, phase.newScaleY, sr.transform.localScale.z);
                sr.transform.position = originalPositions[sr];

                currentPhaseIndex[sr]++;
                collisionCounts[sr] = 0;
                lastAdvanceTime[sr] = Time.time;

                Debug.Log($"[Unified] Sprite {sr.name} avançou para fase {currentPhaseIndex[sr]}");
            }
        }
    }

    bool DetectMouseMovement()
    {
        Vector2 currentPos = Input.mousePosition;
        float distance = Vector2.Distance(currentPos, lastMousePosition);
        lastMousePosition = currentPos;
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
        float totalDiff = 0f;

        for (int i = 0; i < currentFrame.Length; i += 10)
        {
            Color32 c = currentFrame[i];
            Color32 p = previousFrame[i];
            float diff = Mathf.Abs(c.r - p.r) + Mathf.Abs(c.g - p.g) + Mathf.Abs(c.b - p.b);
            totalDiff += diff / 3f;
        }

        currentFrame.CopyTo(previousFrame, 0);
        return totalDiff > cameraThreshold * 1000f;
    }

    IEnumerator ActivateAfterDelay()
    {
        yield return new WaitForSeconds(activationDelay);
        if (!isEmitting && targetParticleSystem != null)
        {
            var emission = targetParticleSystem.emission;
            emission.enabled = true;
            targetParticleSystem.Play();
            isEmitting = true;
            Debug.Log("[Unified] ParticleSystem ativado");
        }
        activationCoroutine = null;
    }

    void DeactivateParticleSystem()
    {
        if (targetParticleSystem != null)
        {
            var emission = targetParticleSystem.emission;
            emission.enabled = false;
            targetParticleSystem.Stop();
            Debug.Log("[Unified] ParticleSystem desativado por inatividade");
        }
        isEmitting = false;
        movementCount = 0;
        inactivityTimer = 0f;
        if (activationCoroutine != null)
        {
            StopCoroutine(activationCoroutine);
            activationCoroutine = null;
        }
    }

    public void SetPaused(bool paused)
    {
        isPaused = paused;
    }
}
