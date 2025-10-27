using System.Collections;
using UnityEngine;
using DG.Tweening;

public class SpawningMeat : MonoBehaviour
{
    [Header("Configuración")]
    public GameObject meat;
    public Transform spawnPoint;
    public float launchForce = 5f;
    public float preparationTime = 0.6f;

    [Header("Referencias")]
    [SerializeField] private GameObject meatMachine;

    private bool isWorking;

    public void SpawnMeat()
    {
        if (isWorking) return;

        if (meat == null)
        {
            Debug.LogWarning("⚠️ No se asignó prefab de carne en SpawningMeat");
            return;
        }

        StartCoroutine(SpawnMeatRoutine());
    }

    private IEnumerator SpawnMeatRoutine()
    {
        isWorking = true;

        meatMachine.transform.DOKill();
        meatMachine.transform.localScale = Vector3.one;

        meatMachine.transform.DOShakeScale(
            preparationTime,
            strength: 0.3f,
            vibrato: 10,
            randomness: 90f,
            fadeOut: true
        );

        yield return new WaitForSeconds(preparationTime);

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