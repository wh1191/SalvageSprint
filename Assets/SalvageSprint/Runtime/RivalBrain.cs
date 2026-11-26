using System.Collections.Generic;
using SalvageSprint.Core;
using UnityEngine;

namespace SalvageSprint
{
    [RequireComponent(typeof(RaceActor), typeof(Navigator), typeof(PickupSensor))]
    public sealed class RivalBrain : MonoBehaviour
    {
        [SerializeField] private MatchController match;
        [SerializeField] private Transform[] surveyPoints;
        [SerializeField, Min(.05f)] private float decisionInterval = .15f;
        [SerializeField] private bool logDecisionChanges;
        // The rival remembers which pickups have failed to reach, and ignores them for a while.
        private readonly Dictionary<int, float> failedUntil = new Dictionary<int, float>();
        private RaceActor actor;
        private Navigator navigator;
        private PickupSensor sensor;
        private int rememberedId = -1, surveyIndex;
        private float nextDecision;
        public Decision CurrentDecision { get; private set; }
        public Observation LastObservation { get; private set; }
        public AgentFacts CurrentFacts { get; private set; }
        public TravelStatus Travel => navigator != null ? navigator.Status : TravelStatus.Idle;
        public int DecisionCount { get; private set; }
        public int DestinationRequests => navigator != null ? navigator.DestinationRequests : 0;
        public string LastFailure { get; private set; } = "None";

        public void Configure(MatchController owner, Transform[] points)
        { match = owner; surveyPoints = points; }

        private void Awake()
        {
            actor = GetComponent<RaceActor>();
            navigator = GetComponent<Navigator>();
            sensor = GetComponent<PickupSensor>();
            CurrentDecision = new Decision(TaskKind.Explore, -1, "Waiting for first observation");
        }

        private void Update()
        {
            if (!match.IsRunning)
            {
                navigator.Cancel();
                return;
            }
            
            TravelStatus travel = navigator.Poll();
            
            if (Time.time < nextDecision) return;
            
            nextDecision = Time.time + decisionInterval;

            // If the last navigation attempt failed, remember that pickup ID and ignore it for a while.
            if (travel == TravelStatus.Failed && rememberedId >= 0)
            {
                failedUntil[rememberedId] = Time.time + 6; // Ignore this pickup for 6 seconds after a failed navigation attempt.
                LastFailure = "Route failed for pickup " + rememberedId + "; ignore for 6 seconds";
            }

            // If the remembered pickup is no longer available, forget it.
            if (rememberedId >= 0 && !match.IsAvailable(rememberedId)) rememberedId = -1;

            // If the rival is exploring and has arrived at or failed to reach the current survey point, move to the next survey point in the list.
            if (CurrentDecision.Task == TaskKind.Explore
                && (travel == TravelStatus.Arrived || travel == TravelStatus.Failed))
                surveyIndex = (surveyIndex + 1) % surveyPoints.Length; // Move to the next survey point if exploring and done with the current one.

            LastObservation = sensor.Capture(
                actor: actor,
                rememberedId: rememberedId,
                isBlocked: IsBlocked); // Capture the current observation of the environment.
            
            CurrentFacts = DecisionPolicy.Infer(LastObservation);
            
            Decision next = DecisionPolicy.Choose(CurrentFacts);

            // If the decision has changed, log it and update the navigation request.
            bool changed = next.Task != CurrentDecision.Task || next.TargetId != CurrentDecision.TargetId;
            CurrentDecision = next;
            DecisionCount++;
            
            if (changed && logDecisionChanges)
                Debug.Log($"[{Time.time:F1}] {next.Task} target={next.TargetId}: {next.Reason}", this);

            
            if (next.Task == TaskKind.Deliver)
            {
                rememberedId = -1;
                navigator.Request(-2, actor.DepotPosition);
            }
            else if (next.Task == TaskKind.Collect)
            {
                rememberedId = next.TargetId;
                SalvagePickup target = match.GetPickup(next.TargetId);
                if (target != null && match.IsAvailable(next.TargetId))
                    navigator.Request(next.TargetId, target.transform.position);
                else navigator.Cancel();
            }
            else
            {
                rememberedId = -1;
                navigator.Request(-100 - surveyIndex, surveyPoints[surveyIndex].position);
            }
        }

        private bool IsBlocked(int id) => failedUntil.TryGetValue(id, out float until) && Time.time < until;
        private void OnDisable() { if (navigator != null) navigator.Cancel(); }
    }
}
