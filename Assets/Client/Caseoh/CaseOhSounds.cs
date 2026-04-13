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

        sonidosinicioCaseOh();

    }

    
    public void sonidossentarseCaseOh()
    {
        AudioManager.I.Play("vfx_sentarseCaseoh");

  
    }
    
    public void sonidosinicioCaseOh()
    {
        AudioManager.I.Play("vfx_inicioCaseoh");

  
    }
    
    public void sonidosmorirCaseOh()
    {
        AudioManager.I.Play("vfx_muerteCaseoh");

  
    }
    
    public void sonidosPasosCaseOh()
    {
        AudioManager.I.Play("vfx_pasosCaseoh");

        if (cameraShake != null)
            cameraShake.Shake();
    }
}