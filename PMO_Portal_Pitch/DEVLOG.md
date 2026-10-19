# Development log — AI-assisted, human-verified

AI was used for ideation, storyboarding, code drafting and debugging. Every iteration below shows the goal, the prompt or approach, what was wrong with the AI's output, how it was refined, and how the fix was **independently verified** by measurement rather than by trusting the explanation.

Verification harness used throughout: Playwright driving headless Chromium against `file:///…/index.html` with **network disabled**. It screenshots every scene at 1920×1080 and 1366×768, exercises each interaction, runs a 25-keypress rapid-navigation stress test, and logs every console error, warning and failed request.

---

## Iteration 1 — Grounding the story in the real product

- **Goal:** pitch content that matches what PMO Portal actually does, not generic "PMO software" claims.
- **Prompt / approach:** "Explore the running app at localhost:5173, list every tab, endpoint and feature, and build the narrative from that."
- **AI output issue:** the first plan invented modules (resource planning, Gantt charts) and generic KPIs that the app does not have.
- **Refinement:** read the app's own route list, i18n strings (`en.ts`) and API client (`pmo.ts`) to extract the real modules: projects, tasks, milestones, risks & issues, change requests with history, finance with EAC, dashboard tabs, automation/outbox/audit, and the 12-provider AI gateway. Pulled real seed records (milestones, risks, CRs, monthly snapshots) into `data.js`. Finance endpoints were permission-restricted for the demo user, so financial figures are labelled **illustrative** instead of being guessed and presented as real.
- **Verification:** every module named on screen maps to a real route or endpoint. Counts on screen (e.g. 11 of 22 decided milestones missed) were recomputed in Node from `models.js` and cross-checked against the portal's Milestones table.

## Iteration 2 — Business-case simulator that is explainable

- **Goal:** at least two inputs that change real computed KPIs and a chart, in a model I can defend line by line.
- **Prompt:** "Write a simulator for portfolio value, projects onboarded, overrun reduction and rollout strategy that outputs payback and ROI."
- **AI output issue:** the first draft computed payback as `investment / annual benefit` and then drew a cash curve from a *different* formula, so the payback marker and the curve disagreed. It also mixed DOM updates into the maths.
- **Refinement:** moved all maths into a pure `simulate()` in `models.js`. Payback is now *read from* the same 36-month cumulative-cash series that is drawn, so the marker and the curve agree by construction. Added explicit adoption ramps and first-year friction for big bang, and exposed `ASSUMPTIONS` so they print on screen.
- **Verification:** ran `simulate()` in Node for the base case (benefit AED 2.60M/yr, investment AED 0.86M, payback month 8, 3-year net AED 5.77M), for big bang (payback month 7), and for a hostile low case (no payback, negative net). Checked that the scene-10 ask equals the scene-8 investment for any inputs.

## Iteration 3 — Line charts rendered as filled polygons ❌ AI weakness found & corrected

- **Goal:** a shared chart kit with colour utility classes (`c-teal`, `c-slate`, …) reused by bars, donuts and lines.
- **AI suggestion:** single colour classes that set `fill`, applied to bars *and* to line paths, with `.line { fill: none }` declared earlier in the stylesheet.
- **Weakness:** both selectors have equal specificity, so the later `.c-slate { fill: … }` won. Every line path whose class included `c-slate` was filled into a grey polygon (visible in the Delivery tab's progress chart). The AI's review claimed the CSS was correct "because `.line` sets fill: none".
- **Correction:** `.line { fill: none !important; }`, documented in the stylesheet, and the area fill moved to a separate `.area` path.
- **Verification:** before/after screenshots of the Progress-trend panel. The polygon is gone and the lines are intact at both resolutions.

## Iteration 4 — Insight bar intermittently invisible (root cause, not a patch)

- **Goal:** fix the dashboard's AI-insight bar, which vanished after revisiting the scene.
- **AI output issue:** the first suggestion was "add `style.visibility = 'visible'` in `enter()`", a patch that hides the symptom without explaining it.
- **Investigation:** a probe script reproduced the bug in 2 of 3 runs. Computed style showed `visibility: hidden` on `#insight` while its parent `.scene.active` was `visible`. Chrome DevTools Protocol (`CSS.getMatchedStylesForNode`) showed no rule or inline style hiding it, and no running animation. The pattern: the element's entrance animation had been cancelled while its scene was `visibility: hidden`, leaving a stale inherited value.
- **Fix:** inactive scenes now use `display: none` instead of `visibility: hidden`, which removes the inherited-visibility dependency entirely and also stops hidden scenes from being laid out.
- **Verification:** the same probe passed 4 of 4 runs. The full test suite reports zero console errors at both resolutions.

## Iteration 5 — Presenter could get "stuck"

- **Goal:** keyboard navigation must never silently stop working.
- **AI output issue:** the key handler ignored *all* `<input>` targets, so after clicking the outage checkbox (scene 09) the arrow keys stopped changing scenes. The AI's stress test hadn't focused a checkbox, so it missed this.
- **Refinement:** only text-entry fields keep their keys. Range sliders stop propagation themselves, so arrows adjust the value. Checkboxes keep only Space.
- **Verification:** the stress test now clicks the checkbox, presses 25 navigation keys, then `Home`, and asserts that the only remaining scene is `s-open: active`, with no `leaving` scene left behind. It passes.

## Iteration 6 — "It is not working" on the reviewer's machine

- **Report:** the presentation did not work when opened on a Windows laptop.
- **Diagnosis:** the ZIP had not been extracted. Double-clicking `index.html` inside a ZIP makes Windows copy only that one file to a temp folder, so `css/`, `js/` and the fonts were missing. Reproduced by copying `index.html` alone into an empty folder.
- **Fix 1:** added `build.py`, which inlines CSS, all scripts and base64 fonts into one self-contained `index.html`. The editable source now lives in `src/`.
- **Second bug found by the same test:** at 1366×768 the scaled stage drifted right and down and was clipped. The cause was that `display:grid; place-items:center` falls back to start alignment when the child (1920×1080) is larger than the container, before `scale()` is applied. My earlier 1366 check used shrunken screenshots and missed it.
- **Fix 2:** the stage is now absolutely positioned at 50%/50% with negative half-size margins, then scaled about its centre.
- **Verification:** the lone `index.html` in an empty folder, offline, passes the full interaction suite with zero errors. The stage's bounding box was measured at 7 window sizes; all are centred and fully visible.

## Smaller corrections

- Re-rendering a dashboard tab appended a second legend each time → `legend()` helper now replaces instead of appending.
- Word masks clipped descenders ("everything") → mask padding raised from .08em to .2em.
- Background nodes overlapped headings on content scenes → edge layouts are dimmed to 35% and moved to the margins.
- PowerPoint: titles defaulted to centred and line-shape flips drew links in the wrong direction → fixed in the generator. `validate.py` passes; all 10 slides were visually checked.

## What I would change with more time

- Unit tests for `models.js` in CI (the functions are pure, so this is cheap).
- An "export my scenario" button on scene 08 that downloads the committee's chosen inputs as JSON.
- An Arabic RTL version of the pitch itself, matching the product's bilingual support.
