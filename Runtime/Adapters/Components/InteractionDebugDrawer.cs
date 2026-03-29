namespace P3k.InteractionsController.Adapters.Components
{
   using P3k.InteractionsController.Abstractions.Interfaces;
   using P3k.InteractionsController.Implementations.Services;

   using System.Linq;

   using UnityEngine;

   public class InteractionDebugDrawer : MonoBehaviour
   {
      [Header("Colors")]
      [SerializeField] private Color _noTargetColor = Color.grey;
      [SerializeField] private Color _targetColor = Color.green;
      [SerializeField] private Color _holdingColor = Color.yellow;

      [Header("Hit Point")]
      [SerializeField] private float _hitSphereRadius = 0.05f;

      [Header("Info Panel")]
      [SerializeField] private Vector2 _panelOffset = new Vector2(0.15f, 0.15f);
      [SerializeField] private int _fontSize = 12;
      [SerializeField] private Color _panelBackgroundColor = new Color(0f, 0f, 0f, 0.8f);
      [SerializeField] private Color _panelTextColor = Color.white;

      private InteractionService _service;

      public void Initialize(InteractionService service)
      {
         _service = service;
      }

#if UNITY_EDITOR
      private void OnDrawGizmos()
      {
         if (_service == null) return;

         var ray = _service.RayProvider.InteractionRay;
         var distance = _service.RayDistance;
         var target = _service.CurrentTarget;
         var isHolding = _service.IsHolding;

         Color color;
         if (isHolding)
            color = Color.Lerp(_holdingColor, Color.red, _service.HoldProgress);
         else if (target != null)
            color = _targetColor;
         else
            color = _noTargetColor;

         Gizmos.color = color;

         if (target != null && Physics.Raycast(ray, out var hit, distance))
         {
            Gizmos.DrawLine(ray.origin, hit.point);
            Gizmos.DrawWireSphere(hit.point, _hitSphereRadius);
            DrawInfoPanel(hit.point, target);
         }
         else
         {
            Gizmos.DrawLine(ray.origin, ray.origin + ray.direction * distance);
         }

         if (isHolding)
         {
            DrawHoldProgressBar(ray.origin, _service.HoldProgress);
         }
      }

      private void DrawInfoPanel(Vector3 hitPoint, IInteractable target)
      {
         var cam = UnityEditor.SceneView.lastActiveSceneView?.camera;
         if (cam == null) return;

         var objectName = target is Component comp ? comp.gameObject.name : "Unknown";

         var label = $"{objectName}\n" + $"Channel: {target.Channel}\n" + $"Type: {target.Type}\n"
                     + $"CanInteract: {target.CanInteract}";

         if (target.Type != Abstractions.Enums.InteractionType.Press)
            label += $"\nHoldDuration: {target.HoldDuration:F1}s";

         if (_service.IsHolding)
            label += $"\nProgress: {_service.HoldProgress:P0}";

         var style = new GUIStyle(GUI.skin.box)
         {
            fontSize = _fontSize,
            alignment = TextAnchor.UpperLeft,
            richText = false,
            wordWrap = false
         };
         style.normal.textColor = _panelTextColor;

         var bgTex = new Texture2D(1, 1);
         bgTex.SetPixel(0, 0, _panelBackgroundColor);
         bgTex.Apply();
         style.normal.background = bgTex;
         style.padding = new RectOffset(6, 6, 4, 4);

         var panelPos = hitPoint + cam.transform.right * _panelOffset.x + cam.transform.up * _panelOffset.y;

         UnityEditor.Handles.Label(panelPos, label, style);
      }

      private void DrawHoldProgressBar(Vector3 origin, float progress)
      {
         var cam = UnityEditor.SceneView.lastActiveSceneView?.camera;
         if (cam == null) return;

         var right = cam.transform.right;
         var up = cam.transform.up;

         var barWidth = 0.3f;
         var barHeight = 0.04f;
         var barOffset = up * -0.08f;

         var barStart = origin + barOffset - right * (barWidth * 0.5f);
         var barEnd = barStart + right * barWidth;
         var fillEnd = barStart + right * (barWidth * progress);

         // Background
         Gizmos.color = new Color(0f, 0f, 0f, 0.5f);
         DrawQuad(barStart, barEnd, barHeight, up);

         // Fill
         Gizmos.color = Color.Lerp(_holdingColor, Color.red, progress);
         DrawQuad(barStart, fillEnd, barHeight, up);
      }

      private static void DrawQuad(Vector3 start, Vector3 end, float height, Vector3 up)
      {
         var halfUp = up * (height * 0.5f);
         var bl = start - halfUp;
         var tl = start + halfUp;
         var tr = end + halfUp;
         var br = end - halfUp;

         Gizmos.DrawLine(bl, tl);
         Gizmos.DrawLine(tl, tr);
         Gizmos.DrawLine(tr, br);
         Gizmos.DrawLine(br, bl);
      }
#endif
   }
}
