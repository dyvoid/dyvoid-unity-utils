# Utility Suggestions

Proposals for new utilities to add to `com.dyvoid.utility`, based on the current contents of the package (FSM, Observable, ServiceLocator, Iterators, ValueRange, Cooldown, MomentumCalculator, and the TimeScaler / ScenePopulator / FSMWizard editor tools).

All suggestions follow the existing package philosophy:

- Small, plain-C# classes driven manually from an `Update` loop (`UpdateTime(deltaTime)` idiom) — no coroutines, no hidden statics.
- Testable in EditMode with the Unity Test Runner.
- Editor and debug tools kept self-contained, in the style of `TimeScaler` and `ScenePopulator`.

**Impact** is rated ★☆☆ (nice to have) to ★★★ (almost every project needs it), weighed against implementation effort.

---

## 1. Rounding out existing modules

These complete patterns the package has already started, so they are the cheapest wins.

### 1.1 `ObservableList<T>` — Impact: ★★★

The Observable module handles single values well, but there is no reactive collection. Inventories, lobbies, quest logs, and UI lists all need "notify me when an item is added / removed / replaced".

```csharp
var inventory = new ObservableList<Item>();

inventory.ItemAdded.Subscribe((item, index) => SpawnSlot(item));
inventory.ItemRemoved.Subscribe((item, index) => RemoveSlot(index));
inventory.ItemReplaced.Subscribe((newItem, oldItem, index) => RefreshSlot(index));

inventory.Add(sword);     // fires ItemAdded
inventory[0] = shield;    // fires ItemReplaced
inventory.RemoveAt(0);    // fires ItemRemoved
```

**Why it fits:** direct extension of `ObservableValue<T>`; reuses the existing subscribe/unsubscribe conventions so users learn nothing new.

### 1.2 `ValueRangeInt` + `RangedObservableInt` — Impact: ★★☆

`ValueRange` and `RangedObservableFloat` exist only for floats. Health, ammo, currency, and level counters are integers, and casting back and forth is noise.

```csharp
var healthRange = new ValueRangeInt(0, 100);

var health = new RangedObservableInt(healthRange, startValue: 100);
health.Subscribe((newVal, _) => healthBar.fillAmount = healthRange.GetPercentageOf(newVal));

health.Value -= 250; // clamped to 0, single callback
```

**Why it fits:** the API is a 1:1 mirror of the float variants (`GetValueAt`, `GetPercentageOf`, `Clamp`, `Contains`, `GetRandomValue`, operators), including the paired `From`/`To` Inspector drawer.

### 1.3 `Stopwatch` — a counting-up counterpart to `Cooldown` — Impact: ★★☆

`Cooldown` counts down toward expiry; the other half of timing needs is measuring how long something has been going on (combo timers, hold-to-charge, session length).

```csharp
var chargeTime = new Stopwatch();

void Update()
{
    if (fireButton.isPressed)
    {
        chargeTime.UpdateTime(Time.deltaTime);
    }
    else if (chargeTime.Elapsed > 0f)
    {
        Fire(power: chargeRange.GetPercentageOf(chargeTime.Elapsed));
        chargeTime.Reset();
    }
}
```

**Why it fits:** identical `UpdateTime(deltaTime)` / `Reset()` idiom as `Cooldown`; trivially EditMode-testable.

### 1.4 FSM transition rules (guards + transition table) — Impact: ★★☆

`FSM<T>` currently allows any `ChangeState` call. A declarative, opt-in transition table with guard conditions catches invalid transitions at the source and documents the state graph in code.

```csharp
fsm.AddTransition(PlayerState.Idle, PlayerState.Walk);
fsm.AddTransition(PlayerState.Walk, PlayerState.Jump, guard: () => isGrounded);
fsm.AddAnyTransition(PlayerState.Dead, guard: () => health.Value <= 0);

fsm.ChangeState(PlayerState.Jump); // ignored (or logs) unless Walk + grounded
```

**Why it fits:** layers on top of the existing `ChangeState` without breaking the current permissive API — no rules registered means everything stays allowed. `FSMWizard` could optionally generate the table.

---

## 2. New runtime utilities

### 2.1 `ComponentPool<T>` / `GameObjectPool` — Impact: ★★★

The biggest gap for a general-purpose Unity utility package. Unity ships `UnityEngine.Pool.ObjectPool<T>`, but what projects actually reach for is a prefab-aware pool that handles instantiation, parenting, activation, and a reset hook.

