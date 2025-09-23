using UnityEngine;

public class SpawningMeat : MonoBehaviour
{
    public GameObject meat;
    public float launchForce = 5f;
    public Transform spawnPoint;

    public bool SpawnMeat()
    {
        if (meat == null)
        {
            return false;
        }

        // Instanciar en la posición y rotación del objeto actual
        GameObject spawnedMeat = Instantiate(meat, spawnPoint.position, Quaternion.Euler(-90, 0, 0));

        // Verificar si tiene Rigidbody
        Rigidbody rb = spawnedMeat.GetComponent<Rigidbody>();
        if (rb != null)
        {
            // Lanzar siempre hacia la derecha (eje +X)
            Vector3 forceDirection = Vector3.right;

            // Agregamos un poco de elevación para que se vea más natural
            forceDirection += Vector3.up * 0.5f;

            rb.AddForce(forceDirection.normalized * launchForce, ForceMode.Impulse);
        }
        
        return true;
    }

}
