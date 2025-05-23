using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class SceneRestartTimer : MonoBehaviour
{
    [Tooltip("Tempo inicial (em segundos) para reiniciar a cena 'Game'")]
    public float restartTimer = 10f;

    [Tooltip("Componente TextMeshProUGUI que exibirá a contagem regressiva na tela")]
    public TextMeshProUGUI timerText;

    void Start()
    {
        UpdateTimerText();
    }

    void Update()
    {
        restartTimer -= Time.deltaTime;
        if (restartTimer < 0f)
            restartTimer = 0f;

        UpdateTimerText();

        if (restartTimer <= 0f)
        {
            // NÃO para mais a câmera. Apenas reinicia a cena.
            SceneManager.LoadScene("Game");
        }
    }

    void UpdateTimerText()
    {
        if (timerText != null)
        {
            timerText.text = "Reiniciando em " + Mathf.CeilToInt(restartTimer) + " segundos";
        }
    }
}
