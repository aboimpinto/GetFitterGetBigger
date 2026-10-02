# Product direction and interaction proposal

Discussion draft, 6 September 2026. Examples are fictional; targets illustrate interface behavior, not training prescriptions.

## Positioning

**Bring your training. See your progress.**

The product keeps a long-term training record, makes its measurements understandable, and helps people see whether their work relates to their chosen outcomes. Plans may come from a coach, the user, a template, or an AI assistant.

Do not base the business on a claim that LLMs can never remember training. Durable storage and retrieval can be connected to an assistant. The valuable product is the dependable logging experience, consistent exercise identities, longitudinal comparisons, explicit measurement rules, and social trust around those records.

The hypothesis that LLMs caused fitness apps to lose customers was not established by this review. Existing competitors already provide relevant features: [Hevy](https://www.hevyapp.com/features/) lists progress, muscle distribution and social features; [Strong](https://help.strongapp.io/article/238-add-measurements) supports measurement history; [Fitbod](https://help.fitbod.me/hc/en-us/articles/360006269014-Muscle-Recovery) uses logged history in muscle recovery estimates. These are vendor descriptions, not independent evidence of effectiveness.

The opportunity to test is more specific: **a training record that connects goals, exercise-level evidence, understandable muscle workload, and fair small-group challenges, regardless of who wrote the workout.** This combination is a positioning hypothesis, not a claim of exclusive features.

For an initial pilot, focus on adults already doing strength training who want clearer evidence of progress and optional competition with friends. Keep the model open to sport benchmarks. Launching every sport, beginner fitness, bodybuilding, coaches and a public network simultaneously would make the first experience difficult to judge.

## Names to discuss

| Name | Meaning and fit | Tradeoff |
| --- | --- | --- |
| **Repward** | Reps + forward. Short, active, friendly; strongest initial consumer direction. | Suggests gym training more than endurance or mobility. Pronunciation and recall need testing. |
| **SetTrail** | Every set leaves a useful record. Closely matches history and measurement. | Less emotional; still strength-oriented. |
| **TrainThread** | Continuity across sessions, plans, goals and assistants. | Broad enough for sport, but longer and can sound technical. |

Working recommendation: **Repward**, with the descriptor “Training & progress” and the line “Bring your training. See your progress.” The same brand can have a client app, Coach workspace, and Studio for catalogue operations.

Limited exact-name web searches did not surface an obvious fitness app for these three candidates. This is not confirmation of domain, app-store, company-name or trademark availability; none is reserved or cleared. Searches also surfaced existing businesses/products for FormLedger and Metrivo, so those were set aside. [FormLedger](https://getformledger.com/), [Metrivo](https://www.metrivo.co/).

## One platform, three roles

The existing admin combines platform administration and personal-trainer work. Make those responsibilities explicit, even if they share a web application and design system.

| Workspace | Primary question | Main areas | Access boundary |
| --- | --- | --- | --- |
| **Studio — platform admin** | Can users trust our catalogue and measurements? | Review queue; exercises; relationships; measurement policies; benchmarks; challenge rules; reports; access and audit history | Catalogue authority does not automatically grant unrestricted athlete-record access. |
| **Coach** | Which consenting athlete needs attention, and what should we review? | Roster; goals; calendar; session templates; actual vs planned; progress reviews | Only assigned, consenting athletes; access can be revoked. |
| **Client** | What am I doing today, and am I progressing? | Today; Train; Progress; Circle; profile/settings | Own records; explicit sharing of selected metrics and challenge results. |

An independent user never needs a coach account to start. A coach may propose a goal or session, but the record distinguishes who proposed it, who accepted it, and what actually happened.

## The core loop

1. Choose a goal and a repeatable way to measure it.
2. Bring or create a session; resolve exercises to catalogue identities.
3. See the last comparable performance and log what actually happens.
4. Review changes in performance, consistency and muscle workload.
5. Decide whether to continue or adjust the plan; preserve the reason.
6. Optionally share a benchmark or participate in a small challenge.

Example: a user chooses “8 pull-ups with this technique standard.” Their baseline is 4. Over several weeks the app records the attempts, associated training and any equipment/technique changes. Reaching 6 means two additional reps under comparable conditions. It does not automatically prove how much muscle they gained or which accessory exercise caused the change.

Variation should be deliberate. Keep some repeatable anchor exercises/benchmarks so progress remains comparable; allow accessory substitutions with reasons such as equipment availability or preference. More variety is not itself a progress metric. A new variation gets its own history until a valid comparison is established.

## Measuring what matters

Distinguish three layers throughout the product:

| Layer | Examples | How to present it |
| --- | --- | --- |
| Recorded facts | Completed reps, entered load, duration, distance, attendance, measurements | Show source, date, units and completeness. User-entered results are self-reported. |
| Calculated indicators | Weekly working sets, volume load, benchmark trend, adherence | Show inputs and calculation rules. Compare like with like. |
| Estimates/interpretation | Fractional muscle-set contribution, estimated maximum, possible plateau | Label estimates and state missing context. Keep separate from measured outcomes. |

For a first muscle-workload view, expose **direct working sets** and **indirect working sets** separately. An optional combined estimate might count a direct set as 1 and an indirect set as 0.5. Those are policy weights, not physiological percentages or a measurement of growth. A research meta-regression evaluated direct and fractional set counting, which supports treating attribution explicitly; it does not validate arbitrary exercise-specific precision for every person. [Pelland and colleagues](https://pubmed.ncbi.nlm.nih.gov/41343037/).

Illustrative calculation: three completed bench-press working sets contribute three direct chest sets and three indirect triceps sets in the selected catalogue mapping; under the illustrative 0.5 policy the latter displays as 1.5 estimated set equivalents. Multiple muscles can receive contributions from one set, so summing muscle counts does not recover the number of workout sets.

Keep warm-up and cool-down activity recorded but excluded from working-set totals by default. Record set purpose separately from an exercise's catalogue type: an exercise that can be used in a warm-up may also be performed as a working exercise. Track timed work in time-based views; do not silently convert stretches, runs or band resistance into kilograms lifted.

For comparable external-load exercises, volume load is the sum of completed repetitions × normalized external load. Establish per-hand vs total-load and unilateral conventions first. Bodyweight, assistance and machine variants need explicit rules. Never divide the lifted kilograms across muscles and present the result as a direct measurement of tissue loading.

The progress screen should pair workload with the actual goal metric. Strength performance, body measurements and sport performance are different outcomes. Higher workload alone is not proof of improvement. For a swimmer, show gym trends alongside a standardized start/turn test; do not claim that correlation proves transfer from a specific exercise.

Every chart should disclose the comparison period, eligible observations, missing sessions and definition changes. Do not show a reassuring trend when the user has only one comparable observation. Target ranges are chosen for the user's plan and reviewed with them; no universal green/red thresholds or invented recovery percentages.

## Studio: how the admin supports the client

The landing page is a review queue with actionable items: ambiguous exercise names, incomplete muscle mappings, missing media, unreviewed substitutions and challenged results. Counts come from real records.

**Exercise editor:** use a persistent exercise header, six clear sections, and a live client preview.

| Section | Admin action | What the client gets |
| --- | --- | --- |
| Identity & guidance | Define canonical name, aliases, equipment/variation, instructions and media | Search/import matching and consistent instructions. |
| Muscles & movement | Choose primary/secondary/stabilizer roles and movement patterns | Explainable workload breakdown and substitution context. |
| Preparation & alternatives | Curate warm-up, cool-down and alternative relationships with purpose/equipment constraints | Relevant preparation and a replacement picker that explains differences. |
| What to record | Select reps/load/time/distance, units, load convention, unilateral behavior and optional effort | Only the relevant fields in the session logger. |
| Measurement preview | Simulate a few sample sets under the selected policy | Admin sees exactly which totals, exclusions and labels the client will see. |
| Review & history | Review changes, inspect affected templates, publish a version or retire it | Stable historical records and clear updates to future content. |

Suggested publishing states: Draft → In review → Published → Retired. The small initial team can hold multiple roles, but publishing remains an explicit action with a change record.

Editing a published muscle mapping creates a new version. A completed workout retains the definition and calculation version used. A later policy can offer a clearly labelled recalculation across an entire comparison period; it must not silently mix methods or rewrite old competition standings.

**Session builder:** show Warm-up → Working blocks → Cool-down. Add catalogue exercises, sets and optional alternatives; preview duration and workload as planned estimates. Deduplicate repeated preparation suggestions and let the author decide what applies to the session. Do not automatically concatenate every linked warm-up.

**Program builder, later:** arrange sessions in a calendar, choose goal benchmarks and review dates, identify anchor exercises, and inspect planned versus recorded workload. Changing the future plan does not alter completed sessions.

**Benchmark editor:** define the event, metric, direction of improvement, units, equipment, execution standard, evidence requirement and version. A benchmark cannot be compared across incompatible variants just because its name matches.

**Challenge editor:** select an eligible benchmark or consistency rule, define group size, dates, timezone, scoring cap, baseline window, ties, evidence tier, late-sync policy and dispute handling. Preview edge cases before opening a challenge. Rules lock once it begins.

## Client: a fast logger with a useful memory

**Today:** show the current goal, next or resumable session, last comparable result, a small weekly summary and one primary action. Avoid making a promotional carousel or social feed the home screen.

**Train:** start from a saved session, blank session, coach assignment or imported text. An import is a review flow: paste text → resolve aliases → confirm ambiguous exercise matches and units → review working/preparation sets → save. An imported plan is never recorded as completed activity.

**During the session:** large numeric fields, previous actual performance beside today's prescription, one-tap set confirmation, optional effort input, adjustable rest timer, and accessible substitute/skip actions. Persist every confirmed set locally. A completed-set action can be undone; stopping a session preserves partial work. A timer derives from timestamps so returning from a phone lock does not restart the rest period.

**Finish:** summarize actual versus planned activity, comparable personal records, workload contributions and optional notes. The session contributes to personal history immediately; competition status may remain pending until synchronized/validated. Avoid requiring a long questionnaire.

**Progress:** start with the user's chosen goal, then an exercise trend, weekly muscle work, consistency and measurement history. Tap any number to see the supporting sets. Allow period and exercise-variant filters. Body measurements and photos, if added, are private by default and never required for a friends challenge.

**Circle:** private groups first. Show each participant's progress toward the selected challenge objective and allow encouragement. Later, offer explicitly joined small public groups matched to the chosen benchmark and broad experience level. Public discovery should not expose a person's full training diary or location.

## Healthy competition

Start with a four-week **consistency challenge**: complete three qualifying sessions per week. Each scheduled week is capped at three points; extra workouts produce no additional points. Specify minimum qualifying-session rules in advance. A tied first place is acceptable. Joining displays what evidence is visible to others.

Then test benchmark challenges with common execution standards. For personal-improvement formats, establish the baseline before the challenge; do not let someone lower it after joining. Require enough baseline evidence and show how sparse or inconsistent records affect eligibility. Beginners and experienced athletes have different improvement patterns, so percentage gains alone are not automatically fair.

Use separate labels for self-reported, witnessed and device-supported evidence. Device support does not prove correct technique. Allow reporting and adjudication; keep edits and result invalidations auditable. Material log corrections trigger a score recalculation under the locked rules. Do not promise perfect anti-cheat protection.

Avoid an overall score combining kilograms, swimming times, mobility and body size. Prefer a small understandable contest, a common benchmark, and comparisons with one's own baseline. Keep recovery/rest compatible with success, and omit weight-loss or maximal-volume leaderboards from the initial product.

## Visual direction

See the [dark red concept board](design-concept-dark-red.png) and the [design language](design-language.md). The board uses fictional records and the provisional name Repward. The name remains undecided.

User direction, 7 September 2026: dark backgrounds throughout, with red over black/charcoal as the defining combination. Use graphite page backgrounds, layered charcoal panels, crimson primary buttons, bright red active states and pale text. White, cream and teal surfaces from the first concept are superseded. The admin is a spacious desktop workspace with compact tables, a focused editor and a client preview. The client has larger numbers and touch targets, with Today, Train, Progress and Circle as its main navigation. Apply this language consistently to Studio, Coach and Client, including forms, dialogs, menus, empty states and charts.

Reserve color for meaning, but always pair it with text. A muscle view describes recorded work, not a diagnosis of readiness. Support keyboard use in the admin, screen-reader labels, scalable text and reduced motion. Design clear first-session, empty-history, offline, sync-conflict and permission-revoked states before adding decorative screens.

The visible idea is “a personal training journal you can understand.” Use trend lines, dated comparisons and short explanations. Keep database terminology and policy identifiers in Studio; client explanations use everyday language.

## Architecture to support the interactions

Keep a shared API as the authority for identity, catalogue definitions, measurements and challenge rules. Organize the backend into catalogue, planning, recorded training, goals/analytics, and community responsibilities. A modular application is enough initially; separate microservices are not a requirement.

Key additions are versioned exercise definitions and mappings; goal/benchmark definitions; plan/session snapshots; completed sets with purpose and units; metric observations; versioned calculation results; memberships and consent; and challenge entries with evidence status.

For offline logging, give each session/set a stable client-created identity and synchronize idempotently. Retrying a request must not duplicate volume or competition points. Keep revisions/tombstones for corrections, resolve conflicting edits explicitly, and distinguish device time from server receipt time. Preserve a raw record from which aggregates can be recomputed. Backup and restore must include catalogue content and user records, not only source code.

The assistant receives selected goals, relevant history and data-completeness information from this store. It can explain or propose changes; deterministic services calculate metrics and rankings. Any proposed plan change is reviewed and accepted before it changes future training. A new assistant conversation must not mean starting the athlete's history over.

Preserve the .NET/PostgreSQL domain investment unless a concrete delivery constraint argues otherwise. Choose the replacement frontend after testing the actual logger on Android and iPhone: offline capture, lock/resume, keyboard, timers and synchronization. A web prototype can validate navigation quickly; that alone does not prove a browser client meets the intended mobile behavior. Avoid selecting a new framework merely as part of the rebrand.

## First release and validation

1. **Recover and establish trust:** catalogue inventory, tracked database baseline, verified identity/access, definitions and units, sample content restoration, backup/restore proof.
2. **Prove personal value:** focused Studio exercise editor, start/log/resume/finish a session, durable history, one goal/benchmark, exercise trend, direct/indirect muscle-set view and export. Validate with representative users over several weeks.
3. **Connect existing training:** reviewed text import and a concise assistant-readable progress summary. Measure unresolved exercise matches and correction frequency.
4. **Add a small circle:** invites and one capped consistency challenge; verify ties, corrections, retries, late syncing and privacy behavior.
5. **Expand from evidence:** consenting coach roster/review, sport-specific benchmarks and carefully scoped public groups.

Pilot success criteria are proposed targets, not forecasts: most testers can log a set without help; a confirmed set survives app termination and sync retries; participants can explain what their progress chart means; and enough return for several weeks to evaluate value beyond novelty. Track logging friction, missing data, weekly retention, calculation trust and voluntary willingness to pay. Do not optimize for workouts generated or total volume accumulated.

Commercial hypothesis: keep basic logging, access to personal history and export useful; charge for deeper longitudinal analysis, richer goals and later coach/team workspaces. Test payment intent before fixing prices. Selling workout plans is not required for this model.

The next concrete design exercise should follow one person through a complete week and one administrator through publishing the exercises that person uses. That connects the admin investment directly to the value experienced by the client.
