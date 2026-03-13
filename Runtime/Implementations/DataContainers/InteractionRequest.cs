namespace P3k.InteractionsController.Implementations.DataContainers
{
   using P3k.InteractionsController.Abstractions.Enums;
   using P3k.InteractionsController.Abstractions.Interfaces;

   using System.Linq;

   public readonly struct InteractionRequest
   {
      public readonly float StartTime;

      public readonly IInteractable Interactable;

      public readonly InteractionChannel Channel;

      public InteractionRequest(InteractionChannel channel, IInteractable interactable, float startTime)
      {
         Channel = channel;
         Interactable = interactable;
         StartTime = startTime;
      }
   }
}