```csharp
public class Bullet : MonoBehaviour, IPoolable
{
    public void OnSpawned()  { trail.Clear(); }
    public void OnDespawned() { rb.linearVelocity = Vector3.zero; }
}

var pool = new ComponentPool<Bullet>(bulletPrefab, parent: poolRoot, prewarm: 32);

Bullet b = pool.Get(muzzle.position, muzzle.rotation);
// ... later
pool.Release(b);
```

**Impact rationale:** virtually every project with projectiles, VFX, or spawned enemies re-implements this; eliminating `Instantiate`/`Destroy` churn is also a real performance win on every platform.

### 2.2 `Easing` + `ValueAnimator` — Impact: ★★★

A static easing library (quad/cubic/quart/sine/expo/elastic/back/bounce, in/out/in-out) plus a small animator driven by `UpdateTime(deltaTime)`. Covers the 80% tweening case with zero dependencies — and pairs directly with `ValueRange`.

```csharp
// Static easing — pairs with ValueRange.GetValueAt
float y = heightRange.GetValueAt(Easing.OutBounce(t));

// Manually-driven animator, same idiom as Cooldown
var fade = new ValueAnimator(from: 0f, to: 1f, duration: 0.4f, Easing.InOutQuad);

void Update()
{
    if (fade.UpdateTime(Time.deltaTime))  // returns true while running
    {
        canvasGroup.alpha = fade.Value;
    }
}
```

**Impact rationale:** the alternative is importing DOTween/LeanTween for a single fade. A dependency-free animator that follows the package's manual-update idiom is unique and highly reusable. `Easing` is also pure math — perfect EditMode test coverage.

### 2.3 `SmoothedFloat` / `DampedValue` — Impact: ★★☆

A wrapper around exponential smoothing / `Mathf.SmoothDamp` with `Target` and `Current`, updated per frame. `MomentumCalculator` measures rates; this smooths them. Ideal for camera zoom, UI bars, audio parameters.

```csharp
var zoom = new SmoothedFloat(initial: 10f, smoothTime: 0.25f);

void Update()
{
    zoom.Target = aiming ? 4f : 10f;
    cam.orthographicSize = zoom.UpdateTime(Time.deltaTime);
}
```

**Why it fits:** could optionally expose its `Current` as an `ObservableFloat`, tying the Observable and timing modules together.

### 2.4 `Debouncer` and `Throttle` — Impact: ★★☆

Cousins of `Cooldown` and `MomentumCalculator`. Debounce = "fire once input has been quiet for X seconds" (autosave, search-as-you-type). Throttle = "at most once per X seconds" (footstep sounds, network sync).

```csharp
var autosave = new Debouncer(delay: 2f, onFire: SaveGame);

void OnSettingChanged() => autosave.Trigger();
void Update() => autosave.UpdateTime(Time.deltaTime);
```

```csharp
var footstep = new Throttle(interval: 0.3f);

void OnFootRaycastHit()
{
    if (footstep.TryFire()) PlayFootstepSound();
}
```

**Impact rationale:** tiny classes (each well under 50 lines), broad applicability, and they slot into the existing timing family naturally.

### 2.5 `WeightedRandom<T>` — Impact: ★★☆

`RandomStrategy` already implements no-repeat random cycling; a weighted picker is its natural sibling for loot tables, spawn variation, and dialogue barks.

```csharp
var loot = new WeightedRandom<ItemDef>()
    .Add(commonSword, weight: 70f)
    .Add(rareShield,  weight: 25f)
    .Add(epicStaff,   weight: 5f);

ItemDef drop = loot.Pick();
ItemDef dropNoRepeat = loot.PickExcluding(lastDrop);
```

**Why it fits:** pure C#, EditMode-testable (seedable RNG), complements both the Iterators module and `ValueRange.GetRandomValue()`.

### 2.6 `SerializableDictionary<TKey, TValue>` — Impact: ★★★

One of the most re-implemented utilities in the Unity ecosystem. A paired-list backing with `ISerializationCallbackReceiver` and a property drawer, matching how `ValueRange` already ships a custom Inspector presentation.

```csharp
[SerializeField]
private SerializableDictionary<SurfaceType, AudioClip> footstepSounds;

void PlayFootstep(SurfaceType surface)
{
    if (footstepSounds.TryGetValue(surface, out var clip))
        source.PlayOneShot(clip);
}
```

**Impact rationale:** unlocks Inspector-editable mappings (enum → asset, string → prefab) that otherwise force parallel-array hacks or ScriptableObject boilerplate in every project.

### 2.7 Typed `EventBus` — Impact: ★★☆

Fills the gap between `ObservableValue` (state you own) and `ServiceLocator` (services you fetch): fire-and-forget domain events with no reference to the sender.

