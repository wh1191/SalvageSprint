# Salvage Sprint

This is your individual Game AI assignment project for the whole course. Keep your guided work, experiments, final implementation and evidence in this repository. There is no separate lab repository.

**Your two working documents:** this README tells you what to do next. [EVIDENCE.md](EVIDENCE.md) records your observations and checkpoint. Omniway contains announcements, submission links and the official course plan/Study Guide. Slides support class discussion; you do not need to search them for additional requirements.

## This week

| When | Work | Expected stopping point |
|---|---|---|
| Mon 23 Nov | 15-25 minutes of prediction; check Unity setup | Three observations and one NPC question; no assignment implementation expected yet |
| Tue 24 Nov, 09:00-15:30 | Stages 0-1, guided navigation and debugging | A working baseline commit and one traced navigation outcome |
| Wed 25 Nov, 09:00-15:30 | Stages 2-3, perception and task commitment | Known targets and stable task choices; route-recovery tests may still fail |
| Thu 26 Nov, 09:00-12:00 | Stages 4-5, recovery and completion clinic | A reliable rival and a short evidence record |
| Thu afternoon | Independent completion if needed | Finish the current stage and evidence; no taught afternoon session |
| Fri 27 Nov, 23:59 | Part 1 progress checkpoint | Repository link and exact commit, plus EVIDENCE.md |

Friday has other scheduled activities. Aim to finish during Thursday's clinic; Friday's deadline provides a submission buffer, not an additional AI teaching day. All times are Stockholm time. Part 1 has no separate formal grade. The single assignment has one overall IG/G/VG grade after the final submission on **22 January 2027 at 23:59**. Late or incomplete checkpoint work receives specific feedback and a completion route. Submit what works and identify what remains.

You are **not** expected to implement behaviour trees, GOAP, A*, group AI or January's danger response this week. Continue with the current stage instead of trying to complete future systems.

## Monday preparation

Before searching for answers, write short predictions at the top of EVIDENCE.md, or bring them to Tuesday if the repository is not available yet:

1. Name three observations a salvage rival needs. Which observations could be wrong or unavailable?
2. A rival can see scrap across a gap. Does seeing it prove it can reach it? Explain your prediction.
3. Write one question about believable or useful NPC behaviour from a game you know.

Check that Unity Hub can open **Unity 6.3 LTS**. Our teaching baseline is **6000.3.26f1**. If your school computer already has a newer released 6000.3.x patch, keep it for now and record the exact version. Do not upgrade during class merely to chase the newest patch. Installation time is separate from the short prediction exercise. Tell the teacher if setup is blocked; no coding is required on Monday.

## Stage 0 - Play and establish a baseline

**Purpose:** learn the game and establish a working checkpoint before changing AI.

1. Create your own repository from the supplied project using the course's announced route. Use it through January. You should create a repo by using _this_ repo _as a template_. You're also allowed to download as ZIP and create from that, as in the previous course. download a ZIP, extract it, keep Assets/Packages/ProjectSettings, initialise your repository and follow the teacher's submission/access instructions.
2. Open this folder in Unity Hub. Wait for package import and C# compilation. Enable the Input System backend and restart if Unity prompts you.
3. The teacher's prepared release should open `Assets/SalvageSprint/Generated/Scenes/01_Normal.unity`. If generated scenes are absent, use **Tools > Salvage Sprint > Create Course Scenes** once and let me know if you succeeded. This is a setup fallback, not an AI exercise.
4. Press Play. Move with WASD/arrows or a gamepad left stick. Pick up gold scrap automatically and bank it at the cyan depot. Carry one at a time. First to five banked scrap wins; otherwise the highest score wins at the two-minute timeout. Equal scores produce a draw.
5. R restarts. F1 toggles the AI debug view. F2 returns to the normal race. Select the rival in the Hierarchy with Scene Gizmos enabled to see sensing range and route drawing.
6. Play to a result and restart. Do not repair unrelated infrastructure yourself. Report compile, input, scoring or restart problems to the teacher.
7. Commit the baseline. Keep Assets and their `.meta` files, Packages, ProjectSettings and your evidence. Do not commit Library, Temp, Logs, generated IDE files or build output. Tests are source files under Assets, not files in bin/obj.

**Done:** the game runs, scores, finishes and restarts, and your baseline is committed. Suggested commit: `week1: working baseline`.

## Stage 1 - Explain and inspect navigation

**Support:** work through this with the teacher. The supplied motor is a worked example; you do not need to write an engine navigation framework.

