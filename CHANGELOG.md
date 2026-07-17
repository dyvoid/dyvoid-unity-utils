# Changelog

All notable changes to this package are documented in this file.

## [Unreleased]

## [1.1.0] - 2026-07-17

### Added
- `ITraversalStrategy` and the built-in strategies (`SequentialStrategy`, `PingPongStrategy`, `RandomStrategy`) are now public, and `InfiniteIterator`, `LoopingIterator`, and `FilteredIterator` accept a custom strategy, so custom traversal orders can be plugged in.
- `WeightedRandomIterator<T>`: draws items with probability proportional to per-item weights; zero-weight items are never returned.
- `LoopingIterator<T>`: iterates a collection a fixed number of times, exposes `IsDone`/`LoopsCompleted`, and its enumerator terminates (safe with `foreach`/LINQ without `Take(n)`).
- `FilteredIterator<T>`: returns only items passing a predicate, with a non-throwing `TryNext(out item)` for when no match is an expected state.
- `CollectionIteratorBase<T>.GetEnumerator` is now virtual so finite subclasses can provide terminating enumerators.
- `ExponentialMomentumCalculator`: leaky-integrator momentum calculator where input fades out gradually with a configurable `halfLife`. Smoother and cheaper than the sliding-window variant.
- `SlidingWindowMomentumCalculator`: the original sliding-window momentum calculator, now as a concrete subclass. Only samples within the last `sampleTimeRange` seconds count.
- `ValueRange.Default`: static property returning the conventional `(0, 1)` range, replacing the old default parameter values that were silently ignored by struct zero-initialization.
- `MomentumCalculator` subclasses accept an optional `timeProvider` parameter (defaults to `Time.time`), useful for tests or running on unscaled time.

### Fixed
- **Player builds no longer fail to compile**: removed an unguarded `using UnityEditor;` from `RangedObservableFloat.cs` in the runtime assembly.
- `TimeScaler` no longer throws `NullReferenceException` when no keyboard device is present.
- `Cooldown` overflow correction no longer produces `NaN` when resetting with a zero or negative duration.
- `ValueRange` property drawer no longer leaks its label width into subsequent inspector fields.
- `RangedObservableFloat` property drawer uses editor line-height constants and only writes the slider value on actual changes.
- `ScenePopulator` clear now also finds inactive and duplicate root objects.

### Changed
- **Breaking**: `MomentumCalculator` is now abstract. `new MomentumCalculator(...)` no longer compiles — use `SlidingWindowMomentumCalculator` or `ExponentialMomentumCalculator` instead.
- **Breaking**: `ValueRange(float from, float to)` no longer has default parameter values. `new ValueRange()` always zero-initialized to `(0, 0)` regardless of those defaults (C# struct semantics); use the new `ValueRange.Default` for `(0, 1)`.
- **Breaking**: `Cooldown.Reset(min, max)` is renamed to `Cooldown.ResetRandom(min, max)`. The old overload could be selected accidentally through implicit int-to-float conversion (e.g. `Reset(5f, 0)`).
- **Breaking**: `RangedObservableFloat` default `Range` changed from `(0, 0)` to `(0, 1)` via `ValueRange.Default`. Code relying on the old zero-initialized default will see different clamping behavior.
- `TimeScaler` moved from the Editor assembly to the Runtime assembly so scenes using it no longer contain missing scripts in player builds. It is only active in the editor and development builds; in release builds the component is inert. The `Unity.InputSystem` assembly reference moved from the editor to the runtime asmdef accordingly.
- `ServiceLocator` now removes emptied registration stacks, so unused `(Type, name)` keys no longer accumulate.
- `ObservableValue` constructor changed from `[Optional] T value` to `T value = default` — functionally equivalent, removes the `System.Runtime.InteropServices` dependency.

### Documentation
- `FSM<TState>`: documented lifecycle and re-entrancy caveats (behaviours are gathered in `Start`, changing to the current state is a no-op, avoid `ChangeState` inside `OnEnter`/`OnExit`).
- `CombinedIterator`: corrected docs — `Next()` never advances to the next collection automatically; empty collections passed to the constructor are skipped.
- `ObservableValue.Subscribe`: documented that duplicate subscriptions are ignored.
- README: corrected the FSM wizard menu path (`Tools > FSM > Create FSM...`).

## [1.0.0]

Initial UPM package release: FSM, Observables, ServiceLocator, Iterators, ValueRange, Cooldown, MomentumCalculator, and editor tools (FSM Wizard, TimeScaler, ScenePopulator).
