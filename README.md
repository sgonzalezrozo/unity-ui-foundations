# UI Foundations

**Small uGUI building blocks for strict MVC: click Controllers and view states.**

[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![Tested on Unity 6000.3](https://img.shields.io/badge/Unity-6000.3%20tested-black?logo=unity)](#compatibility)
[![Version 0.1.0](https://img.shields.io/badge/version-0.1.0-blue.svg)](CHANGELOG.md)

UI Foundations replaces uGUI's `Button` and `Toggle` with two small parts that keep input and visuals apart: `BaseClickController` receives clicks, and `ViewStateMachine` shows one look at a time.

## Why I built it

`Button` and `Toggle` are a View and a Controller in one component. In a strict MVC project, they mix input with visuals, and the logic drifts into Inspector events.

I built and refined these parts on my own time in personal projects. They aren't used in production yet. In November 2026 I extracted them into this standalone package and documented them. Earlier history lives in a private repository.

## Features

- `BaseClickController`: an invisible raycast target that calls `OnClick()`
- An `Interactable` switch that also turns off raycasts
- `ViewStateMachine`: one active state from a list, chosen by name or index
- Instant and fade transitions
- A custom Inspector for both components

## Compatibility

| Requirement | Supported |
|---|---|
| Unity | Tested on Unity 6 (`6000.3.16f1`). The package declares `2022.3` as its minimum, but older versions aren't tested yet. |
| Scripting backend | Not verified yet |
| Platforms | Not verified yet |
| Dependencies | uGUI |

## Installation

Git must be installed on your machine.

1. In Unity, open **Window > Package Manager**.
2. Select **+ > Add package from git URL**.
3. Enter:

```text
https://github.com/sgonzalezrozo/unity-ui-foundations.git
```

## Quick start

### Handle a click

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

The component is a transparent `Graphic`, so give it a `RectTransform` that covers the clickable area. Override `InitializeController()` and return `false` if the Controller can't start. It then logs an error and disables itself.

### Switch between looks

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

## Use with coding agents

[Unity Architecture Agents](https://github.com/sgonzalezrozo/unity-architecture-agents) works with or without this package. Its rules use Unity's own `IPointerClickHandler` by default.

An agent reads `Packages/manifest.json`. If it lists `com.sgonzalezrozo.ui-foundations`, the agent uses this package as follows:

| Need | Use |
|---|---|
| A click Controller | Subclass `BaseClickController` and override `OnClick()` |
| Disable clicks | Set `Interactable` to `false` |
| Cache a Cell Model | Override `InitializeController()` and return `true` |
| Mutually exclusive looks | `ViewStateMachine` and `SetState(stateName)` |

Name each state after its enum value. If a Controller cleans up in `OnDestroy`, override it and call `base.OnDestroy()`, because `BaseClickController` derives from `Graphic`.

## Project status

This is a pre-release package at version `0.1.0`. Its API and behavior may change before `1.0.0`. Not yet used in a shipped game. There's no automated test suite yet.

Until `1.0.0`, minor versions (`0.x.0`) can include breaking changes, and patch versions (`0.x.y`) don't. Breaking changes are marked **Breaking** in the [changelog](CHANGELOG.md).

## Contributing

Bug reports and pull requests are welcome. Please open an issue before starting a large change. See the [contributing guide](.github/CONTRIBUTING.md) and [security policy](.github/SECURITY.md).

## Related packages

| Package | Purpose |
|---|---|
| [Unity Architecture Agents](https://github.com/sgonzalezrozo/unity-architecture-agents) | Architecture rules for coding agents |
| [Fast IoC](https://github.com/sgonzalezrozo/unity-fast-ioc) | Code-generated dependency injection |
| [Screen System](https://github.com/sgonzalezrozo/unity-screen-system) | Declarative UI navigation |
| [UI Tweens](https://github.com/sgonzalezrozo/unity-ui-tweens) | ECS-based UI animation |
| [Recyclable Scroller](https://github.com/sgonzalezrozo/unity-recyclable-scroller) | Virtualized lists |
| [Enhanced Console](https://github.com/sgonzalezrozo/unity-enhanced-console) | Unity Editor log triage |

## License

Released under the [MIT License](LICENSE).
