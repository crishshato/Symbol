public interface IInteractable
{
    // Called when player presses Interact while looking at this object
    void Interact(PlayerInteractor interactor);
    // Optional: show/hide prompt when targeted
    void SetTargeted(bool targeted);
}
