using UnityEngine;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class MainMenuTransition : MonoBehaviour
{
    [Header("Menus")]
    public RectTransform mainMenuGroup;           // Main menu
    public RectTransform newMenuGroup;            // Play Menu

    [Header("Animación")]
    public float moveOutDistance = 1000f;
    public float moveDuration = 0.5f;
    public float delayBeforeNewMenu = 0.1f;

    public List<RectTransform> mainButtons;       // Botones del menú actual
    public List<RectTransform> newButtons;        // Botones a mostrar luego

    private Vector3[] newButtonOriginalPos;
    
    void Start()
    {
        // Desactivar el nuevo menú al inicio
        if (newMenuGroup != null)
            newMenuGroup.gameObject.SetActive(false);

        // Guardar posiciones originales para entrada animada
        newButtonOriginalPos = new Vector3[newButtons.Count];
        for (int i = 0; i < newButtons.Count; i++)
        {
            newButtonOriginalPos[i] = newButtons[i].anchoredPosition3D;
        }
    }
    
    public void OnStartButtonPressed()
    {
        EventSystem.current.SetSelectedGameObject(null);
        StartCoroutine(AnimateMenuTransition());
        EventSystem.current.SetSelectedGameObject(newButtons[0].gameObject);
    }

    private IEnumerator AnimateMenuTransition()
    {
        // 1. Mover botones actuales fuera de pantalla (hacia izquierda)
        mainMenuGroup.DOKill();
        mainMenuGroup.DOAnchorPosX(mainMenuGroup.anchoredPosition.x - moveOutDistance, moveDuration).SetEase(Ease.InBack);

        yield return new WaitForSeconds(moveDuration + delayBeforeNewMenu);
        
        
        // 2. Activar el nuevo menú
        newMenuGroup.gameObject.SetActive(true);

        // 3. Mover botones nuevos desde fuera de pantalla hacia adentro
        for (int i = 0; i < newButtons.Count; i++)
        {
            var btn = newButtons[i];
            btn.DOKill();
            btn.anchoredPosition = new Vector2(newButtonOriginalPos[i].x + moveOutDistance, newButtonOriginalPos[i].y); // Comienza fuera
            btn.DOAnchorPos(newButtonOriginalPos[i], moveDuration).SetEase(Ease.OutBack).SetDelay(i * 0.05f); // entrada con escalonamiento
        }
        
    }
    
    
    
}
