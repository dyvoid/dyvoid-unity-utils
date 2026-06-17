# dyvoid Utility

A collection of Unity utilities as a UPM package.

**Package name:** `com.dyvoid.utility`  
**Namespace:** `dyvoid.Utility`  
**Unity:** 2023.1+  
**Dependency:** [Input System](https://docs.unity3d.com/Packages/com.unity.inputsystem@latest)

---

## Installation

Copy the package folder into your project's `Packages/` directory (name it `com.dyvoid.utility`), or add it to `Packages/manifest.json`:

```json
"com.dyvoid.utility": "file:/absolute/path/to/package"
```

---

## Modules

### FSM

A component-based finite state machine. The FSM and its state behaviours all live on the same `GameObject`.

```csharp
public enum PlayerState { Idle, Walk, Jump }

public class PlayerFSM : FSM<PlayerState> { }

public class IdleState : FSMStateBehaviour<PlayerState>
{
    public override PlayerState State => PlayerState.Idle;

    public override void OnEnter(PlayerState prevState) { }
    public override void OnExit(PlayerState nextState) { }
    public override void OnUpdateState() { }
}
```

```csharp
// Trigger a transition
fsm.ChangeState(PlayerState.Walk);

// Read state
fsm.CurrentState;
fsm.PreviousState;

// Subscribe to changes
fsm.StateObservable.Subscribe((newState, oldState) => { });
```

**Editor tool:** `FSMWizard` generates FSM and state behaviour boilerplate from the Unity editor (`Tools > FSM Wizard`).

---

### Observable

A reactive value container that notifies subscribers on change.

```csharp
var health = new ObservableValue<int>(100);

health.Subscribe((newVal, oldVal) => Debug.Log($"HP: {newVal}"));

health.Value = 80; // fires callback
```

**Derived observables:**

```csharp
// Map: transform value into a new observable
var isAlive = health.Map((val, _) => val > 0);

// Merge: combine two observables
var canInteract = ObservableValue<bool>.Merge(isInRange, hasItem, (a, b) => a && b);
```

**Typed variants:** `ObservableFloat`, `RangedObservableFloat` (value clamped to a `ValueRange`).

---

### ServiceLocator

A static service registry with stacking support. Later registrations shadow earlier ones; unregistering reveals the previous.

```csharp
// Register
ServiceLocator.Register<IInputService>(new InputService());

// Retrieve
var input = ServiceLocator.Get<IInputService>();

// Named registration
ServiceLocator.Register(cameraService, "main");
ServiceLocator.Get<ICameraService>("main");

// Unregister
ServiceLocator.Unregister(inputService);
```

Use `ServiceBootstrap` to bind services to a `MonoBehaviour` lifetime — bindings are automatically removed on `OnDestroy`. Toggle `isGlobal` to persist across scene loads.

---

### Iterators

Infinite iterators over `IList<T>` with pluggable traversal strategies.

```csharp
var iter = new InfiniteIterator<Sprite>(sprites, IterationMode.PingPong);
Sprite next = iter.Next();

// Also IEnumerable — use with LINQ
var sample = iter.Take(5).ToList();
```

| Mode | Pattern |
|---|---|
| `Sequential` | 0, 1, 2, 0, 1, 2, … |
| `PingPong` | 0, 1, 2, 1, 0, 1, … |
| `Random` | every index once per cycle, no consecutive repeats |

**`CombinedIterator`** — chains multiple collections, switch with `NextCollection()`.  
**`GroupedIterator`** — returns items in batches via `NextGroup(n)`.  
**`TimedIterator`** — advances based on accumulated time, call `UpdateTime(deltaTime)` each frame.

---

### ValueRange

A serializable float range with utility methods.

```csharp
var range = new ValueRange(0f, 10f);

range.GetValueAt(0.5f);       // 5 — lerp by percentage
range.GetPercentageOf(7.5f);  // 0.75
range.Clamp(15f);             // 10
range.Contains(5f);           // true
range.GetRandomValue();       // random float within range
range.Remap(5f, new ValueRange(0f, 1f)); // 0.5

// Operators
var shifted = range + 2f;     // (2, 12)
var scaled  = range * 0.5f;   // (2.5, 7.5)
```

Shows as a paired `From` / `To` field in the Inspector.

---

### Cooldown

A simple frame-rate-independent timeout for use in `Update` loops.

```csharp
var cd = new Cooldown(2f);

void Update()
{
    if (cd.UpdateTime(Time.deltaTime))
    {
        // expired — do the thing
        cd.Reset();
    }
}
```

---

### MomentumCalculator

Tracks the rate of accumulated input over a rolling time window.

```csharp
var momentum = new MomentumCalculator(momentumThreshold: 5f, sampleTimeRange: 1f);

// Call with input values each frame/event
bool thresholdExceeded = momentum.AddSample(inputValue);

momentum.Momentum;          // current momentum value
momentum.ExceedsThreshold;  // true if >= threshold
momentum.Clear();
```

---

## Tests

Tests use the Unity Test Runner (EditMode). Open via `Window > General > Test Runner` and run all EditMode tests.
