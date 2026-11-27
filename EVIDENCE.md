# Week 1 evidence

<!--Keep this short. Bullet points, a small table and one diagram or code trace are enough. This is your checkpoint record, not an extra report. Replace the prompts as you work.-->

Name: Sebastian Valck
Exact Unity version: Unity 6000.3.26f1
Checkpoint commit / repository: https://github.com/wh1191/SalvageSprint
Current stage and anything unfinished: Stage 5

## Initial prediction

Three observations the rival needs, including one it might not have:
- Whether the rival is already carrying cargo (`actor.Carrying`), which determines if it should deliver instead of collect.
- Available pickup candidates with distance and whether they are visible or remembered (`PickupSensor.Capture`).
- Whether a target route recently failed (`isBlocked` / `failedUntil`), which the rival may not initially have before attempting travel.


Seeing a pickup across a gap proves / does not prove reachability because:
- Does not prove reachability: line-of-sight (`Physics.Linecast` in `PickupSensor.Capture`) 
only checks whether sight blockers sit between eye and pickup height, not whether a walkable 
`NavMesh` path exists across a gap or onto an isolated platform.

One question about NPC behaviour:
- When a known pickup fails navigation and no other pickups are visible, should the rival cycle 
immediately the survey points or stay near known pickups until the block cooldown expires?

## One complete trace

<!-- Use one actual situation. Include the observation data, derived facts, 
selected task and reason, movement request/result, and how feedback 
affects a later decision. Identify the responsible code locations. 
A tiny state diagram can accompany this trace. -->

Scenario: Rival holds cargo and delivers it to score toward the five point goal (Scenario F2).

**From code:**
1. Observation (`PickupSensor.Capture` in `PickupSensor.cs`): Captures `actor.Carrying = true`, `rememberedId = -1`, and in-range/remembered candidates.
2. Derived facts (`DecisionPolicy.Infer` in `DecisionPolicy.cs`): Sees `observation.Carrying == true` and returns `AgentFacts(carrying: true, selectedTargetId: -1, retainedTarget: false)`.
3. Selected task and reason (`DecisionPolicy.Choose` in `DecisionPolicy.cs`): Maps `NeedsDelivery` to `Decision(TaskKind.Deliver, -1, "Cargo held: return to depot")`.
4. Movement request (`RivalBrain.Update` -> `Navigator.Request` / `Navigator.Poll` -> `NavigationFeedback.Classify`): Clears `rememberedId` and calls `navigator.Request(-2, actor.DepotPosition)` to navigate to the depot.
5. Gameplay feedback (`MatchController.Update`): When `actor.Carrying` is true and the rival is within `1.7f` (`1.7f * 1.7f` flat distance) of `actor.DepotPosition`, `MatchController.Update` calls `State.TryDeliver(actor.Team)` to bank a point toward the score goal.
**From Play mode:**
Rival switched to `Deliver` after collecting scrap, moved to the red depot, scored a point, and then transitioned to `Collect` or `Explore`.

Explore -> Collect -> Deliver -> Explore/Collect
or
Collect -> Explore/Collect (on target taken/blocked)

## Scenario record

| Scenario | Predicted result | Observed result and short evidence / remaining issue |
|---|---|---|
| Normal loop and restart | Collect scrap, deliver to depot for scoring more points, repeat, and restart cleanly with `R`. | Observed in F2: Collected and delivered scrap, score updated, R restarted match cleanly |
| Carrying cargo | Prioritize `Deliver` (`-1`) to depot before collecting more pickups. | Observed in F3: Rival headed to depot first while carrying cargo |
| Hidden / remembered pickup | Ignore hidden unknown pickups, keep or select visible or remembered available pickups. | Observed in F4 Hidden unknown scrap ignored, remembered/visible scrap pursued |
| Target taken | Release unavailable target (`!match.IsAvailable`) and pick another eligible target or `Explore`. | Observed in F5: When target was taken, rival cleared memory and switched task/target |
| Unreachable target | Mark failed target blocked for 6s (`failedUntil`), drop commitment, and try alternative or `Explore`. | Observed in F6: Failure logged, target blocked for 6s, rival switched to alternative/Explore |
| No usable pickup | Choose `Explore` (`-1`) and cycle survey points without errors or invented targets. | Observed in F7: Cycled survey points in Explore with TargetId = -1 and no exceptions |

