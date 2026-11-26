using NUnit.Framework;
using SalvageSprint.Core;

namespace SalvageSprint.Tests
{
    [Category("Infrastructure")]
    public sealed class NavigationRegressionTests
    {
        [Test]
        public void CompletePathWithUnknownDistanceRemainsInProgress()
        {
            // A route exists, but its distance is not yet known.
            // This must neither stop the agent nor report arrival.
            Assert.That(NavigationFeedback.Classify(
                true, false, true, true, float.PositiveInfinity, .6f),
                Is.EqualTo(TravelStatus.Moving));
        }

        [Test]
        public void IncompletePathWithUnknownDistanceStillFails()
        {
            // Unknown distance must not hide a partial/invalid route.
            Assert.That(NavigationFeedback.Classify(
                true, false, false, true, float.PositiveInfinity, .6f),
                Is.EqualTo(TravelStatus.Failed));
        }
    }
}
