using UnityEngine;
using System.Collections;

public class RainController : MonoBehaviour
{
    [SerializeField] private ParticleSystem rain; // Certifica-se de que podemos atribuir no Inspector
    [SerializeField] private float startDelay = 15f; // Tempo de espera antes de come�ar a chover

    private void Start()
    {
        if (rain == null)
        {
            Debug.LogError("Particle System da chuva n�o foi atribu�do! Atribua no Inspector.", this);
            return;
        }

        StartCoroutine(StartRainAfterDelay());
    }

    private IEnumerator StartRainAfterDelay()
    {
        rain.Stop(); // Garante que a chuva come�a desativada
        yield return new WaitForSeconds(startDelay);
        rain.Play(); // Ativa a chuva ap�s o delay
    }
}
