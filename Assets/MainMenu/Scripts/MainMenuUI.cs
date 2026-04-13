using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private GameObject[] bloodDecals;

    [Header("Animación de decals")]
    [SerializeField] private float decalDelay = 0.15f;

    [Header("Escena a cargar")]
    [SerializeField] private string gameSceneName = "Scene_MapReal";

    private bool isLoading = false;

    private void Start()
    {
        // Asegura que al inicio todos estén apagados
        foreach (GameObject decal in bloodDecals)
        {
            if (decal != null)
                decal.SetActive(false);
        }
    }

    public void JugarTutorial()
    {
        if (isLoading) return;

        GameSettings.Instance.cantidadJugadores = 1;
        SceneManager.LoadScene("TutorialScene");
    }

    public void JugarConUnJugador()
    {
        if (isLoading) return;
        StartCoroutine(InitializeMatch(1));
    }

    public void JugarConDosJugadores()
    {
        if (isLoading) return;
        StartCoroutine(InitializeMatch(2));
    }

    public IEnumerator InitializeMatch(int numberOfPlayers)
    {
        isLoading = true;

        GameSettings.Instance.cantidadJugadores = numberOfPlayers;

        // Prender decals poco a poco
        foreach (GameObject decal in bloodDecals)
        {
            if (decal != null)
            {
                decal.SetActive(true);
                yield return new WaitForSeconds(decalDelay);
            }
        }

        // Carga asíncrona de escena
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(gameSceneName);
        asyncLoad.allowSceneActivation = false;

        // Esperar a que la escena esté casi lista
        while (asyncLoad.progress < 0.9f)
        {
            yield return null;
        }

        // Aquí podrías esperar un poco más si quieres que se vea mejor la transición
        yield return new WaitForSeconds(0.2f);

        // Activar cambio final
        asyncLoad.allowSceneActivation = true;
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}