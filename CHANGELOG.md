# Changelog

All notable changes to this package are documented in this file.

## [Unreleased]

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
