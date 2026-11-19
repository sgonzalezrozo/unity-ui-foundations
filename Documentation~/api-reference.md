# API Reference

[← Back to README](../README.md)

- [BaseClickController](#baseclickcontroller)
- [ViewStateMachine](#viewstatemachine)
- [UIGradientGroup](#uigradientgroup)
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

## UIGradientGroup

1. Add `UIGradientGroup` to a parent object with a `RectTransform`.
2. Set its gradient and angle in the Inspector. 0° runs left to right, 90° bottom to top, across the parent's rectangle.
3. Every `Graphic` below it gets a hidden `UIGradient`, so a background, a border and an icon read as one shape.

To leave a child out, add `IgnoreGroupGradient` to it. Its `IgnoreMode` is `SelfOnly` or `IncludeChildren`. A Graphic under two groups belongs to the nearest one.

```csharp
using UIFoundations;
using UnityEngine;

public class RarityView : MonoBehaviour
{
    [SerializeField] private UIGradientGroup background;

    public void Render(Gradient rarityGradient)
    {
        background.Gradient = rarityGradient;
    }
}
```

| Member | Purpose |
|---|---|
| `Gradient` | The gradient spread across the children. Setting it repaints them. |
| `Angle` | Direction in degrees, 0–360. Setting it repaints the children. |
| `Refresh()` | Finds the Graphics again and repaints them. Call it after changing `IgnoreGroupGradient` or moving a child without resizing it. |

### How it works and what it costs

- **Nothing runs per frame.** A child is painted when uGUI rebuilds its mesh. Changing the gradient, the angle or the group's size only marks the children for rebuild.
- **No allocations while painting.** The gradient is read into a 256-color table when it changes, so a vertex costs one lookup.
- **Extra keys cost more.** A gradient with only its two end keys recolors vertices in place. A key inside a Graphic slices that Graphic's triangles so the key's color shows exactly, which roughly doubles its rebuild cost.
- **It replaces vertex colors.** The Graphic's own `color` is ignored while it's in a group, so fade with a `CanvasGroup` instead.
- **Moving a child** without changing its size doesn't rebuild its mesh, so its gradient stays where it was until the next rebuild or `Refresh()`.
- **TextMesh Pro** text isn't painted: TMP builds its mesh without uGUI mesh effects.
- **Edit `Gradient` by assigning it.** If you change the keys of the same `Gradient` object in code, assign it again so the group reads the new keys.

## Requirements

| Requirement | Supported |
|---|---|
| Unity | Tested on Unity 6 (`6000.3.16f1`). The package declares `2022.3` as its minimum, but older versions aren't tested yet. |
| Scripting backend | Not verified yet |
| Platforms | Not verified yet |
| UI framework | uGUI only; UI Toolkit isn't supported |
| Dependencies | uGUI |
