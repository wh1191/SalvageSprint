using SalvageSprint.Core;
using UnityEngine;
using UnityEngine.AI;

namespace SalvageSprint
{
    [RequireComponent(typeof(NavMeshAgent))]
    public sealed class Navigator : MonoBehaviour
    {
        [SerializeField, Min(0.1f)]
        private float failedRequestRetryDelay = 1f;
        
        [SerializeField]
        private bool logNavigationFailures = true;
        
        // Visible in the Inspector, including failures during delivery.
        [SerializeField, TextArea]
        private string lastFailure = "None";
        
        private NavMeshAgent agent;
        private bool requested; // True if a destination has been requested and not yet completed or canceled.
        private int goalKey = int.MinValue;
        private int requestFrame;

        private Vector3 goal;
        private Vector3 lastProgressPosition;
        private float lastProgressTime; // Time.time of the last significant movement, for detecting if the agent is stuck.
        private float retryAt;
        
        public TravelStatus Status { get; private set; } = TravelStatus.Idle;
        public Vector3 Goal => goal;
        public int DestinationRequests { get; private set; }
        public string LastFailure => lastFailure;

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
        }

        public void Request(int key, Vector3 destination)
        {
            bool sameGoal =
                requested
                && key == goalKey
                && (destination - goal).sqrMagnitude < .01f;
            
            // Reuse an unchanged request while it is pending, moving
            // or already satisfied.
            //
            // A FAILED request must eventually be allowed through again.
            // Otherwise, delivery can remain stuck forever because the
            // brain keeps requesting the same depot.
            //
            // The delay prevents retrying on every decision update.
            if (sameGoal
                && (Status != TravelStatus.Failed || Time.time < retryAt))
            {
                return;
            }
            
            requested = true;
            goalKey = key;
            goal = destination;
            requestFrame = Time.frameCount;
            
            // Start a fresh progress-monitoring window/timeframe for this attempt.
            lastProgressTime = Time.time;
            lastProgressPosition = transform.position;
            
            if (!CanNavigate())
            {
                Fail("Agent unavailable or not on a NavMesh");
                return;
            }

            // A previous failure may have stopped the agent.
            agent.isStopped = false;
            DestinationRequests++;

            // Success means Unity accepted the request.
            // The resulting route still needs to be checked by Poll.
            if (agent.SetDestination(destination))
            {
                Status = TravelStatus.Pending;
            }
            else
            {
                Fail("SetDestination rejected the request");
            }
        }

        public TravelStatus Poll()
        {
            // Failed attempts remain failed until Request starts another
            // attempt, or Cancel removes the request.
            if (!requested || Status == TravelStatus.Failed)
                return Status;

            // Avoid interpreting navigation feedback in the same frame
            // that we submitted the destination.
            if (Time.frameCount == requestFrame)
                return Status;

            if (!CanNavigate())
            {
                Fail("Agent unavailable or no longer on a NavMesh");
                return Status;
            }

            Status = NavigationFeedback.Classify(
                agent.isOnNavMesh,
                agent.pathPending,
                agent.pathStatus == NavMeshPathStatus.PathComplete,
                agent.hasPath,
                agent.remainingDistance,
                agent.stoppingDistance + .12f);

            // Movement of more than 0.2 units counts as progress:
            // 0.2 squared = 0.04.
            if ((transform.position - lastProgressPosition).sqrMagnitude > .04f)
            {
                lastProgressPosition = transform.position;
                lastProgressTime = Time.time;
            }

            if (Status == TravelStatus.Failed)
            {
                Fail("Navigation feedback rejected the route");
            }
            else if (
                (Status == TravelStatus.Moving || Status == TravelStatus.Pending)
                && Time.time - lastProgressTime > 5f)
            {
                // A route can exist without the agent making progress.
                // Keep the timeout even when distance is temporarily unknown.
                Fail("No significant movement for five seconds");
            }

            return Status;
        }

        private bool CanNavigate()
        {
            return agent != null
                   && agent.isActiveAndEnabled
                   && agent.isOnNavMesh;
        }
        
        private void Fail(string reason)
        {
            bool onMesh = CanNavigate();

            // Capture the failure evidence BEFORE ResetPath clears it.
            lastFailure =
                $"{reason}; key={goalKey}; goal={goal}"
                + $"; position={transform.position}"
                + $"; onMesh={onMesh}"
                + $"; noProgressFor={Time.time - lastProgressTime:F2}s";

            // Only inspect path properties when the agent can navigate.
            if (onMesh)
            {
                lastFailure +=
                    $"; pending={agent.pathPending}"
                    + $"; hasPath={agent.hasPath}"
                    + $"; pathStatus={agent.pathStatus}"
                    + $"; remainingDistance={agent.remainingDistance}"
                    + $"; stoppingDistance={agent.stoppingDistance}"
                    + $"; isStopped={agent.isStopped}";
            }

            Status = TravelStatus.Failed;
            retryAt = Time.time + failedRequestRetryDelay;

            if (onMesh)
            {
                agent.isStopped = true;
                agent.ResetPath();
            }

            // This also reports delivery failures, which the brain's
            // existing pickup-only failure message does not cover.
            if (logNavigationFailures)
                Debug.LogWarning("[Navigator] " + lastFailure, this);
        }
        
        public void Cancel()
        {
            if (!requested)
                return;

            if (CanNavigate())
            {
                agent.isStopped = true;
                agent.ResetPath();
            }

            requested = false;
            goalKey = int.MinValue;
            Status = TravelStatus.Idle;
        }

        private void OnDrawGizmosSelected()
        {
            NavMeshAgent navigation = GetComponent<NavMeshAgent>();

            if (navigation == null
                || !navigation.isActiveAndEnabled
                || !navigation.isOnNavMesh
                || !navigation.hasPath)
            {
                return;
            }

            Vector3[] corners = navigation.path.corners;
            Gizmos.color = Color.yellow;

            for (int i = 1; i < corners.Length; i++)
            {
                Gizmos.DrawLine(
                    corners[i - 1] + Vector3.up * .15f,
                    corners[i] + Vector3.up * .15f);
            }
        }
    }
}
