<div align="center">

# Unity UI Foundations

<sub>DESIGNED & BUILT BY</sub><br>
**✦ [`SANTIAGO GONZÁLEZ ROZO (@sgonzalezrozo)`](https://github.com/sgonzalezrozo) ✦**

Small uGUI building blocks for strict MVC: click Controllers, view states and group gradients. An alternative to `Button` and `Toggle` that keeps input and visuals apart.

[![Unity](https://img.shields.io/badge/Unity-6000.3%20tested-black?style=for-the-badge&logo=unity)](Documentation~/api-reference.md#requirements)
[![Version](https://img.shields.io/badge/version-0.3.1-blue?style=for-the-badge)](CHANGELOG.md)
[![License](https://img.shields.io/badge/License-MIT-green?style=for-the-badge)](LICENSE)

[**Installation**](#installation) • [**Key Features**](#key-features) • [**Quickstart**](#quickstart) • [**API Reference**](Documentation~/api-reference.md) • [**AI Agents**](Documentation~/ai-agents.md) • [**FAQ**](Documentation~/faq.md)

<sub>[Project Status](Documentation~/project-status.md)</sub>

</div>

---

## Installation

1. Open **Window > Package Manager** in Unity.
2. Click **`+`** in the top-left corner and select **Add package from git URL...**
3. Paste the URL below and click **Add**:

```text
https://github.com/sgonzalezrozo/unity-ui-foundations.git
```

There's nothing to set up after installing.

**Assembly references.** If your code is in an assembly definition (`.asmdef`), add `UIFoundations` and `UnityEngine.UI` to its **Assembly Definition References**. Unity doesn't add them for you, and without them the build fails with `CS0246`.

---

## Key Features

* **[Click Controllers](Documentation~/api-reference.md#baseclickcontroller)**: `BaseClickController` is an invisible raycast target with an `OnClick()` override.
  Click logic stays in code, not in Inspector events, and an `Interactable` switch also turns off raycasts.
* **[View states](Documentation~/api-reference.md#viewstatemachine)**: `ViewStateMachine` shows one look at a time from a list, such as locked, available and owned.
  Choose a state by name or index, with instant or fade transitions.
* **[Group gradients](Documentation~/api-reference.md#uigradientgroup)**: `UIGradientGroup` paints one gradient across several Graphics, as if they were a single shape.
  Set the gradient and angle on the parent; children can opt out with `IgnoreGroupGradient`.

---

## Quickstart

Handle a click by subclassing `BaseClickController`, and switch looks with a `ViewStateMachine`:

```csharp
public class BuyController : BaseClickController
{
    internal ShopModel model;
    protected override void OnClick() => model.Buy();
}

states.SetState("Locked"); // on a ViewStateMachine with a child per state
```

<details>
<summary><strong>View full sample script</strong></summary>

```csharp
using UIFoundations;
using UnityEngine;

// A click Controller. Give it a RectTransform that covers the clickable area.
public class BuyController : BaseClickController
{
    internal ShopModel model;

    protected override void OnClick()
    {
        model.Buy();
    }
}

// A View that picks a look. Add ViewStateMachine to a parent object, one child
// per state (each with a CanvasGroup), and list the children in the Inspector.
public class PurchaseStateView : MonoBehaviour
{
    [SerializeField] private ViewStateMachine states;

    public void ShowLocked()
    {
        states.SetState("Locked");
    }
}
```

</details>

Every member is in the [API Reference](Documentation~/api-reference.md).
