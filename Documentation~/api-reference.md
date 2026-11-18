# API Reference

[← Back to README](../README.md)

- [BaseClickController](#baseclickcontroller)
- [ViewStateMachine](#viewstatemachine)
- [Requirements](#requirements)

---

## BaseClickController

Inherit from `BaseClickController` and implement `OnClick()`:

```csharp
using UIFoundations;

public class BuyController : BaseClickController
{
    internal ShopModel model;

    protected override void OnClick()
    {
        model.Buy();
    }
}
```

- **It's a transparent `Graphic`,** so give it a `RectTransform` that covers the clickable area.
- **`Interactable`:** set it to `false` to disable clicks. It also turns off raycasts.
- **`InitializeController()`:** override it to cache what the Controller needs, such as a Cell Model. Return `false` if the Controller can't start; it then logs an error and disables itself.
- **`OnDestroy`:** `BaseClickController` derives from `Graphic`, so if you clean up in `OnDestroy`, override it and call `base.OnDestroy()`.

## ViewStateMachine

1. Add `ViewStateMachine` to a parent object.
2. Add one child per state, and add them to the list in the Inspector. Each state needs a `CanvasGroup`.
3. Set the state from a View:

```csharp
using UIFoundations;
using UnityEngine;

public class PurchaseStateView : MonoBehaviour
{
    [SerializeField] private ViewStateMachine states;

    public void ShowLocked()
    {
        states.SetState("Locked");
    }
}
```

| Member | Purpose |
|---|---|
| `SetState(name)`, `SetState(index)` | Changes state with the default transition |
| `SetState(name, transition)` | Changes state with a specific `IStateMachineTransition` |
| `SetStateInstant(name)` | Changes state with no transition |
| `IsInState(name)` | Checks the current state |
| `CurrentStateName`, `CurrentStateIndex` | Reads the current state |
| `IsTransitioning` | Says whether a transition is running |
| `OnStateChanged` | Runs after the state changes |

Name each state after its enum value, so code can pass `state.ToString()`.

## Requirements

| Requirement | Supported |
|---|---|
| Unity | Tested on Unity 6 (`6000.3.16f1`). The package declares `2022.3` as its minimum, but older versions aren't tested yet. |
| Scripting backend | Not verified yet |
| Platforms | Not verified yet |
| UI framework | uGUI only; UI Toolkit isn't supported |
| Dependencies | uGUI |
