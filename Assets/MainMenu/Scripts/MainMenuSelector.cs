using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic; // Necesario para usar Listas

public class MenuManager : MonoBehaviour
{
    // 1. Definimos una "cajita" que agrupa botón y menú
    [System.Serializable] 
    public class MenuVinculo
    {
        public string nombre; // Solo para que te organices en el editor (ej: "Menu Inventario")
        public Button boton;
        public GameObject submenu;
    }

    [Header("Configuración de Vinculaciones")]
    // En lugar de dos arrays, usamos una sola lista de vínculos
    public List<MenuVinculo> menus; 

    [Header("Configuración de animación")]
    public float animationDuration = 0.5f;
    public Ease animationEase = Ease.OutQuad;

    private MenuVinculo menuActivo = null; // Guardamos la REFERENCIA directa, no el índice
    private bool isAnimating = false;

    void Start()
    {
        // Recorremos cada vínculo configurado
        foreach (var vinculo in menus)
        {
            // Seguridad: si falta algo, lo saltamos
            if (vinculo.boton == null || vinculo.submenu == null) continue;

            // Inicializar: ocultar menú
            vinculo.submenu.SetActive(true);
            vinculo.submenu.transform.localScale = Vector3.zero;

            // Limpiar eventos previos
            vinculo.boton.onClick.RemoveAllListeners();

            // ASIGNACIÓN ESTÁTICA:
            // Copiamos el vínculo actual a una variable local para el lambda
            MenuVinculo esteVinculo = vinculo; 
            
            // Al hacer click, pasamos EL OBJETO exacto, no un número
            vinculo.boton.onClick.AddListener(() => OnMenuButtonClicked(esteVinculo));
        }
    }

    public void OnMenuButtonClicked(MenuVinculo vinculoSeleccionado)
    {
        // 1. Si está animando, ignorar.
        // 2. Si pulsamos el botón del menú que YA está abierto, ignorar.
        if (isAnimating || menuActivo == vinculoSeleccionado) return;

        Debug.Log($"Abriendo: {vinculoSeleccionado.nombre}");

        isAnimating = true;

        // Ocultar el anterior (si hay uno activo)
        if (menuActivo != null)
        {
            OcultarSubmenu(menuActivo.submenu);
        }

        // Mostrar el nuevo
        MostrarSubmenu(vinculoSeleccionado.submenu);
        
        // Actualizar la referencia
        menuActivo = vinculoSeleccionado;
    }

    private void MostrarSubmenu(GameObject obj)
    {
        obj.transform.DOScale(Vector3.one, animationDuration)
            .SetEase(animationEase)
            .OnComplete(() => isAnimating = false); // Solo desbloqueamos al terminar de ABRIR
    }

    private void OcultarSubmenu(GameObject obj)
    {
        obj.transform.DOScale(Vector3.zero, animationDuration)
            .SetEase(animationEase);
    }
}