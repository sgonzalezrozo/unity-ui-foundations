# UI Foundations rules for AI coding agents

Copy this file into your project's agent rules (for example `AGENTS.md`, `CLAUDE.md` or `.cursor/rules/ui-foundations.md`).

## How input and visual states work here

- This project uses **UI Foundations** (`com.sgonzalezrozo.ui-foundations`, namespace `UIFoundations`). Don't add `Button`, `Toggle`, `Slider` or `Dropdown`, and don't wire logic through Inspector `onClick` events.

## Clicks

```csharp
using UIFoundations;

public class BuyController : BaseClickController
{
    protected override void OnClick()
    {
        // Call a command on a Model. Don't change visuals here.
    }
}
```

- `BaseClickController` is a transparent `Graphic`, so its `RectTransform` is the clickable area. Don't add an `Image` for raycasts.
- Disable clicks with `Interactable = false`. It also turns off raycasts.
- Override `InitializeController()` to cache references. Return `false` if the Controller can't start: it logs an error and disables itself.
- It derives from `Graphic`. To clean up, write `protected override void OnDestroy()` and call `base.OnDestroy()`.

## Visual states

- `ViewStateMachine` shows one child state at a time. Each state needs a `CanvasGroup`.
- Name each state after its enum value, and call `SetState(state.ToString())` from a View.
- Members: `SetState(name)`, `SetState(index)`, `SetState(name, transition)`, `SetStateInstant(name)`, `IsInState(name)`, `CurrentStateName`, `CurrentStateIndex`, `IsTransitioning`, `OnStateChanged`.

## Don't

- Don't write `.prefab` files as text. Describe the hierarchy and Inspector steps instead, or use Unity editor tools if you have them.
- Don't invent members. If one isn't listed here or in the README, read the package source.
