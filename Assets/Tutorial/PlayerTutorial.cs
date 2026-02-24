using System;
using UnityEngine;

public class PlayerTutorial : MonoBehaviour
{
    public event Action<Transform> OnReachedTutorialPoint;

    private Transform tutorialTarget;
    [SerializeField] private float reachDistance = 1.2f;

    public void SetTutorialTarget(Transform t)
    {
        tutorialTarget = t;
    }

    void Update()
    {
        if (!tutorialTarget) return;

        float d = Vector3.Distance(transform.position, tutorialTarget.position);
        if (d <= reachDistance)
        {
            var reached = tutorialTarget;
            tutorialTarget = null;
            OnReachedTutorialPoint?.Invoke(reached);
        }
    }
}