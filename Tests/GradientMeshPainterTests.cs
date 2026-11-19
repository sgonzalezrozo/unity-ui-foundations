using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools.Constraints;
using UnityEngine.UI;
using Is = UnityEngine.TestTools.Constraints.Is;

namespace UIFoundations.Tests
{
    public class GradientMeshPainterTests
    {
        private const float HALF_WIDTH = 50f;
        private const float HALF_HEIGHT = 25f;
        private const int QUAD_VERTEX_COUNT = 4;

        private static readonly Rect Group = Rect.MinMaxRect(-HALF_WIDTH, -HALF_HEIGHT, HALF_WIDTH, HALF_HEIGHT);

        private readonly List<UIVertex> stream = new();
        private VertexHelper helper;

        [SetUp]
        public void SetUp()
        {
            helper = new VertexHelper();
        }

        [TearDown]
        public void TearDown()
        {
            helper.Dispose();
        }

        private static GradientSampler MakeSampler(params float[] colorKeyTimes)
        {
            GradientSampler sampler = new();
            sampler.SetGradient(GradientSamplerTests.MakeGradient(colorKeyTimes));
            sampler.SetFrame(0f, Group);
            return sampler;
        }

        private void AddQuad(float xMin, float xMax)
        {
            int first = helper.currentVertCount;
            helper.AddVert(new Vector3(xMin, -HALF_HEIGHT), Color.white, new Vector4(0f, 0f));
            helper.AddVert(new Vector3(xMin, HALF_HEIGHT), Color.white, new Vector4(0f, 1f));
            helper.AddVert(new Vector3(xMax, HALF_HEIGHT), Color.white, new Vector4(1f, 1f));
            helper.AddVert(new Vector3(xMax, -HALF_HEIGHT), Color.white, new Vector4(1f, 0f));
            helper.AddTriangle(first, first + 1, first + 2);
            helper.AddTriangle(first + 2, first + 3, first);
        }

        private List<UIVertex> ReadVertices()
        {
            stream.Clear();
            helper.GetUIVertexStream(stream);
            return stream;
        }

        // Unity stores gradient key times in 16 bits, so a key at 0.5 sits at 0.500008 and splits slightly off center.
        private const float POSITION_TOLERANCE = 0.01f;

        private static UIVertex FindVertexAtX(List<UIVertex> vertices, float x)
        {
            for (int i = 0; i < vertices.Count; i++)
            {
                if (Mathf.Abs(vertices[i].position.x - x) < POSITION_TOLERANCE)
                    return vertices[i];
            }
            Assert.Fail($"No vertex at x = {x}");
            return default;
        }

        [Test]
        public void Paint_NoStopInsideMesh_KeepsVertexCountAndColorsEnds()
        {
            AddQuad(-HALF_WIDTH, HALF_WIDTH);

            new GradientMeshPainter().Paint(helper, MakeSampler(0f, 1f), Matrix4x4.identity);

            Assert.That(helper.currentVertCount, Is.EqualTo(QUAD_VERTEX_COUNT));
            List<UIVertex> vertices = ReadVertices();
            Assert.That((Color)FindVertexAtX(vertices, -HALF_WIDTH).color, Is.EqualTo(Color.black));
            Assert.That((Color)FindVertexAtX(vertices, HALF_WIDTH).color, Is.EqualTo(Color.white));
        }

        [Test]
        public void Paint_StopInsideMesh_AddsVerticesAtTheStop()
        {
            AddQuad(-HALF_WIDTH, HALF_WIDTH);

            new GradientMeshPainter().Paint(helper, MakeSampler(0f, 0.5f, 1f), Matrix4x4.identity);

            UIVertex middle = FindVertexAtX(ReadVertices(), 0f);
            Assert.That(middle.color.r, Is.EqualTo(128).Within(1));
        }

        [Test]
        public void Paint_SplitVertex_InterpolatesUv()
        {
            AddQuad(-HALF_WIDTH, HALF_WIDTH);

            new GradientMeshPainter().Paint(helper, MakeSampler(0f, 0.5f, 1f), Matrix4x4.identity);

            UIVertex middle = FindVertexAtX(ReadVertices(), 0f);
            Assert.That(middle.uv0.x, Is.EqualTo(0.5f).Within(0.001f));
        }

        [Test]
        public void Paint_StopOutsideMesh_DoesNotSplit()
        {
            AddQuad(-HALF_WIDTH, -10f);

            new GradientMeshPainter().Paint(helper, MakeSampler(0f, 0.75f, 1f), Matrix4x4.identity);

            Assert.That(helper.currentVertCount, Is.EqualTo(QUAD_VERTEX_COUNT));
        }

        [Test]
        public void Paint_MeshOffsetFromGroup_UsesGroupSpace()
        {
            AddQuad(-10f, 10f);
            Matrix4x4 shiftRight = Matrix4x4.Translate(new Vector3(40f, 0f));

            new GradientMeshPainter().Paint(helper, MakeSampler(0f, 1f), shiftRight);

            Color rightEdge = FindVertexAtX(ReadVertices(), 10f).color;
            Assert.That(rightEdge.r, Is.EqualTo(1f).Within(0.01f));
        }

        [Test]
        public void Paint_EmptyMesh_DoesNothing()
        {
            new GradientMeshPainter().Paint(helper, MakeSampler(0f, 0.5f, 1f), Matrix4x4.identity);

            Assert.That(helper.currentVertCount, Is.EqualTo(0));
        }

        [Test]
        public void Paint_AfterWarmUp_AllocatesNothing()
        {
            GradientMeshPainter painter = new();
            GradientSampler sampler = MakeSampler(0f, 0.25f, 0.5f, 0.75f, 1f);
            AddQuad(-HALF_WIDTH, HALF_WIDTH);
            painter.Paint(helper, sampler, Matrix4x4.identity);

            Assert.That(() =>
            {
                helper.Clear();
                AddQuad(-HALF_WIDTH, HALF_WIDTH);
                painter.Paint(helper, sampler, Matrix4x4.identity);
            }, Is.Not.AllocatingGCMemory());
        }
    }
}
