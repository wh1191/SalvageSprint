using NUnit.Framework;
using SalvageSprint.Core;

namespace SalvageSprint.Tests
{
    // Optional teacher examples extending the 17 supplied tests.
    // These test decision snapshots, not Unity movement or physics.
    [Category("Week1Reference")]
    public sealed class ReferencePolicyTests
    {
        [Test]
        public void CurrentTargetIsRetainedEvenWhenListedAfterADistraction()
        {
            var observation = new Observation(false, 42, new[]
            {
                new Candidate(7, 1, true, true),
                new Candidate(42, 25, true, false, true)
            });

            AgentFacts facts = DecisionPolicy.Infer(observation);

            Assert.That(facts.SelectedTargetId, Is.EqualTo(42));
            Assert.That(facts.RetainedTarget, Is.True);
        }

        [Test]
        public void MissingCurrentTargetFallsBackToANewEligibleTarget()
        {
            var observation = new Observation(false, 42, new[]
            {
                new Candidate(7, 9, true, true)
            });

            AgentFacts facts = DecisionPolicy.Infer(observation);

            Assert.That(facts.SelectedTargetId, Is.EqualTo(7));
            Assert.That(facts.RetainedTarget, Is.False);
            Assert.That(DecisionPolicy.Choose(facts).Task, Is.EqualTo(TaskKind.Collect));
        }

        [Test]
        public void MatchingCurrentIdDoesNotMakeAnUnknownHiddenPickupKnown()
        {
            var observation = new Observation(false, 42, new[]
            {
                new Candidate(42, 1, true, false, false)
            });

            AgentFacts facts = DecisionPolicy.Infer(observation);
            Decision decision = DecisionPolicy.Choose(facts);

            Assert.That(facts.RetainedTarget, Is.False);
            Assert.That(decision.Task, Is.EqualTo(TaskKind.Explore));
            Assert.That(decision.TargetId, Is.EqualTo(-1));
        }
    }
}
