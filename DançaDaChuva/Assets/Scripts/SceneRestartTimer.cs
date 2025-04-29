using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class SceneRestartTimer : MonoBehaviour
{
    [Tooltip("Tempo inicial (em segundos) para reiniciar a cena 'Game'")]
    public float restartTimer = 10f; // Valor inicial que pode ser ajustado no Inspector

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
            // Antes de recarregar, para a câmera
            StopCameraIfExists();

            // Agora recarrega
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

    void StopCameraIfExists()
    {
        var cameraSelector = FindObjectOfType<CameraMovement>();
        if (cameraSelector != null)
        {
            cameraSelector.StopCamera();
        }
    }
}
