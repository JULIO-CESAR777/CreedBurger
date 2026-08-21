namespace CreedBurger.Interaction
{
    /// <summary>
    /// Contract shared by every object the player can use.
    /// The receiver owns the decision of what an interaction means.
    /// </summary>
    public interface IInteractable
    {
        void Interact(PlayerInteractor player);
    }
}
