using System;
using System.Collections.Generic;
using SalvageSprint.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SalvageSprint
{
    [DefaultExecutionOrder(-100)]
    public sealed class MatchController : MonoBehaviour
    {
        [SerializeField] private RaceActor[] actors;
        [SerializeField] private SalvagePickup[] pickups;
        [SerializeField, Min(1)] private int scoreGoal = 5;
        [SerializeField, Min(1)] private float matchSeconds = 120;
        [SerializeField] private LayerMask sightBlockers;
        [SerializeField] private string scenarioName = "Normal race";
        [SerializeField] private bool initialRivalCargo;
        private readonly Dictionary<int, SalvagePickup> byId = new Dictionary<int, SalvagePickup>();
        private bool restarting;
        public MatchState State { get; private set; }
        public bool IsRunning => State != null && State.Outcome == MatchOutcome.Running;
        public IReadOnlyList<SalvagePickup> Pickups => pickups;
        public LayerMask SightBlockers => sightBlockers;
        public string ScenarioName => scenarioName;

        public void Configure(RaceActor[] competitors, SalvagePickup[] items, LayerMask blockers,
            string scenario, float seconds = 120, bool rivalCargo = false)
        {
            actors = competitors; pickups = items; sightBlockers = blockers;
            scenarioName = scenario; matchSeconds = seconds; initialRivalCargo = rivalCargo;
        }

        private void Awake()
        {
            var ids = new List<int>();
            foreach (SalvagePickup item in pickups)
            {
                byId.Add(item.Id, item);
                if (item.gameObject.activeSelf) ids.Add(item.Id);
            }
            State = new MatchState(ids, scoreGoal, matchSeconds);
            if (initialRivalCargo && pickups.Length > 0 && State.TryCollect(1, pickups[0].Id))
                pickups[0].gameObject.SetActive(false);
        }

        private void Update()
        {
            if (!IsRunning) return;
            State.Tick(Time.deltaTime);
            if (!IsRunning) return;

            // Shared rules, deliberately outside AI. Player wins an exact same-frame pickup tie.
            foreach (RaceActor actor in actors)
            {
                if (!IsRunning) break;
                Vector3 position = actor.transform.position;
                if (actor.Carrying && FlatDistanceSquared(position, actor.DepotPosition) <= 1.7f * 1.7f)
                    State.TryDeliver(actor.Team);
                if (!IsRunning || actor.Carrying) continue;
                foreach (SalvagePickup item in pickups)
                {
                    if (!State.IsAvailable(item.Id) || FlatDistanceSquared(position, item.transform.position) > 1.1f * 1.1f)
                        continue;
                    if (Physics.Linecast(position + Vector3.up * .5f, item.transform.position + Vector3.up * .5f,
                        sightBlockers, QueryTriggerInteraction.Ignore)) continue;
                    if (State.TryCollect(actor.Team, item.Id)) item.gameObject.SetActive(false);
                    break;
                }
            }
        }

        // Availability is public game feedback; positions are learned through the sensor.
        public bool IsAvailable(int id) => State != null && State.IsAvailable(id);
        public SalvagePickup GetPickup(int id) => byId.TryGetValue(id, out SalvagePickup item) ? item : null;

        public bool SimulateTargetTaken(int id)
        {
            // Only used by the clearly labelled target-loss scene, never by the normal race.
            if (!State.TryCollect(0, id)) return false;
            GetPickup(id).gameObject.SetActive(false);
            return true;
        }

        public void Restart()
        {
            if (restarting) return;
            restarting = true;
            SceneManager.LoadScene(SceneManager.GetActiveScene().path);
        }

        public static float FlatDistanceSquared(Vector3 a, Vector3 b)
        { float x = a.x - b.x, z = a.z - b.z; return x * x + z * z; }
    }
}
