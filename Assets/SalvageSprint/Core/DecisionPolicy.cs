namespace SalvageSprint.Core
{
    // Week 1 reference: acquire known targets, retain useful work,
    // and respond to the navigation-failure flag supplied by the brain.
    public static class DecisionPolicy
    {
        public const string Label = "Part 1 checkpoint: known, committed, recoverable";

        // A pickup qualifies only if it is available, known (visible or remembered),
        // and not temporarily blocked after a navigation failure.
        private static bool IsEligible(Candidate candidate)
        {
            return candidate.Available && (candidate.Visible || candidate.Remembered) && !candidate.Blocked;
        }

        public static AgentFacts Infer(Observation observation)
        {
            // 1. Cargo priority: always deliver held cargo before collecting more.
            if (observation.Carrying)
                return new AgentFacts(true, -1, false);

            // 2. Target retention: match CurrentTargetId by ID (not array index) and
            // keep it while still eligible, avoiding switching to closer distractions.
            foreach (Candidate candidate in observation.Candidates)
            {
                if (candidate.Id == observation.CurrentTargetId
                    && IsEligible(candidate))
                {
                    return new AgentFacts(false, candidate.Id, true);
                }
            }

            // 3. New target search: pick the nearest eligible pickup by squared horizontal
            // distance; if none qualify, nearest stays -1 so Choose falls back to Explore.
            int nearest = -1;
            float distance = float.PositiveInfinity;
            foreach (Candidate candidate in observation.Candidates)
            {
                if (IsEligible(candidate) && candidate.DistanceSquared < distance)
                {
                    nearest = candidate.Id;
                    distance = candidate.DistanceSquared;
                }
            }

            // nearest remains -1 when nothing qualifies; Choose will Explore.
            return new AgentFacts(false, nearest, false);
        }

        public static Decision Choose(Observation observation) => Choose(Infer(observation));

        public static Decision Choose(AgentFacts facts)
        {
            if (facts.NeedsDelivery)
                return new Decision(TaskKind.Deliver, -1, "Cargo held: return to depot");

            if (!facts.HasUsableTarget)
                return new Decision(TaskKind.Explore, -1,
                    "No eligible known pickup: visit the next survey point");

            // Distinguish retaining an existing target from picking a new one.
            string reason = facts.RetainedTarget
                ? "Retain current eligible target"
                : "Nearest eligible known pickup";

            return new Decision(TaskKind.Collect, facts.SelectedTargetId, reason);
        }
    }
}
