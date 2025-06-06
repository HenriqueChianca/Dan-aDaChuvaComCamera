using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class GameLoopController : MonoBehaviour
{
    [Header("Refer�ncias")]
    public GameObject[] gameplayObjects; // Coloque aqui todos os objetos de gameplay
    public RawImage rawImageDisplay; // RawImage da c�mera
    public TMP_Text messageText; // TextMeshPro para mostrar a mensagem

    [Header("Configura��es")]
    public Vector3 rawImageHiddenPosition = new Vector3(9999, 9999, 0); // Posi��o fora da tela
    private Vector3 rawImageOriginalPosition; // Guarda a posi��o original da RawImage

    private void Start()
    {
        if (rawImageDisplay != null)
        {
            // Salva a posi��o original da RawImage
            rawImageOriginalPosition = rawImageDisplay.rectTransform.anchoredPosition;
        }
        else
        {
            Debug.LogError("RawImage n�o atribu�da!");
        }

        if (messageText != null)
        {
            messageText.gameObject.SetActive(false); // Esconde o texto inicialmente
        }
        else
        {
            Debug.LogError("TMP_Text n�o atribu�do!");
        }
    }

    // M�todo p�blico para ser chamado quando vencer ou perder
    public void TriggerEndGame(bool victory)
    {
        StartCoroutine(HandleEndGame(victory));
    }

    private IEnumerator HandleEndGame(bool victory)
    {
        // Esconde objetos de gameplay
        foreach (var obj in gameplayObjects)
        {
            obj.SetActive(false);
        }

        // Move a RawImage para fora da tela
        rawImageDisplay.rectTransform.anchoredPosition = rawImageHiddenPosition;

        // Mostra mensagem
        messageText.gameObject.SetActive(true);
        int countdown = 10;

        while (countdown > 0)
        {
            if (victory)
            {
                messageText.text = $"You Win! Reiniciando em {countdown} segundos...";
            }
            else
            {
                messageText.text = $"Game Over! Reiniciando em {countdown} segundos...";
            }

            yield return new WaitForSeconds(1f);
            countdown--;
        }

        // Esconde mensagem
        messageText.gameObject.SetActive(false);

        // Reativa objetos de gameplay
        foreach (var obj in gameplayObjects)
        {
            obj.SetActive(true);
        }

        // Move a RawImage de volta para a posi��o original
        rawImageDisplay.rectTransform.anchoredPosition = rawImageOriginalPosition;

        Debug.Log("Gameplay reiniciada!");
    }
}
