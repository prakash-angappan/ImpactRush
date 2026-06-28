# Impact Rush — Architecture

## Overview

Impact Rush follows a layered, assembly-driven architecture optimized for Unity 6 mobile development. Each layer communicates through interfaces to uphold SOLID principles.

## Layers

```
┌─────────────────────────────────────────┐
│              ImpactRush.UI              │
├─────────────────────────────────────────┤
│           ImpactRush.Gameplay         │
├─────────────────────────────────────────┤
│           ImpactRush.Physics          │
├─────────────────────────────────────────┤
│             ImpactRush.Core             │
├─────────────────────────────────────────┤
│          ImpactRush.Utilities           │
└─────────────────────────────────────────┘
         ImpactRush.Editor (Editor only)
```

## SOLID Mapping

### Single Responsibility

- `SceneLoadService` — only loads scenes
- `SceneFlowManager` — only coordinates scene transitions
- `BootstrapInitializer` — only handles bootstrap entry

### Open/Closed

- Marker interfaces (`IGameSession`, `IUIScreen`, `IPhysicsConfigurator`) allow new implementations without modifying consumers

### Liskov Substitution

- All scene loading goes through `ISceneLoadService`; any compliant implementation can replace `SceneLoadService`

### Interface Segregation

- Small, focused interfaces per domain rather than a single god-interface

### Dependency Inversion

- `SceneFlowManager` depends on `ISceneLoadService`, not `SceneManager` directly
- Future gameplay systems should depend on abstractions in Core/Gameplay

## Scene Flow

```
Bootstrap (index 0)
    │  BootstrapInitializer.Start()
    ▼
MainMenu (index 1)
    │  [future: Play button]
    ▼
Gameplay (index 2)
    │  [future: session end]
    ▼
MainMenu
```

## Assembly References

| Assembly | References |
|----------|------------|
| `ImpactRush.Utilities` | — |
| `ImpactRush.Core` | Utilities |
| `ImpactRush.Physics` | Core, Utilities |
| `ImpactRush.Gameplay` | Core, Physics, Utilities |
| `ImpactRush.UI` | Core, Utilities |
| `ImpactRush.Editor` | All runtime assemblies |

## Managers Folder

`Assets/Scripts/Managers/` uses an **Assembly Definition Reference** (`.asmref`) to compile into `ImpactRush.Core`, keeping the user-facing folder layout while maintaining a single Core assembly.

## Configuration

- **URP Mobile Profile:** `Assets/Settings/Mobile_RPAsset.asset`
- **ScriptableObjects:** `Assets/ScriptableObjects/` for data-driven tuning
- **Scene names:** `ImpactRush.Core.SceneNames` constants

## Conventions

- One public type per file
- Namespace matches assembly root
- No gameplay logic in Bootstrap or Editor assemblies
- Prefer `async` scene loading over synchronous calls
