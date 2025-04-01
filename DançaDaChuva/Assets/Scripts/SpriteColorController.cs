using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SpriteColorController : MonoBehaviour
{
    [System.Serializable]
    public class ColorPhase
    {
        public int collisionThreshold; // N�mero de colis�es necess�rias para avan�ar para esta fase
        public Color targetColor;      // Cor a ser aplicada nesta fase
        public float newScaleY = 1f;   // Nova escala em Y para esta fase
    }

    [System.Serializable]
    public class SpriteColorInfo
    {
        public SpriteRenderer spriteRenderer; // Refer�ncia ao SpriteRenderer principal
        public List<ColorPhase> colorPhases = new List<ColorPhase>(); // Lista de fases para este sprite
        [Tooltip("MeshRenderer do filho que ser� ativado quando o sprite avan�ar de fase e desativado quando voltar ao estado inicial.")]
        public MeshRenderer childMeshRenderer; // Mesh do objeto filho (inicialmente desativada)
    }

    public List<SpriteColorInfo> spriteColorList = new List<SpriteColorInfo>();

    // Dicion�rios para rastrear colis�es e o �ndice da fase atual para cada sprite
    private Dictionary<SpriteRenderer, int> collisionCounts = new Dictionary<SpriteRenderer, int>();
    private Dictionary<SpriteRenderer, int> currentPhaseIndex = new Dictionary<SpriteRenderer, int>();
    private Dictionary<SpriteRenderer, Vector3> originalPositions = new Dictionary<SpriteRenderer, Vector3>();

    // Controla se j� houve ao menos uma colis�o (para n�o acionar Game Over de imediato)
    private bool hasCollided = false;

    // Timers para vit�ria e derrota
    private float winTimer = 0f;
    public float winDelay = 5f; // Tempo para acionar a vit�ria

    private float loseTimer = 0f;
    public float loseDelay = 10f; // Tempo necess�rio no estado inicial para acionar o Game Over

    // Timer para regress�o: se n�o houver colis�es por esse tempo, os sprites retrocedem uma fase
    private float collisionResetTimer = 0f;
    public float collisionResetDelay = 5f;

    void Start()
    {
        // Inicializa cada item da lista, mas ignora os que n�o est�o configurados corretamente
        foreach (var spriteInfo in spriteColorList)
        {
            if (spriteInfo == null)
            {
                Debug.LogWarning("Um item da lista est� nulo e ser� ignorado.");
                continue;
            }
            if (spriteInfo.spriteRenderer == null)
            {
                Debug.LogWarning("SpriteRenderer n�o atribu�do em um SpriteColorInfo; item ignorado.");
                continue;
            }

            // Adiciona a chave somente se o spriteRenderer n�o for nulo
            collisionCounts[spriteInfo.spriteRenderer] = 0;
            currentPhaseIndex[spriteInfo.spriteRenderer] = 0;
            originalPositions[spriteInfo.spriteRenderer] = spriteInfo.spriteRenderer.transform.position;

            if (spriteInfo.colorPhases != null && spriteInfo.colorPhases.Count > 0)
            {
                spriteInfo.spriteRenderer.color = spriteInfo.colorPhases[0].targetColor;
            }

            if (spriteInfo.childMeshRenderer != null)
            {
                spriteInfo.childMeshRenderer.enabled = false;
            }
        }
    }

    void Update()
    {
        collisionResetTimer += Time.deltaTime;
        if (collisionResetTimer >= collisionResetDelay)
        {
            foreach (var spriteInfo in spriteColorList)
            {
                // Certifique-se de que o spriteRenderer n�o � nulo antes de usar os dicion�rios
                if (spriteInfo == null || spriteInfo.spriteRenderer == null)
                    continue;

                if (currentPhaseIndex[spriteInfo.spriteRenderer] > 0)
                {
                    currentPhaseIndex[spriteInfo.spriteRenderer]--;
                    ColorPhase phase = spriteInfo.colorPhases[currentPhaseIndex[spriteInfo.spriteRenderer]];
                    spriteInfo.spriteRenderer.color = phase.targetColor;
                    if (spriteInfo.spriteRenderer.material != null)
                    {
                        spriteInfo.spriteRenderer.material.color = phase.targetColor;
                    }
                    Vector3 newScale = spriteInfo.spriteRenderer.transform.localScale;
                    newScale.y = phase.newScaleY;
                    spriteInfo.spriteRenderer.transform.localScale = newScale;
                    spriteInfo.spriteRenderer.transform.position = originalPositions[spriteInfo.spriteRenderer];

                    collisionCounts[spriteInfo.spriteRenderer] = 0;

                    Debug.Log("Sprite regrediu para a fase: " + currentPhaseIndex[spriteInfo.spriteRenderer]);

                    if (currentPhaseIndex[spriteInfo.spriteRenderer] == 0 && spriteInfo.childMeshRenderer != null)
                    {
                        spriteInfo.childMeshRenderer.enabled = false;
                    }
                }
            }
            collisionResetTimer = 0f;
        }

        CheckWinCondition();
        CheckLoseCondition();
    }

    void CheckWinCondition()
    {
        bool allAtFinal = true;
        foreach (var spriteInfo in spriteColorList)
        {
            if (spriteInfo == null || spriteInfo.spriteRenderer == null)
                continue;

            // Estado final quando o �ndice � igual ao n�mero de fases
            if (currentPhaseIndex[spriteInfo.spriteRenderer] < spriteInfo.colorPhases.Count)
            {
                allAtFinal = false;
                break;
            }
        }

        if (allAtFinal)
        {
            winTimer += Time.deltaTime;
            if (winTimer >= winDelay)
            {
                Debug.Log("Vit�ria: Todos os sprites no estado final por " + winDelay + " segundos.");
                SceneManager.LoadScene("GameOverWin");
            }
        }
        else
        {
            winTimer = 0f;
        }
    }

    void CheckLoseCondition()
    {
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
                Debug.Log("Game Over: Todos os sprites no estado inicial por " + loseDelay + " segundos.");
                SceneManager.LoadScene("GameOverLose");
            }
        }
        else
        {
            loseTimer = 0f;
        }
    }

    // Chamado quando part�culas colidem com este objeto
    void OnParticleCollision(GameObject other)
    {
        hasCollided = true;
        foreach (var spriteInfo in spriteColorList)
        {
            if (spriteInfo == null || spriteInfo.spriteRenderer == null)
                continue;

            // Verifica se a chave existe no dicion�rio
            if (!collisionCounts.ContainsKey(spriteInfo.spriteRenderer) || !currentPhaseIndex.ContainsKey(spriteInfo.spriteRenderer))
                continue;

            collisionCounts[spriteInfo.spriteRenderer]++;
            int phaseIndex = currentPhaseIndex[spriteInfo.spriteRenderer];

            if (phaseIndex < spriteInfo.colorPhases.Count)
            {
                ColorPhase phase = spriteInfo.colorPhases[phaseIndex];
                if (collisionCounts[spriteInfo.spriteRenderer] >= phase.collisionThreshold)
                {
                    currentPhaseIndex[spriteInfo.spriteRenderer]++;

                    if (currentPhaseIndex[spriteInfo.spriteRenderer] < spriteInfo.colorPhases.Count)
                    {
                        ColorPhase newPhase = spriteInfo.colorPhases[currentPhaseIndex[spriteInfo.spriteRenderer]];
                        spriteInfo.spriteRenderer.color = newPhase.targetColor;
                        if (spriteInfo.spriteRenderer.material != null)
                        {
                            spriteInfo.spriteRenderer.material.color = newPhase.targetColor;
                        }
                        Vector3 newScale = spriteInfo.spriteRenderer.transform.localScale;
                        newScale.y = newPhase.newScaleY;
                        spriteInfo.spriteRenderer.transform.localScale = newScale;
                        spriteInfo.spriteRenderer.transform.position = originalPositions[spriteInfo.spriteRenderer];
                    }
                    Debug.Log("Sprite atualizado para a fase: " + currentPhaseIndex[spriteInfo.spriteRenderer]);

                    collisionCounts[spriteInfo.spriteRenderer] = 0;

                    if (currentPhaseIndex[spriteInfo.spriteRenderer] > 0 && spriteInfo.childMeshRenderer != null)
                    {
                        spriteInfo.childMeshRenderer.enabled = true;
                    }
                }
            }
        }
    }
}
