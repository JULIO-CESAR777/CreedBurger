using UnityEngine;

public class LookCamera : MonoBehaviour
{
    public float rotationSpeed = 2f;   // velocidad de rotación
    private Transform cam;

    void Start()
    {
        cam = Camera.main.transform; // referencia a la cámara principal
    }

    void Update()
    {
        if (cam == null) return;

        // Dirección hacia la cámara
        Vector3 direction = cam.position - transform.position;
        direction.y = 0; // opcional: mantener el objeto siempre recto en el eje Y

        // Rotación objetivo
        Quaternion targetRotation = Quaternion.LookRotation(direction);
    
        // corregir offset de 90 grados en X (o el valor que necesites)
        targetRotation *= Quaternion.Euler(-145, 0, 0);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );

    }
}
