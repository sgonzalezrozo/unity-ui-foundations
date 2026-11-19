using UnityEditor;
using UnityEngine;

namespace UIFoundations.Editor
{
    /// <summary>Links to the UI Foundations documentation: the README on GitHub.</summary>
    internal static class UIFoundationsDocumentation
    {
        internal const string Url = "https://github.com/sgonzalezrozo/unity-ui-foundations#readme";

        [MenuItem("Tools/UI Foundations/Documentation", priority = 100)]
        private static void OpenDocumentation() => Application.OpenURL(Url);
    }
}