1. Locate `Yard Geometry` and its NavMeshSurface, then the rival's NavMeshAgent. Explain which component describes walkable space and which executes movement. Inspect baked clearance separately from the runtime avoidance radius.
2. Locate `Runtime/Navigator.cs`. Trace one request from `RivalBrain` to `SetDestination`, then back through `Poll` and `Core/NavigationFeedback.Classify`.
3. Predict the result of four snapshots: pending with unknown remaining distance; a partial route near its endpoint; a complete route still four units away; and a complete route within stopping distance. Discuss before running the supplied tests.
4. Watch the debug panel's task, target, travel status, decision count and destination-request count. Explain why decisions can increase without requesting a new route each time.
5. In the stopped normal scene, move one existing shelf slightly, rebake the NavMesh using the NavMeshSurface Inspector, and predict the changed route. Run and observe. Restore the original shelf and rebake before saving the baseline. Keep all work in this project.
6. Try a rival stopping distance of **1.6** temporarily. The collection radius is **1.1**. Predict how the rival could report arrival while failing to collect. Restore the supplied **0.6** stopping distance afterwards. Do not change scoring rules to hide the mismatch.
7. Change the policy `Label` to identify your version. Add one truthful decision reason of your own and trace where it appears. Record one observation-to-action trace in EVIDENCE.md.

**Done:** you can distinguish a requested route, a pending route, a usable route and a completed game action. The supplied navigation feedback and game rules still work. No policy tests need to turn green during Stage 1.

## The code you will work with

| Location under Assets/SalvageSprint | Responsibility | This week's use |
|---|---|---|
| `Core/DecisionPolicy.cs` | Interpret observations and choose a task | Main implementation file, Stages 2-4 |
| `Core/DecisionModel.cs` | Candidate data, derived facts, intents and travel-result classification | Read the contracts; preserve supplied behaviour |
| `Runtime/PickupSensor.cs` | Range and line-of-sight observations | Trace visibility and remembered targets |
| `Runtime/RivalBrain.cs` | Cadence, target memory, failure cooldown and action dispatch | Inspect state across decisions; small justified changes are allowed |
| `Runtime/Navigator.cs` | Route request, status, cancellation and stall detection | Worked example and Stage 1 experiments |
| `Runtime/RaceHud.cs` | Visible AI facts, intent, reason and feedback | Use it; extend only when useful |
| `Core/MatchState.cs` and `Runtime/MatchController.cs` | Shared cargo, scoring and match rules | Supplied infrastructure; do not alter to satisfy AI tests |
| `Tests/EditMode` | Public behaviour checks | Compare tests with actual scene behaviour |

A range-query candidate is not automatically something the rival knows about. Static pickup positions become actionable through visibility or valid memory. Pickup availability changes are public game feedback in this exercise. The rival knows its depot and authored survey points. This is the information contract for this game, not a universal rule for every game genre.

## Stage 2 - Separate candidates from known targets

**Support:** guided reasoning, then your implementation.

1. Open `Core/DecisionPolicy.Infer`. Explain the current nearest-candidate baseline in your own words. Predict its choice in F4 before running it.
2. Write a tiny truth table: available/visible; available/hidden/unknown; available/hidden/remembered; unavailable/remembered. Decide which candidates are known and usable before considering distance.
3. Filter out unavailable candidates and unknown hidden candidates. A valid remembered target may remain known outside current sight. Preserve the supplied delivery priority and empty-world fallback.
4. Compare F4 with the normal race. Use facts and a decision reason to explain the difference.
5. Run the Stage2 tests. Also check the cargo and empty-world regressions.

**Done:** hidden unknown targets cannot win selection merely because they are nearer. Previously observed targets do not automatically disappear from knowledge when occluded. Suggested commit: `week1: respect perception and memory`.

## Stage 3 - Commit to a useful task

**Support:** a group trace, then your implementation.

1. Predict two successive decisions. The rival knows target 4, distance squared 16. A new visible target 5 appears at distance squared 1. Must the rival abandon its current task?
2. Retain the current remembered target while it remains usable. Do not rely on array position being the same as a pickup ID.
3. Set `RetainedTarget` truthfully. Use the existing facts and decision reason to explain commitment. You may restructure Infer into small helpers if that improves clarity.
4. Trace a stolen target in F5. Availability invalidation is supplied in the baseline. Explain which code releases the old target rather than pretending that every green check requires another repair.
5. Draw or describe the Explore/Collect/Deliver transitions in EVIDENCE.md. Include the conditions: discovery, successful collection, delivery and loss of a usable target.

**Done:** a closer distraction does not cause unnecessary target switching, and unavailable targets can still be released. Route-failure cases remain for Stage 4. Suggested commit: `week1: keep a usable task`.

## Stage 4 - Recover when execution fails

**Support:** independent diagnosis with hints available. You now have the concepts needed for this stage.

