using System;
using System.Collections.Generic;
using System.Diagnostics;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace UIFoundations.Tests
{
    /// <summary>
    /// Measures what a gradient costs on uGUI rebuilds. Timings depend on the machine, so it only runs by hand;
    /// compare runs on the same machine. Results go to the Console.
    /// </summary>
    [Explicit("Benchmark: timings depend on the machine. Run it by hand and compare runs on the same machine.")]
    public class GradientBenchmark
    {
        private const int GRAPHIC_COUNT = 50;
        private const int WARMUP_ITERATIONS = 10;
        private const int MEASURED_ITERATIONS = 200;
        private const int SPRITE_SIZE = 32;
        private const int SPRITE_BORDER = 8;
        private const float GROUP_WIDTH = 1000f;
        private const float GROUP_HEIGHT = 200f;

        private GameObject root;
        private Texture2D texture;
        private Sprite slicedSprite;
        private UIGradientGroup group;
        private readonly List<Image> images = new();

        [TearDown]
        public void TearDown()
        {
            images.Clear();
            Object.DestroyImmediate(root);
            Object.DestroyImmediate(slicedSprite);
            Object.DestroyImmediate(texture);
        }

        [TestCase(2)]
        [TestCase(4)]
        public void RefreshGroup(int keyCount)
        {
            Build(withGradient: true, keyCount);
            Measure($"Refresh group, {keyCount} keys", RefreshGroupAndRebuild);
        }

        [TestCase(2)]
        [TestCase(4)]
        public void RebuildOneChild(int keyCount)
        {
            Build(withGradient: true, keyCount);
            Measure($"Rebuild one child, {keyCount} keys", RebuildFirstChild);
        }

        [TestCase(2)]
        [TestCase(4)]
        public void ChangeAngle(int keyCount)
        {
            Build(withGradient: true, keyCount);
            Measure($"Change angle (all children repaint), {keyCount} keys", RotateAndRebuild);
        }

        [Test]
        public void RebuildAllChildren_WithoutGradient()
        {
            Build(withGradient: false, keyCount: 0);
            Measure("Rebuild all children, no gradient", RebuildAllChildren);
        }

        [Test]
        public void RebuildOneChild_WithoutGradient()
        {
            Build(withGradient: false, keyCount: 0);
            Measure("Rebuild one child, no gradient", RebuildFirstChild);
        }

        private void RefreshGroupAndRebuild()
        {
            group.Refresh();
            Canvas.ForceUpdateCanvases();
        }

        private void RotateAndRebuild()
        {
            group.Angle += 1f;
            Canvas.ForceUpdateCanvases();
        }

        private void RebuildAllChildren()
        {
            for (int i = 0; i < images.Count; i++)
                images[i].SetVerticesDirty();
            Canvas.ForceUpdateCanvases();
        }

        private void RebuildFirstChild()
        {
            images[0].SetVerticesDirty();
            Canvas.ForceUpdateCanvases();
        }

        private static void Measure(string label, Action iteration)
        {
            for (int i = 0; i < WARMUP_ITERATIONS; i++)
                iteration();

            Recorder allocations = Recorder.Get("GC.Alloc");
            allocations.FilterToCurrentThread();
            allocations.enabled = true;
            Stopwatch stopwatch = Stopwatch.StartNew();
            for (int i = 0; i < MEASURED_ITERATIONS; i++)
                iteration();
            stopwatch.Stop();
            allocations.enabled = false;

            double microseconds = stopwatch.Elapsed.TotalMilliseconds * 1000.0 / MEASURED_ITERATIONS;
            double allocationsPerIteration = allocations.sampleBlockCount / (double)MEASURED_ITERATIONS;
            Debug.Log($"[GradientBenchmark] {label}: {microseconds:F1} µs, " +
                $"{allocationsPerIteration:F1} GC allocations per iteration");
        }

        private void Build(bool withGradient, int keyCount)
        {
            texture = new Texture2D(SPRITE_SIZE, SPRITE_SIZE);
            Vector4 border = new(SPRITE_BORDER, SPRITE_BORDER, SPRITE_BORDER, SPRITE_BORDER);
            slicedSprite = Sprite.Create(texture, new Rect(0, 0, SPRITE_SIZE, SPRITE_SIZE), Vector2.one * 0.5f,
                100f, 0, SpriteMeshType.FullRect, border);

            root = new GameObject("GradientBenchmark", typeof(RectTransform), typeof(Canvas));
            RectTransform groupTransform = CreateRect("Group", root.transform, GROUP_WIDTH, GROUP_HEIGHT);
            for (int i = 0; i < GRAPHIC_COUNT; i++)
                images.Add(CreateImage(groupTransform, i));

            if (!withGradient)
                return;

            group = groupTransform.gameObject.AddComponent<UIGradientGroup>();
            group.Gradient = CreateGradient(keyCount);
            group.Angle = 30f;
            RefreshGroupAndRebuild();
        }

        private Image CreateImage(Transform parent, int index)
        {
            float width = GROUP_WIDTH / GRAPHIC_COUNT;
            RectTransform rect = CreateRect($"Image {index}", parent, width, GROUP_HEIGHT);
            rect.anchoredPosition = new Vector2(index * width - GROUP_WIDTH * 0.5f + width * 0.5f, 0f);
            Image image = rect.gameObject.AddComponent<Image>();
            image.sprite = slicedSprite;
            image.type = Image.Type.Sliced;
            return image;
        }

        private static RectTransform CreateRect(string name, Transform parent, float width, float height)
        {
            GameObject gameObject = new(name, typeof(RectTransform));
            RectTransform rect = (RectTransform)gameObject.transform;
            rect.SetParent(parent, false);
            rect.sizeDelta = new Vector2(width, height);
            return rect;
        }

        private static Gradient CreateGradient(int keyCount)
        {
            Color[] palette = { Color.red, Color.yellow, Color.green, Color.blue };
            GradientColorKey[] colors = new GradientColorKey[keyCount];
            for (int i = 0; i < keyCount; i++)
                colors[i] = new GradientColorKey(palette[i % palette.Length], i / (float)(keyCount - 1));

            Gradient gradient = new();
            gradient.SetKeys(colors, new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) });
            return gradient;
        }
    }
}
