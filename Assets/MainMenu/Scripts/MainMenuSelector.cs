using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;

public class MainMenuSelector : MonoBehaviour
{
    // Primer boton a seleccionar
    public Button defaultButton;
    public Button PlayDefaultButton;
    
    [Header("Offset de movimiento")]
    public float offsetXRight = 50f;
    public float offsetXLeft = -50f;
    public float animationTime = 0.3f;

    [Header("Botones a animar")]
    public List<RectTransform> buttons = new List<RectTransform>();
    
    private GameObject currentSelected;
    private Dictionary<RectTransform, Vector3> originalPositions = new();

    private void Start()
    {
        
        // Guardar las posiciones originales
        foreach (var btn in buttons)
        {
            originalPositions[btn] = btn.anchoredPosition3D;
        }
        
    }
    
    void AnimateButtons()
    {
        foreach (var btn in buttons)
        {
            btn.DOKill(); // Evita que se acumulen animaciones

            if (currentSelected.transform.IsChildOf(btn))
            {
                // Mover hacia la derecha
                btn.DOAnchorPosX(originalPositions[btn].x + offsetXRight, animationTime).SetEase(Ease.OutBack);
            }
            else
            {
                // Mover hacia la izquierda
                btn.DOAnchorPosX(originalPositions[btn].x + offsetXLeft, animationTime).SetEase(Ease.OutBack);
            }
        }
    }

    public void ExitGame()
    {
        Application.Quit();
    }
    
    
    
    
    
}
