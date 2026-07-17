using CreedBurger.Ingredients;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CreedBurger.Interaction
{
    [RequireComponent(typeof(PlayerInput))]
    public sealed class PlayerInteractor : MonoBehaviour
    {
        [Header("Detection")]
        [SerializeField] private Transform interactionOrigin;
        [SerializeField, Min(0f)] private float forwardOffset = 1f;
        [SerializeField, Min(0.01f)] private float interactionRadius = 0.65f;
        [SerializeField] private LayerMask interactionLayers = ~0;
        [SerializeField, Min(1)] private int overlapBufferSize = 16;

        [Header("Input System")]
        [SerializeField] private string interactionActionName = "Interact";

        [Header("Holding")]
        [SerializeField] private Transform handAnchor;

        private Collider[] overlapBuffer;
        private InputAction interactionAction;
        private IHighlightable currentHighlightable;

        public IInteractable CurrentInteractable { get; private set; }
        public Ingredient HeldIngredient { get; private set; }
        public bool HasHeldIngredient => HeldIngredient != null;
        public Transform HandAnchor => handAnchor != null ? handAnchor : transform;

        private void Awake()
        {
            overlapBuffer = new Collider[Mathf.Max(1, overlapBufferSize)];

            PlayerInput playerInput = GetComponent<PlayerInput>();
            interactionAction = playerInput.actions.FindAction(
                interactionActionName,
                throwIfNotFound: true);
        }

        private void Update()
        {
            RefreshCurrentInteractable();

            if (interactionAction.WasPerformedThisFrame())
            {
                TryInteract();
            }
        }

        public void TryInteract()
        {
            RefreshCurrentInteractable();
            CurrentInteractable?.Interact(this);
        }

        public bool TryHoldIngredient(Ingredient ingredient)
        {
            if (ingredient == null || HasHeldIngredient)
            {
                return false;
            }

            HeldIngredient = ingredient;
            ingredient.SetHeld(HandAnchor);
            return true;
        }

        public bool TryTakeHeldIngredient(out Ingredient ingredient)
        {
            ingredient = HeldIngredient;

            if (ingredient == null)
            {
                return false;
            }

            HeldIngredient = null;
            return true;
        }

        public void RefreshCurrentInteractable()
        {
            Transform origin =
                interactionOrigin != null ? interactionOrigin : transform;

            Vector3 centre =
                origin.position + origin.forward * forwardOffset;

            int hitCount = Physics.OverlapSphereNonAlloc(
                centre,
                interactionRadius,
                overlapBuffer,
                interactionLayers,
                QueryTriggerInteraction.Collide);

            IInteractable closestInteractable = null;
            MonoBehaviour closestInteractableBehaviour = null;
            float closestSqrDistance = float.PositiveInfinity;

            for (int hitIndex = 0; hitIndex < hitCount; hitIndex++)
            {
                Collider hitCollider = overlapBuffer[hitIndex];

                if (hitCollider == null)
                {
                    continue;
                }

                MonoBehaviour[] behaviours =
                    hitCollider.GetComponentsInParent<MonoBehaviour>();

                foreach (MonoBehaviour behaviour in behaviours)
                {
                    IInteractable interactable = behaviour as IInteractable;

                    if (interactable == null || !behaviour.isActiveAndEnabled)
                    {
                        continue;
                    }

                    float sqrDistance =
                        (hitCollider.ClosestPoint(origin.position) -
                        origin.position).sqrMagnitude;

                    if (sqrDistance < closestSqrDistance)
                    {
                        closestSqrDistance = sqrDistance;
                        closestInteractable = interactable;
                        closestInteractableBehaviour = behaviour;
                    }
                }
            }

            SetCurrentInteractable(
                closestInteractable,
                closestInteractableBehaviour);
        }

        private void SetCurrentInteractable(
            IInteractable nextInteractable,
            MonoBehaviour nextBehaviour)
        {
            if (ReferenceEquals(CurrentInteractable, nextInteractable))
            {
                return;
            }

            currentHighlightable?.SetHighlighted(false);

            CurrentInteractable = nextInteractable;
            currentHighlightable = FindHighlightable(nextBehaviour);

            currentHighlightable?.SetHighlighted(true);
        }

        private static IHighlightable FindHighlightable(
            MonoBehaviour interactableBehaviour)
        {
            if (interactableBehaviour == null)
            {
                return null;
            }

            MonoBehaviour[] components =
                interactableBehaviour.GetComponents<MonoBehaviour>();

            foreach (MonoBehaviour component in components)
            {
                IHighlightable highlightable = component as IHighlightable;

                if (highlightable != null)
                {
                    return highlightable;
                }
            }

            return null;
        }

        private void OnDisable()
        {
            currentHighlightable?.SetHighlighted(false);
            currentHighlightable = null;
            CurrentInteractable = null;
        }

        private void OnDrawGizmosSelected()
        {
            Transform origin =
                interactionOrigin != null ? interactionOrigin : transform;

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(
                origin.position + origin.forward * forwardOffset,
                interactionRadius);
        }
    }
}