using NUnit.Framework;
using SalvageSprint.Core;

namespace SalvageSprint.Tests
{
    [Category("Infrastructure")]
    public sealed class CoreTests
    {
        [Test]
        public void PickupIsExclusiveAndCargoCapacityIsOne()
        {
            var state = new MatchState(new[] { 1, 2 }, 5, 120);
            Assert.That(state.TryCollect(0, 1), Is.True);
            Assert.That(state.TryCollect(1, 1), Is.False);
            Assert.That(state.TryCollect(0, 2), Is.False);
            Assert.That(state.IsAvailable(2), Is.True);
        }

        [Test]
        public void OnlyDeliveryScoresAndCannotScoreTwice()
        {
            var state = new MatchState(new[] { 1 }, 5, 120);
            state.TryCollect(1, 1);
            Assert.That(state.Score(1), Is.Zero);
            Assert.That(state.TryDeliver(1), Is.True);
            Assert.That(state.TryDeliver(1), Is.False);
            Assert.That(state.Score(1), Is.EqualTo(1));
        }

        [Test]
        public void ScoreGoalEndsMatchAndRejectsFurtherChanges()
        {
            var state = new MatchState(new[] { 1, 2 }, 1, 120);
            state.TryCollect(0, 1); state.TryDeliver(0);
            Assert.That(state.Outcome, Is.EqualTo(MatchOutcome.PlayerWins));
            Assert.That(state.TryCollect(1, 2), Is.False);
            Assert.That(state.TryDeliver(0), Is.False);
        }

        [Test]
        public void TimeoutUsesBankedScoreAndEqualScoresDraw()
        {
            var state = new MatchState(new[] { 1, 2 }, 5, 10);
            state.TryCollect(1, 1); state.TryDeliver(1); state.TryCollect(0, 2);
            state.Tick(10);
            Assert.That(state.Outcome, Is.EqualTo(MatchOutcome.RivalWins));
            var tied = new MatchState(new[] { 1 }, 5, 1);
            tied.TryCollect(0, 1); tied.Tick(1);
            Assert.That(tied.Outcome, Is.EqualTo(MatchOutcome.Draw));
        }

        [Test]
        public void PendingPathIsNotMistakenForFailureOrArrival()
        {
            Assert.That(NavigationFeedback.Classify(true, true, false, false, float.PositiveInfinity, .6f),
                Is.EqualTo(TravelStatus.Pending));
            Assert.That(NavigationFeedback.Classify(true, false, false, true, .1f, .6f),
                Is.EqualTo(TravelStatus.Failed));
            Assert.That(NavigationFeedback.Classify(true, false, true, true, 4, .6f),
                Is.EqualTo(TravelStatus.Moving));
            Assert.That(NavigationFeedback.Classify(true, false, true, false, .2f, .6f),
                Is.EqualTo(TravelStatus.Arrived));
        }

        [Test]
        public void CargoPriorityAndEmptyFallbackArePreserved()
        {
            var item = new Candidate(7, 1, true, true);
            Assert.That(DecisionPolicy.Choose(new Observation(true, 7, new[] { item })).Task,
                Is.EqualTo(TaskKind.Deliver));
            Assert.That(DecisionPolicy.Choose(new Observation(false, -1, null)).Task,
                Is.EqualTo(TaskKind.Explore));
        }

        [Test]
        public void NewMatchHasNoStateFromPreviousMatch()
        {
            var oldMatch = new MatchState(new[] { 1 }, 5, 1);
            oldMatch.TryCollect(0, 1); oldMatch.Tick(1);
            var fresh = new MatchState(new[] { 1 }, 5, 120);
            Assert.That(fresh.HasCargo(0), Is.False);
            Assert.That(fresh.Score(0), Is.Zero);
            Assert.That(fresh.IsAvailable(1), Is.True);
            Assert.That(fresh.Outcome, Is.EqualTo(MatchOutcome.Running));
        }
    }
}
