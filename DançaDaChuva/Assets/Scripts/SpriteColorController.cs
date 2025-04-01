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
        public List<SpriteRenderer> childSpriteRenderers = new List<SpriteRenderer>();
    }

    public List<SpriteColorInfo> spriteColorList = new List<SpriteColorInfo>();

    [SerializeField] private ParticleSystem particleSystem;
    private Dictionary<SpriteRenderer, int> collisionCounts = new Dictionary<SpriteRenderer, int>();
    private Dictionary<SpriteRenderer, int> currentPhaseIndex = new Dictionary<SpriteRenderer, int>();

    private bool hasCollided = false;
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
            foreach (var childRenderer in spriteInfo.childSpriteRenderers)
            {
                childRenderer.enabled = false;
            }
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

                Debug.Log($"Colisão detectada! Contagem atual: {collisionCounts[spriteInfo.spriteRenderer]}");

                if (currentIndex < spriteInfo.colorPhases.Count &&
                    collisionCounts[spriteInfo.spriteRenderer] >= spriteInfo.colorPhases[currentIndex].collisionThreshold)
                {
                    spriteInfo.spriteRenderer.color = spriteInfo.colorPhases[currentIndex].targetColor;
                    Debug.Log($"Mudando cor do sprite para: {spriteInfo.colorPhases[currentIndex].targetColor}");

                    Transform spriteTransform = spriteInfo.spriteRenderer.transform;
                    Vector3 fixedPosition = originalPositions[spriteInfo.spriteRenderer];
                    Vector3 currentScale = spriteTransform.localScale;
                    spriteTransform.localScale = new Vector3(currentScale.x, spriteInfo.colorPhases[currentIndex].newScaleY, currentScale.z);
                    spriteTransform.position = fixedPosition;

                    currentPhaseIndex[spriteInfo.spriteRenderer]++;
                    collisionCounts[spriteInfo.spriteRenderer] = 0;

                    Debug.Log($"Novo índice da fase atual: {currentPhaseIndex[spriteInfo.spriteRenderer]}");

                    for (int i = 0; i < spriteInfo.childSpriteRenderers.Count; i++)
                    {
                        if (currentPhaseIndex[spriteInfo.spriteRenderer] - 1 < spriteInfo.colorPhases.Count)
                        {
                            spriteInfo.childSpriteRenderers[i].enabled = true;
                            spriteInfo.childSpriteRenderers[i].color = spriteInfo.colorPhases[currentPhaseIndex[spriteInfo.spriteRenderer] - 1].targetColor;
                            Debug.Log($"Mudando cor do filho {i} para: {spriteInfo.childSpriteRenderers[i].color}");
                        }
                    }
                }
            }
        }
    }
}
