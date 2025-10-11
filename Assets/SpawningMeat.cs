using System.Collections;
using UnityEngine;
using DG.Tweening;

public class SpawningMeat : MonoBehaviour
{
    public GameObject meat;
    public float launchForce = 5f;
    public Transform spawnPoint;
    
    public float preparationTime = 0.6f;
    private bool isWorking;
    [SerializeField] private GameObject meatMachine;

    public bool SpawnMeat()
    {
        if (isWorking) return false;
        if (meat == null)
        {
            Debug.LogWarning("No se asignó prefab de carne");
            return false;
        }

        StartCoroutine(SpawnMeatRoutine());
        return true;
    }
    
    private IEnumerator SpawnMeatRoutine()
    {
        isWorking = true;

        // 1. Preparación con animación DOTween (temblor)
        meatMachine.transform.DOKill(); // Cancela tweens activos si los hay
        meatMachine.transform.localScale = Vector3.one; // Reset por si se quedó escalado

        // Shake durante "preparationTime"
        meatMachine.transform.DOShakeScale(preparationTime, strength: 0.3f, vibrato: 10, randomness: 90f, fadeOut: true);

        // 2. Esperar a que termine la animación
        yield return new WaitForSeconds(preparationTime);

        // 3. Instanciar la carne y lanzarla
        GameObject spawnedMeat = Instantiate(meat, spawnPoint.position, Quaternion.Euler(-90f, 0f, 0f));

        Rigidbody rb = spawnedMeat.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 forceDirection = Vector3.right + Vector3.up * 0.5f;
            rb.AddForce(forceDirection.normalized * launchForce, ForceMode.Impulse);
        }

        isWorking = false;
    }

}