Supplied test result, with any added tests identified: 17/17 passed in EditMode (CoreTests + Week1PolicyTests); no test modifications
Live walkthrough checked by / recording link if used: See Recording 1
<!--Record what you observed while playing-->

## One correction and one independent check

The original weakness and why it happened:
An earlier version of the starter policy, `DecisionPolicy.Infer` only checked `Available` 
and `DistanceSquared`. It ignored `Visible`, `Remembered`, and `Blocked`, and did not 
retain `CurrentTargetId` causing the rival to chase hidden unknown pickups, abandon 
valid targets for closer distractions, or repeatedly retry blocked routes.

What I changed and why it addresses the cause:
My latest code was taken from the recent update that my teacher uploaded through GitHub, 
so the Stage 2–4 logic was already present. I reviewed the code, updated 
`DecisionPolicy.Label`, and added inline comments to document the three-part fix:
1. `IsEligible` enforces `Available && (Visible || Remembered) && !Blocked` so unknown or 
failed routes are excluded.
2. `Infer` prioritizes `Carrying` (`Deliver`), then retains `CurrentTargetId` if still 
eligible (`RetainedTarget = true`), before falling back to the nearest eligible 
candidate (`RetainedTarget = false`) or `Explore` (`-1`).
3. `Choose` separates the `Collect` reason strings for retained vs. newly selected targets.

My counterexample or experiment: prediction, observed result, and one limitation:
**Experiment:** Blocked remembered target in scenario F6 (Unreachable), see both Recording 1 and Recording 2.
**Prediction:** Once navigation to the remembered pickup fails, `failedUntil` marks it blocked for 6 seconds, so `IsEligible` rejects it despite `Remembered == true` and the rival switches to another eligible pickup or `Explore`.
**Observed result:** After failing to reach the isolated pickup, the debug panel reported a 6 second route block, dropped the target instead of staying stuck on it, and switched to `Explore` / another eligible target until the 6 second cooldown expired.
**Limitation:** Straight-line `DistanceSquared` is not actual NavMesh path length, and the 6 second cooldown expires by timer rather than checking whether the route actually became navigable.

## Collaboration and help

- People I discussed or playtested with.
Hampus and Abbie

- Main code responsibilities.
| File | Important functions/types | Purpose and connection to other code |
|---|---|---|
| DecisionPolicy.cs | Infer(), Choose()| The AI makes and Observation, and we send that in the the policy script,  it returns a Decision with the enum value TaskKind.X where X can be like "Collect" for example |
| DecisionModel.cs | TaskKind, TravelStatus, Candidate, Observation, Decision, AgentFacts, NavigationFeedback(), Classify()| DecisionModel contains definitions and implementations for presenting data for logical usage in other scripts.|
| PickupSensor.cs | Capture()| The range for the AI to interact with scrap, if close enough, then pick it up.|
| RivalBrain.cs | Awake(), Update(), IsBlocked(), | Manages the AI:s decision-making logic, by gathering and processing information (data) it requests from the other scripts|

- Describe one concrete example.
observation -> derived facts -> decision -> movement -> gameplay feedback

- Name the functions involved. Separate what we observed while playing
from what we established by reading the code.
When TaskKind is set to "Deliver" (TaskKind.Deliver), the logic of the RivalBrain starts moving the GameObject towards the position of the depot. The reason is that the AI has its current value of "carrying" set to true, which makes the AI want to go to the depo, so its able to get a point for dropping off the scrap at the depo. As the AI:s main goal is to get 5 points first.

- Explain one deliberate weakness.
That its deliberately ignoring data such as "Visible", "Remembered" and "Blocked" which makes the AI not play as efficently as the player can.

- Which condition or missing check causes it?
That we dont have any logic in the if statement that allows us to check the values/status of "Visible", "Remembered" and "Blocked".

- What behaviour should the corrected version produce?
That it gives priority to certain scrap pieces, to make use of its time more efficently. As it can avoid needing to take certain routes that are inefficent for reachig its main goal.

- Code, sources or substantive tools/AI assistance I used, and how I verified them.
Traced `DecisionPolicy`, `DecisionModel`, `PickupSensor`, and `RivalBrain` and 
verified behavior against the Unity EditMode tests (`CoreTests` and `Week1PolicyTests`) 
and `F2`–`F7` scenarios in Unity.

If unfinished: what works, exact reproduction steps, and the help I need:
