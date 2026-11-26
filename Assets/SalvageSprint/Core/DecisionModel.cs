using System;

namespace SalvageSprint.Core
{
    public enum TaskKind { Explore, Collect, Deliver }
    public enum TravelStatus { Idle, Pending, Moving, Arrived, Failed }

    public readonly struct Candidate
    {
        public readonly int Id;
        public readonly float DistanceSquared; // Squared distance to the candidate, for efficiency.
        public readonly bool Available, Visible, Remembered, Blocked;
        
        public Candidate(int id, float distanceSquared, bool available, bool visible, bool remembered = false, bool blocked = false)
        {
            Id = id;
            DistanceSquared = distanceSquared;
            Available = available;
            Visible = visible;
            Remembered = remembered;
            Blocked = blocked;
        }
    }

    public readonly struct Observation
    {
        public readonly bool Carrying; // True if the agent currently holds cargo; collection is handled by MatchController.
        public readonly int CurrentTargetId; // The pursued/retained pickup ID, or -1 if there is no current target. This is not the cargo ID.
        public readonly Candidate[] Candidates; // Candidate data from the sensor. Presence in this array does not establish eligibility or knowledge.
        public Observation(bool carrying, int currentTargetId, Candidate[] candidates)
        {
            Carrying = carrying;
            CurrentTargetId = currentTargetId;
            Candidates = candidates ?? Array.Empty<Candidate>();
        }
    }

    public readonly struct Decision
    {
        public readonly TaskKind Task; // The task the agent has decided to perform: Explore, Collect, or Deliver.
        public readonly int TargetId; // The selected pickup ID for Collect. Deliver and Explore use -1; the brain supplies their destinations.
        public readonly string Reason; // A string explaining the rationale behind the decision, useful for debugging or logging.
        public Decision(TaskKind task, int targetId, string reason)
        { Task = task; TargetId = targetId; Reason = reason; }
    }

    public readonly struct AgentFacts
    {
        // Facts about the agent's current state and decision-making context.
        public readonly bool NeedsDelivery, HasUsableTarget, RetainedTarget;
        public readonly int SelectedTargetId;
        public AgentFacts(bool carrying, int selectedTargetId, bool retainedTarget)
        {
            NeedsDelivery = carrying;
            SelectedTargetId = selectedTargetId;
            HasUsableTarget = selectedTargetId >= 0;
            RetainedTarget = retainedTarget;
        }
    }

    public static class NavigationFeedback
    {
        // A request is asynchronous. A complete route is not the same as arrival.
        // parameters: onMesh, pending, completePath, hasPath, remainingDistance, stoppingDistance
        // onMesh: true if the agent is on a valid NavMesh surface; false if it has fallen off or is otherwise invalid.
        // pending: true if the agent is still calculating a path; false if it has finished calculating.
        // completePath: true if the agent has a complete path to the destination; false if the path is partial or invalid.
        // hasPath: true if the agent currently has a path; false if it does not.
        // remainingDistance: remaining path distance. Positive infinity means that the distance is unknown.
        // stoppingDistance: the distance from the destination at which the agent will stop; used to determine if the agent has arrived.
        public static TravelStatus Classify(bool onMesh, bool pending, bool completePath,
            bool hasPath, float remainingDistance, float stoppingDistance)
        {
            // Without a NavMesh placement, the agent cannot navigate.
            if (!onMesh)
                return TravelStatus.Failed;

            // While Unity calculates the route, other path values may not
            // describe the newly requested journey yet.
            if (pending)
                return TravelStatus.Pending;

            // Partial/invalid routes fail this task.
            // NaN and negative distances are also unusable feedback.
            if (!completePath
                || float.IsNaN(remainingDistance)
                || remainingDistance < 0)
            {
                return TravelStatus.Failed;
            }

            // Positive infinity means the remaining distance is unknown.
            // It does not, by itself, mean the route is impossible.
            //
            // If a complete path exists, keep the journey in progress.
            // Navigator's no-progress timeout still handles a stuck agent.
            // "Moving" here describes travel in progress, not guaranteed
            // physical movement during this particular frame.
            if (float.IsPositiveInfinity(remainingDistance))
                return hasPath ? TravelStatus.Moving : TravelStatus.Failed;
            
            // Arrival requires a known distance within the stopping threshold.
            // This still does not prove that collection/delivery has happened.
            if (remainingDistance <= stoppingDistance)
                return TravelStatus.Arrived;

            // If the agent has a valid path and is still moving towards the destination, it is in a moving state.
            return hasPath ? TravelStatus.Moving : TravelStatus.Failed;
        }
    }
}
