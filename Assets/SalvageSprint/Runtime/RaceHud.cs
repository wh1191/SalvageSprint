using SalvageSprint.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SalvageSprint
{
    public sealed class RaceHud : MonoBehaviour
    {
        [SerializeField] private MatchController match;
        [SerializeField] private RivalBrain rival;
        private bool debugVisible = true;
        private GUIStyle title, text;
        public void Configure(MatchController owner, RivalBrain brain) { match = owner; rival = brain; }
        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.f1Key.wasPressedThisFrame)
                debugVisible = !debugVisible;
        }

        private void OnGUI()
        {
            if (match == null || match.State == null) return;
            if (title == null)
            {
                title = new GUIStyle(GUI.skin.label) { fontSize = 25, fontStyle = FontStyle.Bold };
                text = new GUIStyle(GUI.skin.label) { fontSize = 16, wordWrap = true };
            }
            float scale = Mathf.Max(.65f, Mathf.Min(Screen.width / 1280f, Screen.height / 800f));
            Matrix4x4 original = GUI.matrix;
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1));
            float width = Screen.width / scale, height = Screen.height / scale;
            GUI.Box(new Rect(14, 12, width - 28, 130), "");
            GUI.Label(new Rect(30, 20, 350, 36), "SALVAGE SPRINT", title);
            GUI.Label(new Rect(30, 60, width - 60, 28),
                $"PLAYER {match.State.Score(0)}/{match.State.Goal}    RIVAL {match.State.Score(1)}/{match.State.Goal}"
                + $"     TIME {System.Math.Ceiling(match.State.TimeRemaining):0}s"
                + $"     Your cargo: {(match.State.HasCargo(0) ? "1 scrap" : "empty")}", text);
            GUI.Label(new Rect(30, 86, width - 60, 25),
                "WASD / arrows / left stick: move     Auto pickup + delivery     R: restart     F1: AI debug", text);
            GUI.Label(new Rect(30, 112, width - 60, 25),
                "F2: race   F3: cargo   F4: hidden pickup   F5: target taken   F6: unreachable   F7: no salvage", text);

            if (debugVisible)
            {
                GUI.Box(new Rect(14, height - 147, width - 28, 133), "");
                int visible = 0, remembered = 0, blocked = 0;
                if (rival.LastObservation.Candidates != null)
                    foreach (Candidate c in rival.LastObservation.Candidates)
                    { if (c.Visible) visible++; if (c.Remembered) remembered++; if (c.Blocked) blocked++; }
                GUI.Label(new Rect(30, height - 138, width - 60, 25),
                    match.ScenarioName + "  |  " + DecisionPolicy.Label, text);
                GUI.Label(new Rect(30, height - 112, width - 60, 25),
                    $"Task: {rival.CurrentDecision.Task}   Target: {rival.CurrentDecision.TargetId}   Travel: {rival.Travel}"
                    + $"   Cargo: {match.State.HasCargo(1)}   Decisions: {rival.DecisionCount}   Requests: {rival.DestinationRequests}", text);
                GUI.Label(new Rect(30, height - 86, width - 60, 25),
                    $"Visible: {visible}   Memory: {remembered}   Blocked: {blocked}"
                    + $"   NeedsDelivery: {rival.CurrentFacts.NeedsDelivery}   UsableTarget: {rival.CurrentFacts.HasUsableTarget}", text);
                GUI.Label(new Rect(30, height - 60, width - 60, 40), "Last failure: " + rival.LastFailure, text);
                GUI.Label(new Rect(30, height - 37, width - 60, 25), "Reason: " + rival.CurrentDecision.Reason, text);
            }
            if (!match.IsRunning)
            {
                float x = (width - 420) / 2, y = (height - 190) / 2;
                GUI.Box(new Rect(x, y, 420, 190), "");
                string result = match.State.Outcome == MatchOutcome.PlayerWins ? "YOU WIN"
                    : match.State.Outcome == MatchOutcome.RivalWins ? "RIVAL WINS" : "DRAW";
                GUI.Label(new Rect(x + 30, y + 24, 360, 42), result, title);
                GUI.Label(new Rect(x + 30, y + 73, 360, 30), "Bank scrap at your coloured depot.", text);
                if (GUI.Button(new Rect(x + 30, y + 118, 360, 42), "PLAY AGAIN")) match.Restart();
            }
            GUI.matrix = original;
        }
    }
}
