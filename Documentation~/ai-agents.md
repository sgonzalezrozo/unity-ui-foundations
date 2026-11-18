# AI Coding Agents

[← Back to README](../README.md)

Copy [`agent-rules.md`](agent-rules.md) into your project's agent rules, for example `AGENTS.md`, `CLAUDE.md` or `.cursor/rules/ui-foundations.md`, so agents use the real API.

[Unity Architecture Agents](https://github.com/sgonzalezrozo/unity-architecture-agents) works with or without this package. Its rules use Unity's own `IPointerClickHandler` by default.

An agent reads `Packages/manifest.json`. If it lists `com.sgonzalezrozo.ui-foundations`, the agent uses this package as follows:

| Need | Use |
|---|---|
| A click Controller | Subclass `BaseClickController` and override `OnClick()` |
| Disable clicks | Set `Interactable` to `false` |
| Cache a Cell Model | Override `InitializeController()` and return `true` |
| Mutually exclusive looks | `ViewStateMachine` and `SetState(stateName)` |

Name each state after its enum value. If a Controller cleans up in `OnDestroy`, override it and call `base.OnDestroy()`, because `BaseClickController` derives from `Graphic`.

It hasn't been tested with agents beyond the author's own projects.
