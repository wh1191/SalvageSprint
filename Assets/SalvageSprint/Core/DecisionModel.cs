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
        public readonly bool Carrying; // True if the agent is currently carrying a target; false if it is empty-handed.
        public readonly int CurrentTargetId; // The ID of the target the agent is currently carrying, or -1 if it is not carrying anything.
        public readonly Candidate[] Candidates; // An array of candidates that the agent can consider for collection. Each candidate has an ID, distance, and availability status.
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
        public readonly int TargetId; // The ID of the target the agent has selected for collection or delivery, or -1 if no target is selected.
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
        // remainingDistance: the distance remaining to the destination along the current path; NaN or Infinity if no valid path exists.
        // stoppingDistance: the distance from the destination at which the agent will stop; used to determine if the agent has arrived.
        public static TravelStatus Classify(bool onMesh, bool pending, bool completePath,
            bool hasPath, float remainingDistance, float stoppingDistance)
        {
            if (!onMesh) return TravelStatus.Failed; // If the agent is not on the NavMesh, it cannot navigate.

            if (pending) return TravelStatus.Pending; // If the agent is still calculating a path, it is in a pending state.

            // If the path is not complete or the remaining distance is invalid, the navigation has failed.
            if (!completePath || float.IsNaN(remainingDistance) || float.IsInfinity(remainingDistance))
                return TravelStatus.Failed;

            // If the remaining distance is less than or equal to the stopping distance, the agent has arrived at its destination.
            if (remainingDistance <= stoppingDistance) return TravelStatus.Arrived;

            // If the agent has a valid path and is still moving towards the destination, it is in a moving state.
            return hasPath ? TravelStatus.Moving : TravelStatus.Failed;
        }
    }
}
