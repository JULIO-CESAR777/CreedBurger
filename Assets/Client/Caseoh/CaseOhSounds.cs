using System.Collections;
using UnityEngine;

public class CaseOhSounds : MonoBehaviour
{
    private StepCameraShake cameraShake;

    private IEnumerator Start()
    {
        while (cameraShake == null)
        {
            cameraShake = FindFirstObjectByType<StepCameraShake>();
            yield return null;
        }
    }

    public void sonidosPasosCaseOh()
    {
        AudioManager.I.Play("vfx_pasosCaseoh");

        if (cameraShake != null)
            cameraShake.Shake();
    }
}