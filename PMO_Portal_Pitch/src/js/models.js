/* ==========================================================================
   models.js — pure functions only. No DOM access here.
   Every KPI the dashboard and simulator display is computed in this file,
   so the maths can be read, tested and explained on its own.
   ========================================================================== */
PMO.models = (function () {
  'use strict';
  const D = PMO.data;
  const sum = (a) => a.reduce((s, v) => s + v, 0);
  const days = (a, b) => (new Date(b) - new Date(a)) / 86400000;

  /* ---------- Portfolio analytics (Executive Dashboard) ----------------- */

  // filter: null = whole portfolio, or a Set of project ids (cross-filter).
  function inScope(filter) { return (id) => !filter || filter.size === 0 || filter.has(id); }

  function delivery(filter) {
    const ok = inScope(filter);
    const ps = D.projects.filter(p => ok(p.id));
    const ts = D.tasks.filter(t => ok(t.p));
    const ms = D.milestones.filter(m => ok(m.p));
    const open = ts.filter(t => t.status !== 'Done');
    const overdue = open.filter(t => t.due < D.CUTOFF);
    const due14 = ms.filter(m => m.status === 'Pending' && days(D.CUTOFF, m.due) >= -30 && days(D.CUTOFF, m.due) <= 14);
    const decided = ms.filter(m => m.status !== 'Pending');
    // Portfolio progress is budget-weighted, so a 5M project counts more than a 0.6M one.
    const wsum = sum(ps.map(p => p.budget)) || 1;
    const progress = sum(ps.map(p => p.progress * p.budget)) / wsum;
    const statusByProject = ps.map(p => {
      const pt = ts.filter(t => t.p === p.id);
      return { id: p.id, Done: pt.filter(t => t.status === 'Done').length, InProgress: pt.filter(t => t.status === 'InProgress').length,
        ToDo: pt.filter(t => t.status === 'ToDo').length, Blocked: pt.filter(t => t.status === 'Blocked').length };
    });
    return {
      active: ps.filter(p => p.status === 'Active').length, total: ps.length,
      progress, overdue: overdue.length, blocked: open.filter(t => t.status === 'Blocked').length,
      openTasks: open.length, milestonesDue: due14.length,
      hitRate: decided.length ? decided.filter(m => m.status === 'Achieved').length / decided.length : 0,
      trend: ps.map(p => ({ id: p.id, values: D.snapshots[p.id] })),
      statusByProject, overdueTasks: overdue, milestones: ms
    };
  }

  function riskGov(filter) {
    const ok = inScope(filter);
    const rs = D.risks.filter(r => ok(r.p));
    const open = rs.filter(r => r.status !== 'Closed');
    const high = open.filter(r => r.prob * r.imp >= 15);
    const crs = D.changeRequests.filter(c => ok(c.p));
    const pending = crs.filter(c => c.status === 'Submitted' || c.status === 'UnderReview');
    // 5×5 heat grid of OPEN items: grid[prob-1][imp-1] = list of risks.
    const grid = Array.from({ length: 5 }, () => Array.from({ length: 5 }, () => []));
    open.forEach(r => grid[r.prob - 1][r.imp - 1].push(r));
    const crStatus = {};
    crs.forEach(c => { crStatus[c.status] = (crStatus[c.status] || 0) + 1; });
    return { open: open.length, high, pending: pending.length, pendingCost: sum(pending.map(c => c.cost)),
      approvedCost: sum(crs.filter(c => c.status === 'Approved').map(c => c.cost)), grid, crStatus, risks: open };
  }

  function finance(filter) {
    const ok = inScope(filter);
    const ps = D.projects.filter(p => ok(p.id));
    const budget = sum(ps.map(p => p.budget));
    const actual = sum(ps.map(p => p.actual));
    const eac = sum(ps.map(p => p.eac));
    const monthly = D.months.map((_, i) => sum(ps.map(p => D.monthlyCost[p.id][i])));
    return {
      budget, actual, remaining: budget - actual, utilization: budget ? actual / budget : 0, eac,
      variance: eac - budget, overruns: ps.filter(p => p.eac > p.budget * 1.02).length,
      perProject: ps.map(p => ({ id: p.id, budget: p.budget, actual: p.actual, eac: p.eac })),
      monthly, categories: D.categories.map(c => ({ ...c, value: actual * c.share }))
    };
  }

  /* ---------- Business-case simulator ------------------------------------
     Inputs (all chosen live by the audience):
       portfolio  – annual project portfolio value, AED M
       projects   – number of projects managed in the portal
       reduction  – % of today's cost overrun that earlier visibility avoids
       rollout    – 'phased' | 'bigbang'
     Assumptions are exported so the scene can print them on screen. */
  const ASSUMPTIONS = {
    baselineOverrun: 0.11,     // 11% average overrun today (illustrative)
    reportHoursPerProject: 3.5, // PMO hours / project / week on manual reporting
    hoursAutomated: 0.7,        // 70% of those hours automated
    hourlyCost: 210,            // AED per PMO hour, fully loaded
    weeks: 46,
    buildCost: 0.62,            // AED M one-off: rollout, integration, training
    perProjectOnboard: 0.006,   // AED M per project onboarded
    runCost: 0.16,              // AED M / year hosting + support
    aiPerProject: 0.0012,       // AED M / year AI tokens per project
    horizon: 36                 // months modelled
  };

  function simulate({ portfolio, projects, reduction, rollout }) {
    const A = ASSUMPTIONS;
    const bigBang = rollout === 'bigbang';
    const overrunAvoided = portfolio * A.baselineOverrun * (reduction / 100);          // AED M / yr
    const labour = projects * A.reportHoursPerProject * A.hoursAutomated * A.weeks * A.hourlyCost / 1e6; // AED M / yr
    const annualBenefit = overrunAvoided + labour;
    const invest = (A.buildCost + projects * A.perProjectOnboard) * (bigBang ? 1.18 : 1); // big bang needs parallel teams
    const annualRun = A.runCost + projects * A.aiPerProject;

    // Adoption ramp: phased reaches 100% in month 6; big bang in month 2 but
    // carries a 12% benefit haircut in year one for change-management friction.
    const cash = []; let cum = -invest; let payback = null;
    for (let m = 1; m <= A.horizon; m++) {
      const ramp = bigBang ? Math.min(1, m / 2) : Math.min(1, m / 6);
      const friction = bigBang && m <= 12 ? 0.88 : 1;
      cum += (annualBenefit * ramp * friction - annualRun) / 12;
      cash.push(cum);
      if (payback === null && cum >= 0) payback = m;
    }
    const net3y = cum;
    const roi = net3y / (invest + annualRun * 3);
    return { overrunAvoided, labour, annualBenefit, invest, annualRun, cash, payback, net3y, roi,
      hoursSaved: projects * A.reportHoursPerProject * A.hoursAutomated };
  }

  /* ---------- Automation comparison (manual vs portal) ------------------ */
  const weeklyCycle = [
    { task: 'Collect status from PMs',    manual: 9, auto: 1.0, how: 'Nightly snapshot job + task progress updates' },
    { task: 'Reconcile cost sheets',      manual: 7, auto: 1.5, how: 'Posted cost transactions with categories' },
    { task: 'Chase overdue & blocked',    manual: 5, auto: 0.5, how: 'Automation rules → outbox → SignalR alerts' },
    { task: 'Build executive pack',       manual: 8, auto: 0.5, how: 'Live dashboard + AI insights' },
    { task: 'Track change decisions',     manual: 4, auto: 0.5, how: 'CR workflow with history & audit trail' }
  ];

  return { delivery, riskGov, finance, simulate, ASSUMPTIONS, weeklyCycle };
})();
