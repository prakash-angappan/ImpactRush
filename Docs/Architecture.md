# Impact Rush — Architecture

## Overview

Impact Rush follows a layered, assembly-driven architecture optimized for Unity 6 mobile development. Core infrastructure (managers, services, events) lives in `ImpactRush.Core` and is wired at bootstrap time. Gameplay, UI, and physics layers consume these abstractions without owning application lifecycle.

## Layers

```
┌─────────────────────────────────────────┐
│              ImpactRush.UI              │
├─────────────────────────────────────────┤
│           ImpactRush.Gameplay           │
├─────────────────────────────────────────┤
│           ImpactRush.Physics            │
├─────────────────────────────────────────┤
│             ImpactRush.Core             │
├─────────────────────────────────────────┤
│          ImpactRush.Utilities           │
└─────────────────────────────────────────┘
         ImpactRush.Editor (Editor only)
```

## Core Foundation

### Managers (`ImpactRush.Core.Managers`)

| Type | Responsibility |
|------|----------------|
| `GameManager` | Persists via `DontDestroyOnLoad`, applies `GameSettings` (target FPS, fixed timestep, master volume), configures VSync, registers services, runs `IInitializable` pass |
| `SceneLoader` | Async scene loading via `GameScene` enum (no raw scene strings at call sites) |
| `ServiceLocator` | Lightweight composition-root registry — not a service singleton |
| `EventBus` | Strongly typed publish/subscribe using `struct` events implementing `IGameEvent` |

### Interfaces (`ImpactRush.Core.Interfaces`)

| Interface | Purpose |
|-----------|---------|
| `IGameService` | Marker for services registered by `GameManager` |
| `IInitializable` | Explicit initialization hook invoked after registration |

### Data (`ImpactRush.Core.Data`)

| Asset | Purpose |
|-------|---------|
| `GameSettings` | ScriptableObject for application/audio defaults: target FPS, fixed timestep, master/music/SFX volume. No gameplay tuning. |

### Utilities (`ImpactRush.Utilities`)

| Type | Purpose |
|------|---------|
| `Constants` | Shared numeric defaults |
| `Layers` | Canonical layer names and mask helpers |
| `Extensions` | Cross-cutting helpers (e.g. volume clamping) |
| `Guard` | Argument validation (Editor / Development builds) |

### Scene Identifiers

`GameScene` enum values align with Build Settings indices. Use `GameSceneExtensions.ToSceneName()` for the underlying Unity scene name.

## Bootstrap Flow

```
Bootstrap scene
    │  GameBootstrap root object
    │    ├─ GameManager   (Awake: persist, apply settings, register services)
    │    ├─ SceneLoader   (async loading API)
    │    └─ GameBootstrap (Start: load initial scene)
    ▼
Gameplay (default initial scene)
    │  [future: session end → MainMenu]
    ▼
MainMenu
```

On play, `GameManager` runs first (`DefaultExecutionOrder -100`), then `SceneLoader` (`-50`), then `GameBootstrap.Start()` loads `GameScene.Gameplay`.

`GameManager` survives the scene transition; bootstrap-only objects are replaced by the loaded scene.

## Service Registration

Registration happens once at bootstrap through `GameManager`:

1. `GameSettings` asset is registered in `ServiceLocator`
2. All `IGameService` components on the bootstrap object are registered by concrete type
3. All `IInitializable` components receive `Initialize()`

Consumers should prefer constructor/method injection where practical. Use `ServiceLocator.Get<T>()` only when passing dependencies is impractical (e.g. deep UI leaf nodes).

## Event System

`EventBus` uses generic struct events — no `UnityEvent`, no string event names:

```csharp
public readonly struct SceneLoadStartedEvent : IGameEvent
{
    public GameScene Scene { get; }
}

EventBus.Subscribe<SceneLoadStartedEvent>(OnSceneLoadStarted);
EventBus.Publish(new SceneLoadStartedEvent(GameScene.Gameplay));
```

Subscribers are snapshotted during publish to avoid mutation issues mid-dispatch.

## SOLID Mapping

### Single Responsibility

- `GameManager` — application lifecycle and service wiring only
- `SceneLoader` — scene loading only
- `GameBootstrap` — bootstrap entry and initial scene transition only
- `EventBus` / `ServiceLocator` — one concern each (events vs. DI registry)

### Open/Closed

- New services implement `IGameService` / `IInitializable` without modifying manager internals
- New events are new `IGameEvent` structs without changing `EventBus`

### Liskov Substitution

- Any `IGameService` implementation can be registered and retrieved by type

### Interface Segregation

- Small focused interfaces (`IGameService`, `IInitializable`, domain markers in other assemblies)

### Dependency Inversion

- Higher layers depend on Core abstractions (`IGameService`, `EventBus`, `ServiceLocator`, `GameScene`)
- Scene loading call sites use `GameScene` enum, not `SceneManager` directly

## Assembly References

| Assembly | References |
|----------|------------|
| `ImpactRush.Utilities` | — |
| `ImpactRush.Core` | Utilities |
| `ImpactRush.Physics` | Core, Utilities |
| `ImpactRush.Gameplay` | Core, Physics, Utilities |
| `ImpactRush.UI` | Core, Utilities |
| `ImpactRush.Editor` | All runtime assemblies |

## Configuration

- **Game settings asset:** `Assets/ScriptableObjects/GameSettings.asset`
- **URP mobile profile:** `Assets/Settings/Mobile_RPAsset.asset`
- **Scene enum:** `ImpactRush.Core.GameScene` (sync with Build Settings order)
- **Editor menu:** `Impact Rush/Create Game Settings Asset`, `Impact Rush/Validate Build Settings`

## Conventions

- One public type per file
- Namespace matches assembly root
- No gameplay logic in Bootstrap or Editor assemblies
- Prefer async scene loading via `SceneLoader`
- Avoid singleton MonoBehaviours for business logic — use `ServiceLocator` at the composition root only