1. Open F6. Predict what the rival can see and why a complete route may still be impossible. Observe the weak behaviour before editing.
2. Find where a failed route becomes a temporary `Blocked` flag. What does the six-second cooldown mean? Does it prove the route will become valid later?
3. Decide how the blocked flag affects both acquiring a new target and retaining a remembered one. Implement that rule in your policy. Preserve cargo priority, memory and the no-target fallback.
4. Check an alternative reachable pickup and the case where **every** known pickup is blocked. Avoid an empty-list exception or invented target.
5. Revisit F4 and F5 to detect regressions. Improve the reason text so it reflects the rule you actually implemented.

**Hints, in order:** distinguish known from currently usable; check eligibility before nearest-distance ranking; check eligibility before committing to memory; use Explore when nothing qualifies. Ask for a smaller worked trace if you are stuck. Do not copy the teacher's whole solution.

**Done:** failed targets no longer trap the rival indefinitely. A finite cooldown can permit later retries; it is not a permanent blacklist or a reachability proof. Suggested commit: `week1: recover from failed routes`.

## Stage 5 - Verify and preserve your work

Run all six scenes and record a short result for each in EVIDENCE.md. Then choose **one** small independent counterexample or experiment. Examples: a blocked remembered target, two equally near candidates, or changing stopping distance while keeping pickup range fixed. State a prediction, observe the result and explain a limitation. An extra unit test is welcome but not compulsory; a concrete recorded scenario is sufficient.

| Key | Scenario | Required completed behaviour |
|---|---|---|
| F2 | Normal | Collect, bank cargo, continue, finish and restart |
| F3 | Cargo already held | Deliver before pursuing more scrap |
| F4 | Occlusion | Ignore an unknown hidden target; valid memory may persist |
| F5 | Target taken | Release unavailable target and continue useful work |
| F6 | Unreachable | Use failure feedback, temporarily avoid the target and choose another task |
| F7 | No salvage | Explore/wait as documented without crashing or inventing pickups |

F5 is a labelled diagnostic scene: a helper remotely transfers the chosen pickup to the player once to reproduce target loss. That synthetic action is absent from the normal race. F6 contains a visible isolated platform. These scenes test failures; their limited pickup counts are not normal five-point match layouts.

## Tests and what green means

Open **Window > General > Test Runner > EditMode**. There are **17 supplied tests**: seven infrastructure checks and ten policy checks. The unmodified starter should show **11 passing and 6 failing**. Initial failures are intentional behaviour gaps, not a broken test installation.

| Milestone | Expected supplied tests, if only that stage has changed |
|---|---|
| Starter / Stage 1 | 11 pass, 6 fail |
| Stage 2 complete | 13 pass, 4 fail |
| Stage 3 complete | 14 pass, 3 fail |
| Stage 4 complete | 17 pass, 0 fail |

Implementing later rules early can turn additional tests green. Adding your own tests changes the total. Read the names and assertions rather than chasing the count. Do not delete tests, change expected values, hard-code the fixture IDs, or replace sensor rules with an all-knowing world query.

These tests check decision snapshots and game-rule logic. They cannot prove that the actual scene, physics, input or NavMesh works. The six scene checks are required evidence too. Tests under Assets remain source-controlled. Unity recreates its generated assemblies; deleting generated output does not delete the test source.

## Part 1 checkpoint

Submit the repository link and exact commit through Omniway's announced submission location by **Friday 27 November at 23:59**. Make sure the teacher can access it. If the submission system requires an upload, provide a clean ZIP of this same project as instructed; do not start another project or repository.

Your checkpoint consists of the working project and **EVIDENCE.md**. A short live walkthrough or an approximately two-minute recording can demonstrate the result; you do not need an additional report or separate lab hand-ins. Use the live check in class when available. If something is unfinished, submit the current state with a precise help request rather than waiting silently.

The target is the six scenario behaviours, explainable decisions, a complete trace, and a preserved working checkpoint. There is no VG feature quota this week. One overall course assignment assesses all four official learning outcomes in January.

## Collaboration and sources

Predict and trace decisions in groups of 2-3, reproduce bugs together and peer-playtest. Each student keeps their own project, makes their implementation choices and can explain the work. Sharing reasoning is encouraged; submitting another person's implementation as your own is not. Record reused code, external sources and substantive tool or AI assistance in EVIDENCE.md. Be able to explain and verify any code you use.

## The gap and January

Tagging a commit `week1-checkpoint` is optional; an exact commit hash is enough. Keep the same repository and known working state. There is no required AI coursework during the intervening game project/break. Apply these ideas in that project where useful, but your grade does not depend on whether your team uses NPCs.

In January we will deepen navigation and movement, introduce behaviour trees and planning, and develop the same rival further. You are not required to solve those stages before they are taught.

Optional reference when an API detail blocks you: [NavMeshAgent](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/AI.NavMeshAgent.html), [SetDestination](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/AI.NavMeshAgent.SetDestination.html), [AI Navigation](https://docs.unity3d.com/Packages/com.unity.ai.navigation@2.0/manual/index.html). Predict and inspect your current code before browsing for a whole implementation.
