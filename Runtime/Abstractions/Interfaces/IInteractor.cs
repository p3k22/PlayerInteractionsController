namespace P3k.InteractionsController.Abstractions.Interfaces
{
   using System.Linq;

   using UnityEngine;

   public interface IInteractor
   {
      ulong InteractorId { get; }

      GameObject GameObject { get; }
   }
}
