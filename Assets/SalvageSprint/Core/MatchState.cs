using System;
using System.Collections.Generic;

namespace SalvageSprint.Core
{
    public enum MatchOutcome { Running, PlayerWins, RivalWins, Draw }

    // Ordinary C#: scoring has no dependency on scene objects, input, or AI.
    public sealed class MatchState
    {
        private readonly HashSet<int> available = new HashSet<int>();
        private readonly int[] scores = new int[2];
        private readonly bool[] cargo = new bool[2];
        public int Goal { get; }
        public double TimeRemaining { get; private set; }
        public MatchOutcome Outcome { get; private set; }

        public MatchState(IEnumerable<int> pickupIds, int goal, double seconds)
        {
            if (pickupIds == null) throw new ArgumentNullException(nameof(pickupIds));
            if (goal < 1) throw new ArgumentOutOfRangeException(nameof(goal));
            if (double.IsNaN(seconds) || double.IsInfinity(seconds) || seconds <= 0)
                throw new ArgumentOutOfRangeException(nameof(seconds));
            foreach (int id in pickupIds)
                if (id < 0 || !available.Add(id))
                    throw new ArgumentException("Pickup IDs must be unique and non-negative.");
            Goal = goal;
            TimeRemaining = seconds;
            Outcome = MatchOutcome.Running;
        }

        public int Score(int team) { ValidateTeam(team); return scores[team]; }
        public bool HasCargo(int team) { ValidateTeam(team); return cargo[team]; }
        public bool IsAvailable(int id) => available.Contains(id);

        public bool TryCollect(int team, int id)
        {
            ValidateTeam(team);
            if (Outcome != MatchOutcome.Running || cargo[team] || !available.Remove(id)) return false;
            cargo[team] = true;
            return true;
        }

        public bool TryDeliver(int team)
        {
            ValidateTeam(team);
            if (Outcome != MatchOutcome.Running || !cargo[team]) return false;
            cargo[team] = false;
            scores[team]++;
            if (scores[team] >= Goal)
                Outcome = team == 0 ? MatchOutcome.PlayerWins : MatchOutcome.RivalWins;
            return true;
        }

        public void Tick(double seconds)
        {
            if (double.IsNaN(seconds) || double.IsInfinity(seconds) || seconds < 0)
                throw new ArgumentOutOfRangeException(nameof(seconds));
            if (Outcome != MatchOutcome.Running) return;
            TimeRemaining = Math.Max(0, TimeRemaining - seconds);
            if (TimeRemaining > 0) return;
            Outcome = scores[0] > scores[1] ? MatchOutcome.PlayerWins
                : scores[1] > scores[0] ? MatchOutcome.RivalWins : MatchOutcome.Draw;
        }

        private static void ValidateTeam(int team)
        {
            if (team != 0 && team != 1) throw new ArgumentOutOfRangeException(nameof(team));
        }
    }
}
