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

        public Sprite icon;   // 👈 NUEVO

        public Transform point;
        public MoveClientTutorial client;
        public int recipeId;
        public float seconds;
    }

    [SerializeField] private TutorialUi ui;
    [SerializeField] private Step[] steps;

    private int i;
    private bool waiting;

    // dependencias (tu player, etc.)
    [SerializeField] private PlayerTutorial player; // o tu script real

    void OnEnable()
    {
        if (player) player.OnReachedTutorialPoint += OnPlayerReachedPoint;
        MoveClientTutorial.OnClientReceivedRecipe += OnClientReceivedRecipe; // evento estático (lo agregamos abajo)
    }

    void OnDisable()
    {
        if (player) player.OnReachedTutorialPoint -= OnPlayerReachedPoint;
        MoveClientTutorial.OnClientReceivedRecipe -= OnClientReceivedRecipe;
    }

    void Start()
    {
        i = 0;
        RunCurrentStep();
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

        // Activación del paso
        switch (s.type)
        {
            case StepType.GoToPoint:
                // opcional: resaltar punto, poner marker, etc.
                player.SetTutorialTarget(s.point);
                break;

            case StepType.GiveRecipeToClient:
                player.SetTutorialTarget(null);
                break;

            case StepType.WaitSeconds:
                player.SetTutorialTarget(null);
                StartCoroutine(WaitThenNext(s.seconds));
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

    // ====== “Condiciones” que completan pasos ======

    void OnPlayerReachedPoint(Transform point)
    {
        if (!waiting) return;

        var s = steps[i];
        if (s.type != StepType.GoToPoint) return;
        if (s.point != point) return;

        Next();
    }

    private void OnClientReceivedRecipe(MoveClientTutorial client, int recipeId)
    {
        if (!waiting) return;

        var s = steps[i];
        if (s.type != StepType.GiveRecipeToClient) return;
        if (s.client != client) return;
        if (s.recipeId != recipeId) return;

        Next();
    }
}