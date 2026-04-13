using System.Collections;
using UnityEngine;

public class StepCameraShake : MonoBehaviour
{
    [Header("Shake")]
    public float duration = 0.08f;
    public float magnitude = 0.05f;

    private SmoothCameraFollow cameraFollow;
    private Coroutine shakeRoutine;

    void Awake()
    {
        cameraFollow = GetComponent<SmoothCameraFollow>();

        if (cameraFollow == null)
            cameraFollow = FindFirstObjectByType<SmoothCameraFollow>();
    }

    public void Shake()
    {
        if (cameraFollow == null) return;

        if (shakeRoutine != null)
            StopCoroutine(shakeRoutine);

        shakeRoutine = StartCoroutine(DoShake());
    }

    private IEnumerator DoShake()
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            Vector2 random2D = Random.insideUnitCircle * magnitude;
            cameraFollow.shakeOffset = new Vector3(random2D.x, random2D.y, 0f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        cameraFollow.shakeOffset = Vector3.zero;
        shakeRoutine = null;
    }
}