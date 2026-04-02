# C# Scripts

## Code Style

- `private` fields use `camelCase`; public methods/types use `PascalCase`
- Inspector-exposed fields use `[SerializeField] private`, never `public` fields for Unity serialization
- One class per file; filename must match class name
- No namespaces — all classes are in the global namespace
- `System` usings before `UnityEngine` usings; remove unused usings
- **No abbreviated variable names** — write the full word every time. `skillController` not `sc`, `enemy` not `e`, `gameObject` not `go`. This applies to loop variables, temporaries, and all local variables.

## Architecture

### Player Sub-Controller Pattern

`PlayerController` is a thin coordinator — it calls explicit `Update*` and `Handle*` methods each frame on six sub-controllers:

| Sub-controller | Responsibility |
|---|---|
| `PlayerStateController` | HP, stamina, execution gauge; fires C# events for UI |
| `PlayerMovementController` | WASD movement, LeftShift dash, mouse-based rotation, teleport |
| `PlayerHitController` | Q/E parry & guard input; `IDamageable.Hit()` entry point |
| `PlayerSwordController` | 1 key — throws `SwordController` prefab toward cursor |
| `PlayerShurikenController` | F key — first press throws shuriken, second press teleports player to it |
| `PlayerExecutionController` | LMB when execution gauge is full — raycast-kills an enemy, teleports player to it |

### Weapon Hierarchy

```
Weapon (abstract)          — fire-rate throttle via TryUse() / Use()
└── GunWeapon (abstract)   — ammo, reload coroutine, fires Bullet prefab
    ├── HandGun
    └── MachineGun
SwordController            — thrown projectile with range limit; stopped by SwordHitController
ShurikenController         — forward-moving projectile used for teleport
```

`Weapon.TryUse()` enforces the `useDelay` cooldown. Subclasses implement `protected abstract void Use()`.

### Bullet & Damage System

- `Bullet` (abstract) moves forward each frame and self-destructs after `destroyTime`.
- `Bullet.OnTriggerEnter` calls `IDamageable.Hit(bullet)` on whatever it hits, **skipping enemies by tag** unless the bullet has been reflected (`isReflected = true`).
- `IDamageable` is the single damage interface (`Hit(Bullet)`).
- `PlayerHitController.Hit()` checks for parry (timing window + stamina) → guard (holding Q/E + stamina) → take damage.
- **Parry**: `ParryController` queues timestamps of Q/E presses; a parry is valid within `parryDuration` seconds. On success, the bullet is reflected toward the mouse cursor and the execution gauge increases.
- **Guard**: bullet is reflected at a random ±60° angle; stamina is consumed.
- `SwordHitController` implements `IDamageable` to simply destroy bullets that hit the flying sword.

### Enemy AI

`Enemy` uses a three-state FSM (`Idle → MoveToTarget → Attack`). It supports both NavMesh navigation and direct transform movement (`useNavMesh` toggle). Gun-armed enemies lead their shots by calculating the player's velocity before firing.

`EnemySpawner` manages a live list of enemies, spawns them with a random weapon from `weaponPrefabs` on an interval, and removes them from the list via `Enemy.OnDeath`.

### UI

`UIController` subscribes to `PlayerStateController` events (`OnHpChanged`, `OnExecutionGaugeChanged`, `OnStaminaChanged`) and scales bar GameObjects on the X axis (0–1 ratio).

### Camera

`CameraController` follows the player with `SmoothDamp` and offsets toward the mouse cursor position to extend the visible area in the aiming direction.

## Key Input Bindings

| Key | Action |
|---|---|
| WASD | Move |
| Left Shift | Dash (2× speed) |
| Q / E | Parry / Guard |
| F | Throw shuriken / Teleport to shuriken |
| 1 | Throw sword |
| LMB (when gauge full) | Execute enemy |
