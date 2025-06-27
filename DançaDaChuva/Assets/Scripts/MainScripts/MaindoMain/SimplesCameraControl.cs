using UnityEngine;

public class SimplesCameraControl : MonoBehaviour
{
    private Vector3 initialPosition;
    private Quaternion initialRotation;

    void Start()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;
    }

    public void RestartCamera()
    {
        transform.position = initialPosition;
        transform.rotation = initialRotation;
        Debug.Log("Camera resetada para a posição inicial.");
    }
}