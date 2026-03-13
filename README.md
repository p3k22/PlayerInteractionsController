# P3k.InteractionController

A lightweight, raycast-based interaction system for Unity that supports press, hold, and cancellable-hold interactions across multiple input channels.

## Features

- **Raycast targeting** – detects the closest `IInteractable` along a configurable ray each tick.
- **Interactor identity** – every interaction carries an `IInteractor` reference so interactables know *who* is interacting (essential for multiplayer).
- **Three interaction types** – `Press` (instant), `Hold` (timed), and `HoldCancellable` (timed, cancels on release).
- **Dual input channels** – `Primary` and `Secondary` channels allow different objects to respond to different inputs.
- **Snapshot & restore** – capture and restore hold state with `InteractionSnapshot` for pause/resume scenarios.
- **Runtime configuration** – `InteractionConfig` ScriptableObject controls ray distance, layer mask, and default hold duration.
- **Editor debug visualization** – `InteractionDebugDrawer` component renders the interaction ray, hit point, target info, and hold progress in the Scene view.

## Architecture

```
Abstractions/
├── Enums/
│   ├── InteractionChannel   – Primary | Secondary
│   └── InteractionType      – Press | Hold | HoldCancellable
└── Interfaces/
    ├── IInteractable         – Implement on objects that can be interacted with
    ├── IInteractionRayProvider – Provides the ray used for interaction detection
    └── IInteractor            – Implement on the entity performing interactions (e.g. player)

Implementations/
├── DataContainers/
│   ├── InteractionConfig     – ScriptableObject with ray distance, layer mask, default hold time
│   ├── InteractionRequest    – Readonly struct describing a pending interaction
│   ├── InteractionResult     – Readonly struct describing an interaction outcome
│   └── InteractionSnapshot   – Serializable hold-state snapshot
└── Services/
    ├── InteractionProcessor  – Core tick logic: raycast, targeting, hold tracking
    ├── InteractionRayProvider – Default ray provider using a Transform's position/forward
    └── InteractionService    – High-level façade with events, disposal, and ray-provider swapping

Adapters/
└── Components/
    └── InteractionDebugDrawer – MonoBehaviour that draws gizmos for the interaction ray
```

## Getting Started

### 1. Create a configuration asset

**Assets → Create → P3k → Interaction Config**

Configure the ray distance, layer mask, and default hold time in the Inspector.

### 2. Implement `IInteractable`

```csharp
using P3k.InteractionsController.Abstractions.Enums;
using P3k.InteractionsController.Abstractions.Interfaces;
using UnityEngine;

public class Lever : MonoBehaviour, IInteractable
{
    public bool CanInteract => true;
    public float HoldDuration => 0f;               // uses config default when <= 0
    public InteractionChannel Channel => InteractionChannel.Primary;
    public InteractionType Type => InteractionType.Press;
    public string InteractionTag => "Pull Lever";

    public void OnInteractionStarted(IInteractor interactor)
        => Debug.Log($"{interactor.GameObject.name} started pulling");

    public void OnInteractionCompleted(IInteractor interactor)
        => Debug.Log($"{interactor.GameObject.name} pulled the lever");

    public void OnInteractionCancelled(IInteractor interactor)
        => Debug.Log($"{interactor.GameObject.name} cancelled");
}
```

Attach a **Collider** on the same GameObject (or a parent) so the raycast can detect it.

### 3. Implement `IInteractor` and tick the service

The entity that performs interactions (typically a player controller) implements `IInteractor` and passes itself when creating the service. This lets every interactable know *who* triggered the interaction.

```csharp
using P3k.InteractionsController.Abstractions.Interfaces;
using P3k.InteractionsController.Implementations.DataContainers;
using P3k.InteractionsController.Implementations.Services;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour, IInteractor
{
    [SerializeField] private InteractionConfig _config;
    [SerializeField] private Transform _rayOrigin;

    private InteractionService _service;

    public GameObject GameObject => gameObject;

    private void Awake()
    {
        var rayProvider = new InteractionRayProvider(_rayOrigin);
        _service = new InteractionService(this, rayProvider, _config);

        _service.InteractionStarted   += t => Debug.Log($"Started: {t.InteractionTag}");
        _service.InteractionCompleted += t => Debug.Log($"Completed: {t.InteractionTag}");
        _service.InteractionCancelled += t => Debug.Log($"Cancelled: {t.InteractionTag}");
        _service.TargetChanged        += t => Debug.Log($"Target: {t?.InteractionTag ?? "none"}");
    }

    private void Update()
    {
        _service.Tick(
            interact01Pressed: Input.GetKeyDown(KeyCode.E),
            interact01Held:    Input.GetKey(KeyCode.E),
            interact02Pressed: Input.GetKeyDown(KeyCode.Q),
            interact02Held:    Input.GetKey(KeyCode.Q),
            deltaTime:         Time.deltaTime);
    }

    private void OnDestroy()
    {
        _service.Dispose();
    }
}
```

### 4. (Optional) Add debug visualization

Attach `InteractionDebugDrawer` to any GameObject and call `Initialize(_service)` to see the interaction ray, hit point, and target info as gizmos in the Scene view.

## API Reference

### `InteractionService`

| Member | Description |
|---|---|
| `Tick(...)` | Drives the interaction loop each frame. |
| `AllowInteractions(bool)` | Enable or disable all interactions at runtime. |
| `ForceCancel()` | Immediately cancels any active hold. |
| `SetRayDistance(float)` | Override the ray distance at runtime. |
| `SetRayProvider(IInteractionRayProvider)` | Swap the ray source (e.g., switching cameras). |
| `GetSnapshot()` / `RestoreSnapshot(...)` | Save and restore hold state. |
| `Interactor` | The `IInteractor` that owns this service. |
| `CurrentTarget` | The `IInteractable` currently under the ray (or `null`). |
| `ActiveInteraction` | The `IInteractable` being held (or `null`). |
| `IsHolding` | Whether a hold interaction is in progress. |
| `HoldProgress` | Normalized hold progress (`0`–`1`). |

### Events

| Event | Signature |
|---|---|
| `TargetChanged` | `Action<IInteractable>` |
| `InteractionStarted` | `Action<IInteractable>` |
| `InteractionCompleted` | `Action<IInteractable>` |
| `InteractionCancelled` | `Action<IInteractable>` |
| `HoldProgressChanged` | `Action<IInteractable, float>` |

## Requirements

- **Unity 2019.3+** (tested with Unity 6000.3)
- **.NET Standard 2.1**
