using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SpriteColorController : MonoBehaviour
{
    [System.Serializable]
    public class ColorPhase
    {
        public int collisionThreshold;  // N�mero de colis�es necess�rias para avan�ar para esta fase
        public Color targetColor;       // Cor a ser aplicada nesta fase
        public float newScaleY = 1f;    // Nova escala em Y para esta fase
    }

    [System.Serializable]
    public class SpriteColorInfo
    {
        public SpriteRenderer spriteRenderer;      // Refer�ncia ao SpriteRenderer principal
        public List<ColorPhase> colorPhases = new List<ColorPhase>(); // Lista de fases
    }

    public List<SpriteColorInfo> spriteColorList = new List<SpriteColorInfo>();

    [SerializeField] private ParticleSystem particleSystem;
    private Dictionary<SpriteRenderer, int> collisionCounts = new Dictionary<SpriteRenderer, int>();
    private Dictionary<SpriteRenderer, int> currentPhaseIndex = new Dictionary<SpriteRenderer, int>();

    private bool isMouseMoving = false;
    private float idleTime = 0f;
    public float idleThreshold = 2f;

    // Timer para regress�o (quando n�o h� colis�es)
    private float regressionTimer = 0f;
    public float regressionDelay = 5f;

    // Timer para a condi��o de derrota (quando todos est�o no estado inicial ap�s colis�es)
    private float loseTimer = 0f;
    public float loseDelay = 5f;
    private bool hasCollided = false;

    // Timer para a condi��o de vit�ria (quando todos atingiram o estado final)
    private float winTimer = 0f;
    public float winDelay = 5f;

    private Dictionary<SpriteRenderer, Vector3> originalPositions = new Dictionary<SpriteRenderer, Vector3>();

    void Start()
    {
        // Inicializa cada item da lista, verificando refer�ncias
        foreach (var spriteInfo in spriteColorList)
        {
            if (spriteInfo == null || spriteInfo.spriteRenderer == null)
            {
                Debug.LogWarning("Um item da lista ou seu SpriteRenderer est� nulo e ser� ignorado.");
                continue;
            }
            collisionCounts[spriteInfo.spriteRenderer] = 0;
            currentPhaseIndex[spriteInfo.spriteRenderer] = 0;
            originalPositions[spriteInfo.spriteRenderer] = spriteInfo.spriteRenderer.transform.position;
            if (spriteInfo.colorPhases != null && spriteInfo.colorPhases.Count > 0)
            {
                // Inicializa com a cor da primeira fase
                spriteInfo.spriteRenderer.color = spriteInfo.colorPhases[0].targetColor;
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
        // Ativa/desativa a emiss�o do Particle System com base no movimento do mouse
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
            if (idleTime >= idleThreshold && particleSystem != null && particleSystem.isEmitting)
            {
                isMouseMoving = false;
                var emission = particleSystem.emission;
                emission.enabled = false;
            }
        }
    }

    private void HandleRegression()
    {
        // Verifica se TODOS os sprites est�o na fase final
        bool allAtFinal = true;
        foreach (var spriteInfo in spriteColorList)
        {
            if (spriteInfo == null || spriteInfo.spriteRenderer == null)
                continue;
            if (currentPhaseIndex[spriteInfo.spriteRenderer] < spriteInfo.colorPhases.Count)
            {
                allAtFinal = false;
                break;
            }
        }
        // Se n�o estiverem todos no final, permite a regress�o
        if (!allAtFinal)
        {
            if (!isMouseMoving)
            {
                regressionTimer += Time.deltaTime;
                if (regressionTimer >= regressionDelay)
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
                        }
                    }
                    regressionTimer = 0f;
                }
            }
        }
        else
        {
            // Se todos est�o no final, reseta o timer de regress�o para n�o interferir no win timer
            regressionTimer = 0f;
        }

        // Verifica a condi��o de derrota: se todos os sprites estiverem no estado inicial (fase 0) ap�s colis�es
        bool allAtInitial = true;
        foreach (var spriteInfo in spriteColorList)
        {
            if (spriteInfo == null || spriteInfo.spriteRenderer == null)
                continue;
            if (currentPhaseIndex[spriteInfo.spriteRenderer] != 0)
            {
                allAtInitial = false;
                break;
            }
        }
        if (allAtInitial && hasCollided)
        {
            loseTimer += Time.deltaTime;
            if (loseTimer >= loseDelay)
            {
                SceneManager.LoadScene("GameOverLose");
            }
        }
        else
        {
            loseTimer = 0f;
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
            if (currentIndex < spriteInfo.colorPhases.Count)
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
                }
            }
        }
    }
}
