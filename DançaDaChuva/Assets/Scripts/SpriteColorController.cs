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
        public Color childTargetColor = Color.white;
    }

    [System.Serializable]
    public class SpriteColorInfo
    {
        public SpriteRenderer spriteRenderer;
        public List<ColorPhase> colorPhases = new List<ColorPhase>();
        public SpriteRenderer childSpriteRenderer;
    }

    public List<SpriteColorInfo> spriteColorList = new List<SpriteColorInfo>();

    [SerializeField] private ParticleSystem particleSystem;
    private Dictionary<SpriteRenderer, int> collisionCounts = new Dictionary<SpriteRenderer, int>();
    private Dictionary<SpriteRenderer, int> currentPhaseIndex = new Dictionary<SpriteRenderer, int>();
    private Dictionary<SpriteRenderer, Vector3> originalPositions = new Dictionary<SpriteRenderer, Vector3>();

    private bool hasCollided = false;

    void Start()
    {
        foreach (var spriteInfo in spriteColorList)
        {
            if (spriteInfo == null || spriteInfo.spriteRenderer == null)
                continue;
            collisionCounts[spriteInfo.spriteRenderer] = 0;
            currentPhaseIndex[spriteInfo.spriteRenderer] = 0;
            originalPositions[spriteInfo.spriteRenderer] = spriteInfo.spriteRenderer.transform.position;
            spriteInfo.spriteRenderer.color = spriteInfo.colorPhases[0].targetColor;
            if (spriteInfo.childSpriteRenderer != null)
            {
                spriteInfo.childSpriteRenderer.color = spriteInfo.colorPhases[0].childTargetColor;
                spriteInfo.childSpriteRenderer.enabled = false;
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
            collisionCounts[spriteInfo.spriteRenderer]++;
            int currentIndex = currentPhaseIndex[spriteInfo.spriteRenderer];
            if (currentIndex < spriteInfo.colorPhases.Count &&
                collisionCounts[spriteInfo.spriteRenderer] >= spriteInfo.colorPhases[currentIndex].collisionThreshold)
            {
                spriteInfo.spriteRenderer.color = spriteInfo.colorPhases[currentIndex].targetColor;
                Transform spriteTransform = spriteInfo.spriteRenderer.transform;
                spriteTransform.localScale = new Vector3(spriteTransform.localScale.x, spriteInfo.colorPhases[currentIndex].newScaleY, spriteTransform.localScale.z);
                spriteTransform.position = originalPositions[spriteInfo.spriteRenderer];
                currentPhaseIndex[spriteInfo.spriteRenderer]++;
                collisionCounts[spriteInfo.spriteRenderer] = 0;
                if (spriteInfo.childSpriteRenderer != null)
                {
                    spriteInfo.childSpriteRenderer.enabled = true;
                    spriteInfo.childSpriteRenderer.color = spriteInfo.colorPhases[currentIndex].childTargetColor;
                }
            }
        }
    }
}
