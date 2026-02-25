using System;
using System.Collections;
using UnityEngine;

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
        public int recipeId;
        public float seconds;

        [Header("Markers / Objetos a prender")]
        public GameObject[] enableOnEnter;

        [Header("Markers / Objetos a apagar (opcional)")]
        public GameObject[] disableOnEnter;  // si quieres apagar cosas específicas al entrar

        [Header("Si true, apaga automáticamente los enableOnEnter del step anterior")]
        public bool autoDisablePreviousMarkers = true;
    }

    [SerializeField] private TutorialUi ui;
    [SerializeField] private Step[] steps;

    [Header("Player auto-detect")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private float findPlayerEvery = 0.25f;

    private PlayerTutorial player;
    private int i;
    private bool waiting;
    private Coroutine findPlayerCR;

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

        // Apagar markers del step actual por si se desactiva el tutorial
        DisableStepMarkers(i);
    }

    void Start()
    {
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
        if (i >= steps.Length)
        {
            ui.Show("¡Tutorial terminado!", null);

            // Apaga los markers del último step
            DisableStepMarkers(i - 1);
            return;
        }

        var s = steps[i];

        // Apagar markers del step anterior (si aplica)
        if (s.autoDisablePreviousMarkers)
            DisableStepMarkers(i - 1);

        // Apagar lo que quieras al entrar (opcional)
        SetActiveSafe(s.disableOnEnter, false);

        // Prender markers del step actual
        SetActiveSafe(s.enableOnEnter, true);

        // UI / estado
        ui.Show(s.text, s.icon);
        waiting = true;

        ApplyStepTargetToPlayer();

        if (s.type == StepType.WaitSeconds)
            StartCoroutine(WaitThenNext(s.seconds));
    }

    void ApplyStepTargetToPlayer()
    {
        if (player == null) return;

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
        waiting = false;

        // Apaga markers del step actual al salir (por si el siguiente step no auto-apaga)
        DisableStepMarkers(i);

        i++;
        RunCurrentStep();
    }

    void OnPlayerReachedPoint(Transform point)
    {
        if (!waiting) return;

        var s = steps[i];
        if (s.type != StepType.GoToPoint) return;
        if (s.point != point) return;

        Next();
    }

    void OnClientReceivedRecipe(MoveClientTutorial client, int recipeId)
    {
        if (!waiting) return;

        var s = steps[i];
        if (s.type != StepType.GiveRecipeToClient) return;
        if (s.client != client) return;
        if (s.recipeId != recipeId) return;

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
}