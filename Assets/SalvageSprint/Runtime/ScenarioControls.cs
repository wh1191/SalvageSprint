using SalvageSprint.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace SalvageSprint
{
    public sealed class ScenarioControls : MonoBehaviour
    {
        public const string SceneFolder = "Assets/SalvageSprint/Generated/Scenes/";
        [SerializeField] private MatchController match;
        [SerializeField] private RivalBrain rival;
        [SerializeField] private bool simulateTargetLoss;
        private bool taken;
        public void Configure(MatchController owner, RivalBrain brain, bool targetLoss)
        { match = owner; rival = brain; simulateTargetLoss = targetLoss; }

        private void Update()
        {
            Keyboard k = Keyboard.current;
            string scene = null;
            if (k != null)
            {
                if (k.f2Key.wasPressedThisFrame) scene = "01_Normal";
                if (k.f3Key.wasPressedThisFrame) scene = "02_Cargo";
                if (k.f4Key.wasPressedThisFrame) scene = "03_Occlusion";
                if (k.f5Key.wasPressedThisFrame) scene = "04_TargetTaken";
                if (k.f6Key.wasPressedThisFrame) scene = "05_Unreachable";
                if (k.f7Key.wasPressedThisFrame) scene = "06_NoSalvage";
            }
            if (scene != null) { SceneManager.LoadScene(SceneFolder + scene + ".unity"); return; }
            if (!simulateTargetLoss || taken || !match.IsRunning || Time.timeSinceLevelLoad < 1) return;
            Decision decision = rival.CurrentDecision;
            if (decision.Task == TaskKind.Collect && match.SimulateTargetTaken(decision.TargetId))
            { taken = true; Debug.Log("SCENARIO: player took the rival's target remotely to reproduce target loss."); }
        }
    }
}
