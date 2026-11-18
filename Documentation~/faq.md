# FAQ

[← Back to README](../README.md)

- [Why not use Button and Toggle?](#why-not-use-button-and-toggle)
- [How do I handle clicks in Unity without a Button component?](#how-do-i-handle-clicks-in-unity-without-a-button-component)
- [How do I make a custom button in Unity?](#how-do-i-make-a-custom-button-in-unity)
- [How do I switch a UI element between states like locked, available and owned?](#how-do-i-switch-a-ui-element-between-states-like-locked-available-and-owned)
- [Does it work with Unity 6?](#does-it-work-with-unity-6)
- [Does it work with UI Toolkit?](#does-it-work-with-ui-toolkit)
- [Is it production-ready? Does anyone use it?](#is-it-production-ready-does-anyone-use-it)
- [Is it free for commercial use?](#is-it-free-for-commercial-use)

---

### Why not use Button and Toggle?

`Button` and `Toggle` are a View and a Controller in one component. In a strict MVC project, they mix input with visuals, and the logic drifts into Inspector events.

I built and refined these parts on my own time in personal projects. In November 2026 I extracted them into this standalone package and documented them. Earlier history lives in a private repository.

### How do I handle clicks in Unity without a Button component?

Implement `IPointerClickHandler` on a MonoBehaviour, or subclass `BaseClickController` and override `OnClick()`. Either way, the click logic stays in code instead of Inspector events. See [BaseClickController](api-reference.md#baseclickcontroller).

### How do I make a custom button in Unity?

Subclass `BaseClickController`, override `OnClick()`, and put the visuals on separate components driven by a View. Without this package, implement `IPointerClickHandler` on a MonoBehaviour with a transparent `Image`.

### How do I switch a UI element between states like locked, available and owned?

Add `ViewStateMachine` with one child per state, then call `SetState("Locked")`. See [ViewStateMachine](api-reference.md#viewstatemachine).

### Does it work with Unity 6?

Yes. It's tested on Unity `6000.3`. The package declares `2022.3` as its minimum, but older versions aren't tested yet.

### Does it work with UI Toolkit?

No. It's built on uGUI.

### Is it production-ready? Does anyone use it?

Not yet. UI Foundations is a pre-release package (`0.x`) by a single author, with no automated tests yet. It isn't used in a shipped game, and it has no other users yet. Bug reports are very welcome. See [Project Status](project-status.md).

### Is it free for commercial use?

Yes. It's MIT licensed.
