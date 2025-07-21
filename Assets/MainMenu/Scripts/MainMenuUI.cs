using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    public void JugarConUnJugador()
    {
        GameSettings.Instance.cantidadJugadores = 1;
        print("un player");
        SceneManager.LoadScene("SampleScene"); // reemplaza con tu escena
    }

    public void JugarConDosJugadores()
    {
        GameSettings.Instance.cantidadJugadores = 2;
        print("2 players");
        SceneManager.LoadScene("SampleScene");
    }
}
