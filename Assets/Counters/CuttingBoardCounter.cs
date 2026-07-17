using CreedBurger.Ingredients;
using CreedBurger.Interaction;
using UnityEngine;

namespace CreedBurger.Counters
{
    public sealed class CuttingBoardCounter :
        ProcessingCounter,
        IHoldInteractable
    {
        [SerializeField, Min(0.1f)] private float requiredCutDuration = 1.5f;

        private bool isCutting;
        private float cutTimer;

        public float Progress01 =>
            isCutting
                ? Mathf.Clamp01(cutTimer / requiredCutDuration)
                : 0f;

        protected override string DefaultStationId => "cutting_board";

        public override void Interact(PlayerInteractor player)
        {
            if (isCutting)
            {
                return;
            }

            // Si hay una receta de corte válida, E no procesa instantáneamente.
            // El ingrediente permanece en la tabla hasta usar la acción Cut.
            if (player != null &&
                !player.HasHeldIngredient &&
                CanProcessStoredIngredient(out IngredientData ignoredResult))
            {
                return;
            }

            base.Interact(player);
        }

        public bool TryBeginHold(PlayerInteractor player)
        {
            if (isCutting ||
                !CanProcessStoredIngredient(out IngredientData ignoredResult))
            {
                return false;
            }

            cutTimer = 0f;
            isCutting = true;
            return true;
        }

        public bool ContinueHold(PlayerInteractor player, float deltaTime)
        {
            if (!isCutting)
            {
                return true;
            }

            if (!CanProcessStoredIngredient(out IngredientData ignoredResult))
            {
                CancelHold(player);
                return true;
            }

            cutTimer += deltaTime;

            if (cutTimer < requiredCutDuration)
            {
                return false;
            }

            bool processed = TryProcessStoredIngredient();
            isCutting = false;
            cutTimer = 0f;

            return processed;
        }

        public void CancelHold(PlayerInteractor player)
        {
            isCutting = false;
            cutTimer = 0f;
        }
    }
}