```csharp
public readonly struct PlayerDied
{
    public readonly Vector3 Position;
    public PlayerDied(Vector3 position) => Position = position;
}

// Subscribe (an EventBinding bound to a MonoBehaviour lifetime,
// mirroring the ServiceBootstrap pattern)
EventBus<PlayerDied>.Subscribe(OnPlayerDied);

// Publish from anywhere
EventBus<PlayerDied>.Publish(new PlayerDied(transform.position));
```

**Why it fits:** the lifetime-binding problem is already solved once in `ServiceBootstrap`; the same auto-unsubscribe-on-destroy pattern applies here. Caution: like any global bus it can be overused, so the README should position it for cross-system notifications only.

---

## 3. Editor & debug tools

`TimeScaler` and `ScenePopulator` establish an appetite for small dev-workflow tools. All of these follow the same mold.

### 3.1 Runtime stats overlay — Impact: ★★☆

FPS, frame time (ms), and allocated memory in an `OnGUI` corner label, toggleable by key. A direct sibling of `TimeScaler` — same `#if UNITY_EDITOR` guard, same Input System key handling, and it can reuse the overlay-fade-via-`Cooldown` trick `TimeScaler` already implements (worth extracting into a shared `DebugOverlay` helper).

```csharp
// Drop the StatsOverlay component on any GameObject.
// Toggle with Shift+F (configurable), just like TimeScaler's key scheme.
```

### 3.2 Screenshot hotkey tool — Impact: ★★☆

Capture a supersampled screenshot to a timestamped file on a hotkey. About 30 lines, and it earns its keep in every demo, store-page, and bug-report situation.

```csharp
[SerializeField] private Key captureKey = Key.F12;
[SerializeField, Range(1, 4)] private int superSize = 2;

// -> Screenshots/2026-07-12_14-33-02_x2.png
```

### 3.3 Missing-reference finder — Impact: ★★☆

An `EditorWindow` that scans the open scene (and optionally the project) for missing scripts and null serialized object fields, with click-to-select results. Fits the `ScenePopulator` EditorWindow mold exactly.

```
Tools > Find Missing References
  ├─ Scan Scene
  ├─ Scan Prefabs in Project
  └─ Results: [Enemy_03] missing script  [HUD/HealthBar] 'targetCamera' is None
```

### 3.4 Play-from-bootstrap-scene toggle — Impact: ★☆☆

A one-click menu toggle that sets `EditorSceneManager.playModeStartScene`, so pressing Play always starts from the init scene. Pairs naturally with `ServiceBootstrap`'s global-services pattern: services registered in the bootstrap scene are guaranteed to exist no matter which scene you were editing.

```
Tools > Play From Bootstrap Scene  [toggle, persisted in EditorPrefs]
```

---

## 4. Deliberately out of scope

- **Singleton `MonoBehaviour` base class** — `ServiceLocator` + `ServiceBootstrap` already cover this better; shipping a singleton base would give users two competing patterns for the same problem.
- **Full tween library, save system, addressables wrapper** — each is large enough to deserve its own package and would dilute the "small utilities" identity of `com.dyvoid.utility`.

---

## 5. Suggested priority

Ranked by impact-to-effort:

| # | Utility | Impact | Effort | Notes |
|---|---|---|---|---|
| 1 | `ComponentPool<T>` | ★★★ | Medium | Most-requested utility category in Unity |
| 2 | `Easing` + `ValueAnimator` | ★★★ | Medium | Pure math + one small class; pairs with `ValueRange` |
| 3 | `SerializableDictionary` | ★★★ | Medium | Needs a solid property drawer |
| 4 | `ObservableList<T>` | ★★★ | Low | Natural Observable extension |
| 5 | `Debouncer` / `Throttle` | ★★☆ | Low | Tiny, joins the Cooldown family |
| 6 | `Stopwatch` | ★★☆ | Low | Mirrors Cooldown |
| 7 | `WeightedRandom<T>` | ★★☆ | Low | Sibling of RandomStrategy |
| 8 | `ValueRangeInt` variants | ★★☆ | Low | Mechanical mirror of float versions |
| 9 | FSM transition rules | ★★☆ | Medium | Opt-in, non-breaking |
| 10 | `SmoothedFloat` | ★★☆ | Low | SmoothDamp wrapper |
| 11 | `EventBus` | ★★☆ | Medium | Document usage boundaries |
| 12 | Stats overlay | ★★☆ | Low | Shares code with TimeScaler |
| 13 | Screenshot tool | ★★☆ | Low | ~30 lines |
| 14 | Missing-reference finder | ★★☆ | Medium | EditorWindow scan |
| 15 | Bootstrap-scene toggle | ★☆☆ | Low | One menu item |
