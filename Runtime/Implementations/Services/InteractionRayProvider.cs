namespace P3k.InteractionsController.Implementations.Services
{
   using P3k.InteractionsController.Abstractions.Interfaces;
   using System;
   using System.Linq;

   using UnityEngine;

   public class InteractionRayProvider : IInteractionRayProvider
   {
      private readonly Func<Vector3> _origin;
      private readonly Func<Vector3> _direction;

      public Ray InteractionRay => new(_origin(), _direction());

      public InteractionRayProvider(Func<Vector3> origin, Func<Vector3> direction)
      {
         _origin = origin;
         _direction = direction;
      }
   }
}