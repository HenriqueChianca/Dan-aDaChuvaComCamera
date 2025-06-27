using System.Collections;
using UnityEngine;

[System.Serializable]
public class ParticleSystemData : MonoBehaviour

{
    [Tooltip("Particle System a ser controlado.")]
    public ParticleSystem ps;

    [Tooltip("Delay individual de ativa��o (em segundos) para este Particle System.")]
    public float activationDelay;

    [HideInInspector] public bool isActive = false;
    [HideInInspector] public Coroutine activationCoroutine = null;
}
