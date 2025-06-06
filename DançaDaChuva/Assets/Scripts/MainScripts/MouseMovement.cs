using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseMovement : MonoBehaviour
{

    [System.Serializable]
    public class ParticleSystemData
    {
        [Tooltip("Particle System a ser controlado.")]
        public ParticleSystem ps;

        [Tooltip("Delay individual de ativa��o (em segundos) para este Particle System.")]
        public float activationDelay;

        [HideInInspector]
        public bool isActive = false;

        [HideInInspector]
        public Coroutine activationCoroutine = null;
    }

    [Tooltip("Lista de Particle Systems (com delay individual) a serem controlados pelo movimento do mouse.")]
    public List<ParticleSystemData> particleSystemsData;

    [Tooltip("Tempo em segundos para desativar as part�culas ap�s inatividade.")]
    public float inactivityThreshold = 3f;

    [Tooltip("N�mero de movimentos necess�rios para iniciar a ativa��o.")]
    public int movementsNeeded = 10;

    private Vector2 lastMousePosition;
    private int movementCount = 0;
    private float inactivityTimer = 0f;

    void Start()
    {
        // Para cada Particle System, garanta que a emiss�o comece desativada.
        foreach (ParticleSystemData data in particleSystemsData)
        {
            if (data.ps != null)
            {
                data.ps.Stop();
                var emission = data.ps.emission;
                emission.enabled = false;
            }
        }
        lastMousePosition = Input.mousePosition;
    }

    void Update()
    {
        DetectMouseMovement();

        // Se algum sistema estiver ativo, atualiza o timer de inatividade
        bool anyActive = false;
        foreach (ParticleSystemData data in particleSystemsData)
        {
            if (data.isActive)
            {
                anyActive = true;
                break;
            }
        }

        if (anyActive)
        {
            inactivityTimer += Time.deltaTime;
            if (inactivityTimer >= inactivityThreshold)
            {
                DeactivateAll();
            }
        }
    }

    void DetectMouseMovement()
    {
        Vector2 currentMousePosition = Input.mousePosition;
        if (currentMousePosition != lastMousePosition)
        {
            movementCount++;
            lastMousePosition = currentMousePosition;
            if (movementCount >= movementsNeeded)
            {
                foreach (ParticleSystemData data in particleSystemsData)
                {
                    if (data.ps != null && !data.isActive && data.activationCoroutine == null)
                    {
                        data.activationCoroutine = StartCoroutine(ActivateAfterDelay(data));
                    }
                }
            }
        }
    }

    IEnumerator ActivateAfterDelay(ParticleSystemData data)
    {
        yield return new WaitForSeconds(data.activationDelay);
        if (!data.isActive)
        {
            data.isActive = true;
            if (data.ps != null)
            {
                var emission = data.ps.emission;
                emission.enabled = true;
                data.ps.Play();
            }
        }
        data.activationCoroutine = null;
    }

    void DeactivateAll()
    {
        movementCount = 0;
        foreach (ParticleSystemData data in particleSystemsData)
        {
            if (data.ps != null)
            {
                var emission = data.ps.emission;
                emission.enabled = false;
                data.ps.Stop();
                data.isActive = false;
                if (data.activationCoroutine != null)
                {
                    StopCoroutine(data.activationCoroutine);
                    data.activationCoroutine = null;
                }
            }
        }
        inactivityTimer = 0f;
    }
}
