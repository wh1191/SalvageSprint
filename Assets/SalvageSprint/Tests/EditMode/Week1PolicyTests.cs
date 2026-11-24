using NUnit.Framework;
using SalvageSprint.Core;

namespace SalvageSprint.Tests
{
    // Initial starter: 4 pass, 6 fail in this class. Together with CoreTests: 11 pass, 6 fail.
    // Tests are public behaviour contracts. Fix the policy, not the expected results.
    [Category("Week1")]
    public sealed class Week1PolicyTests
    {
        private static Decision Decide(bool cargo, int memory, params Candidate[] items)
            => DecisionPolicy.Choose(new Observation(cargo, memory, items));

        [Test] public void Stage2_HiddenUnknownPickupIsNotAUsableTarget()
        {
            Assert.That(Decide(false, -1, new Candidate(1, 1, true, false)).Task,
                Is.EqualTo(TaskKind.Explore));
        }
        [Test] public void Stage2_VisibleAlternativeBeatsNearerHiddenPickup()
        {
            var d = Decide(false, -1, new Candidate(1, 1, true, false), new Candidate(2, 9, true, true));
            Assert.That(d.TargetId, Is.EqualTo(2));
        }
        [Test] public void Stage2_PreviouslyKnownPickupMayRemainKnownOutsideSight()
        {
            var d = Decide(false, 4, new Candidate(4, 16, true, false, true));
            Assert.That(d.Task, Is.EqualTo(TaskKind.Collect));
            Assert.That(d.TargetId, Is.EqualTo(4));
        }
        [Test] public void Stage3_UsableCurrentTargetSurvivesCloserDistraction()
        {
            var o = new Observation(false, 4, new[]
            { new Candidate(4, 16, true, false, true), new Candidate(5, 1, true, true) });
            AgentFacts facts = DecisionPolicy.Infer(o);
            Assert.That(DecisionPolicy.Choose(facts).TargetId, Is.EqualTo(4));
            Assert.That(facts.RetainedTarget, Is.True);
        }
        [Test] public void Stage3_UnavailableRememberedTargetIsReleased()
        {
            var d = Decide(false, 4, new Candidate(4, 1, false, true, true), new Candidate(5, 9, true, true));
            Assert.That(d.TargetId, Is.EqualTo(5));
        }
        [Test] public void Stage4_BlockedNearestPickupDoesNotPreventAnotherTask()
        {
            var d = Decide(false, -1, new Candidate(1, 1, true, true, false, true), new Candidate(2, 9, true, true));
            Assert.That(d.TargetId, Is.EqualTo(2));
        }
        [Test] public void Stage4_NoUsableAlternativeFallsBackToExploration()
        {
            Assert.That(Decide(false, -1, new Candidate(1, 1, true, true, false, true)).Task,
                Is.EqualTo(TaskKind.Explore));
        }
        [Test] public void Stage4_BlockedRememberedTargetMustNotRemainCommitted()
        {
            var d = Decide(false, 4, new Candidate(4, 1, true, false, true, true), new Candidate(5, 9, true, true));
            Assert.That(d.TargetId, Is.EqualTo(5));
        }
        [Test] public void Regression_CargoTakesPriorityOverCollection()
        {
            Assert.That(Decide(true, 4, new Candidate(4, 1, true, true, true)).Task,
                Is.EqualTo(TaskKind.Deliver));
        }
        [Test] public void Regression_EmptyWorldDoesNotInventPickupTargets()
        {
            Assert.That(Decide(false, -1).Task, Is.EqualTo(TaskKind.Explore));
        }
    }
}
