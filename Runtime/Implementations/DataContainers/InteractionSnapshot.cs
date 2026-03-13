namespace P3k.InteractionsController.Implementations.DataContainers
{
   using P3k.InteractionsController.Abstractions.Enums;

   using System.Linq;

   public struct InteractionSnapshot
   {
      public bool IsHolding;

      public float HoldElapsed;

      public int InteractableId;

      public InteractionChannel Channel;
   }
}