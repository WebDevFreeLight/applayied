/* ==========================================================================
   data.js — the single source of truth for every number on screen.

   Delivery records (projects, tasks, milestones, risks, change requests,
   monthly snapshots) mirror the PMO Portal's seed/demo dataset, plus a few
   illustrative records for NBU/DLP/CYB/AIC, which the demo account could not see.
   Financial figures (actual spend, EAC, monthly costs, category split) are
   ILLUSTRATIVE ASSUMPTIONS created for this pitch. Nothing here is real
   company data; the organisation "Sahel Holding Group" is fictional.
   ========================================================================== */
window.PMO = window.PMO || {};

PMO.data = (function () {
  'use strict';

  // Reporting cutoff used by every "overdue" / "due in 14 days" calculation.
  const CUTOFF = '2026-10-19';

  // budget / actual / eac in AED millions. progress = % from latest snapshot.
  const projects = [
    { id: 'CBM', name: 'Core Banking Modernization', dept: 'IT Delivery',  owner: 'Hamdan Al Zaabi',   status: 'Active',    priority: 'Critical', budget: 5.0, actual: 3.62, eac: 5.46, progress: 76 },
    { id: 'NBU', name: 'Network Backbone Upgrade',   dept: 'Infrastructure', owner: 'Ali Al Hammadi',  status: 'Active',    priority: 'High',     budget: 2.2, actual: 1.71, eac: 2.24, progress: 80 },
    { id: 'AIC', name: 'AI Customer Insights',       dept: 'Data & AI',    owner: 'Layla Al Mazrouei', status: 'Active',    priority: 'High',     budget: 1.6, actual: 1.18, eac: 1.93, progress: 71 },
    { id: 'DLP', name: 'Data Lake Platform',         dept: 'Data & AI',    owner: 'Sara Al Hashimi',   status: 'Active',    priority: 'Medium',   budget: 2.8, actual: 2.02, eac: 2.71, progress: 75 },
    { id: 'CYB', name: 'Cybersecurity Hardening',    dept: 'Infrastructure', owner: 'Saeed Al Ketbi',  status: 'Active',    priority: 'Critical', budget: 1.4, actual: 1.04, eac: 1.37, progress: 80 },
    { id: 'HRP', name: 'HR Self-Service Portal',     dept: 'PMO',          owner: 'Fatima Al Qubaisi', status: 'Active',    priority: 'Medium',   budget: 0.8, actual: 0.61, eac: 0.93, progress: 71 },
    { id: 'MOB', name: 'Mobile App Revamp',          dept: 'IT Delivery',  owner: 'Hamdan Al Zaabi',   status: 'Active',    priority: 'Low',      budget: 0.6, actual: 0.31, eac: 0.58, progress: 72 },
    { id: 'LEG', name: 'Legacy Decommissioning',     dept: 'IT Delivery',  owner: 'Fatima Al Qubaisi', status: 'Completed', priority: 'Medium',   budget: 0.9, actual: 0.93, eac: 0.93, progress: 100 }
  ];

  // Monthly progress snapshots (Automation job writes one per project per month).
  const snapshotMonths = ['Jul', 'Aug', 'Sep', 'Oct'];
  const snapshots = {
    CBM: [17, 35, 58, 76], NBU: [20, 40, 60, 80], AIC: [14, 28, 57, 71], DLP: [18, 37, 56, 75],
    CYB: [20, 40, 60, 80], HRP: [14, 28, 57, 71], MOB: [18, 36, 54, 72], LEG: [16, 33, 50, 100]
  };

  // Tasks: [project, status, priority, due]
  const tasks = [
    ['MOB','Done','Low','2026-07-10'],['CBM','InProgress','High','2026-07-14'],['MOB','InProgress','Medium','2026-07-15'],
    ['CBM','InProgress','Critical','2026-07-28'],['CBM','ToDo','Medium','2026-07-31'],['CBM','Done','Low','2026-08-06'],
    ['LEG','Done','Medium','2026-08-11'],['HRP','Blocked','Critical','2026-08-12'],['LEG','Done','Medium','2026-08-19'],
    ['LEG','Done','Critical','2026-08-22'],['CBM','ToDo','Low','2026-09-08'],['HRP','InProgress','High','2026-09-17'],
    ['CBM','Blocked','Critical','2026-09-20'],['CBM','InProgress','High','2026-09-30'],['CBM','ToDo','Medium','2026-10-06'],
    ['HRP','InProgress','Critical','2026-10-14'],['CBM','ToDo','Low','2026-10-15'],['CBM','InProgress','Low','2026-10-16'],
    ['CBM','Done','Medium','2026-11-06'],['MOB','InProgress','High','2026-11-06'],['HRP','Done','High','2026-11-07'],
    ['LEG','Done','Medium','2026-11-08'],['HRP','Blocked','Medium','2026-11-09'],['MOB','Done','Critical','2026-11-09'],
    ['CBM','Done','Low','2026-11-15'],['MOB','Done','Medium','2026-11-16'],['CBM','Blocked','Medium','2026-11-26'],
    ['CBM','ToDo','Low','2026-12-05'],['MOB','Blocked','Low','2026-12-17'],['MOB','Blocked','High','2026-12-25'],
    ['LEG','Done','Medium','2026-12-26'],['CBM','ToDo','Critical','2026-12-27'],['LEG','Done','High','2026-12-31'],
    ['MOB','ToDo','High','2027-01-09'],['HRP','ToDo','Critical','2027-01-10'],['CBM','InProgress','Low','2027-01-21'],
    ['MOB','InProgress','Low','2027-01-21'],['HRP','Done','Low','2027-01-22'],['MOB','ToDo','Medium','2027-01-23'],
    ['CBM','Blocked','Critical','2027-01-24'],['MOB','Blocked','Medium','2027-01-25'],
    ['NBU','InProgress','High','2026-10-28'],['DLP','InProgress','Medium','2026-10-30'],['CYB','ToDo','High','2026-11-02'],
    ['AIC','Blocked','Critical','2026-10-02'],['AIC','InProgress','High','2026-10-24']
  ].map(([p, status, priority, due]) => ({ p, status, priority, due }));

  // Milestones: [project, name, due, status]
  const milestones = [
    ['LEG','Kickoff','2025-07-31','Achieved'],['LEG','Design Complete','2025-09-29','Achieved'],['DLP','Kickoff','2025-10-31','Achieved'],
    ['LEG','Build Complete','2025-11-28','Achieved'],['CBM','Kickoff','2025-11-30','Missed'],['DLP','Design Complete','2025-12-30','Achieved'],
    ['NBU','Kickoff','2026-01-14','Missed'],['CBM','Design Complete','2026-01-29','Achieved'],['CYB','Kickoff','2026-01-30','Missed'],
    ['DLP','Build Complete','2026-02-28','Missed'],['AIC','Kickoff','2026-03-11','Missed'],['NBU','Design Complete','2026-03-15','Achieved'],
    ['CBM','Build Complete','2026-03-30','Missed'],['CYB','Design Complete','2026-03-31','Achieved'],['HRP','Kickoff','2026-04-02','Missed'],
    ['AIC','Design Complete','2026-05-10','Achieved'],['NBU','Build Complete','2026-05-14','Achieved'],['CYB','Build Complete','2026-05-30','Missed'],
    ['HRP','Design Complete','2026-06-01','Missed'],['AIC','Build Complete','2026-07-09','Missed'],['HRP','Build Complete','2026-07-31','Achieved'],
    ['AIC','UAT Sign-off','2026-09-07','Missed'],['NBU','Cutover readiness','2026-10-07','Pending'],['MOB','Kickoff','2026-10-31','Pending'],
    ['MOB','Design Complete','2026-12-30','Pending'],['MOB','Build Complete','2027-02-28','Pending'],['MOB','UAT Sign-off','2027-04-29','Pending']
  ].map(([p, name, due, status]) => ({ p, name, due, status }));

  // Risks & issues: probability (1-5) × impact (1-5) = score. ≥15 is "high exposure".
  const risks = [
    { p: 'HRP', kind: 'Risk',  title: 'Data quality gap',          owner: 'Khalid Al Shamsi',  prob: 5, imp: 5, status: 'Open' },
    { p: 'LEG', kind: 'Issue', title: 'Vendor delay',              owner: 'Ali Al Hammadi',    prob: 5, imp: 5, status: 'Mitigating' },
    { p: 'CBM', kind: 'Risk',  title: 'Core migration data loss',  owner: 'Hamdan Al Zaabi',   prob: 5, imp: 4, status: 'Closed' },
    { p: 'CBM', kind: 'Risk',  title: 'Integration failure',       owner: 'Tariq Al Jaberi',   prob: 4, imp: 4, status: 'Mitigating' },
    { p: 'MOB', kind: 'Risk',  title: 'Security vulnerability',    owner: 'Saeed Al Ketbi',    prob: 3, imp: 5, status: 'Mitigating' },
    { p: 'LEG', kind: 'Issue', title: 'Security vulnerability',    owner: 'Ali Al Hammadi',    prob: 3, imp: 4, status: 'Mitigating' },
    { p: 'MOB', kind: 'Risk',  title: 'Third-party API change',    owner: 'Layla Al Mazrouei', prob: 4, imp: 2, status: 'Open' },
    { p: 'LEG', kind: 'Issue', title: 'Integration failure',       owner: 'Sara Al Hashimi',   prob: 1, imp: 3, status: 'Mitigating' },
    { p: 'MOB', kind: 'Issue', title: 'Scope creep',               owner: 'Tariq Al Jaberi',   prob: 1, imp: 1, status: 'Open' },
    { p: 'AIC', kind: 'Risk',  title: 'Model drift on churn score', owner: 'Layla Al Mazrouei', prob: 4, imp: 4, status: 'Open' },
    { p: 'AIC', kind: 'Risk',  title: 'Consent data gaps',         owner: 'Khalid Al Shamsi',  prob: 3, imp: 5, status: 'Open' },
    { p: 'DLP', kind: 'Risk',  title: 'Storage cost growth',       owner: 'Sara Al Hashimi',   prob: 3, imp: 3, status: 'Open' },
    { p: 'CYB', kind: 'Issue', title: 'Pen-test findings backlog',  owner: 'Saeed Al Ketbi',   prob: 4, imp: 3, status: 'Mitigating' },
    { p: 'NBU', kind: 'Risk',  title: 'Cutover window slip',       owner: 'Ali Al Hammadi',    prob: 2, imp: 4, status: 'Open' }
  ];

  // Change requests (cost impact in AED).
  const changeRequests = [
    { p: 'LEG', title: 'Change hosting region',   by: 'Ali Al Hammadi',   status: 'Returned', cost: 25000 },
    { p: 'HRP', title: 'Increase scope: SSO',     by: 'Mariam Al Ali',    status: 'Approved', cost: 0 },
    { p: 'MOB', title: 'Add audit logging',       by: 'Layla Al Mazrouei', status: 'Rejected', cost: 0 },
    { p: 'HRP', title: 'Switch vendor',           by: 'Tariq Al Jaberi',  status: 'Approved', cost: 30000 },
    { p: 'LEG', title: 'Increase scope: SSO',     by: 'Mariam Al Ali',    status: 'Approved', cost: 0 },
    { p: 'HRP', title: 'Add mobile support',      by: 'Amna Al Rumaithi', status: 'Approved', cost: 60000 },
    { p: 'CBM', title: 'Extend timeline 4 weeks', by: 'Mariam Al Ali',    status: 'Returned', cost: 0 },
    { p: 'AIC', title: 'Add Arabic NLP model',    by: 'Layla Al Mazrouei', status: 'UnderReview', cost: 120000 },
    { p: 'CBM', title: 'Parallel-run extension',  by: 'Hamdan Al Zaabi',  status: 'Submitted', cost: 210000 }
  ];

  // ILLUSTRATIVE: monthly posted cost (AED M), Jan–Sep 2026, whole portfolio.
  const months = ['Jan','Feb','Mar','Apr','May','Jun','Jul','Aug','Sep'];
  const monthlyCost = {
    CBM: [0.30,0.34,0.38,0.40,0.42,0.43,0.45,0.46,0.44],
    NBU: [0.15,0.17,0.19,0.20,0.21,0.21,0.19,0.20,0.19],
    AIC: [0.08,0.10,0.12,0.13,0.14,0.15,0.15,0.15,0.16],
    DLP: [0.18,0.20,0.22,0.23,0.24,0.24,0.24,0.24,0.23],
    CYB: [0.09,0.10,0.11,0.12,0.12,0.13,0.12,0.13,0.12],
    HRP: [0.04,0.05,0.06,0.07,0.07,0.08,0.08,0.08,0.08],
    MOB: [0.00,0.00,0.02,0.03,0.04,0.05,0.05,0.06,0.06],
    LEG: [0.12,0.12,0.11,0.10,0.10,0.10,0.10,0.09,0.09]
  };

  // ILLUSTRATIVE: share of spend by cost category (codes match the portal).
  const categories = [
    { code: 'LAB', name: 'Labour',   share: 0.46 },
    { code: 'SVC', name: 'Services', share: 0.27 },
    { code: 'SW',  name: 'Software', share: 0.17 },
    { code: 'HW',  name: 'Hardware', share: 0.10 }
  ];

  // The 12 OpenAI-compatible providers configured in the portal's AI gateway, in failover order.
  const aiProviders = ['Groq','OpenRouter','Cerebras','Gemini','SambaNova','Together','HuggingFace','Qwen','DeepSeek','Mistral','NVIDIA NIM','Grok'];

  const byId = Object.fromEntries(projects.map(p => [p.id, p]));

  return { CUTOFF, projects, byId, snapshotMonths, snapshots, tasks, milestones, risks, changeRequests, months, monthlyCost, categories, aiProviders };
})();
