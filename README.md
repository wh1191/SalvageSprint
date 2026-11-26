# Salvage Sprint

Your Game AI assignment project for the whole course. Keep the same project and repository through January.

**Start with your current stage. You do not need to understand every file before making progress.** The supplied code handles the game, input, scoring and navigation. Your main implementation work this week is in **[DecisionPolicy.cs](Assets/SalvageSprint/Core/DecisionPolicy.cs)**.

This README is your work order. [EVIDENCE.md](EVIDENCE.md) holds brief observations and your checkpoint record. Omniway holds announcements, submission links and official course information. There is no separate lab project, lab repository or lab submission.
After Thursday’s reference release, `main` will contain the completed Week 1 reference implementation. The original policy is preserved under `week1-before-fixes`. Keep working in your existing repository.. Read about how to use it here: [Teacher versions: tags and reference code](#teacher-versions-tags-and-reference-code).

**Thursday 26 November: continue from your current stage.** We will trace unexpected behaviour, complete the perception, commitment and recovery rules, then verify the six scenarios. If Stages 2–3 are unfinished, begin there with support. Keep your existing repository and record brief evidence as you work.

## Where to start

| Your situation | Next step |
|---|---|
| The game or IDE does not work | Ask for setup help; use Stage 0 below. |
| Wednesday: the game runs but the AI code feels confusing | Start with **Wednesday: read one decision together**. |
| You can explain the starter's target selection | Begin Stage 2. |
| Your policy respects visibility and memory | Begin Stage 3. |
| Your rival retains useful targets | Begin Stage 4. |
| Your policy handles failed routes | Complete Stage 5 and the checkpoint. |

## This week

| When | Focus | Intended stopping point |
|---|---|---|
| Mon 23 Nov | Short preparation and setup check | Predictions; no implementation required |
| Tue 24 Nov, 09:00–15:30 | Stages 0–1: play, navigation and feedback | Working baseline commit and one trace |
| Wed 25 Nov, 09:00–15:30 | Read code together, then Stages 2–3 | Known targets and stable task choices |
| Thu 26 Nov, 09:00–12:00 | Stages 4–5 and supported completion | Recovery, scenario checks and checkpoint commit |
| Thu afternoon | Independent completion if needed | Finish the current step and evidence |
| Fri 27 Nov, 23:59 | Part 1 checkpoint | Repository link, exact commit and EVIDENCE.md |

Friday has other scheduled activities. Aim to finish during Thursday's session if you can. Part 1 is formative/checkpoint: feedback, no separate formal grade. If unfinished, submit what works and identify the next problem. The single assignment receives one overall IG/G/VG grade after the final submission on **22 January 2027 at 23:59**.

This week focuses on Explore, Collect and Deliver. Behaviour trees, A*, steering/group behaviour, GOAP and the danger response belong to January.

## Teacher versions: tags and reference code

**Keep working in your existing assignment repository and on your normal working branch.** The teacher's versions are for comparison and support.

A **tag** names a particular saved commit: a snapshot of the project at that point. Later changes to a branch do not update that tagged snapshot.

| Teacher tag | What it contains | When to use it |
|---|---|---|
| `week1-before-fixes` | The teaching baseline before the Stage 2–4 policy corrections, including deliberate weaknesses | Revisit the original behaviour and understand what a correction changes |
| `week1-reference` | One completed implementation of the Week 1 policy rules | Compare approaches, investigate a failure or recover when stuck; the teacher will announce its publication after verification |

The reference is supporting material, not an additional assignment or a new project to submit.

### View a version or compare the changes

1. Open the [teacher-provided repository](https://github.com/CalleFreme/gp26-ai-salvage-sprint-starter) on GitHub.
2. Open the branch selector above the file list, select **Tags**, and choose the announced tag. You can also use the repository's [tags page](https://github.com/CalleFreme/gp26-ai-salvage-sprint-starter/tags).
3. Open `SalvageSprint/Assets/SalvageSprint/Core/DecisionPolicy.cs` and inspect `Infer`, the eligibility rule and `Choose`.
4. Once both tags have been published, use the [before/after comparison](https://github.com/CalleFreme/gp26-ai-salvage-sprint-starter/compare/week1-before-fixes...week1-reference). Read each changed block alongside the corresponding README stage.

Browsing a tag on GitHub leaves your own working files unchanged. Teacher tags and updates do not automatically appear in repositories created from the template. View the teacher/template repo on GitHub.com to read and compare them; no local tag checkout or merge is required for this workflow.

### Use the reference in your own work

1. Commit your current progress before applying changes, so you can compare with or return to it.
2. Identify one failing case. Compare the relevant function with your implementation and explain the difference before changing it.
3. Implement or copy only the correction you need into your existing project. Preserve your other changes, scenes and `EVIDENCE.md`.
4. Run the relevant tests, then all supplied tests, and replay the affected scenario. Extra teacher example tests may increase the total; the original suite contains 17 tests.
5. Record reference code you used under **Collaboration and help** in `EVIDENCE.md`, including what you changed and how you checked it. Commit the result.

You may use the supplied reference with acknowledgement and understanding. Your checkpoint still needs your own trace, scenario results, correction and independent check. Submit your own repository and exact commit. The optional `week1-checkpoint` tag mentioned below belongs to your repository and marks your own work; it is separate from the teacher's tags.

## Stage 0 — Setup, controls and baseline

Our baseline is **Unity 6000.3.26f1 (Unity 6.3 LTS)**. If another released 6000.3.x patch is already installed, keep it for now and record its exact version. Ask for help if setup is blocked.

1. On the starter repository, choose **Use this template → Create a new repository**, then clone your own copy. ZIP download and creating your own repository is also supported; retain the whole project, including README, EVIDENCE.md and configuration files.
2. If you create your repository after the reference/solution release, it will already contain the completed policy. To practice Stages 2-4 from the original starting point, copy only `DecisionPolicy.cs` from `week1-before-fixes` into your project.
3. Add the nested **SalvageSprint** folder to Unity Hub. It contains Assets, Packages and ProjectSettings. Wait for import and compilation; enable the Input System backend and restart if prompted.
3. For Visual Studio, check Package Manager for **Visual Studio Editor** (`com.unity.ide.visualstudio`); install it if missing. Under **Edit → Preferences → External Tools**, select your installed Visual Studio and **Regenerate project files**. Open a script from Unity and check type recognition/code completion. The package dependency belongs in the committed package files; generated solution/project files stay ignored.
4. Open `Assets/SalvageSprint/Generated/Scenes/01_Normal.unity`. If absent, run **Tools → Salvage Sprint → Create Course Scenes** once. Tell the teacher if creation fails. This is setup, not an AI implementation task. Re-running the builder does not reset existing scenes.
5. Play: WASD/arrows or gamepad left stick. Collect gold scrap automatically; carry one piece and deliver to the cyan depot. First to five deliveries wins; at the two-minute timeout, highest banked score wins, or equal scores draw.
6. R restarts; F1 toggles AI debug; F2–F7 select scenarios. Click Game view for keyboard input. Select Rival in Scene view with Gizmos enabled to inspect sensing range and path drawing.
7. Confirm scoring, result and restart. Report infrastructure failures rather than redesigning those systems.
8. Commit the baseline: Assets and their `.meta` files, including generated scenes/materials/NavMesh data; Packages including its lockfile; ProjectSettings; README and EVIDENCE.md. Keep Unity caches, IDE-generated files and build output ignored. The repository-root ignore file covers root-level `.vs/`; the Unity-project ignore file covers files beneath SalvageSprint.

**Done:** a playable, committed baseline. Suggested commit: `week1: working baseline`.

## Stage 1 instructions can be found summarized towards the end of the README. Finish it before or after the Wednesday investigations.
**Make sure to use the fixed version of `Navigator.cs` and the `NavigationFeedback` class's `Classify` method in `DecisionModel.cs`.**

## Wednesday: read one decision together

Investigations 1–2 describe the original policy preserved under `week1-before-fixes`. If you are viewing the completed reference, open the baseline version on GitHub alongside it. The reference’s conditions, control flow and test results will differ.

Begin with the group reading and prediction tasks below; you will edit the policy in Stage 2 afterwards. Predict each answer before reading “Check together.”

Work in groups of **2–3**, as arranged in class (contact CJ if you want to do this but did not attend Wednesday). Take turns operating the computer and explaining. Everyone should explain at least one case. Then make changes in your own repository. These investigations prepare Stages 2–3; they are not extra submissions.

### First: the small part of the code you need

All paths below are relative to this Unity project folder, `SalvageSprint/`.

| File | Read now | Your responsibility |
|---|---|---|
| [Core/DecisionPolicy.cs](Assets/SalvageSprint/Core/DecisionPolicy.cs) | `Choose(AgentFacts)` and `Infer` | Main implementation file, Stages 2–4 |
| [Core/DecisionModel.cs](Assets/SalvageSprint/Core/DecisionModel.cs) | `Candidate`, `Observation`, `AgentFacts`, `Decision` | Understand the values passed between functions |
| [Runtime/RivalBrain.cs](Assets/SalvageSprint/Runtime/RivalBrain.cs) | The capture/infer/choose calls and task branches in `Update` | See where decisions enter the running game |

Read `PickupSensor.Capture` when investigating visibility, and `RivalBrain.IsBlocked` when investigating recovery. `Navigator`, `RaceHud`, `MatchState`, `MatchController` and the scene builder are supplied infrastructure. Investigate a particular section when a question requires it; you do not need to rewrite them.

### Investigation 1: choose a task

**Open:** `DecisionPolicy.cs`, the overload `Choose(AgentFacts facts)`. An overload is a function with the same name but different parameters. Start with this one; it receives already-processed facts.

**Read:** `if (facts.NeedsDelivery)` gives delivery priority. Otherwise, the conditional expression `condition ? firstResult : secondResult` chooses the first result when the condition is true, and the second when it is false. Here that selects Collect or Explore.

Predict the task for each row, then point to the responsible branch:

| NeedsDelivery | HasUsableTarget | SelectedTargetId | Your predicted task |
|---|---|---|---|
| true | false | -1 | ? |
| false | true | 7 | ? |
| false | false | -1 | ? |

**Check together:** Deliver, Collect target 7, Explore. Choosing a task does not move the character or award cargo. The returned `Decision` contains a task, target ID and explanation.

### Investigation 2: follow the candidate loop

**Open:** `DecisionPolicy.Infer`, from the initial cargo check through its `foreach` loop and final return. Locate the related fields in `DecisionModel.cs` as needed.

| Term | Meaning in this project |
|---|---|
| `Observation` | One snapshot: cargo, current target ID and candidate data |
| `Candidate` | A possible pickup described by an ID, squared distance and flags; inclusion does not prove the rival knows it |
| `Available` | The pickup has not been taken |
| `Visible` | The pickup is within sensing range and has clear line of sight |
| `Remembered` | This is the target held in the rival's retained-target slot |
| `Blocked` | A failed route temporarily excludes this pickup once Stage 4 is implemented |
| `AgentFacts` | The policy's conclusions, used by `Choose` |
| `-1` target ID | No selected pickup; an ID is not an array index |

`new AgentFacts(carrying, selectedTargetId, retainedTarget)` creates the result of `Infer`. The first argument determines delivery need; the ID identifies the selected pickup; the last argument says whether it was retained. A non-negative selected ID makes `HasUsableTarget` true: the constructor does not independently check visibility or reachability. Your selection rule must make that result meaningful.

Each call starts a fresh local search with `nearest = -1`; this variable is separate from the remembered target passed into the observation. `distance = float.PositiveInfinity` lets the first qualifying finite distance become the best so far. `foreach` visits each candidate. A successful comparison updates both the selected **ID** and its distance.

Assume empty cargo and current remembered target ID 9. Read these rows in order:

| ID | Available | Visible | Remembered | DistanceSquared |
|---|---|---|---|---|
| 4 | true | true | false | 16 |
| 7 | true | false | false | 1 |
| 9 | true | false | true | 9 |

Track `nearest` and `distance` after each row. Which fields does the starter's condition actually inspect? Which candidate wins?

**Check together:** the selected IDs are 4, then 7, then still 7. The starter checks availability and distance, ignoring visibility and memory. Stage 2 should instead select 9: candidate 7 is hidden and unknown, while candidate 9 is remembered and nearer than candidate 4. These are illustrative data, not IDs to hard-code.

Squared distance is distance multiplied by itself: 16 represents 4 units and 9 represents 3 units. Comparing non-negative squared distances preserves their order; you do not need square roots to choose the nearer candidate.

### Investigation 3: connect the decision to the game

**Open:** `RivalBrain.Update`. Find these existing calls; do not replace them:

```csharp
LastObservation = sensor.Capture(actor, rememberedId, IsBlocked);
CurrentFacts = DecisionPolicy.Infer(LastObservation);
Decision next = DecisionPolicy.Choose(CurrentFacts);
```

Read this as: **capture observations → derive facts → choose a task**. Next, find the Deliver, Collect and Explore branches. Collect obtains the selected pickup and calls `navigator.Request` with its position. The navigator handles movement; `MatchController` confirms actual pickup and delivery.

Run the normal scene. Pause while the rival travels and identify its task, target, travel status and cargo in the lower debug panel. Resume and observe collection. What should the next decision be once cargo is held?

**Check together:** Deliver. The upper HUD's **Your cargo** is player cargo; the lower **Cargo** is rival cargo. Facts/reasons describe the latest decision snapshot, so a displayed fact can briefly lag behind an interaction.

Keep one useful observation for your existing EVIDENCE.md trace. You do not need to transcribe every function or submit these tables separately.

## Stage 2 — Choose from visible or remembered pickups
**From here on, you can look at the teacher repo's version of `DecisionPolicy` to get a sense of the fixes we need for our AI. It's a good idea to look at the commit which introduced these changes, so you can see exactly which lines of code changed**

**Goal:** a nearby hidden unknown pickup cannot win merely by being nearer.

**Open:** `Core/DecisionPolicy.cs`, `Infer(Observation observation)`. Your main change is the candidate eligibility rule inside the loop. Preserve the cargo-priority return and the fallback when no candidate qualifies.

### Predict before editing

Which candidates should Stage 2 allow before comparing distance?

| Available | Visible | Remembered | Eligible at Stage 2? |
|---|---|---|---|
| true | true | false | Yes |
| true | false | false | No |
| true | false | true | Yes |
| false | false | true | No |

**Rule in words:** the pickup must be available, and it must be visible or remembered. Distance ranks candidates only after this eligibility check. `Blocked` handling comes in Stage 4.

### Make one change

1. Find the existing condition that checks `candidate.Available` and compares `candidate.DistanceSquared` with `distance`.
2. Add the knowledge rule from the table. Named local booleans or a small helper are fine if they make your reasoning clearer.
3. Keep updating the selected ID and distance together. Do not select an ID when no candidate qualifies.
4. Explain why a remembered hidden candidate can qualify while an unknown hidden candidate cannot.

**Syntax help:** `&&` means both conditions must be true; `||` means at least one must be true. Parentheses group the visibility/memory choice. If needed, read only the `&&` and `||` sections of [Microsoft's Boolean operator reference](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/boolean-logical-operators). Then explain your own condition aloud; no full tutorial is required.

### Check the change

In **Window → General → Test Runner → EditMode**, run the three `Stage2_` tests in `Week1PolicyTests`:

- `Stage2_HiddenUnknownPickupIsNotAUsableTarget`
- `Stage2_VisibleAlternativeBeatsNearerHiddenPickup`
- `Stage2_PreviouslyKnownPickupMayRemainKnownOutsideSight`

Also run both `Regression_` tests: cargo must still take priority, and an empty world must produce Explore. One Stage 2 test already passes in the starter; keeping existing behaviour working is part of the task.

Restart into F4 after editing, then compare it with F2. A fresh run avoids retaining a target acquired by the old, incorrect policy. Use observations and the selected target to explain the result. If needed, inspect `Runtime/PickupSensor.cs`, `Capture`, where `visible` and `remembered` are recorded.

**Memory limit:** the scaffold retains one selected target, not a history of every pickup ever seen. The naive starter can select an unknown hidden target and subsequently mark it remembered. A Memory count alone therefore does not prove prior sight. The corrected acquisition rule is what makes that retained knowledge legitimate.

**Done:** the three Stage 2 tests pass, the regressions remain green, and you can explain one accepted and one rejected candidate. Expected overall result: **13 pass, 4 fail**, if no later-stage rules have changed.

Suggested commit: `week1: respect perception and memory`. Keep evidence brief: add the changed rule and one observed result to your existing trace.

## Stage 3 — Keep a useful target

**Goal:** avoid abandoning a useful task every time a closer pickup appears.

**Open:** `DecisionPolicy.Infer`. Read `Observation.CurrentTargetId` and `AgentFacts.RetainedTarget` in `DecisionModel.cs`.

### Predict two decisions

The rival currently pursues remembered target 4 at squared distance 16. Visible target 5 appears at squared distance 1. Both are available.

- Should it keep 4 or switch to 5?
- What if 4 has been taken?

**Required behaviour:** keep 4 while eligible; select another eligible candidate when 4 becomes unavailable. Commitment depends on continued usefulness, not distance alone.

### Implement in this order

1. Preserve delivery priority when cargo is held.
2. Look for a candidate whose `Id` matches `observation.CurrentTargetId`. Check that it meets your current eligibility rule; Stage 4 will add blocked-target rejection.
3. If that target qualifies, select it and set `RetainedTarget` to true.
4. Otherwise, use the Stage 2 search for the nearest eligible candidate. Set `RetainedTarget` to false for this new selection or for no target.
5. In `Choose(AgentFacts)`, make the Collect reason distinguish retaining a target from selecting a new one. Preserve the task and target behaviour.

Use the candidate's **ID** for matching. `Candidates[4]` means the fifth array entry, not pickup ID 4. The current target may be missing entirely; that must fall through to another useful choice.

### Check and explain

Run `Stage3_UsableCurrentTargetSurvivesCloserDistraction` and `Stage3_UnavailableRememberedTargetIsReleased`, then rerun Stage 2 and the regressions.

The unavailable-target test already passes in the starter. In `RivalBrain.Update`, find `!match.IsAvailable(rememberedId)` and the memory reset before sensing. In F5, identify how the chosen target becomes unavailable and how the rival releases it. You are explaining supplied invalidation, while adding commitment to a still-useful target.

Add a short state-transition description to EVIDENCE.md:

- Explore → Collect when an eligible pickup is selected.
- Collect → Deliver after successful collection gives cargo.
- Deliver → Collect or Explore after delivery, depending on available knowledge.
- Collect → another Collect target or Explore when the current target becomes unusable.

**Done:** useful targets survive closer distractions, unavailable targets are released, and `RetainedTarget` and the reason are truthful. Expected overall result: **14 pass, 3 fail**, if Stage 4 is unchanged.

Suggested commit: `week1: keep a usable task`.

## Stage 4 — Use failed-route feedback

**Goal:** an impossible route should lead to another useful choice.

**Observe:** open F6 and predict why a visible pickup may be unreachable. Inspect task, target, travel status and Last failure before editing.

**Read:** in `Runtime/RivalBrain.cs`, find the failed-travel branch in `Update`, the `failedUntil` dictionary and `IsBlocked`. Failure marks the selected pickup blocked for six seconds. `PickupSensor.Capture` passes that flag into candidate data. The policy must use it; the cooldown is not itself a completed recovery solution.

**Change:** in `DecisionPolicy.Infer`, reject blocked candidates when selecting a new target **and** when deciding whether to retain the current one. Preserve availability, knowledge, cargo priority and the no-target fallback.

**Check:** run the three `Stage4_` tests. They cover a blocked nearest candidate with an alternative, no usable alternative, and a blocked remembered target. Then run all supplied tests and revisit F4–F6. Explain both an alternative choice and the case where every known target is blocked.

**Done:** the policy no longer stays committed to a blocked target. A finite cooldown permits later retries; it does not prove that a route becomes valid. Suggested commit: `week1: recover from failed routes`.

## Stage 5 — Verify and preserve

Run all six scenes and record one brief result per scene in EVIDENCE.md:

| Key | Scenario | Completed behaviour |
|---|---|---|
| F2 | Normal race | Collect, deliver, continue, finish and restart |
| F3 | Cargo already held | Deliver before pursuing more scrap |
| F4 | Occlusion | Ignore unknown hidden targets; valid memory may persist |
| F5 | Target taken | Release the unavailable target and continue |
| F6 | Unreachable | Use failure feedback and temporarily avoid the target |
| F7 | No salvage | Explore without crashing or inventing pickup targets |

F5 uses a labelled helper that remotely transfers the selected pickup to the player once to reproduce target loss. This helper is absent from the normal race. F6 contains an isolated platform. Diagnostic scenes have fewer pickups and are not all normal five-point match layouts.

Choose **one** small independent counterexample or controlled experiment. State your prediction, observed result and a limitation. Examples: a blocked remembered target, equal-distance candidates, or the stopping-distance experiment from Stage 1. A concrete recorded scenario is enough; an additional unit test is optional. You may reuse a Stage 1 experiment if it meets these requirements.

Keep one complete observation → facts → decision → action → feedback trace, and explain one correction. Restore temporary geometry/movement experiments and preserve a working checkpoint commit.

## Tests: what green means

There are **17 supplied tests**: seven `CoreTests` infrastructure checks and ten `Week1PolicyTests`. Initial policy failures are deliberate tasks, not an installation fault.

| Milestone | Expected result if only these stages have changed |
|---|---|
| Starter / Stage 1 | 11 pass, 6 fail |
| Stage 2 | 13 pass, 4 fail |
| Stage 3 | 14 pass, 3 fail |
| Stage 4 | 17 pass, 0 fail |

Later rules or additional tests can change these counts. Read the failing name, inputs and assertion. Do not delete tests, alter their expected values or hard-code their candidate IDs. Passing tests check decision snapshots and game rules; gameplay checks also verify scene, input, physics and navigation integration.

## If you are stuck

Tell your group or teacher: **stage and method → input/example → expected behaviour → actual behaviour → last change**. A short code excerpt, test name or paused scene is more useful than “the AI does not work.” Ask for a smaller worked example if syntax or a data type blocks you. You can receive guided help and still explain and own your implementation.

## Part 1 checkpoint

Submit your repository link and exact commit through Omniway by **Friday 27 November, 23:59**. Confirm teacher access. Include the working project and **EVIDENCE.md**: one complete trace, six scenario results, one explained correction and one small counterexample/experiment. These can share the same concise entries; no separate report or lab submissions are needed.

Use an in-class live walkthrough when available, or an approximately two-minute recording. If Omniway requires an upload, submit a clean ZIP of this same project as instructed. If unfinished, submit the current state and a precise help request. Part 1 has no separate grade or VG feature quota.

## Collaboration, sources and January

Discuss, trace and playtest together. Each student implements in their own repository and can explain their decisions. Record reused code, sources and substantive assistance, including AI/tool assistance, in EVIDENCE.md. Verify and understand code you use.

Keep this repository and checkpoint for January. There is no required AI coursework during the game-project/break gap, and your grade does not depend on your game-project team using AI. January develops this same rival further. A `week1-checkpoint` tag is optional; an exact commit hash is sufficient.


<details>
<summary><strong>Stage 1 — Tuesday navigation work: reference and catch-up</strong></summary>

This stage introduces navigation and feedback. If unfinished, join Wednesday’s group investigations first, then complete the remaining steps during supported work time. You do not need to finish every navigation experiment before starting Stage 2.

The supplied motor is a worked example. Work through this with guidance; you do not need to write a navigation framework.

1. Inspect `Yard Geometry` → NavMeshSurface (walkable data) and `Rival` → NavMeshAgent (moving agent). Baked clearance and runtime avoidance radius are different settings.
2. Follow `RivalBrain.Update` → `Navigator.Request` → `SetDestination`. Then inspect `Navigator.Poll` and `NavigationFeedback.Classify` in **Core/DecisionModel.cs**.
3. Predict Pending for pending computation with unknown distance; Failed for a partial route even near its endpoint; Moving for a complete route four units away; Arrived for a complete route within the stopping threshold. Compare with `CoreTests.PendingPathIsNotMistakenForFailureOrArrival`.
4. Compare decision and route-request counts: repeated decisions can retain the same requested destination. Arriving at a navigation goal does not itself prove collection or delivery.
5. Outside Play mode, record a shelf's position, move it slightly, rebake with NavMeshSurface and inspect the changed walkable area. Observe a route if it crosses that area. Restore the shelf and rebake. Record what actually changed; an unaffected route need not change.
6. Temporarily change Rival's stopping distance from **0.6** to **1.6**, leaving collection radius **1.1**. Predict how arrival might occur without collection. Observe, then restore **0.6** and confirm collection. This runtime stopping-distance change does not require rebaking.
7. Change the policy `Label`, write a truthful reason and record one trace in EVIDENCE.md. No policy tests need to become green during Stage 1.

Optional API lookup when needed: [NavMeshAgent](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/AI.NavMeshAgent.html), [SetDestination](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/AI.NavMeshAgent.SetDestination.html), [AI Navigation](https://docs.unity3d.com/Packages/com.unity.ai.navigation@2.0/manual/index.html). Look up the specific component or property blocking your investigation; no complete tutorial is required.

</details>

<details>
<summary><strong>Original Monday preparation</strong></summary>

Spend about 15–25 minutes predicting: name three observations the rival needs and one that may be unavailable; explain whether seeing scrap proves reachability; write one question about NPC behaviour in a game you know. Keep notes or put them in EVIDENCE.md. No coding or repository setup was required for this preparation. Installation time is separate.

</details>
