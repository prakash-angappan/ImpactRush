# Impact Rush

Commercial mobile game built with **Unity 6**, **Universal Render Pipeline (URP)**, targeting **Android** in **portrait** orientation.

## Overview

Impact Rush is a mobile-first production project with a modular architecture designed for maintainability, fast iteration, and clean separation of concerns.

## Requirements

| Component | Version |
|-----------|---------|
| Unity | 6000.3.x (Unity 6) |
| Render Pipeline | URP 17.x |
| Primary Platform | Android |
| Orientation | Portrait |

## Project Structure

```
Assets/
├── Art/                 # Meshes, textures, materials, animations
├── Audio/               # Music, SFX, audio mixers
├── Prefabs/             # Reusable scene objects
├── Scenes/              # Bootstrap, MainMenu, Gameplay
├── Scripts/
│   ├── Core/            # Bootstrap, loading, scene flow
│   ├── Gameplay/        # Game rules and session logic
│   ├── Physics/         # Physics configuration and systems
│   ├── Managers/        # High-level coordinators (Core assembly)
│   ├── UI/              # Screens and HUD
│   ├── Utilities/       # Shared helpers
│   └── Editor/          # Editor-only tooling
├── ScriptableObjects/   # Data-driven configuration assets
├── Settings/            # URP and render pipeline assets
├── VFX/                 # Particle systems and VFX Graph
└── ThirdParty/          # Licensed external packages and plugins
```

## Assembly Definitions

| Assembly | Purpose |
|----------|---------|
| `ImpactRush.Utilities` | Shared helpers with no project dependencies |
| `ImpactRush.Core` | Bootstrap, scene loading, managers |
| `ImpactRush.Physics` | Physics abstractions and configuration |
| `ImpactRush.Gameplay` | Game session and rules |
| `ImpactRush.UI` | User interface layer |
| `ImpactRush.Editor` | Editor validation and tooling |

### Dependency Graph

```
Utilities (base)
    ↑
Core ──────────────┐
    ↑              │
Physics            │
    ↑              │
Gameplay           │
                   │
UI ────────────────┘

Editor → references all runtime assemblies
```

## Scenes & Build Order

1. **Bootstrap** — Application entry; initializes services and loads Main Menu
2. **MainMenu** — Front-end navigation
3. **Gameplay** — Core game session

## Getting Started

1. Open the project in **Unity 6** (6000.3.18f1 or later).
2. Open `Assets/Scenes/Bootstrap.unity`.
3. Press **Play** — the Bootstrap scene automatically transitions to Main Menu.
4. Validate build settings via **Impact Rush → Validate Build Settings**.

## Documentation

See the [`Docs/`](Docs/) folder:

- [GameDesign.md](Docs/GameDesign.md) — Design pillars and mechanics overview
- [Architecture.md](Docs/Architecture.md) — Technical architecture and SOLID principles
- [Roadmap.md](Docs/Roadmap.md) — Milestone plan
- [Todo.md](Docs/Todo.md) — Active task tracker

## Coding Standards

- Follow namespaces matching assembly names (`ImpactRush.Core`, etc.).
- Prefer interfaces and dependency injection over direct static access.
- Keep gameplay logic out of UI and bootstrap layers.
- Editor code lives exclusively in `ImpactRush.Editor`.

## License

Proprietary — APTech. All rights reserved.
