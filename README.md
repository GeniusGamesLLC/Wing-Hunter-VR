# Wing Hunter VR

A VR duck hunting arcade game built for Meta Quest. A modern take on the classic Duck Hunt concept, designed for immersive VR gameplay.

## Core Gameplay

Players use VR controllers to shoot ducks that fly across the sky. Ducks spawn from various points around the player and follow curved flight paths (using spline-based movement) toward target points. Hit ducks before they escape to score points — miss too many and it's game over.

## Features

- VR shooting with right trigger, haptic feedback on shots
- Multiple unlockable guns (N-ZAP 85, N-ZAP 89, S-BLAST 92, Golden Gun) with unique sounds and muzzle flash effects
- Progressive difficulty system (5 levels) — ducks spawn faster and fly quicker as you score
- Object pooling for performance optimization on Quest hardware
- Curved flight paths via Catmull-Rom splines for more natural duck movement
- Spawn/escape animations with fade effects
- Feather poof particle effects on hits (URP-compatible)

## Game Flow

1. Game starts in Idle state
2. Player triggers StartGame → ducks begin spawning
3. Score points by hitting ducks (configurable points per duck)
4. Miss a duck → missed counter increments
5. Hit max missed ducks threshold → Game Over
6. Difficulty increases automatically at score thresholds

## Controls

| Input | Action |
|-------|--------|
| Right Trigger | Shoot |
| Right B Button | Switch gun (debug) |

## Technical Stack

- Unity with Universal Render Pipeline (URP)
- XR Interaction Toolkit for VR input
- ScriptableObjects for game configuration (DuckHuntConfig, GunCollection)
- Event-driven architecture between managers
- Debug UI system with announcement board for in-game settings

## Project Structure

```
Assets/
├── Scripts/
│   ├── Controllers/     # DuckController, ShootingController
│   ├── Managers/        # GameManager, SpawnManager, ScoreManager, GunSelectionManager
│   ├── Data/            # ScriptableObjects and config classes
│   ├── UI/              # Menu and debug UI components
│   └── VR/              # VR-specific utilities
├── Prefabs/
│   ├── Guns/            # Gun prefabs with MuzzlePoint setup
│   ├── UI/              # World-space UI elements
│   └── Duck.prefab      # Pooled duck prefab
└── Scenes/
    └── MainScene.unity  # Primary game scene
```

## Credits

See [CREDITS.md](CREDITS.md) for asset attributions.
