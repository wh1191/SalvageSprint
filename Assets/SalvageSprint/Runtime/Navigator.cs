using SalvageSprint.Core;
using UnityEngine;
using UnityEngine.AI;

namespace SalvageSprint
{
    [RequireComponent(typeof(NavMeshAgent))]
    public sealed class Navigator : MonoBehaviour
    {
        private NavMeshAgent agent;
        private bool requested; // True if a destination has been requested and not yet completed or canceled.
        private int goalKey = int.MinValue, requestFrame;
        private Vector3 goal, lastProgressPosition;
        private float lastProgressTime; // Time.time of the last significant movement, for detecting if the agent is stuck.
        public TravelStatus Status { get; private set; } = TravelStatus.Idle;
        public Vector3 Goal => goal;
        public int DestinationRequests { get; private set; }

        private void Awake() { agent = GetComponent<NavMeshAgent>(); }

        public void Request(int key, Vector3 destination)
        {
            if (requested && key == goalKey && (destination - goal).sqrMagnitude < .01f) return;
            
            requested = true;
            goalKey = key;
            goal = destination;
            
            requestFrame = Time.frameCount;
            lastProgressTime = Time.time;
            lastProgressPosition = transform.position;
            
            if (agent == null || !agent.isOnNavMesh)
            {
                Status = TravelStatus.Failed;
                return;
            }
            
            agent.isStopped = false;
            DestinationRequests++;
            
            Status = agent.SetDestination(destination) ? TravelStatus.Pending : TravelStatus.Failed;
        }

        public TravelStatus Poll()
        {
            // Poll the current travel status. This method should be called once per frame to update the status based on the agent's movement and pathfinding state.
            if (!requested || Status == TravelStatus.Failed) return Status;
            
            if (Time.frameCount == requestFrame) return Status;
            
            if (agent == null || !agent.isOnNavMesh)
            {
                Status = TravelStatus.Failed;
                return Status;
            }
            
            Status = NavigationFeedback.Classify(agent.isOnNavMesh, agent.pathPending,
                agent.pathStatus == NavMeshPathStatus.PathComplete, agent.hasPath,
                agent.remainingDistance, agent.stoppingDistance + .12f);
            
            if ((transform.position - lastProgressPosition).sqrMagnitude > .04f)
            {
                lastProgressPosition = transform.position; lastProgressTime = Time.time;
            }
            
            if ((Status == TravelStatus.Moving || Status == TravelStatus.Pending)
                && Time.time - lastProgressTime > 5)
            {
                Status = TravelStatus.Failed;
            }
            
            if (Status == TravelStatus.Failed && agent.isOnNavMesh)
            {
                agent.isStopped = true; agent.ResetPath();
            }
            
            return Status;
        }

        public void Cancel()
        {
            if (!requested) return;
            if (agent != null && agent.isOnNavMesh)
            { agent.isStopped = true; agent.ResetPath(); }
            requested = false;
            goalKey = int.MinValue;
            Status = TravelStatus.Idle;
        }

        private void OnDrawGizmosSelected()
        {
            NavMeshAgent navigation = GetComponent<NavMeshAgent>();
            if (navigation == null || !navigation.isOnNavMesh || !navigation.hasPath) return;
            Vector3[] corners = navigation.path.corners;
            Gizmos.color = Color.yellow;
            for (int i = 1; i < corners.Length; i++)
                Gizmos.DrawLine(corners[i - 1] + Vector3.up * .15f, corners[i] + Vector3.up * .15f);
        }
    }
}
