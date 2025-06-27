using System.Collections.Generic;
using UnityEngine;

public class RainController : MonoBehaviour
{
    [System.Serializable]
    public class ParticleSystemData
    {
        public ParticleSystem ps;
        [HideInInspector] public bool isActive = false;
        [HideInInspector] public Coroutine activationCoroutine = null;
    }

    [Header("Sistemas de Partículas")]
    public List<ParticleSystemData> particleSystemsData;

    public void ActivateAll()
    {
        foreach (var data in particleSystemsData)
        {
            if (data.ps != null && !data.isActive)
            {
                var emission = data.ps.emission;
                emission.enabled = true;
                data.ps.Play();
                data.isActive = true;
            }
        }
    }

    public void DeactivateAll()
    {
        foreach (var data in particleSystemsData)
        {
            if (data.ps != null && data.isActive)
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
    }
}
