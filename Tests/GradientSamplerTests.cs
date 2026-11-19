using NUnit.Framework;
using UnityEngine;

namespace UIFoundations.Tests
{
    public class GradientSamplerTests
    {
        private const float TOLERANCE = 0.0001f;

        private static readonly Rect Group = Rect.MinMaxRect(-50f, -25f, 50f, 25f);

        internal static Gradient MakeGradient(params float[] colorKeyTimes)
        {
            GradientColorKey[] colors = new GradientColorKey[colorKeyTimes.Length];
            for (int i = 0; i < colorKeyTimes.Length; i++)
            {
                Color gray = Color.Lerp(Color.black, Color.white, colorKeyTimes[i]);
                colors[i] = new GradientColorKey(gray, colorKeyTimes[i]);
            }

            Gradient gradient = new();
            gradient.SetKeys(colors, new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) });
            return gradient;
        }

        private static GradientSampler MakeSampler(float angle, params float[] colorKeyTimes)
        {
            GradientSampler sampler = new();
            sampler.SetGradient(MakeGradient(colorKeyTimes));
            sampler.SetFrame(angle, Group);
            return sampler;
        }

        [TestCase(-50f, 0f, 0f)]
        [TestCase(0f, 0f, 0.5f)]
        [TestCase(50f, 0f, 1f)]
        public void CalculateTime_AtZeroDegrees_RunsLeftToRight(float x, float y, float expected)
        {
            GradientSampler sampler = MakeSampler(0f, 0f, 1f);

            Assert.That(sampler.CalculateTime(new Vector3(x, y)), Is.EqualTo(expected).Within(TOLERANCE));
        }

        [TestCase(0f, -25f, 0f)]
        [TestCase(0f, 25f, 1f)]
        public void CalculateTime_AtNinetyDegrees_RunsBottomToTop(float x, float y, float expected)
        {
            GradientSampler sampler = MakeSampler(90f, 0f, 1f);

            Assert.That(sampler.CalculateTime(new Vector3(x, y)), Is.EqualTo(expected).Within(TOLERANCE));
        }

        [Test]
        public void CalculateTime_At180Degrees_RunsRightToLeft()
        {
            GradientSampler sampler = MakeSampler(180f, 0f, 1f);

            Assert.That(sampler.CalculateTime(new Vector3(50f, 0f)), Is.EqualTo(0f).Within(TOLERANCE));
        }

        [Test]
        public void CalculateTime_DiagonalAngle_MapsOppositeCornersToZeroAndOne()
        {
            GradientSampler sampler = MakeSampler(45f, 0f, 1f);

            Assert.That(sampler.CalculateTime(new Vector3(-50f, -25f)), Is.EqualTo(0f).Within(TOLERANCE));
            Assert.That(sampler.CalculateTime(new Vector3(50f, 25f)), Is.EqualTo(1f).Within(TOLERANCE));
        }

        [Test]
        public void CalculateTime_PointOutsideGroup_ReturnsTimeBeyondOne()
        {
            GradientSampler sampler = MakeSampler(0f, 0f, 1f);

            Assert.That(sampler.CalculateTime(new Vector3(150f, 0f)), Is.EqualTo(2f).Within(TOLERANCE));
        }

        [Test]
        public void CalculateTime_GroupWithNoSize_ReturnsZero()
        {
            GradientSampler sampler = new();
            sampler.SetGradient(MakeGradient(0f, 1f));
            sampler.SetFrame(0f, Rect.zero);

            Assert.That(sampler.CalculateTime(new Vector3(10f, 0f)), Is.EqualTo(0f));
        }

        [Test]
        public void Evaluate_TimeOutsideRange_ClampsToEndColors()
        {
            GradientSampler sampler = MakeSampler(0f, 0f, 1f);

            Assert.That(sampler.Evaluate(-1f), Is.EqualTo((Color32)Color.black));
            Assert.That(sampler.Evaluate(2f), Is.EqualTo((Color32)Color.white));
        }

        [Test]
        public void Evaluate_InnerTime_MatchesUnityGradientWithinOneStep()
        {
            Gradient gradient = MakeGradient(0f, 0.3f, 1f);
            GradientSampler sampler = new();
            sampler.SetGradient(gradient);

            Color32 expected = gradient.Evaluate(0.42f);
            Color32 actual = sampler.Evaluate(0.42f);

            Assert.That(actual.r, Is.EqualTo(expected.r).Within(1));
            Assert.That(actual.g, Is.EqualTo(expected.g).Within(1));
        }

        [Test]
        public void SetGradient_Null_Throws()
        {
            Assert.That(() => new GradientSampler().SetGradient(null), Throws.ArgumentNullException);
        }

        [Test]
        public void SetGradient_KeysAtEnds_HasNoStops()
        {
            GradientSampler sampler = MakeSampler(0f, 0f, 1f);

            Assert.That(sampler.StopCount, Is.EqualTo(0));
        }

        [Test]
        public void SetGradient_InnerColorAndAlphaKeys_SortsAndMergesStops()
        {
            Gradient gradient = new();
            gradient.SetKeys(
                new[] { new GradientColorKey(Color.red, 0.7f), new GradientColorKey(Color.blue, 0.3f) },
                new[] { new GradientAlphaKey(1f, 0.3f), new GradientAlphaKey(0.5f, 0.5f) });
            GradientSampler sampler = new();

            sampler.SetGradient(gradient);

            Assert.That(sampler.StopCount, Is.EqualTo(3));
            Assert.That(sampler.GetStop(0), Is.EqualTo(0.3f).Within(TOLERANCE));
            Assert.That(sampler.GetStop(1), Is.EqualTo(0.5f).Within(TOLERANCE));
            Assert.That(sampler.GetStop(2), Is.EqualTo(0.7f).Within(TOLERANCE));
        }

        [Test]
        public void SetGradient_CalledAgain_ReplacesOldStops()
        {
            GradientSampler sampler = MakeSampler(0f, 0f, 0.5f, 1f);

            sampler.SetGradient(MakeGradient(0f, 1f));

            Assert.That(sampler.StopCount, Is.EqualTo(0));
        }

        [TestCase(0.2f, 0.8f, true)]
        [TestCase(0.2f, 0.4f, false)]
        [TestCase(0.6f, 0.9f, false)]
        public void HasStopBetween_StopAtHalf_IsTrueOnlyWhenRangeContainsIt(float minimum, float maximum, bool expected)
        {
            GradientSampler sampler = MakeSampler(0f, 0f, 0.5f, 1f);

            Assert.That(sampler.HasStopBetween(minimum, maximum), Is.EqualTo(expected));
        }

        [Test]
        public void HasStopBetween_RangeEndsExactlyOnStop_IsFalse()
        {
            GradientSampler sampler = MakeSampler(0f, 0f, 0.5f, 1f);
            float stop = sampler.GetStop(0);

            Assert.That(sampler.HasStopBetween(stop, 0.9f), Is.False);
            Assert.That(sampler.HasStopBetween(0.1f, stop), Is.False);
        }
    }
}
