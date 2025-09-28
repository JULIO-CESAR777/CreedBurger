using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    public void JugarConUnJugador()
    {
        GameSettings.Instance.cantidadJugadores = 1;
        SceneManager.LoadScene("Scene_MapReal");
    }

    public void JugarConDosJugadores()
    {
        GameSettings.Instance.cantidadJugadores = 2;
        SceneManager.LoadScene("Scene_MapReal");
    }
}
