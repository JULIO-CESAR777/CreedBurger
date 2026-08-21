using CreedBurger.Interaction;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(PlayerInteractor))]
public class PlayerCutAction : MonoBehaviour
{
    private enum State
    {
        Idle,
        Holding,
        WaitingForRelease
    }

    [SerializeField] private string cutActionName = "Cut";

    private PlayerInteractor playerInteractor;
    private InputAction cutAction;
    private IHoldInteractable currentHoldTarget;
    private State state;

    public float Progress01 { get; private set; }
    public bool IsCutting => state == State.Holding;
    public IHoldInteractable CurrentHoldTarget => currentHoldTarget;

    private void Awake()
    {
        playerInteractor = GetComponent<PlayerInteractor>();

        PlayerInput playerInput = GetComponent<PlayerInput>();
        cutAction = playerInput.actions.FindAction(
            cutActionName,
            throwIfNotFound: true);
    }

    private void Update()
    {
        bool isPressed = cutAction.IsPressed();

        switch (state)
        {
            case State.Idle:
                if (isPressed)
                {
                    TryStartCut();
                }
                break;

            case State.Holding:
                UpdateCut(isPressed);
                break;

            case State.WaitingForRelease:
                if (!isPressed)
                {
                    state = State.Idle;
                    Progress01 = 0f;
                }
                break;
        }
    }

    private void TryStartCut()
    {
        playerInteractor.RefreshCurrentInteractable();

        IHoldInteractable candidate =
            playerInteractor.CurrentInteractable as IHoldInteractable;

        if (candidate == null ||
            !candidate.TryBeginHold(playerInteractor))
        {
            state = State.WaitingForRelease;
            return;
        }

        currentHoldTarget = candidate;
        Progress01 = 0f;
        state = State.Holding;
    }

    private void UpdateCut(bool isPressed)
    {
        if (!isPressed)
        {
            currentHoldTarget.CancelHold(playerInteractor);
            currentHoldTarget = null;
            Progress01 = 0f;
            state = State.Idle;
            return;
        }

        bool completed = currentHoldTarget.ContinueHold(
            playerInteractor,
            Time.deltaTime);

        Progress01 = currentHoldTarget.Progress01;

        if (!completed)
        {
            return;
        }

        Progress01 = 1f;
        currentHoldTarget = null;
        state = State.WaitingForRelease;
    }

    private void OnDisable()
    {
        if (currentHoldTarget != null)
        {
            currentHoldTarget.CancelHold(playerInteractor);
        }

        currentHoldTarget = null;
        Progress01 = 0f;
        state = State.Idle;
    }
}