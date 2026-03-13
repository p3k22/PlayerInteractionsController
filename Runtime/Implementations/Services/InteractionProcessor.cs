namespace P3k.InteractionsController.Implementations.Services
{
   using P3k.InteractionsController.Abstractions.Enums;
   using P3k.InteractionsController.Abstractions.Interfaces;
   using P3k.InteractionsController.Implementations.DataContainers;

   using System;
   using System.Linq;

   using UnityEngine;

   public class InteractionProcessor
   {
      private readonly InteractionConfig _config;

      public IInteractor Interactor { get; }

      public IInteractionRayProvider RayProvider { get; }

      private readonly RaycastHit[] _hitBuffer = new RaycastHit[16];

      private bool _interactionsAllowed = true;

      private float _holdElapsed;

      private InteractionChannel _activeChannel;

      public float HoldProgress => IsHolding ? Mathf.Clamp01(_holdElapsed / GetHoldDuration(ActiveInteraction)) : 0f;

      public bool IsHolding { get; private set; }

      public IInteractable ActiveInteraction { get; private set; }

      public IInteractable CurrentTarget { get; private set; }

      public event Action<IInteractable, float> HoldProgressChanged;

      public event Action<IInteractable> InteractionCancelled;

      public event Action<IInteractable> InteractionCompleted;

      public event Action<IInteractable> InteractionStarted;

      public event Action<IInteractable> TargetChanged;

      public InteractionProcessor(IInteractor interactor, IInteractionRayProvider rayProvider, InteractionConfig config)
      {
         Interactor = interactor;
         RayProvider = rayProvider;
         _config = config;
      }

      public void AllowInteractions(bool allowed)
      {
         _interactionsAllowed = allowed;
      }

      public void ForceCancel()
      {
         if (IsHolding)
         {
            CancelActiveHold();
         }
      }

      public InteractionSnapshot GetSnapshot()
      {
         return new InteractionSnapshot
         {
            IsHolding = IsHolding,
            HoldElapsed = _holdElapsed,
            InteractableId = ActiveInteraction != null ? ActiveInteraction.GetHashCode() : 0,
            Channel = _activeChannel
         };
      }

      public void RestoreSnapshot(InteractionSnapshot snapshot)
      {
         IsHolding = snapshot.IsHolding;
         _holdElapsed = snapshot.HoldElapsed;
         _activeChannel = snapshot.Channel;
      }

      public void Tick(
         bool interact01Pressed,
         bool interact01Held,
         bool interact02Pressed,
         bool interact02Held,
         float deltaTime)
      {
         if (!_interactionsAllowed)
         {
            if (IsHolding)
            {
               CancelActiveHold();
            }

            return;
         }

         // 1. Raycast
         var ray = RayProvider.InteractionRay;
         var hitCount = Physics.RaycastNonAlloc(ray, _hitBuffer, _config.RayDistance, _config.RayLayerMask);

         // 2. Find closest valid IInteractable
         IInteractable closest = null;
         var closestDist = float.MaxValue;

         for (var i = 0; i < hitCount; i++)
         {
            ref var hit = ref _hitBuffer[i];
            if (hit.distance >= closestDist)
            {
               continue;
            }

            var interactable = hit.collider.GetComponentInParent<IInteractable>();
            if (interactable is { CanInteract: true })
            {
               closest = interactable;
               closestDist = hit.distance;
            }
         }

         // 3. Update CurrentTarget
         if (!ReferenceEquals(closest, CurrentTarget))
         {
            CurrentTarget = closest;
            TargetChanged?.Invoke(CurrentTarget);
         }

         // 6. If target changes or goes null mid-hold, cancel active hold
         if (IsHolding && !ReferenceEquals(ActiveInteraction, CurrentTarget))
         {
            CancelActiveHold();
         }

         // 4. Handle new press
         if (!IsHolding)
         {
            if (interact01Pressed)
            {
               TryBeginInteraction(InteractionChannel.Primary);
            }
            else if (interact02Pressed)
            {
               TryBeginInteraction(InteractionChannel.Secondary);
            }
         }

         // 5. Handle active hold
         if (IsHolding)
         {
            var held = _activeChannel == InteractionChannel.Primary ? interact01Held : interact02Held;

            if (!held)
            {
               if (ActiveInteraction.Type == InteractionType.HoldCancellable)
               {
                  ActiveInteraction.OnInteractionCancelled(Interactor);
                  InteractionCancelled?.Invoke(ActiveInteraction);
               }

               ResetHoldState();
               return;
            }

            _holdElapsed += deltaTime;
            var progress = Mathf.Clamp01(_holdElapsed / GetHoldDuration(ActiveInteraction));
            HoldProgressChanged?.Invoke(ActiveInteraction, progress);

            if (_holdElapsed >= GetHoldDuration(ActiveInteraction))
            {
               ActiveInteraction.OnInteractionCompleted(Interactor);
               InteractionCompleted?.Invoke(ActiveInteraction);
               ResetHoldState();
            }
         }
      }

      private void CancelActiveHold()
      {
         if (ActiveInteraction is { Type: InteractionType.HoldCancellable })
         {
            ActiveInteraction.OnInteractionCancelled(Interactor);
            InteractionCancelled?.Invoke(ActiveInteraction);
         }

         ResetHoldState();
      }

      private float GetHoldDuration(IInteractable interactable)
      {
         if (interactable == null)
         {
            return _config.DefaultHoldTime;
         }

         return interactable.HoldDuration > 0f ? interactable.HoldDuration : _config.DefaultHoldTime;
      }

      private void ResetHoldState()
      {
         IsHolding = false;
         _holdElapsed = 0f;
         ActiveInteraction = null;
      }

      private void TryBeginInteraction(InteractionChannel channel)
      {
         if (CurrentTarget == null)
         {
            return;
         }

         if (CurrentTarget.Channel != channel)
         {
            return;
         }

         ActiveInteraction = CurrentTarget;
         _activeChannel = channel;

         ActiveInteraction.OnInteractionStarted(Interactor);
         InteractionStarted?.Invoke(ActiveInteraction);

         if (ActiveInteraction.Type == InteractionType.Press)
         {
            ActiveInteraction.OnInteractionCompleted(Interactor);
            InteractionCompleted?.Invoke(ActiveInteraction);
            ActiveInteraction = null;
         }
         else
         {
            IsHolding = true;
            _holdElapsed = 0f;
         }
      }
   }
}