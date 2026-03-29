using UnityEngine;

namespace P3k.InteractionsController.Abstractions.Interfaces
{
    public interface IInteractionRayProvider
    {
        Ray InteractionRay { get; }
    }
}
