namespace CreedBurger.Interaction
{
    public interface IHoldInteractable
    {
        float Progress01 { get; }

        bool TryBeginHold(PlayerInteractor player);
        bool ContinueHold(PlayerInteractor player, float deltaTime);
        void CancelHold(PlayerInteractor player);
    }
}