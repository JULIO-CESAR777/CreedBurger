using CreedBurger.Ingredients;
using CreedBurger.Interaction;
using UnityEngine;

namespace CreedBurger.Counters
{
    [RequireComponent(typeof(Collider))]
    public sealed class TrashCounter : MonoBehaviour, IInteractable
    {
        public void Interact(PlayerInteractor player)
        {
            if (player == null ||
                !player.TryTakeHeldIngredient(out Ingredient discardedIngredient))
            {
                return;
            }

            Destroy(discardedIngredient.gameObject);
        }
    }
}