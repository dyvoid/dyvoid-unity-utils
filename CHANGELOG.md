# Changelog

All notable changes to this package are documented in this file.

## [Unreleased]

### Fixed
- **Player builds no longer fail to compile**: removed an unguarded `using UnityEditor;` from `RangedObservableFloat.cs` in the runtime assembly.
- `TimeScaler` no longer throws `NullReferenceException` when no keyboard device is present.
- `Cooldown` overflow correction no longer produces `NaN` when resetting with a zero or negative duration.
- `ValueRange` property drawer no longer leaks its label width into subsequent inspector fields.
- `RangedObservableFloat` property drawer uses editor line-height constants and only writes the slider value on actual changes.
- `ScenePopulator` clear now also finds inactive and duplicate root objects.

### Changed
- **Breaking**: `ValueRange(float from, float to)` no longer has default parameter values. `new ValueRange()` always zero-initialized to `(0, 0)` regardless of those defaults (C# struct semantics); use the new `ValueRange.Default` for `(0, 1)`.
- **Breaking**: `Cooldown.Reset(min, max)` is renamed to `Cooldown.ResetRandom(min, max)`. The old overload could be selected accidentally through implicit int-to-float conversion (e.g. `Reset(5f, 0)`).
- `TimeScaler` moved from the Editor assembly to the Runtime assembly so scenes using it no longer contain missing scripts in player builds. It is only active in the editor and development builds; in release builds the component is inert. The `Unity.InputSystem` assembly reference moved from the editor to the runtime asmdef accordingly.
- `ServiceLocator` now removes emptied registration stacks, so unused `(Type, name)` keys no longer accumulate.

### Documentation
- `FSM<TState>`: documented lifecycle and re-entrancy caveats (behaviours are gathered in `Start`, changing to the current state is a no-op, avoid `ChangeState` inside `OnEnter`/`OnExit`).
- `CombinedIterator`: corrected docs — `Next()` never advances to the next collection automatically; empty collections passed to the constructor are skipped.
- `ObservableValue.Subscribe`: documented that duplicate subscriptions are ignored.
- README: corrected the FSM wizard menu path (`Tools > FSM > Create FSM...`).

## [1.0.0]

Initial UPM package release: FSM, Observables, ServiceLocator, Iterators, ValueRange, Cooldown, MomentumCalculator, and editor tools (FSM Wizard, TimeScaler, ScenePopulator).
