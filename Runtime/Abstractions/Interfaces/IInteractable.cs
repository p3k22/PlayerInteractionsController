namespace P3k.InteractionsController.Abstractions.Interfaces
{
   using P3k.InteractionsController.Abstractions.Enums;

   using System.Linq;

   public interface IInteractable
   {
      ulong InteractableId { get; }

      bool CanInteract { get; }

      float HoldDuration { get; }

      InteractionChannel Channel { get; }

      InteractionType Type { get; }

      string InteractionTag { get; }

      void OnInteractionCancelled(IInteractor interactor);

      void OnInteractionCompleted(IInteractor interactor);

      void OnInteractionStarted(IInteractor interactor);
   }
}