using System;

namespace SalvageSprint.Core
{
    public enum TaskKind { Explore, Collect, Deliver }
    public enum TravelStatus { Idle, Pending, Moving, Arrived, Failed }

    public readonly struct Candidate
    {
        public readonly int Id;
        public readonly float DistanceSquared;
        public readonly bool Available, Visible, Remembered, Blocked;
        public Candidate(int id, float distanceSquared, bool available, bool visible,
            bool remembered = false, bool blocked = false)
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
        public readonly bool Carrying;
        public readonly int CurrentTargetId;
        public readonly Candidate[] Candidates;
        public Observation(bool carrying, int currentTargetId, Candidate[] candidates)
        {
            Carrying = carrying;
            CurrentTargetId = currentTargetId;
            Candidates = candidates ?? Array.Empty<Candidate>();
        }
    }

    public readonly struct Decision
    {
        public readonly TaskKind Task;
        public readonly int TargetId;
        public readonly string Reason;
        public Decision(TaskKind task, int targetId, string reason)
        { Task = task; TargetId = targetId; Reason = reason; }
    }

    public readonly struct AgentFacts
    {
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
        public static TravelStatus Classify(bool onMesh, bool pending, bool completePath,
            bool hasPath, float remainingDistance, float stoppingDistance)
        {
            if (!onMesh) return TravelStatus.Failed;
            if (pending) return TravelStatus.Pending;
            if (!completePath || float.IsNaN(remainingDistance) || float.IsInfinity(remainingDistance))
                return TravelStatus.Failed;
            if (remainingDistance <= stoppingDistance) return TravelStatus.Arrived;
            return hasPath ? TravelStatus.Moving : TravelStatus.Failed;
        }
    }
}
