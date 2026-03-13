namespace P3k.InteractionsController.Implementations.Services
{
   using P3k.InteractionsController.Abstractions.Interfaces;
   using P3k.InteractionsController.Implementations.DataContainers;

   using System;
   using System.Linq;

   using Object = UnityEngine.Object;

   public class InteractionService : IDisposable
   {
      private readonly InteractionConfig _runtimeConfig;

      public bool IsHolding => Processor.IsHolding;

      public float HoldProgress => Processor.HoldProgress;

      public IInteractable ActiveInteraction => Processor.ActiveInteraction;

      public IInteractable CurrentTarget => Processor.CurrentTarget;

      public IInteractionRayProvider RayProvider => Processor.RayProvider;

      public float RayDistance => _runtimeConfig.RayDistance;

      public InteractionProcessor Processor { get; private set; }

      public event Action<IInteractable, float> HoldProgressChanged;

      public event Action<IInteractable> InteractionCancelled;

      public event Action<IInteractable> InteractionCompleted;

      public event Action<IInteractable> InteractionStarted;

      public event Action<IInteractable> TargetChanged;

      public IInteractor Interactor { get; }

      public InteractionService(IInteractor interactor, IInteractionRayProvider rayProvider, InteractionConfig config)
      {
         Interactor = interactor;
         _runtimeConfig = Object.Instantiate(config);
         Processor = new InteractionProcessor(interactor, rayProvider, _runtimeConfig);
         SubscribeEvents(Processor);
      }

      public void AllowInteractions(bool allowed)
      {
         Processor.AllowInteractions(allowed);
      }

      public void Dispose()
      {
         UnsubscribeEvents(Processor);

         if (_runtimeConfig != null)
         {
            Object.Destroy(_runtimeConfig);
         }
      }

      public void ForceCancel()
      {
         Processor.ForceCancel();
      }

      public InteractionSnapshot GetSnapshot()
      {
         return Processor.GetSnapshot();
      }

      public void RestoreSnapshot(InteractionSnapshot snapshot)
      {
         Processor.RestoreSnapshot(snapshot);
      }

      public void SetRayDistance(float distance)
      {
         _runtimeConfig.SetRayDistance(distance);
      }

      public void SetRayProvider(IInteractionRayProvider rayProvider)
      {
         UnsubscribeEvents(Processor);
         Processor = new InteractionProcessor(Interactor, rayProvider, _runtimeConfig);
         SubscribeEvents(Processor);
      }

      public void Tick(
         bool interact01Pressed,
         bool interact01Held,
         bool interact02Pressed,
         bool interact02Held,
         float deltaTime)
      {
         Processor.Tick(interact01Pressed, interact01Held, interact02Pressed, interact02Held, deltaTime);
      }

      private void OnHoldProgressChanged(IInteractable t, float p)
      {
         HoldProgressChanged?.Invoke(t, p);
      }

      private void OnInteractionCancelled(IInteractable t)
      {
         InteractionCancelled?.Invoke(t);
      }

      private void OnInteractionCompleted(IInteractable t)
      {
         InteractionCompleted?.Invoke(t);
      }

      private void OnInteractionStarted(IInteractable t)
      {
         InteractionStarted?.Invoke(t);
      }

      private void OnTargetChanged(IInteractable t)
      {
         TargetChanged?.Invoke(t);
      }

      private void SubscribeEvents(InteractionProcessor service)
      {
         service.InteractionStarted += OnInteractionStarted;
         service.InteractionCompleted += OnInteractionCompleted;
         service.InteractionCancelled += OnInteractionCancelled;
         service.HoldProgressChanged += OnHoldProgressChanged;
         service.TargetChanged += OnTargetChanged;
      }

      private void UnsubscribeEvents(InteractionProcessor service)
      {
         service.InteractionStarted -= OnInteractionStarted;
         service.InteractionCompleted -= OnInteractionCompleted;
         service.InteractionCancelled -= OnInteractionCancelled;
         service.HoldProgressChanged -= OnHoldProgressChanged;
         service.TargetChanged -= OnTargetChanged;
      }
   }
}