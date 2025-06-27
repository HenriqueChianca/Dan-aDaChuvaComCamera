using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }

    [Header("Canvas de Fim de Jogo")]
    public GameObject winCanvas;
    public GameObject loseCanvas;

    [Header("Tempo para reiniciar")]
    public float resetDelay = 3f;

    private bool gameEnded = false;

    [Header("Referência do SpriteColorController")]
    public SpriteColorController colorController;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (winCanvas != null) winCanvas.SetActive(false);
        if (loseCanvas != null) loseCanvas.SetActive(false);
    }

    public void OnWinCondition()
    {
        if (gameEnded) return;

        gameEnded = true;
        if (winCanvas != null) winCanvas.SetActive(true);
        Debug.Log("Vitória! Loop vai reiniciar em " + resetDelay + " segundos.");

        if (colorController != null)
        {
            colorController.ResetInvisible(resetDelay);
        }
    }

    public void OnLoseCondition()
    {
        if (gameEnded) return;

        gameEnded = true;
        if (loseCanvas != null) loseCanvas.SetActive(true);
        Debug.Log("Derrota! Loop vai reiniciar em " + resetDelay + " segundos.");

        if (colorController != null)
        {
            colorController.ResetInvisible(resetDelay);
        }
    }

    public void RestartGameState()
    {
        gameEnded = false;
        if (winCanvas != null) winCanvas.SetActive(false);
        if (loseCanvas != null) loseCanvas.SetActive(false);
    }

    // Método adicional necessário para o SpriteColorController
    public void ResetGameUI()
    {
        RestartGameState();
    }
}
