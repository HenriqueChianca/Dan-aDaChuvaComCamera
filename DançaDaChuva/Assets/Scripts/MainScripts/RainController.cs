using UnityEngine;
using System.Collections;

public class RainController : MonoBehaviour
{
    [SerializeField] private ParticleSystem rain; // Certifica-se de que podemos atribuir no Inspector
    [SerializeField] private float startDelay = 15f; // Tempo de espera antes de começar a chover

    private void Start()
    {
        if (rain == null)
        {
            Debug.LogError("Particle System da chuva não foi atribuído! Atribua no Inspector.", this);
            return;
        }

        StartCoroutine(StartRainAfterDelay());
    }

    private IEnumerator StartRainAfterDelay()
    {
        rain.Stop(); // Garante que a chuva começa desativada
        yield return new WaitForSeconds(startDelay);
        rain.Play(); // Ativa a chuva após o delay
    }
}
