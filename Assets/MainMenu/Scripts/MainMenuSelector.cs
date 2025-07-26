using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MainMenuSelector : MonoBehaviour
{
    public Button defaultButton;

    private void Start()
    {
        // Forzar la selección del primer botón
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(defaultButton.gameObject);
    }
}
