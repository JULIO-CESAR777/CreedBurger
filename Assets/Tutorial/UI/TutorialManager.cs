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
        // eventos del cliente (no dependen del player)
        MoveClientTutorial.OnClientReceivedRecipe += OnClientReceivedRecipe;

        // empezar a buscar player
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
            // Si no hay player o se destruyó, busca otro
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

        // Re-aplicar el target del paso actual (por si el player respawneó a mitad)
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
            return;
        }

        var s = steps[i];
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
}