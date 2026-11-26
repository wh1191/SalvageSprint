using System;
using System.Collections.Generic;
using SalvageSprint.Core;
using UnityEngine;

namespace SalvageSprint
{
    public sealed class PickupSensor : MonoBehaviour
    {
        [SerializeField] private MatchController match;
        [SerializeField, Min(1)] private float radius = 10;
        [Header("Perception debugging")]
        [SerializeField] private bool ShowPerceptionRange;
        [SerializeField] private bool showSightChecks;

        [SerializeField, Min(0.01f)]
        private float sighCheckDisplaySeconds = 0.4f;
        public float Radius => radius;
        public void Configure(MatchController owner) { match = owner; }

        /// <summary>
        /// Builds a snapshot of pickup observations for the rival's decision policy.
        /// </summary>
        /// <param name="actor">The rival whose cargo state is observed</param>
        /// <param name="rememberedId">
        /// The retained pickup ID, or -1 if no pickup is remembered. We need this to include the retained
        /// pickup in the observation even if it is currently out of range or blocked.
        /// "retained" means: the rival has previously seen this pickup and is still trying to reach it, even if it is currently out of range or blocked.
        /// </param>
        /// <param name="blocked">
        /// A function supplied by the brain. Given a pickup ID, returns true
        /// wile that pickup is temporarily blocked after a navigation failure.
        /// This method records the result; the policy decides how to use it.
        /// "blocked" means: the rival has previously tried to reach this pickup but failed, so it should be ignored for a while.
        /// </param>
        /// <returns>The rival's vargo, retained target ID and cadidate data.</returns>
        public Observation Capture(RaceActor actor, int rememberedId, Func<int, bool> isBlocked)
        {
            var candidates = new List<Candidate>();
            foreach (SalvagePickup item in match.Pickups)
            {
                if (!match.IsAvailable(item.Id)) continue;
                
                float distance = MatchController.FlatDistanceSquared(transform.position, item.transform.position);
                bool remembered = item.Id == rememberedId;
                bool inRange = distance <= radius * radius;
                
                if (!inRange && !remembered) continue;

                bool visible = false; // Assume not visible until proven otherwise.
                if (inRange)
                {
                    Vector3 eyePosition = transform.position + Vector3.up; // Assume the rival's "eyes" are 1 unit above its position.
                    Vector3 pickupPosition = item.transform.position + Vector3.up * .6f; // Assume the pickup's center is 0.6 units above its position.

                    bool sighObstructed = Physics.Linecast(eyePosition, pickupPosition, match.SightBlockers, QueryTriggerInteraction.Ignore);

                    visible = !sighObstructed;

                    if (showSightChecks)
                    {
                        // Draw a line in the Scene view to visualize the sight check. Green if visible, red if blocked.
                        Debug.DrawLine(eyePosition, pickupPosition, visible ? Color.green : Color.red, sighCheckDisplaySeconds, depthTest: false);
                    }
                }
                
                bool pickupIsBlocked = isBlocked(item.Id);
                // Broad-phase candidates are data to process, not automatically known targets.
                candidates.Add(new Candidate(
                    id: item.Id,
                    distanceSquared: distance,
                    available: true,
                    visible: visible,
                    remembered: remembered,
                    blocked: pickupIsBlocked));
            }
            return new Observation(actor.Carrying, rememberedId, candidates.ToArray());
        }

        private void OnDrawGizmosSelected()
        {
            if (!ShowPerceptionRange) return;

            const int segments = 64;
            
            // The sensor measures horizontal distance, so draw an XZ circle
            // Raise it slightly to keep it above the floor.
            Vector3 centre = transform.position + Vector3.up * 0.05f;

            Color previousColour = Gizmos.color;
            Gizmos.color = Color.cyan;

            Vector3 previousPoint = centre + Vector3.right * radius;

            for (int i = 1; i <= segments; i++)
            {
                float angle = i * (2f * Mathf.PI / segments);

                Vector3 nextPoint = centre + new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);

                Gizmos.DrawLine(previousPoint, nextPoint);
                previousPoint = nextPoint;
            }

            Gizmos.color = previousColour;
        }
    }
}
