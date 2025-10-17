using UnityEngine;

public class LookCameraSpecial : MonoBehaviour
{
    public float rotationSpeed = 2f;


    public float x =0f;
    public float y =0f;
    public float z =0f;
    

    void Update()
    {
        // Calcula la rotación base hacia el cielo
        Quaternion targetRotation = Quaternion.LookRotation(new Vector3(0f, 230f, -70));

        // Aplica inclinación extra (rotar en X)
        targetRotation *= Quaternion.Euler(x, y,  z);

        // Interpola suavemente la rotación
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }
}