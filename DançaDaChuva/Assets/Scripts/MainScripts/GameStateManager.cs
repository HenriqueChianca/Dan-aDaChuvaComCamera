using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }

    [Header("Canvas de Fim de Jogo")]
    public GameObject winCanvas;
    public GameObject loseCanvas;

    [Header("Tempo para reiniciar")]
    public float resetDelay = 3f;

    private bool gameEnded = false;

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
        Debug.Log("Vitória! O jogo vai reiniciar em " + resetDelay + " segundos.");
        Invoke("RestartGame", resetDelay);
    }

    public void OnLoseCondition()
    {
        if (gameEnded) return;

        gameEnded = true;
        if (loseCanvas != null) loseCanvas.SetActive(true);
        Debug.Log("Derrota! O jogo vai reiniciar em " + resetDelay + " segundos.");
        Invoke("RestartGame", resetDelay);
    }

    private void RestartGame()
    {
        Debug.Log("Reiniciando a cena atual...");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
