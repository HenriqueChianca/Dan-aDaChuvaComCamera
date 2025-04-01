using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        [Tooltip("SpriteRenderer do objeto filho que será ativado quando o sprite avançar de fase e desativado quando voltar ao estado inicial.")]
        public SpriteRenderer childSpriteRenderer;
    }

    public List<SpriteColorInfo> spriteColorList = new List<SpriteColorInfo>();

    [SerializeField] private ParticleSystem particleSystem;
    private Dictionary<SpriteRenderer, int> collisionCounts = new Dictionary<SpriteRenderer, int>();
    private Dictionary<SpriteRenderer, int> currentPhaseIndex = new Dictionary<SpriteRenderer, int>();

    private bool isMouseMoving = false;
    private float idleTime = 0f;
    public float idleThreshold = 2f;

    private float regressionTimer = 0f;
    public float regressionDelay = 5f;

    private float loseTimer = 0f;
    public float loseDelay = 5f;
    private bool hasCollided = false;

    private float winTimer = 0f;
    public float winDelay = 5f;

    private Dictionary<SpriteRenderer, Vector3> originalPositions = new Dictionary<SpriteRenderer, Vector3>();

    void Start()
    {
        foreach (var spriteInfo in spriteColorList)
        {
            if (spriteInfo == null || spriteInfo.spriteRenderer == null)
            {
                Debug.LogWarning("Um item da lista ou seu SpriteRenderer está nulo e será ignorado.");
                continue;
            }
            collisionCounts[spriteInfo.spriteRenderer] = 0;
            currentPhaseIndex[spriteInfo.spriteRenderer] = 0;
            originalPositions[spriteInfo.spriteRenderer] = spriteInfo.spriteRenderer.transform.position;
            if (spriteInfo.colorPhases != null && spriteInfo.colorPhases.Count > 0)
            {
                spriteInfo.spriteRenderer.color = spriteInfo.colorPhases[0].targetColor;
            }
            if (spriteInfo.childSpriteRenderer != null)
            {
                spriteInfo.childSpriteRenderer.enabled = false;
            }
        }

        if (particleSystem == null)
        {
            particleSystem = GetComponent<ParticleSystem>();
        }
    }

    void Update()
    {
        HandleMouseActivity();
        HandleRegression();
        CheckWinCondition();
    }

    private void HandleMouseActivity()
    {
        if (Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0)
        {
            isMouseMoving = true;
            idleTime = 0f;
            if (particleSystem != null && !particleSystem.isEmitting)
            {
                var emission = particleSystem.emission;
                emission.enabled = true;
            }
        }
        else
        {
            idleTime += Time.deltaTime;
            if (idleTime >= idleThreshold && particleSystem.isEmitting)
            {
                isMouseMoving = false;
                var emission = particleSystem.emission;
                emission.enabled = false;
            }
        }
    }

    private void HandleRegression()
    {
        foreach (var spriteInfo in spriteColorList)
        {
            if (spriteInfo == null || spriteInfo.spriteRenderer == null)
                continue;
            int currentIndex = currentPhaseIndex[spriteInfo.spriteRenderer];
            if (currentIndex > 0)
            {
                currentPhaseIndex[spriteInfo.spriteRenderer]--;
                ColorPhase previousPhase = spriteInfo.colorPhases[currentPhaseIndex[spriteInfo.spriteRenderer]];
                spriteInfo.spriteRenderer.color = previousPhase.targetColor;
                Transform spriteTransform = spriteInfo.spriteRenderer.transform;
                spriteTransform.localScale = new Vector3(spriteTransform.localScale.x, previousPhase.newScaleY, spriteTransform.localScale.z);
                spriteTransform.position = originalPositions[spriteInfo.spriteRenderer];
                collisionCounts[spriteInfo.spriteRenderer] = 0;

                if (currentPhaseIndex[spriteInfo.spriteRenderer] == 0 && spriteInfo.childSpriteRenderer != null)
                {
                    spriteInfo.childSpriteRenderer.enabled = false;
                }
                else if (spriteInfo.childSpriteRenderer != null)
                {
                    spriteInfo.childSpriteRenderer.enabled = true;
                    spriteInfo.childSpriteRenderer.color = previousPhase.targetColor;
                }
            }
        }
    }

    private void CheckWinCondition()
    {
        bool allAtFinalPhase = true;
        foreach (var spriteInfo in spriteColorList)
        {
            if (spriteInfo == null || spriteInfo.spriteRenderer == null)
                continue;
            int currentIndex = currentPhaseIndex[spriteInfo.spriteRenderer];
            if (currentIndex < spriteInfo.colorPhases.Count - 1)
            {
                allAtFinalPhase = false;
                break;
            }
        }

        if (allAtFinalPhase)
        {
            winTimer += Time.deltaTime;
            if (winTimer >= winDelay)
            {
                SceneManager.LoadScene("GameOverWin");
            }
        }
        else
        {
            winTimer = 0f;
        }
    }

    void OnParticleCollision(GameObject other)
    {
        hasCollided = true;
        foreach (var spriteInfo in spriteColorList)
        {
            if (spriteInfo == null || spriteInfo.spriteRenderer == null)
                continue;
            if (collisionCounts.ContainsKey(spriteInfo.spriteRenderer))
            {
                collisionCounts[spriteInfo.spriteRenderer]++;
                int currentIndex = currentPhaseIndex[spriteInfo.spriteRenderer];
                if (currentIndex < spriteInfo.colorPhases.Count &&
                    collisionCounts[spriteInfo.spriteRenderer] >= spriteInfo.colorPhases[currentIndex].collisionThreshold)
                {
                    spriteInfo.spriteRenderer.color = spriteInfo.colorPhases[currentIndex].targetColor;
                    Transform spriteTransform = spriteInfo.spriteRenderer.transform;
                    Vector3 fixedPosition = originalPositions[spriteInfo.spriteRenderer];
                    Vector3 currentScale = spriteTransform.localScale;
                    spriteTransform.localScale = new Vector3(currentScale.x, spriteInfo.colorPhases[currentIndex].newScaleY, currentScale.z);
                    spriteTransform.position = fixedPosition;
                    currentPhaseIndex[spriteInfo.spriteRenderer]++;
                    collisionCounts[spriteInfo.spriteRenderer] = 0;

                    if (currentPhaseIndex[spriteInfo.spriteRenderer] > 0 && spriteInfo.childSpriteRenderer != null)
                    {
                        spriteInfo.childSpriteRenderer.enabled = true;
                        if (currentPhaseIndex[spriteInfo.spriteRenderer] - 1 < spriteInfo.colorPhases.Count)
                        {
                            spriteInfo.childSpriteRenderer.color = spriteInfo.colorPhases[currentPhaseIndex[spriteInfo.spriteRenderer] - 1].targetColor;
                        }
                    }
                }
            }
        }
    }
}
