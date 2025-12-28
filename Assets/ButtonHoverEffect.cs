using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class ButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Vector3 originalPosition; // Posición inicial del botón
    public float moveDistance = 50f; // Distancia a mover hacia la derecha (ajusta según necesites)
    public float animationDuration = 0.3f; // Duración de la animación en segundos

    void Start()
    {
        // Guarda la posición original al iniciar
        originalPosition = transform.localPosition;
    }

    // Se llama cuando el mouse entra al botón
    public void OnPointerEnter(PointerEventData eventData)
    {
        // Mueve suavemente hacia la derecha
        transform.DOLocalMoveX(originalPosition.x + moveDistance, animationDuration)
            .SetEase(Ease.OutQuad); // Suaviza la animación
    }

    // Se llama cuando el mouse sale del botón
    public void OnPointerExit(PointerEventData eventData)
    {
        // Regresa suavemente a la posición original
        transform.DOLocalMoveX(originalPosition.x, animationDuration)
            .SetEase(Ease.OutQuad);
    }
}