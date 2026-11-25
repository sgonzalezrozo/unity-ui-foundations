# Changelog

All notable changes to this package are documented in this file.

## [Unreleased]

## [0.3.3] - 2026-11-25

### Fixed

- `BaseClickController` logs its initialization error only in Play mode, so editor previews don't log it.
- Gradient: the Editor picks up Graphics added below direct children (commit 3e91218).

### Documentation

- The `BaseClickController` signatures and `InitializeController` are in the API reference.

## [0.3.2] - 2026-11-19

### Documentation

- The README and `agent-rules.md` list the assemblies an assembly definition needs to use UI Foundations: `UIFoundations` and `UnityEngine.UI`.

## [0.3.1] - 2026-11-19

### Changed

- `package.json` lists the author's email, santiago.gonzalezrozo@outlook.com, next to the name and GitHub URL.

## [0.3.0] - 2026-11-19

### Fixed

- **UIGradientGroup** ignored the alpha of the Graphics it painted: a Graphic faded to alpha 0 still showed the gradient at full opacity. The gradient now sets the hue and multiplies its alpha by the Graphic's, in both the in-place and the sliced paths. Tests cover alpha 0, half alpha, a tinted Graphic and a gradient with alpha 0.

### Added

- A **Refresh** button in the `UIGradientGroup` Inspector, outside Play Mode.
- Hierarchy menu **UI Foundations > Click Controller…**: asks for a class name, writes a `BaseClickController` subclass and adds it on a new child of the selected object after the compile.
- Hierarchy menu **UI Foundations > View State Machine (On, Off)**: a `ViewStateMachine` with `On` and `Off` states already set up.
- **Tools > UI Foundations > Documentation** opens the README on GitHub.

### Documentation

- The README's feature list no longer lists the custom Inspectors or the agent rules file.

## [0.2.0] - 2026-11-19

### Added

- `UIGradientGroup` spreads one gradient across every `Graphic` under it, as if they were one shape, at any angle. Each child gets a hidden `UIGradient` mesh effect that colors its vertices by where they sit in the group's rectangle. `IgnoreGroupGradient` leaves a child out, alone or with its children.
- The gradient runs only when uGUI rebuilds a mesh: nothing runs per frame, and painting allocates no memory.
- EditMode tests for the gradient math and mesh painting, including a zero-allocation check, and a benchmark you run by hand (`GradientBenchmark`).

## [0.1.4] - 2026-11-18

### Documentation

- The README's description is plain text instead of a quote, and short enough for two lines.

## [0.1.3] - 2026-11-18

### Documentation

- The README is now a short overview with installation, a quickstart and links. Its content moved into guides in `Documentation~/`: API Reference, AI Coding Agents, FAQ and Project Status.
- Installation instructions no longer pin a release tag.

### Removed

- The welcome dialog shown once per project after the package is imported.

## [0.1.2] - 2026-11-17

### Added

- After the package is imported, a dialog links to the documentation, once per project.
- `documentationUrl` in `package.json`, so the Package Manager links to the documentation.

## [0.1.1] - 2026-11-10

### Documentation

- An FAQ, rules for AI coding agents in `Documentation~/agent-rules.md`, `llms.txt` and more package keywords.
- Documented how coding agents use this package.
- Removed links to packages that aren't published yet.
- How to pin a release.

## [0.1.0] - 2026-11-05

### Added

- `BaseClickController`, a click Controller base class that replaces uGUI `Button`.
- `ViewStateMachine`, with instant and fade transitions and a custom Inspector.
