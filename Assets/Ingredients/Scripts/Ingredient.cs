using CreedBurger.Interaction;
using UnityEngine;

namespace CreedBurger.Ingredients
{
    public sealed class Ingredient : MonoBehaviour, IInteractable
    {
        [SerializeField] private IngredientData data;

        public IngredientData Data => data;
        
        private Collider[] ingredientColliders;
        private Rigidbody[] ingredientRigidbodies;

        public void Initialize(IngredientData ingredientData)
        {
            data = ingredientData;
        }
        private void Awake()
        {
            ingredientColliders = GetComponentsInChildren<Collider>(true);
            ingredientRigidbodies = GetComponentsInChildren<Rigidbody>(true);
        }
        
        public void Interact(PlayerInteractor player)
        {
            if (player == null)
            {
                return;
            }

            player.TryHoldIngredient(this);
        }

        public void SetHeld(Transform handAnchor)
        {
            transform.SetParent(handAnchor, false);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            
            SetPhysicsEnabled(false);
        }
        
        public void PlaceOn(Transform counterAnchor)
        {
            transform.SetParent(counterAnchor, false);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;

            SetPhysicsEnabled(false);
        }
        
        private void SetPhysicsEnabled(bool enabled)
        {
            foreach (Collider ingredientCollider in ingredientColliders)
            {
                ingredientCollider.enabled = enabled;
            }

            foreach (Rigidbody ingredientRigidbody in ingredientRigidbodies)
            {
                ingredientRigidbody.isKinematic = !enabled;
                ingredientRigidbody.useGravity = enabled;
            }
        }
        
    }
}