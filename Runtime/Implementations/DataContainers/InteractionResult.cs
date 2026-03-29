namespace P3k.InteractionsController.Implementations.DataContainers
{
   using P3k.InteractionsController.Abstractions.Enums;
   using P3k.InteractionsController.Abstractions.Interfaces;

   using System.Linq;

   public readonly struct InteractionResult
   {
      public readonly bool Success;

      public readonly IInteractable Interactable;

      public readonly InteractionChannel Channel;

      public readonly string Reason;

      public InteractionResult(
         bool success,
         InteractionChannel channel,
         IInteractable interactable,
         string reason = null)
      {
         Success = success;
         Channel = channel;
         Interactable = interactable;
         Reason = reason;
      }
   }
}