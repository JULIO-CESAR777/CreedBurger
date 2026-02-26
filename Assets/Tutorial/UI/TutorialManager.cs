using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialManager : MonoBehaviour
{
    public enum StepType { GoToPoint, GiveRecipeToClient, WaitSeconds }

    [Serializable]
    public class Step
    {
        public StepType type;
        [TextArea] public string text;
        public Sprite icon;

        public Transform point;
        public MoveClientTutorial client;
        public int recipeId;   // usa -1 para aceptar cualquier receta
        public float seconds;

        [Header("Markers / Objetos a prender")]
        public GameObject[] enableOnEnter;

        [Header("Markers / Objetos a apagar (opcional)")]
        public GameObject[] disableOnEnter;

        [Header("Si true, apaga automáticamente los enableOnEnter del step anterior")]
        public bool autoDisablePreviousMarkers = true;
    }

    [SerializeField] private TutorialUi ui;
    [SerializeField] private Step[] steps;

    [Header("Player auto-detect")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private float findPlayerEvery = 0.25f;

    [Header("Fin del tutorial")]
    [SerializeField] private GameObject tutorialFinishedPanel;

    private PlayerTutorial player;
    private int i;
    private bool waiting;
    private Coroutine findPlayerCR;

    private bool tutorialFinished;

    void OnEnable()
    {
        MoveClientTutorial.OnClientReceivedRecipe += OnClientReceivedRecipe;
        findPlayerCR = StartCoroutine(FindAndBindPlayerLoop());
    }

    void OnDisable()
    {
        MoveClientTutorial.OnClientReceivedRecipe -= OnClientReceivedRecipe;
        UnbindPlayer();

        if (findPlayerCR != null)
        {
            StopCoroutine(findPlayerCR);
            findPlayerCR = null;
        }

        DisableStepMarkers(i);
    }

    void Start()
    {
        if (tutorialFinishedPanel != null)
            tutorialFinishedPanel.SetActive(false);

        i = 0;
        RunCurrentStep();
    }

    IEnumerator FindAndBindPlayerLoop()
    {
        while (true)
        {
            if (player == null)
            {
                var go = GameObject.FindGameObjectWithTag(playerTag);
                if (go != null)
                {
                    var newPlayer = go.GetComponent<PlayerTutorial>();
                    if (newPlayer != null)
                        BindPlayer(newPlayer);
                }
            }
            yield return new WaitForSeconds(findPlayerEvery);
        }
    }

    void BindPlayer(PlayerTutorial newPlayer)
    {
        UnbindPlayer();

        player = newPlayer;
        player.OnReachedTutorialPoint += OnPlayerReachedPoint;

        ApplyStepTargetToPlayer();
    }

    void UnbindPlayer()
    {
        if (player != null)
            player.OnReachedTutorialPoint -= OnPlayerReachedPoint;

        player = null;
    }

    void RunCurrentStep()
    {
        if (tutorialFinished) return;

        if (i >= steps.Length)
        {
            tutorialFinished = true;

            ui.Show("¡Tutorial terminado!", null);
            DisableStepMarkers(i - 1);

            if (tutorialFinishedPanel != null)
                tutorialFinishedPanel.SetActive(true);

            waiting = false;

            // Opcional: limpiar target del player si existe
            if (player != null) player.SetTutorialTarget(null);

            return;
        }

        var s = steps[i];

        if (s.autoDisablePreviousMarkers)
            DisableStepMarkers(i - 1);

        SetActiveSafe(s.disableOnEnter, false);
        SetActiveSafe(s.enableOnEnter, true);

        ui.Show(s.text, s.icon);
        waiting = true;

        ApplyStepTargetToPlayer();

        if (s.type == StepType.WaitSeconds)
            StartCoroutine(WaitThenNext(s.seconds));
    }

    void ApplyStepTargetToPlayer()
    {
        if (player == null || tutorialFinished) return;

        var s = steps[i];

        switch (s.type)
        {
            case StepType.GoToPoint:
                player.SetTutorialTarget(s.point);
                break;

            case StepType.GiveRecipeToClient:
            case StepType.WaitSeconds:
                player.SetTutorialTarget(null);
                break;
        }
    }

    IEnumerator WaitThenNext(float sec)
    {
        yield return new WaitForSeconds(sec);
        Next();
    }

    void Next()
    {
        if (tutorialFinished) return;

        waiting = false;
        DisableStepMarkers(i);

        i++;
        RunCurrentStep();
    }

    void OnPlayerReachedPoint(Transform point)
    {
        if (!waiting || tutorialFinished) return;

        var s = steps[i];
        if (s.type != StepType.GoToPoint) return;
        if (s.point != point) return;

        Next();
    }

    void OnClientReceivedRecipe(MoveClientTutorial client, int recipeId)
    {
        if (!waiting || tutorialFinished) return;

        var s = steps[i];
        if (s.type != StepType.GiveRecipeToClient) return;

        // ✅ Si asignas client en inspector lo valida, si lo dejas null acepta cualquiera:
        if (s.client != null && s.client != client) return;

        // ✅ Si recipeId >= 0 lo valida, si pones -1 acepta cualquiera:
        if (s.recipeId >= 0 && s.recipeId != recipeId) return;

        Next();
    }

    // =========================
    // Helpers
    // =========================

    void DisableStepMarkers(int stepIndex)
    {
        if (stepIndex < 0 || stepIndex >= steps.Length) return;
        SetActiveSafe(steps[stepIndex].enableOnEnter, false);
    }

    void SetActiveSafe(GameObject[] list, bool value)
    {
        if (list == null) return;

        for (int k = 0; k < list.Length; k++)
        {
            var go = list[k];
            if (go == null) continue;
            go.SetActive(value);
        }
    }

    public void cambiarEscena()
    {
        SceneManager.LoadScene("MainMenu");
        
    }
}