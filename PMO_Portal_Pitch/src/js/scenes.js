/* ==========================================================================
   scenes.js — one object per scene.

   Contract used by controller.js:
     id          DOM id of the <section>
     layout      constellation layout + colour mode for the background
     transIn     name of a choreographed entrance (see controller.js)
     enter(ctx)  build/animate the scene. ctx = { el, scope, dir, from }
                 Everything animated or listened to MUST go through
                 ctx.scope so it is torn down when the scene is left.
     state       plain object that survives leaving/re-entering, so the
                 simulator, dashboard filters and journey keep their state.

   DOM is (re)rendered from state; state is never read back from the DOM.
   ========================================================================== */
PMO.scenes = (function () {
  'use strict';
  const D = PMO.data, M = PMO.models, C = PMO.charts, fx = PMO.fx;
  const $ = (sel, root) => (root || document).querySelector(sel);
  const $$ = (sel, root) => [...(root || document).querySelectorAll(sel)];
  const NS = 'http://www.w3.org/2000/svg';
  const svgEl = (tag, attrs, parent) => { const n = document.createElementNS(NS, tag); for (const k in attrs) n.setAttribute(k, attrs[k]); if (parent) parent.appendChild(n); return n; };
  const aed = (m) => 'AED ' + (m >= 1 ? m.toFixed(m >= 10 ? 1 : 2) + 'M' : Math.round(m * 1000) + 'K');
  const store = {
    get(k, d) { try { const v = localStorage.getItem('pmo.' + k); return v ? JSON.parse(v) : d; } catch (e) { return d; } },
    set(k, v) { try { localStorage.setItem('pmo.' + k, JSON.stringify(v)); } catch (e) { /* storage blocked: state stays in memory */ } }
  };

  // Shared entrance for headings and [data-anim] elements.
  function introduce(ctx, { delay = 0 } = {}) {
    let t = delay;
    $$('.kinetic', ctx.el).forEach(h => { if (h.id !== 'closing') t = Math.max(t, fx.revealWords(ctx.scope, h, { delay })); });
    fx.rise(ctx.scope, $$('[data-anim="rise"]', ctx.el), { delay: delay + 350, stagger: 110 });
    $$('[data-anim="fade"]', ctx.el).forEach((n, i) => fx.animate(ctx.scope, n, [{ opacity: 0 }, { opacity: 1 }], { duration: 900, delay: delay + 500 + i * 120 }));
    return t;
  }

  /* ======================= 01 · OPENING ============================== */
  const open = {
    id: 's-open', layout: ['sphere', 'neutral'],
    enter(ctx) {
      introduce(ctx, { delay: 250 });
      const last = PMO.controller.resumeIndex();
      const btn = $('#resume-btn');
      btn.hidden = !(last > 0);
      if (last > 0) { $('#resume-n').textContent = String(last + 1).padStart(2, '0'); btn.onclick = () => PMO.controller.go(last); }
    }
  };

  /* ======================= 02 · PROBLEM ============================== */
  const problem = {
    id: 's-problem', layout: ['chaos', 'alarm'],
    enter(ctx) {
      introduce(ctx);
      const del = M.delivery(null), fin = M.finance(null);
      const vals = { hit: 1 - del.hitRate, overdue: del.overdue, overruns: fin.overruns, lag: 34 };
      $$('[data-count]', ctx.el).forEach((n, i) => {
        const k = n.dataset.count, pct = n.dataset.fmt === 'pct';
        fx.countUp(ctx.scope, n, pct ? vals[k] * 100 : vals[k], { delay: 700 + i * 150, format: (v) => Math.round(v) + (pct ? '%' : '') });
      });
      const decided = D.milestones.filter(m => m.status !== 'Pending').length;
      $('#pb-hit-foot').textContent = `${D.milestones.filter(m => m.status === 'Missed').length} of ${decided} milestones · pilot data`;
      $('#pb-od-foot').textContent = `${del.blocked} more are blocked · cutoff ${D.CUTOFF}`;
      // Artefacts tumble in, then keep drifting (an "unstable" before-state). Paused by P.
      $$('.artefact', ctx.el).forEach((a, i) => {
        fx.animate(ctx.scope, a, [{ opacity: 0, transform: `translate(${120 + i * 30}px, ${-60 - i * 10}px) rotate(calc(var(--r) * 4))` }, { opacity: 1, transform: 'rotate(var(--r))' }],
          { duration: 900, delay: 900 + i * 140, easing: 'cubic-bezier(.3,1.4,.5,1)' });
        if (!fx.state.reduced) ctx.scope.timeout(() => {
          const a2 = a.animate([{ transform: 'rotate(var(--r)) translateY(0)' }, { transform: `rotate(var(--r)) translate(${i % 2 ? 8 : -8}px, ${6 + i}px)` }],
            { duration: 2600 + i * 400, iterations: Infinity, direction: 'alternate', easing: 'ease-in-out' });
          ctx.scope.anim(a2); if (fx.state.paused) a2.pause();
        }, 1900 + i * 140);
      });
    }
  };

  /* ======================= 03 · OPPORTUNITY ========================== */
  const PERSONAS = [
    { id: 'exec', ico: '🏛', name: 'Executive committee', role: 'Decides & funds', x: 820, y: 335,
      outcomes: [['1 view', 'portfolio health, money and risk on one screen — no pack to assemble'], ['Live', 'instead of month-end: overruns surface in days, not quarters'], ['100%', 'of change decisions recorded with who, when and why']] },
    { id: 'cfo', ico: '💰', name: 'Finance', role: 'Controls the money', x: 1180, y: 490,
      outcomes: [['EAC', 'forecast at completion per project, compared with approved budget'], ['−3 pts', 'portfolio overrun target (11% → 8%) · illustrative'], ['Gated', 'budget revisions approved in workflow, never in email']] },
    { id: 'pmo', ico: '🧭', name: 'PMO office', role: 'Runs the cadence', x: 1120, y: 810,
      outcomes: [['−80%', 'weekly reporting hours via snapshots, rules and AI insights'], ['0', 'manual chasing of overdue or blocked tasks — rules do it'], ['1', 'audit trail for every create, update and decision']] },
    { id: 'pm', ico: '🛠', name: 'Project managers', role: 'Deliver the work', x: 520, y: 810,
      outcomes: [['1 line', 'of text and AI fills the risk or change-request form'], ['1 place', 'for tasks, milestones, risks, members and forecasts'], ['Instant', 'notifications when a decision comes back']] },
    { id: 'team', ico: '👥', name: 'Delivery teams', role: 'Update progress', x: 460, y: 490,
      outcomes: [['2 langs', 'English and Arabic with full right-to-left layout'], ['Seconds', 'to update task progress — no status email needed'], ['Role-based', 'access: everyone sees exactly what they should']] }
  ];
  const HUB = { x: 820, y: 600 };
  const value = {
    id: 's-value', layout: ['core', 'healthy'], transIn: 'converge',
    state: { sel: 'exec' },
    enter(ctx) {
      introduce(ctx);
      const spokes = $('#value-spokes'), people = $('#value-personas');
      spokes.innerHTML = ''; people.innerHTML = '';
      PERSONAS.forEach((p, i) => {
        const path = svgEl('path', { d: `M${HUB.x} ${HUB.y} Q ${(HUB.x + p.x) / 2 + (i % 2 ? 60 : -60)} ${(HUB.y + p.y) / 2} ${p.x} ${p.y}`, class: 'spoke', id: 'spoke-' + p.id }, spokes);
        fx.drawPath(path, 900, 500 + i * 120, ctx.scope);
        const g = svgEl('g', { class: 'persona', tabindex: 0, role: 'button', 'aria-label': p.name, 'data-id': p.id, transform: `translate(${p.x} ${p.y})` }, people);
        svgEl('circle', { r: 74 }, g);
        svgEl('text', { class: 'p-ico', y: -6 }, g).textContent = p.ico;
        svgEl('text', { class: 'p-name', y: 106 }, g).textContent = p.name;
        svgEl('text', { class: 'p-role', y: 130 }, g).textContent = p.role;
        fx.animate(ctx.scope, g, [{ opacity: 0, transform: `translate(${HUB.x}px, ${HUB.y}px) scale(.2)` }, { opacity: 1, transform: `translate(${p.x}px, ${p.y}px) scale(1)` }],
          { duration: 1000, delay: 400 + i * 120, fill: 'none' });
        const pick = () => this.select(ctx, p.id, true);
        ctx.scope.on(g, 'click', pick);
        ctx.scope.on(g, 'keydown', (e) => { if (e.key === 'Enter' || e.key === ' ') { e.preventDefault(); pick(); } });
      });
      ctx.scope.timeout(() => this.select(ctx, this.state.sel, false), 1100);
      // Ambient: a data pulse flows from the hub along every spoke.
      if (!fx.state.reduced) {
        const dot = svgEl('circle', { r: 6, class: 'spoke-pulse' }, spokes);
        let k = 0;
        ctx.scope.interval(() => { if (fx.state.paused) return; fx.travel(ctx.scope, dot, $('#spoke-' + PERSONAS[k++ % PERSONAS.length].id), { duration: 1100 }); }, 1300);
      }
    },
    select(ctx, id, animate) {
      this.state.sel = id;
      const p = PERSONAS.find(x => x.id === id);
      $$('.persona', ctx.el).forEach(g => g.classList.toggle('on', g.dataset.id === id));
      $$('.spoke', ctx.el).forEach(s => s.classList.toggle('on', s.id === 'spoke-' + id));
      const panel = $('#persona-panel');
      panel.innerHTML = `<h3>${p.ico} ${p.name}</h3><div class="pp-sub">What changes for them</div>` +
        p.outcomes.map(([n, t]) => `<div class="outcome"><div class="o-num">${n}</div><div class="o-txt">${t}</div></div>`).join('');
      if (animate !== false) fx.rise(ctx.scope, $$('.outcome, h3', panel), { stagger: 70, y: 14, duration: 500 });
    }
  };

  /* ======================= 04 · SOLUTION ============================= */
  const MODULES = [
    { id: 'projects', ico: '📁', name: 'Projects', tag: 'Portfolio register', pts: ['Code, owner, department, priority, dates and budget per project', 'Members with project roles', 'Monthly progress snapshots & forecasts per project', 'One-click AI Summary of any project'], m: [() => D.projects.length, 'projects in pilot'], m2: [() => aed(D.projects.reduce((s, p) => s + p.budget, 0)), 'budget governed'] },
    { id: 'tasks', ico: '✅', name: 'Tasks', tag: 'Weighted progress', pts: ['Assignee, priority, due date and weight', 'Progress % rolls up into project progress', 'Overdue flag computed against the cutoff date', 'Filter by project, status and assignee'], m: [() => D.tasks.length, 'tasks tracked'], m2: [() => M.delivery(null).overdue, 'overdue right now'] },
    { id: 'milestones', ico: '🏁', name: 'Milestones', tag: 'Kickoff → UAT sign-off', pts: ['Standard gates: kickoff, design, build, UAT, cutover', 'Status: pending, achieved or missed', 'Feeds the roadmap and the 14-day "due soon" KPI'], m: [() => D.milestones.length, 'milestones'], m2: [() => Math.round(M.delivery(null).hitRate * 100) + '%', 'hit rate'] },
    { id: 'risks', ico: '⚠️', name: 'Risks & Issues', tag: 'Probability × impact', pts: ['5×5 scoring; score ≥ 15 is "high exposure"', 'Owner, mitigation plan and due date', 'Open → Mitigating → Closed lifecycle', 'AI risk analysis suggests mitigations'], m: [() => M.riskGov(null).open, 'open items'], m2: [() => M.riskGov(null).high.length, 'high exposure'] },
    { id: 'cr', ico: '🔀', name: 'Change Requests', tag: 'Governed approvals', pts: ['Draft → Submitted → Under review → Approved / Returned / Rejected', 'Estimated cost impact on every request', 'Full status history: from, to, actor, when', 'Decision reason captured for audit'], m: [() => D.changeRequests.length, 'requests'], m2: [() => M.riskGov(null).pending, 'awaiting decision'] },
    { id: 'finance', ico: '💳', name: 'Finance', tag: 'Budget · actual · EAC', pts: ['Cost transactions by category (draft → posted)', 'Budget revisions with approval decisions', 'Monthly forecasts → Estimate at Completion', 'Utilization and projected variance per project'], m: [() => aed(M.finance(null).actual), 'posted spend*'], m2: [() => M.finance(null).overruns, 'projected overruns*'] },
    { id: 'dash', ico: '📊', name: 'Executive Dashboard', tag: 'Delivery · Risk · Financial', pts: ['Three tabs of KPIs with a reporting cutoff', 'Budget vs actual vs EAC, cost trend, spend by category', 'Risk matrix and drill-downs to the underlying records', 'AI Insights narrate what changed'], m: [() => 18, 'live KPIs'], m2: [() => 9, 'interactive visuals'] },
    { id: 'auto', ico: '⚙️', name: 'Automation & Logs', tag: 'Rules · outbox · audit', pts: ['Scheduled rules: snapshots, overdue and blocked alerts', 'Outbox → real-time notifications over SignalR', 'Audit trail of every entity change and actor', 'Run-now button for the PMO admin'], m: [() => 32, 'snapshots written'], m2: [() => '24/7', 'scheduler'] },
    { id: 'ai', ico: '✨', name: 'AI Assistant', tag: '12-provider gateway', pts: ['Portfolio chat scoped to one, all or compared projects', 'Project summaries, risk analysis, dashboard insights', '"AI fill" turns one sentence into a complete form', 'Ordered failover across 12 OpenAI-compatible providers'], m: [() => 12, 'providers'], m2: [() => 5, 'AI features'] }
  ];
  const solution = {
    id: 's-solution', layout: ['top', 'healthy'], transIn: 'zoom',
    state: { sel: 'dash' },
    enter(ctx) {
      introduce(ctx);
      const orbit = $('#module-orbit');
      $$('.module, .module-link', orbit).forEach(n => n.remove());
      const cx = 550, cy = 325, rx = 440, ry = 262;
      const links = svgEl('svg', { class: 'module-link', viewBox: '0 0 1100 650' });
      orbit.prepend(links);
      MODULES.forEach((m, i) => {
        const a = (i / MODULES.length) * Math.PI * 2 - Math.PI / 2;
        const x = cx + Math.cos(a) * rx, y = cy + Math.sin(a) * ry;
        svgEl('line', { x1: cx, y1: cy, x2: x, y2: y, 'data-id': m.id }, links);
        const b = document.createElement('button');
        b.className = 'module'; b.dataset.id = m.id;
        b.style.left = (x - 105) + 'px'; b.style.top = (y - 48) + 'px';
        b.innerHTML = `<div class="m-ico">${m.ico}</div><div class="m-name">${m.name}</div><div class="m-tag">${m.tag}</div>`;
        orbit.appendChild(b);
        fx.animate(ctx.scope, b, [{ opacity: 0, transform: `translate(${cx - x}px, ${cy - y}px) scale(.3)` }, { opacity: 1, transform: 'none' }],
          { duration: 900, delay: 300 + i * 70, easing: 'cubic-bezier(.2,1.2,.3,1)', fill: 'backwards' });
        ctx.scope.on(b, 'click', () => this.select(ctx, m.id));
      });
      this.select(ctx, this.state.sel, true);
    },
    select(ctx, id, quiet) {
      this.state.sel = id;
      const m = MODULES.find(x => x.id === id);
      $$('.module', ctx.el).forEach(b => b.classList.toggle('on', b.dataset.id === id));
      $$('.module-link line', ctx.el).forEach(l => l.classList.toggle('on', l.dataset.id === id));
      const d = $('#module-detail');
      d.innerHTML = `<h3>${m.ico} ${m.name}</h3><div class="md-tag">${m.tag}</div><ul>${m.pts.map(p => `<li>${p}</li>`).join('')}</ul>
        <div class="md-metric"><div><b>${m.m[0]()}</b><span>${m.m[1]}</span></div><div><b>${m.m2[0]()}</b><span>${m.m2[1]}</span></div></div>
        ${id === 'finance' ? '<div class="muted" style="font-size:15px;margin-top:10px">* illustrative financial assumptions</div>' : ''}`;
      if (!quiet) fx.rise(ctx.scope, $$('li, .md-metric', d), { stagger: 60, y: 12, duration: 450 });
    }
  };

  /* ======================= 05 · ARCHITECTURE ========================= */
  const LAYERS = [
    { name: 'PRESENTATION · REACT 18 + TYPESCRIPT + VITE', y: 16, h: 140 },
    { name: 'API · ASP.NET CORE', y: 196, h: 150 },
    { name: 'DATA · SQL SERVER', y: 386, h: 140 },
    { name: 'EXTERNAL SERVICES', y: 566, h: 130 }
  ];
  const COMPS = [
    { id: 'spa', l: 0, x: 40, w: 230, t: 'React SPA', s: 'pages · routing · i18n', d: 'Single-page app built with React, TypeScript and Vite. English/Arabic with full RTL, light/dark themes, and permission-aware navigation.' },
    { id: 'charts', l: 0, x: 300, w: 230, t: 'ECharts dashboards', s: 'echarts-for-react', d: 'The Executive Dashboard: KPIs, budget vs actual vs EAC, cost trend, risk matrix and drill-downs.' },
    { id: 'aiui', l: 0, x: 560, w: 230, t: 'AI assistant UI', s: 'chat · summary · AI fill', d: 'Floating assistant, per-project AI Summary, risk analysis and AI form-fill buttons.' },
    { id: 'rt', l: 0, x: 820, w: 230, t: 'SignalR client', s: 'notification bell', d: 'Keeps a hub connection with automatic reconnect (0/2/5/10/20 s back-off) and shows notifications in real time.' },
    { id: 'auth', l: 1, x: 40, w: 200, t: 'JWT + RBAC', s: 'permission keys', d: 'Every endpoint checks a permission key such as pmo:finance:list or pmo:dashboard:view; roles bundle keys and data scope.' },
    { id: 'api', l: 1, x: 270, w: 220, t: 'PMO controllers', s: '/api/pmo/*', d: 'REST endpoints for projects, tasks, milestones, risks, change requests, finance, dashboard analytics and employees.' },
    { id: 'jobs', l: 1, x: 520, w: 200, t: 'Automation jobs', s: 'scheduled rules', d: 'Scheduled tasks write monthly snapshots and raise overdue/blocked alerts into the outbox; admins can run them on demand.' },
    { id: 'hub', l: 1, x: 750, w: 170, t: 'SignalR hub', s: '/msgHub', d: 'Pushes notifications, notices, online counts and forced-logout events to connected users.' },
    { id: 'gw', l: 1, x: 950, w: 240, t: 'AI gateway', s: 'ordered failover', d: 'One OpenAI-compatible client walks an ordered list of 12 providers; if one fails it moves to the next. Calls get a 60 s timeout.' },
    { id: 'db', l: 2, x: 120, w: 300, t: 'PMO schema', s: 'projects · tasks · finance …', d: 'SQL Server tables for the whole portfolio, including cost transactions, budget revisions, forecasts and snapshots.' },
    { id: 'audit', l: 2, x: 470, w: 260, t: 'Audit & outbox', s: 'who · what · when', d: 'Every change and every CR transition is written to the audit trail; notifications queue in the outbox before delivery.' },
    { id: 'sys', l: 2, x: 780, w: 300, t: 'Identity & config', s: 'users · roles · menus', d: 'Users, roles, menus/permissions, dictionaries, scheduled-task config and login/operation logs.' },
    { id: 'llm', l: 3, x: 640, w: 550, t: '12 LLM providers', s: 'Groq → OpenRouter → Cerebras → Gemini → …', d: 'Groq (Llama 3.3 70B) is the default; OpenRouter, Cerebras, Gemini, SambaNova, Together, HuggingFace, Qwen, DeepSeek, Mistral, NVIDIA NIM and Grok follow in order.' },
    { id: 'mail', l: 3, x: 120, w: 380, t: 'Email & integrations', s: 'ERP cost feed · SSO (phase 1)', d: 'Planned integrations for the scale-up: corporate SSO and an ERP feed for posted costs.' }
  ].map(c => ({ ...c, y: LAYERS[c.l].y + (c.l === 3 ? 44 : 46), h: 76 }));
  const TRACES = {
    dashboard: { hops: [['spa', 'auth'], ['auth', 'api'], ['api', 'db'], ['db', 'api'], ['api', 'charts']],
      steps: ['Executive opens the dashboard tab', 'JWT validated; pmo:dashboard:view checked', 'KPIs & analytics endpoints query SQL Server', 'Aggregates returned for the cutoff date', 'ECharts renders 18 KPIs and 9 visuals'] },
    cr: { hops: [['spa', 'auth'], ['auth', 'api'], ['api', 'audit'], ['audit', 'hub'], ['hub', 'rt']],
      steps: ['Sponsor clicks Approve with a reason', 'Permission checked for the decision', 'Status + history row written; budget revision raised', 'Outbox event created for the requester', 'Requester sees it instantly via SignalR'] },
    ai: { hops: [['aiui', 'api'], ['api', 'db'], ['api', 'gw'], ['gw', 'llm'], ['gw', 'aiui']],
      steps: ['PM asks "Where are we overspending?"', 'Context built from data the user may see', 'Prompt sent to the AI gateway', 'Provider #1 answers (or the next one does)', 'Answer streamed back to the assistant'] }
  };
  const arch = {
    id: 's-arch', layout: ['left', 'neutral'],
    state: { sel: null },
    enter(ctx) {
      introduce(ctx);
      const svg = $('#arch-svg'); svg.innerHTML = '';
      const center = (id) => { const c = COMPS.find(x => x.id === id); return { x: c.x + c.w / 2, y: c.y + c.h / 2, c }; };
      LAYERS.forEach((L, i) => {
        const g = svgEl('g', {}, svg);
        svgEl('rect', { x: 0, y: L.y, width: 1230, height: L.h, rx: 14, class: 'layer-bg' }, g);
        svgEl('text', { x: 18, y: L.y + 26, class: 'layer-name' }, g).textContent = L.name;
        fx.animate(ctx.scope, g, [{ opacity: 0, transform: 'translateY(30px)' }, { opacity: 1, transform: 'none' }], { duration: 700, delay: 200 + i * 140 });
      });
      this.wires = svgEl('g', {}, svg);
      COMPS.forEach((c, i) => {
        const g = svgEl('g', { class: 'comp', 'data-id': c.id, tabindex: 0, role: 'button', 'aria-label': c.t }, svg);
        svgEl('rect', { x: c.x, y: c.y, width: c.w, height: c.h, rx: 10 }, g);
        svgEl('text', { x: c.x + c.w / 2, y: c.y + 33 }, g).textContent = c.t;
        svgEl('text', { x: c.x + c.w / 2, y: c.y + 56, class: 'sub' }, g).textContent = c.s;
        fx.animate(ctx.scope, g, [{ opacity: 0 }, { opacity: 1 }], { duration: 500, delay: 500 + i * 45 });
        const pick = () => this.inspect(c);
        ctx.scope.on(g, 'click', pick);
        ctx.scope.on(g, 'keydown', (e) => { if (e.key === 'Enter') pick(); });
      });
      this.packet = svgEl('circle', { r: 8, class: 'packet', opacity: 0 }, svg);
      this.center = center;
      $$('#trace-buttons .btn').forEach(b => { b.classList.remove('on'); ctx.scope.on(b, 'click', () => this.trace(ctx, b.dataset.trace)); });
      $('#trace-steps').innerHTML = '';
      this.ctx = ctx;
      ctx.scope.timeout(() => this.trace(ctx, 'dashboard'), 1500);
    },
    inspect(c) {
      $$('#arch-svg .comp').forEach(g => g.classList.toggle('sel', g.dataset.id === c.id));
      $('#arch-detail').innerHTML = `<h4>${c.t}</h4><div class="mono" style="font-size:15px;color:var(--teal);margin-bottom:8px">${LAYERS[c.l].name}</div>${c.d}`;
    },
    // Animate a request through the stack: light each hop's wire, move the packet, tick the step list.
    trace(ctx, key) {
      const T = TRACES[key];
      this.runId = (this.runId || 0) + 1; const run = this.runId;
      $$('#trace-buttons .btn').forEach(b => b.classList.toggle('on', b.dataset.trace === key));
      this.wires.innerHTML = '';
      $$('#arch-svg .comp').forEach(g => g.classList.remove('hot'));
      $('#trace-steps').innerHTML = T.steps.map(s => `<li>${s}</li>`).join('');
      const lis = $$('#trace-steps li');
      const hopMs = fx.state.reduced ? 1 : 900;
      T.hops.forEach(([a, b], i) => {
        ctx.scope.timeout(() => {
          if (run !== this.runId) return;
          const A = this.center(a), B = this.center(b);
          const sameRow = A.c.l === B.c.l;
          const y1 = sameRow ? A.y : A.y + (B.y > A.y ? A.c.h / 2 : -A.c.h / 2);
          const y2 = sameRow ? B.y : B.y + (B.y > A.y ? -B.c.h / 2 : B.c.h / 2);
          const d = sameRow ? `M${A.x} ${A.y - 38} C ${A.x} ${A.y - 80}, ${B.x} ${B.y - 80}, ${B.x} ${B.y - 38}` : `M${A.x} ${y1} C ${A.x} ${(y1 + y2) / 2}, ${B.x} ${(y1 + y2) / 2}, ${B.x} ${y2}`;
          const w = svgEl('path', { d, class: 'wire lit' }, this.wires);
          fx.drawPath(w, hopMs * 0.7, 0, ctx.scope);
          $$('#arch-svg .comp').forEach(g => { if (g.dataset.id === a || g.dataset.id === b) g.classList.add('hot'); });
          if (!fx.state.reduced) fx.travel(ctx.scope, this.packet, w, { duration: hopMs });
          lis.forEach((li, k) => li.classList.toggle('on', k <= i));
          if (i === 0) this.inspect(A.c);
        }, i * hopMs);
      });
    }
  };

  /* ======================= 06 · USE CASE ============================= */
  const STEPS = ['Raise', 'Submit', 'AI impact', 'Decide', 'Outcome'];
  const usecase = {
    id: 's-usecase', layout: ['left', 'warn'],
    state: { step: 0, branch: null },
    enter(ctx) {
      introduce(ctx);
      const rail = $('#journey-rail'); rail.innerHTML = '';
      const xs = STEPS.map((_, i) => 90 + i * 380);
      svgEl('path', { d: `M${xs[0]} 60 L${xs[xs.length - 1]} 60`, class: 'rail' }, rail);
      this.fill = svgEl('path', { d: `M${xs[0]} 60 L${xs[xs.length - 1]} 60`, class: 'rail-fill' }, rail);
      this.railLen = xs[xs.length - 1] - xs[0];
      this.fill.style.strokeDasharray = this.railLen; this.fill.style.transition = 'stroke-dashoffset .8s cubic-bezier(.2,.75,.15,1), stroke .4s';
      STEPS.forEach((s, i) => {
        const g = svgEl('g', { class: 'st', 'data-i': i, transform: `translate(${xs[i]} 60)`, role: 'button', tabindex: 0, 'aria-label': 'Step ' + (i + 1) + ': ' + s }, rail);
        svgEl('circle', { r: 24 }, g);
        svgEl('text', { class: 'n', y: 6 }, g).textContent = i + 1;
        svgEl('text', { y: 66 }, g).textContent = s;
        ctx.scope.on(g, 'click', () => { if (i <= this.maxReachable()) this.go(ctx, i); });
      });
      ctx.scope.on($('#j-next'), 'click', () => this.go(ctx, this.state.step + 1));
      ctx.scope.on($('#j-prev'), 'click', () => this.go(ctx, this.state.step - 1));
      $$('#j-decision .btn').forEach(b => ctx.scope.on(b, 'click', () => { this.state.branch = b.dataset.branch; this.go(ctx, 4); }));
      this.render(ctx, false);
    },
    maxReachable() { return this.state.branch ? 4 : 3; },
    go(ctx, i) { i = Math.max(0, Math.min(this.maxReachable(), i)); if (i < 4 && this.state.step === 4 && i < 3) this.state.branch = null; this.state.step = i; this.render(ctx, true); },
    render(ctx, animate) {
      const s = this.state.step, br = this.state.branch;
      $$('#journey-rail .st').forEach(g => { const i = +g.dataset.i; g.classList.toggle('done', i < s); g.classList.toggle('cur', i === s); });
      this.fill.style.strokeDashoffset = this.railLen * (1 - s / (STEPS.length - 1));
      this.fill.setAttribute('class', 'rail-fill ' + (s === 4 && br ? br : ''));
      $('#j-prev').disabled = s === 0;
      $('#j-next').hidden = s >= 3;
      $('#j-decision').hidden = s < 3;
      $$('#j-decision .btn').forEach(b => b.classList.toggle('on', b.dataset.branch === br));

      // Project card state is DERIVED from (step, branch) — never accumulated — so going back is always correct.
      const budget = 800000, baseEac = 930000, cr = 30000;
      let bud = budget, eac = baseEac, risk = 25, status = '—';
      if (s >= 1) status = 'Submitted';
      if (s >= 2) { status = 'UnderReview'; eac = baseEac + cr; }
      if (s >= 4 && br === 'Approved') { status = 'Approved'; bud = budget + cr; risk = 12; }
      if (s >= 4 && br === 'Returned') { status = 'Returned'; eac = baseEac; }
      if (s >= 4 && br === 'Rejected') { status = 'Rejected'; eac = baseEac; }
      const setv = (id, txt) => { const n = $(id); if (n.textContent !== txt) { n.textContent = txt; n.classList.add('flash'); ctx.scope.timeout(() => n.classList.remove('flash'), 900); } };
      setv('#pc-budget', 'AED ' + bud.toLocaleString('en-US'));
      setv('#pc-eac', 'AED ' + eac.toLocaleString('en-US'));
      setv('#pc-risk', risk >= 15 ? `1 · score ${risk}` : `0 · score ${risk}`);
      const st = $('#pc-status'); st.textContent = { UnderReview: 'Under review' }[status] || status; st.className = 'pill ' + status;
      const scale = 1.3 * budget;
      $('#pc-fill').style.width = (eac / scale * 100) + '%';
      $('#pc-mark').style.left = (bud / scale * 100) + '%';
      const logs = [];
      if (s >= 0) logs.push('09:12  AI fill · form generated from 1 sentence');
      if (s >= 1) logs.push('09:13  CR-0007 Draft → Submitted · Tariq Al Jaberi');
      if (s >= 2) logs.push('09:13  AI risk analysis · linked to "Data quality gap" (25)');
      if (s >= 4 && br) logs.push(`10:40  CR-0007 Under review → ${br} · Hamdan Al Zaabi`);
      if (s >= 4 && br === 'Approved') logs.push('10:40  Budget revision +30,000 · outbox → SignalR');
      $('#pc-log').innerHTML = logs.slice().reverse().map(l => `<div>${l}</div>`).join('');

      const card = $('#journey-card');
      const cards = [
        () => `<h3>Raise the request — in one sentence</h3><div class="jc-actor">Tariq Al Jaberi · Project manager</div>
          <p>The HR portal's vendor can't fix a data-quality gap rated 25/25. Instead of a form with ten fields, Tariq types one line and presses <b>✨ AI fill</b>.</p>
          <div class="typed" id="typed"></div><div class="formgrid" id="formgrid"></div>`,
        () => `<h3>Submit for decision</h3><div class="jc-actor">Workflow · Change Requests</div>
          <p>The request moves from <b>Draft</b> to <b>Submitted</b>. The history table records the transition, the actor and the time.</p>
          <p>An outbox event notifies the PMO and the sponsor instantly over SignalR — no email chain, no "did you see my request?".</p>`,
        () => `<h3>AI impact analysis</h3><div class="jc-actor">AI gateway · risk analysis</div>
          <p>Before anyone decides, the assistant reads the project's register and finance data the reviewer is allowed to see.</p>
          <div class="ai-note">✨ "Approving adds AED 30,000, taking EAC to AED 960,000 (120% of budget). It directly mitigates the open <b>Data quality gap</b> risk (score 25) — the project's only high-exposure item. Recommend approval with a budget revision, and closing the risk after UAT."</div>`,
        () => `<h3>Committee decision — your call</h3><div class="jc-actor">Hamdan Al Zaabi · Sponsor</div>
          <p>Choose an outcome below. Each one changes the project card on the right, the audit log and the notifications differently.</p>
          <p class="muted">Approve · Return for more information · Reject</p>`,
        () => ({
          Approved: `<h3>Approved → budget revised, risk falls</h3><div class="jc-actor">Outcome · same day</div><p>A budget revision of <b>+AED 30,000</b> is raised and approved, the new vendor is onboarded, and the data-quality risk drops from <b>25 to 12</b>.</p><p>Elapsed time: about one hour, versus roughly two weeks of email today (illustrative).</p>`,
          Returned: `<h3>Returned → back to the requester</h3><div class="jc-actor">Outcome · reason captured</div><p>The sponsor asks for a second vendor quote. Tariq is notified instantly with the reason. EAC stays at <b>AED 930,000</b>; the risk stays open and visible on the dashboard.</p>`,
          Rejected: `<h3>Rejected → risk stays high and visible</h3><div class="jc-actor">Outcome · escalation</div><p>No money moves, but the <b>Data quality gap (25)</b> stays in the high-exposure list on every executive dashboard until another mitigation is approved. Nothing is hidden in an inbox.</p>`
        })[br] || ''
      ];
      card.innerHTML = cards[s]();
      if (animate) fx.rise(ctx.scope, [...card.children], { stagger: 60, y: 14, duration: 450 });
      if (s === 0) this.typeDemo(ctx);
    },
    typeDemo(ctx) {
      const line = 'Switch HR portal vendor to fix data quality gap, +30k AED';
      const typed = $('#typed'), grid = $('#formgrid');
      const fields = [['Title', 'Switch vendor'], ['Project', 'PRJ-HRP · HR Self-Service'], ['Est. cost impact', 'AED 30,000'], ['Linked risk', 'Data quality gap (25)']];
      const showFields = () => { grid.innerHTML = fields.map(([k, v]) => `<div><span>${k}</span>${v}</div>`).join(''); fx.rise(ctx.scope, [...grid.children], { stagger: 90, y: 10, duration: 450 }); };
      if (fx.state.reduced) { typed.textContent = line; showFields(); return; }
      let i = 0; typed.innerHTML = '<span></span><i class="caret"></i>';
      const span = typed.firstChild;
      const id = ctx.scope.interval(() => { span.textContent = line.slice(0, ++i); if (i >= line.length) { clearInterval(id); ctx.scope.timeout(showFields, 350); } }, 34);
    }
  };

  /* ======================= 07 · DASHBOARD ============================ */
  function legend(vizId, items, center) {
    const viz = $('#' + vizId); viz.classList.add('has-legend');
    const old = viz.querySelector('.legend'); if (old) old.remove();
    viz.insertAdjacentHTML('beforeend', `<div class="legend"${center ? ' style="justify-content:center"' : ''}>${items.map(([c, t]) => `<span><i class="${c.replace('c-', 'sw ')}"></i>${t}</span>`).join('')}</div>`);
  }
  const COLORS = { CBM: 'c-teal', NBU: 'c-sky', AIC: 'c-violet', DLP: 'c-amber', CYB: 'c-coral', HRP: 'c-green', MOB: 'c-slate', LEG: 'c-slate' };
  const dash = {
    id: 's-dash', layout: ['top', 'health'], transIn: 'wipe',
    state: { tab: 'delivery', filter: new Set(), cell: null },
    enter(ctx) {
      this.ctx = ctx;
      fx.rise(ctx.scope, [$('.dash-title', ctx.el)], { y: 20 }); // not split into words: it contains the live badge
      const slicer = $('#slicer');
      $$('.slice', slicer).forEach(n => n.remove());
      D.projects.forEach(p => {
        const b = document.createElement('button'); b.className = 'slice'; b.dataset.id = p.id; b.textContent = p.id; b.title = p.name;
        slicer.appendChild(b); ctx.scope.on(b, 'click', () => this.toggle(p.id));
      });
      const clr = document.createElement('button'); clr.className = 'slice clear'; clr.textContent = '✕ Clear'; slicer.appendChild(clr);
      ctx.scope.on(clr, 'click', () => { this.state.filter.clear(); this.state.cell = null; this.render(); });
      $$('#dash-tabs .tab').forEach(t => ctx.scope.on(t, 'click', () => this.setTab(t.dataset.tab)));
      ctx.scope.on($('#dash-tabs'), 'keydown', (e) => {
        if (e.key !== 'ArrowRight' && e.key !== 'ArrowLeft') return;
        e.stopPropagation(); const tabs = ['delivery', 'risk', 'finance'];
        const i = (tabs.indexOf(this.state.tab) + (e.key === 'ArrowRight' ? 1 : 2)) % 3; this.setTab(tabs[i]); $(`#dash-tabs [data-tab="${tabs[i]}"]`).focus();
      });
      this.setTab(this.state.tab, true);
      fx.rise(ctx.scope, $$('.kpi, .panel, .insight', ctx.el), { delay: 300, stagger: 50, y: 20 });
    },
    toggle(id) { const f = this.state.filter; f.has(id) ? f.delete(id) : f.add(id); this.state.cell = null; this.render(); },
    setTab(tab, first) {
      const changed = tab !== this.state.tab;
      this.state.tab = tab; this.state.cell = null;
      $$('#dash-tabs .tab').forEach(t => t.setAttribute('aria-selected', String(t.dataset.tab === tab)));
      ['viz-a', 'viz-b', 'viz-c'].forEach(id => { const n = $('#' + id); n._s = null; n.innerHTML = ''; n.classList.remove('has-legend'); });
      const body = $('#dash-body'); body.classList.remove('swap'); void body.offsetWidth; if (!first || changed) body.classList.add('swap');
      this.render(true);
    },
    kpis(list) {
      const box = $('#kpis');
      if (box.children.length !== list.length) box.innerHTML = list.map(() => '<div class="kpi"><div class="kpi-l"></div><div class="kpi-v">0</div><div class="kpi-s"></div></div>').join('');
      list.forEach((k, i) => {
        const n = box.children[i]; n.className = 'kpi ' + (k.tone || '');
        n.children[0].textContent = k.l; n.children[2].textContent = k.s || '';
        const v = n.children[1], prev = v._v || 0; v._v = k.v;
        fx.countUp(this.ctx.scope, v, k.v, { from: prev, duration: 800, format: k.f || ((x) => Math.round(x)) });
      });
    },
    render(fresh) {
      const f = this.state.filter, tab = this.state.tab;
      $$('#slicer .slice[data-id]').forEach(b => b.classList.toggle('on', f.has(b.dataset.id)));
      const dimmed = (id) => f.size > 0 && !f.has(id);
      const scope = f.size ? `${f.size} project${f.size > 1 ? 's' : ''}` : 'whole portfolio';
      if (tab === 'delivery') this.renderDelivery(f, dimmed, scope);
      if (tab === 'risk') this.renderRisk(f, dimmed, scope);
      if (tab === 'finance') this.renderFinance(f, dimmed, scope);
    },
    renderDelivery(f, dimmed, scope) {
      const d = M.delivery(f);
      this.kpis([
        { l: 'Active projects', v: d.active, s: `of ${d.total} in scope` },
        { l: 'Portfolio progress', v: d.progress, f: (x) => Math.round(x) + '%', s: 'budget-weighted', tone: 'good' },
        { l: 'Overdue tasks', v: d.overdue, s: `of ${d.openTasks} open`, tone: d.overdue ? 'bad' : 'good' },
        { l: 'Blocked tasks', v: d.blocked, s: 'need a decision', tone: d.blocked ? 'warn' : 'good' },
        { l: 'Milestones due (14d)', v: d.milestonesDue, s: 'incl. late pending' },
        { l: 'Milestone hit rate', v: d.hitRate * 100, f: (x) => Math.round(x) + '%', s: 'achieved ÷ decided', tone: d.hitRate < .6 ? 'warn' : 'good' }
      ]);
      $('#pa-t').innerHTML = 'Progress trend · monthly snapshots <span class="hint">% complete</span>';
      C.lines($('#viz-a'), D.projects.map(p => ({ key: p.id, values: D.snapshots[p.id], cls: COLORS[p.id], dim: dimmed(p.id) })), D.snapshotMonths, { max: 100, w: 640, h: 360, tick: (v) => Math.round(v) + '%' });
      legend('viz-a', D.projects.filter(p => p.id !== 'LEG').map(p => [COLORS[p.id], p.id]));
      $('#pb-t').innerHTML = 'Task status by project <span class="hint">click a bar to filter</span>';
      const rows = M.delivery(null).statusByProject.map(r => ({ label: r.id, parts: r, dim: dimmed(r.id) }));
      C.stacked($('#viz-b'), rows, [{ k: 'Done', cls: 'c-teal', name: 'Done' }, { k: 'InProgress', cls: 'c-sky', name: 'In progress' }, { k: 'ToDo', cls: 'c-slate', name: 'To do' }, { k: 'Blocked', cls: 'c-coral', name: 'Blocked' }], { w: 520, h: 380, onClick: (id) => this.toggle(id) });
      $('#pc-t').innerHTML = `Drill-down · overdue tasks <span class="hint">${scope}</span>`;
      const od = d.overdueTasks.slice().sort((a, b) => a.due.localeCompare(b.due));
      $('#viz-c').innerHTML = `<div class="drill"><table><tr><th>Project</th><th>Status</th><th>Priority</th><th>Due</th></tr>${od.map(t => `<tr><td class="mono">${t.p}</td><td>${t.status.replace('InProgress', 'In progress').replace('ToDo', 'To do')}</td><td>${t.priority}</td><td class="sc">${t.due.slice(5)}</td></tr>`).join('') || '<tr><td colspan="4">No overdue tasks 🎉</td></tr>'}</table></div>`;
      const worst = Object.entries(od.reduce((a, t) => (a[t.p] = (a[t.p] || 0) + 1, a), {})).sort((a, b) => b[1] - a[1])[0];
      this.insight(worst ? `${D.byId[worst[0]].name} holds ${worst[1]} of ${od.length} overdue tasks. ${d.blocked} tasks are blocked — clearing them is the fastest lever on portfolio progress (${Math.round(d.progress)}%).` : `No overdue tasks in this selection. Progress is ${Math.round(d.progress)}% budget-weighted.`);
    },
    renderRisk(f, dimmed, scope) {
      const r = M.riskGov(f);
      const avg = r.risks.length ? r.risks.reduce((s, x) => s + x.prob * x.imp, 0) / r.risks.length : 0;
      this.kpis([
        { l: 'Open risks & issues', v: r.open, s: scope },
        { l: 'High exposure (≥15)', v: r.high.length, s: 'probability × impact', tone: r.high.length ? 'bad' : 'good' },
        { l: 'Average score', v: avg, f: (x) => x.toFixed(1), s: 'of 25 max', tone: avg >= 12 ? 'warn' : '' },
        { l: 'Pending CRs', v: r.pending, s: 'submitted / in review', tone: r.pending ? 'warn' : '' },
        { l: 'Pending cost impact', v: r.pendingCost / 1000, f: (x) => Math.round(x) + 'K', s: 'AED, awaiting decision' },
        { l: 'Approved changes', v: r.approvedCost / 1000, f: (x) => Math.round(x) + 'K', s: 'AED cost impact' }
      ]);
      $('#pa-t').innerHTML = 'Risk matrix · open items <span class="hint">click a cell</span>';
      C.heatmap($('#viz-a'), r.grid, { selected: this.state.cell, onCell: (p, i) => { this.state.cell = this.state.cell === `${p}${i}` ? null : `${p}${i}`; this.render(); } });
      $('#pb-t').innerHTML = 'Change requests by status';
      const cs = r.crStatus, cls = { Approved: 'c-teal', Returned: 'c-amber', Rejected: 'c-coral', Submitted: 'c-sky', UnderReview: 'c-violet' };
      const parts = Object.keys(cls).filter(k => cs[k]).map(k => ({ label: k === 'UnderReview' ? 'Under review' : k, value: cs[k], cls: cls[k] }));
      C.donut($('#viz-b'), parts.length ? parts : [{ label: 'None', value: 1, cls: 'c-slate' }], { center: String(parts.reduce((s, p) => s + p.value, 0)), sub: 'requests' });
      legend('viz-b', parts.map(p => [p.cls, `${p.label} ${p.value}`]), true);
      let list = r.risks.slice().sort((a, b) => b.prob * b.imp - a.prob * a.imp);
      if (this.state.cell) list = list.filter(x => `${x.prob}${x.imp}` === this.state.cell);
      $('#pc-t').innerHTML = `Drill-down · ${this.state.cell ? 'P' + this.state.cell[0] + ' × I' + this.state.cell[1] : 'open risks & issues'} <span class="hint">${list.length}</span>`;
      $('#viz-c').innerHTML = `<div class="drill"><table><tr><th>Item</th><th>Proj</th><th>Score</th></tr>${list.map(x => `<tr><td>${x.kind === 'Issue' ? '◆' : '●'} ${x.title}<div class="muted" style="font-size:13px">${x.owner} · ${x.status}</div></td><td class="mono">${x.p}</td><td class="sc">${x.prob * x.imp}</td></tr>`).join('') || '<tr><td colspan="3">No items</td></tr>'}</table></div>`;
      const top = r.high.slice().sort((a, b) => b.prob * b.imp - a.prob * a.imp)[0];
      this.insight(top ? `"${top.title}" on ${D.byId[top.p].name} is the top exposure (score ${top.prob * top.imp}). ${r.pending} change request${r.pending === 1 ? '' : 's'} worth AED ${Math.round(r.pendingCost / 1000)}K await a decision — decide them to unblock mitigation.` : 'No high-exposure risks in this selection.');
    },
    renderFinance(f, dimmed, scope) {
      const x = M.finance(f);
      this.kpis([
        { l: 'Approved budget', v: x.budget, f: aed, s: scope },
        { l: 'Actual spend', v: x.actual, f: aed, s: 'posted to date' },
        { l: 'Remaining', v: x.remaining, f: aed, s: 'budget − actual' },
        { l: 'Utilization', v: x.utilization * 100, f: (v) => Math.round(v) + '%', s: 'actual ÷ budget', tone: x.utilization > .85 ? 'warn' : '' },
        { l: 'Forecast EAC', v: x.eac, f: aed, s: (x.variance >= 0 ? '+' : '−') + aed(Math.abs(x.variance)) + ' vs budget', tone: x.variance > 0 ? 'bad' : 'good' },
        { l: 'Projected overruns', v: x.overruns, s: 'EAC > budget + 2%', tone: x.overruns ? 'bad' : 'good' }
      ]);
      $('#pa-t').innerHTML = 'Budget vs actual vs EAC <span class="hint">AED M · click to filter</span>';
      const all = M.finance(null).perProject;
      C.groupedBars($('#viz-a'), all.map(p => ({ label: p.id, values: [p.budget, p.actual, p.eac], dim: dimmed(p.id) })),
        [{ name: 'Budget', cls: 'c-slate' }, { name: 'Actual', cls: 'c-sky' }, { name: 'EAC', cls: 'c-amber' }], { w: 640, h: 360, tick: (v) => v.toFixed(1), onClick: (id) => this.toggle(id) });
      legend('viz-a', [['c-slate', 'Budget'], ['c-sky', 'Actual'], ['c-amber', 'EAC']]);
      $('#pb-t').innerHTML = 'Monthly cost trend <span class="hint">AED M · 2026</span>';
      C.lines($('#viz-b'), [{ key: 'cost', values: x.monthly, cls: 'c-teal' }], D.months, { area: true, w: 520, h: 380, tick: (v) => v.toFixed(2) });
      $('#pc-t').innerHTML = 'Spend by category <span class="hint">illustrative</span>';
      C.donut($('#viz-c'), x.categories.map((c, i) => ({ label: c.name, value: c.value, cls: ['c-teal', 'c-sky', 'c-violet', 'c-amber'][i] })), { center: aed(x.actual), sub: 'posted', fmt: aed });
      legend('viz-c', x.categories.map((c, i) => [['c-teal', 'c-sky', 'c-violet', 'c-amber'][i], `${c.code} ${Math.round(c.share * 100)}%`]), true);
      const worst = x.perProject.map(p => ({ ...p, v: p.eac - p.budget })).sort((a, b) => b.v - a.v)[0];
      this.insight(worst && worst.v > 0 ? `${D.byId[worst.id].name} drives the largest projected overrun (+${aed(worst.v)}, ${Math.round((worst.eac / worst.budget - 1) * 100)}% over). Portfolio EAC is ${aed(x.eac)} against ${aed(x.budget)} approved.` : `This selection is forecast to finish within budget (EAC ${aed(x.eac)}).`);
    },
    insight(text) {
      const n = $('#insight-text');
      if (n.textContent === text) return;
      fx.animate(this.ctx.scope, n, [{ opacity: 0, transform: 'translateX(-12px)' }, { opacity: 1, transform: 'none' }], { duration: 500 });
      n.textContent = text;
    }
  };

  /* ======================= 08 · SIMULATOR ============================ */
  const BASE = { portfolio: 60, projects: 40, reduction: 25, rollout: 'phased' };
  const sim = {
    id: 's-sim', layout: ['line', 'healthy'],
    state: store.get('sim', { ...BASE }),
    enter(ctx) {
      introduce(ctx);
      this.ctx = ctx; $('#sim-viz')._s = null; $('#sim-viz').innerHTML = '';
      const bind = (id, key) => {
        const inp = $('#in-' + id); inp.value = this.state[key];
        ctx.scope.on(inp, 'input', () => { this.state[key] = +inp.value; this.update(); });
        ctx.scope.on(inp, 'keydown', (e) => e.stopPropagation()); // arrows adjust slider, not slides
      };
      bind('portfolio', 'portfolio'); bind('projects', 'projects'); bind('reduction', 'reduction');
      $$('#in-rollout button').forEach(b => ctx.scope.on(b, 'click', () => { this.state.rollout = b.dataset.v; this.update(); }));
      ctx.scope.on($('#sim-reset'), 'click', () => { Object.assign(this.state, BASE); ['portfolio', 'projects', 'reduction'].forEach(k => { $('#in-' + k).value = BASE[k]; }); this.update(); });
      const A = M.ASSUMPTIONS;
      $('#assump-list').innerHTML = `<dl>
        <dt>Average overrun today</dt><dd>${A.baselineOverrun * 100}%</dd>
        <dt>Reporting hours / project / week</dt><dd>${A.reportHoursPerProject} h</dd>
        <dt>Share of those hours automated</dt><dd>${A.hoursAutomated * 100}%</dd>
        <dt>Loaded PMO cost</dt><dd>AED ${A.hourlyCost}/h · ${A.weeks} wks</dd>
        <dt>Build & rollout (one-off)</dt><dd>AED ${A.buildCost}M + ${A.perProjectOnboard * 1000}K/project</dd>
        <dt>Run cost</dt><dd>AED ${A.runCost}M/yr + AI ${A.aiPerProject * 1e6 / 1000}K/project</dd>
        <dt>Big bang</dt><dd>+18% cost · 12% yr-1 friction</dd></dl>
        <p style="margin:8px 0 0">All figures are illustrative assumptions for this fictional proposal.</p>`;
      this.update(true);
    },
    update(first) {
      const s = this.state; store.set('sim', s);
      ['portfolio', 'projects', 'reduction'].forEach(k => { const inp = $('#in-' + k); inp.style.setProperty('--fill', ((inp.value - inp.min) / (inp.max - inp.min) * 100) + '%'); });
      $('#v-portfolio').textContent = 'AED ' + s.portfolio + 'M';
      $('#v-projects').textContent = s.projects;
      $('#v-reduction').textContent = s.reduction + '%';
      $$('#in-rollout button').forEach(b => b.setAttribute('aria-checked', String(b.dataset.v === s.rollout)));
      const r = M.simulate(s);
      this.result = r;
      const cu = (id, v, f) => { const n = $(id); const prev = n._v === undefined ? 0 : n._v; n._v = v; fx.countUp(this.ctx.scope, n, v, { from: first ? 0 : prev, duration: first ? 1200 : 500, format: f }); };
      cu('#o-benefit', r.annualBenefit, aed); $('#o-benefit-s').textContent = 'per year at full adoption';
      cu('#o-invest', r.invest, aed); $('#o-invest-s').textContent = '+ ' + aed(r.annualRun) + ' / yr to run';
      const pb = $('#o-payback');
      if (r.payback) cu('#o-payback', r.payback, (v) => Math.round(v)); else { pb.textContent = '> 36'; pb._v = 36; }
      cu('#o-net', r.net3y, (v) => (v < 0 ? '−' : '') + aed(Math.abs(v)));
      $('#o-roi').textContent = 'ROI ' + Math.round(r.roi * 100) + '% over 3 years';
      const labels = Array.from({ length: 36 }, (_, i) => (i + 1) % 6 === 0 ? 'M' + (i + 1) : '');
      const lo = Math.min(-r.invest * 1.1, ...r.cash), hi = Math.max(r.invest * 0.5, ...r.cash) * 1.08;
      const g = C.lines($('#sim-viz'), [{ key: 'cash', values: r.cash, cls: r.net3y >= 0 ? 'c-teal' : 'c-coral' }], labels, { area: true, min: lo, max: hi, zero: 0, w: 1060, h: 340, tick: (v) => v.toFixed(1) });
      const flag = $('#payback-flag');
      if (r.payback) { flag.style.opacity = 1; flag.textContent = `Payback · M${r.payback}`; flag.style.left = (20 + (g.X(r.payback - 1) / g.W) * ($('#sim-viz').clientWidth || 1000)) + 'px'; }
      else flag.style.opacity = 0;
      const tot = r.overrunAvoided + r.labour || 1;
      $('#split-overrun').style.width = (r.overrunAvoided / tot * 100) + '%';
      $('#split-labour').style.width = (r.labour / tot * 100) + '%';
      $('#o-overrun').textContent = aed(r.overrunAvoided) + '/yr';
      $('#o-labour').textContent = aed(r.labour) + '/yr · ' + Math.round(r.hoursSaved) + ' h/week';
    }
  };

  /* ======================= 09 · AUTOMATION & AI ====================== */
  const auto = {
    id: 's-auto', layout: ['left', 'ai'],
    state: { mode: 'manual', outage: false },
    enter(ctx) {
      introduce(ctx);
      const cyc = $('#cycle');
      cyc.innerHTML = M.weeklyCycle.map(c => `<div class="cyc-row"><div><div class="cyc-name">${c.task}</div><div class="cyc-how">${c.how}</div></div><div class="cyc-track"><div class="cyc-bar"></div></div><div class="cyc-h">0h</div></div>`).join('');
      $$('#auto-toggle button').forEach(b => ctx.scope.on(b, 'click', () => { this.state.mode = b.dataset.v; this.renderCycle(ctx); }));
      ctx.scope.timeout(() => this.renderCycle(ctx), 400);
      // Gateway diagram
      const svg = $('#gateway'); svg.innerHTML = '';
      const portal = svgEl('g', { class: 'gw-hub' }, svg);
      svgEl('rect', { x: 10, y: 135, width: 120, height: 60, rx: 10 }, portal);
      svgEl('text', { x: 70, y: 171 }, portal).textContent = 'Portal';
      const hub = svgEl('g', { class: 'gw-hub' }, svg);
      svgEl('rect', { x: 180, y: 120, width: 150, height: 90, rx: 12 }, hub);
      svgEl('text', { x: 255, y: 160 }, hub).textContent = 'AI gateway';
      svgEl('text', { x: 255, y: 184, style: 'font-size:13px;fill:var(--text-3)' }, hub).textContent = 'ordered failover';
      this.inWire = svgEl('path', { d: 'M130 165 L180 165', class: 'gw-wire active' }, svg);
      this.nodes = []; this.wires = [];
      D.aiProviders.forEach((name, i) => {
        const col = Math.floor(i / 4), row = i % 4;
        const x = 400 + col * 125, y = 30 + row * 75;
        const w = svgEl('path', { d: `M330 165 C 365 165, 365 ${y + 22}, ${x} ${y + 22}`, class: 'gw-wire' }, svg);
        const g = svgEl('g', { class: 'gw-node' }, svg);
        svgEl('rect', { x, y, width: 115, height: 44, rx: 8 }, g);
        svgEl('text', { x: x + 57, y: y + 27 }, g).textContent = (i + 1) + ' ' + name;
        this.nodes.push(g); this.wires.push(w);
        fx.animate(ctx.scope, g, [{ opacity: 0, transform: 'translateX(-20px)' }, { opacity: 1, transform: 'none' }], { duration: 500, delay: 300 + i * 60 });
      });
      this.packet = svgEl('circle', { r: 7, class: 'gw-packet', opacity: 0 }, svg);
      const out = $('#outage'); out.checked = this.state.outage;
      ctx.scope.on(out, 'change', () => { this.state.outage = out.checked; this.request(ctx); });
      this.request(ctx);
      if (!fx.state.reduced) ctx.scope.interval(() => { if (!fx.state.paused) this.request(ctx); }, 4200);
    },
    renderCycle(ctx) {
      const isAuto = this.state.mode === 'auto';
      $$('#auto-toggle button').forEach(b => b.setAttribute('aria-checked', String(b.dataset.v === this.state.mode)));
      $('#cycle').classList.toggle('auto-on', isAuto);
      const max = Math.max(...M.weeklyCycle.map(c => c.manual));
      $$('.cyc-row', ctx.el).forEach((row, i) => {
        const c = M.weeklyCycle[i], v = isAuto ? c.auto : c.manual;
        row.querySelector('.cyc-bar').style.width = (v / max * 100) + '%';
        row.querySelector('.cyc-how').style.opacity = isAuto ? 1 : 0;
        row.querySelector('.cyc-h').textContent = v + 'h';
      });
      const total = M.weeklyCycle.reduce((s, c) => s + (isAuto ? c.auto : c.manual), 0);
      const tn = $('#cycle-total'); const prev = tn._v || 0; tn._v = total;
      fx.countUp(ctx.scope, tn, total, { from: prev, duration: 900, format: (v) => (Math.round(v * 10) / 10) + 'h' });
      tn.classList.toggle('good', isAuto);
    },
    // One request: try providers in order; with an outage the first two fail, #3 answers.
    request(ctx) {
      const down = this.state.outage ? [0, 1] : [];
      this.nodes.forEach((n, i) => { n.classList.toggle('down', down.includes(i)); n.classList.remove('active'); });
      this.wires.forEach(w => w.setAttribute('class', 'gw-wire'));
      const target = down.length;
      const status = $('#gw-status');
      const seq = [...down, target];
      const ms = fx.state.reduced ? 1 : 700;
      status.textContent = '→ "Summarise PRJ-CBM risks" sent to gateway…';
      seq.forEach((idx, k) => ctx.scope.timeout(() => {
        const ok = idx === target;
        this.wires[idx].setAttribute('class', 'gw-wire ' + (ok ? 'active' : 'fail'));
        if (!fx.state.reduced) { fx.travel(ctx.scope, this.packet, this.wires[idx], { duration: ms * 0.9 }); }
        if (ok) {
          this.nodes[idx].classList.add('active');
          status.innerHTML = down.length
            ? `✗ ${D.aiProviders[0]} timeout · ✗ ${D.aiProviders[1]} 503 → <b style="color:var(--teal)">✓ ${D.aiProviders[idx]} answered</b> · user never noticed`
            : `<b style="color:var(--teal)">✓ ${D.aiProviders[idx]} (default) answered</b> · 11 providers on standby`;
        } else status.textContent = `✗ ${D.aiProviders[idx]} failed — trying next provider…`;
      }, 300 + k * ms));
    }
  };

  /* ======================= 10 · ROADMAP & DECISION =================== */
  const PHASES = [
    { t: 'Pilot', d: 'Jul – Oct 2026', done: true, cost: 'Delivered', pts: ['Portal live with 8 projects and 3 demo roles', 'All 9 modules running on the pilot dataset', 'AI gateway with 12 providers and failover', 'English / Arabic with RTL'] },
    { t: 'Harden & integrate', d: 'Nov 2026 – Jan 2027', cost: 'AED 0.38M', pts: ['Corporate SSO and role mapping', 'ERP feed for posted costs (no double entry)', 'External penetration test and fixes', 'Load test at 80 projects / 400 users'] },
    { t: 'Group rollout', d: 'Feb – Apr 2027', cost: 'AED 0.48M', pts: ['Onboard 40 projects across 5 group entities', 'Train PMs and sponsors (bilingual)', 'Retire spreadsheet status packs', 'Portal becomes the reporting source of truth'] },
    { t: 'Optimise', d: 'May – Sep 2027', cost: 'From run budget', pts: ['Predictive EAC from forecast history', 'AI early-warning on risk trends', 'Power BI dataset export for group finance', 'Mobile approvals for sponsors'] }
  ];
  const GATES = [
    ['Unit & integration tests', 'on every pull request for API and UI'],
    ['Permission tests per role', 'every pmo:* key checked for admin, PM and contributor'],
    ['UAT scripts per module', 'signed off by the PMO before each release'],
    ['Performance budget', 'list pages under 1 s; AI calls time out at 60 s'],
    ['Security', 'pen-test, JWT expiry, audit trail on every change'],
    ['Bilingual QA', 'every screen checked in Arabic RTL and English']
  ];
  const roadmap = {
    id: 's-roadmap', layout: ['line', 'healthy'],
    state: { sel: 1 },
    enter(ctx) {
      introduce(ctx);
      const svg = $('#road-svg'); svg.innerHTML = '';
      const defs = svgEl('defs', {}, svg);
      const lg = svgEl('linearGradient', { id: 'roadGrad' }, defs);
      svgEl('stop', { offset: 0, 'stop-color': '#2dd4bf' }, lg); svgEl('stop', { offset: 1, 'stop-color': '#7cc4ff' }, lg);
      const xs = PHASES.map((_, i) => 140 + i * 480);
      const d = `M${xs[0]} 80 C ${xs[0] + 200} 20, ${xs[1] - 200} 140, ${xs[1]} 80 S ${xs[2] - 200} 20, ${xs[2]} 80 S ${xs[3] - 200} 140, ${xs[3]} 80`;
      svgEl('path', { d, class: 'track' }, svg);
      const fill = svgEl('path', { d, class: 'track-fill' }, svg);
      fx.drawPath(fill, 1800, 400, ctx.scope);
      svgEl('text', { x: (xs[0] + xs[1]) / 2, y: 22, class: 'now' }, svg).textContent = '▼ WE ARE HERE';
      PHASES.forEach((p, i) => {
        const g = svgEl('g', { class: 'ph' + (p.done ? ' done' : ''), 'data-i': i, transform: `translate(${xs[i]} 80)`, role: 'button', tabindex: 0, 'aria-label': p.t }, svg);
        svgEl('circle', { r: 26 }, g);
        svgEl('text', { class: 'ph-n', y: 6 }, g).textContent = p.done ? '✓' : i;
        svgEl('text', { class: 'ph-t', y: 70 }, g).textContent = p.t;
        svgEl('text', { class: 'ph-d', y: 96 }, g).textContent = p.d;
        fx.animate(ctx.scope, g, [{ opacity: 0, transform: `translate(${xs[i]}px, 110px) scale(.6)` }, { opacity: 1, transform: `translate(${xs[i]}px, 80px) scale(1)` }], { duration: 700, delay: 600 + i * 350, fill: 'none' });
        ctx.scope.on(g, 'click', () => this.select(ctx, i));
        ctx.scope.on(g, 'keydown', (e) => { if (e.key === 'Enter') this.select(ctx, i); });
      });
      $('#gates').innerHTML = GATES.map(([b, t]) => `<li><i>✓</i><span><b>${b}</b> — ${t}</span></li>`).join('');
      fx.rise(ctx.scope, $$('#gates li'), { delay: 900, stagger: 90, y: 10 });
      const r = M.simulate(sim.state);
      this.invest = r.invest;
      $('#ask-amount').textContent = aed(r.invest);
      $('#ask-case').innerHTML = `From your simulator inputs (scene 08):<br>benefit ${aed(r.annualBenefit)}/yr · payback ${r.payback ? 'month ' + r.payback : '> 36 months'} · 3-yr net ${(r.net3y < 0 ? '−' : '') + aed(Math.abs(r.net3y))}`;
      this.select(ctx, this.state.sel, true);
      const closing = $('#closing');
      closing.style.opacity = 0;
      ctx.scope.timeout(() => { closing.style.opacity = ''; fx.revealWords(ctx.scope, closing, { stagger: 120, duration: 1100 }); }, 2600);
    },
    select(ctx, i, quiet) {
      this.state.sel = i; const p = PHASES[i];
      $$('#road-svg .ph').forEach(g => g.classList.toggle('sel', +g.dataset.i === i));
      const box = $('#phase-detail');
      box.innerHTML = `<h3>${i === 0 ? 'Phase 0' : 'Phase ' + i} · ${p.t}</h3><div class="pd-when">${p.d}</div><ul>${p.pts.map(x => `<li>${x}</li>`).join('')}</ul><div class="pd-cost">${i === 1 ? aed(this.invest * 0.44) + ' · 44% of the ask' : i === 2 ? aed(this.invest * 0.56) + ' · 56% of the ask' : p.cost}</div>`;
      if (!quiet) fx.rise(ctx.scope, $$('li', box), { stagger: 60, y: 10, duration: 400 });
    }
  };

  return [open, problem, value, solution, arch, usecase, dash, sim, auto, roadmap];
})();
