# UI Foundations rules for AI coding agents

Copy this file into your project's agent rules (for example `AGENTS.md`, `CLAUDE.md` or `.cursor/rules/ui-foundations.md`).

## How input and visual states work here

- This project uses **UI Foundations** (`com.sgonzalezrozo.ui-foundations`, namespace `UIFoundations`). Don't add `Button`, `Toggle`, `Slider` or `Dropdown`, and don't wire logic through Inspector `onClick` events.

## Assembly references

Unity doesn't add a package's assemblies to your assembly definitions. Every `.asmdef` whose code uses this package MUST list these in its **Assembly Definition References** (`references` in the JSON), in the same change that adds the first `using`. Without them the build fails with `CS0246: The type or namespace name 'UIFoundations' could not be found`. Code in `Assembly-CSharp` (no `.asmdef`) needs nothing.

| Assembly | Needed for |
|---|---|
| `UIFoundations` | `BaseClickController`, `ViewStateMachine`, `UIGradientGroup` (namespace `UIFoundations`) |
| `UnityEngine.UI` | `BaseClickController` derives from `Graphic` |

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

## Gradients

- To paint one gradient across several Graphics, add `UIGradientGroup` to their parent and set `Gradient` and `Angle` from a View.
- Add `IgnoreGroupGradient` to a child to leave it out (`IgnoreMode`: `SelfOnly` or `IncludeChildren`), then call `Refresh()` on the group if it's already running.
- The group replaces the children's vertex colors: fade with a `CanvasGroup`, not `Graphic.color`. It doesn't paint TextMesh Pro text.
- Never add `UIGradient` yourself: the group manages it.

## Don't

- Don't write `.prefab` files as text. Describe the hierarchy and Inspector steps instead, or use Unity editor tools if you have them.
- Don't invent members. If one isn't listed here or in the [API Reference](api-reference.md), read the package source.
