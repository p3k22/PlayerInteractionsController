namespace P3k.InteractionsController.Implementations.DataContainers
{
   using System.Linq;

   using UnityEngine;

   [CreateAssetMenu(fileName = "InteractionConfig", menuName = "P3k/Interaction Config")]
   public class InteractionConfig : ScriptableObject
   {
      [Tooltip("Fallback hold duration when IInteractable.HoldDuration <= 0.")]
      [SerializeField]
      private float _defaultHoldTime = 1f;

      [Tooltip("Max interaction raycast distance.")]
      [SerializeField]
      private float _rayDistance = 3f;

      [Tooltip("Which layers the interaction ray can hit.")]
      [SerializeField]
      private LayerMask _rayLayerMask = ~0;

      public float DefaultHoldTime => _defaultHoldTime;

      public float RayDistance => _rayDistance;

      public LayerMask RayLayerMask => _rayLayerMask;

      public void SetRayDistance(float distance)
      {
         _rayDistance = Mathf.Max(0f, distance);
      }
   }
}