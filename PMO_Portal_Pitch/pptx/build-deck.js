/* Build the PowerPoint companion deck.
   Numbers come from the SAME data.js / models.js the HTML presentation uses,
   so the deck and the interactive experience can never disagree.
   Run: node pptx/build-deck.js   (from the project root) */
const fs = require('fs');
const path = require('path');
const pptxgen = require('pptxgenjs');
const { applyTheme } = require('/mnt/skills/public/pptx/scripts/apply_theme.js');

// Load the browser data/model files into Node.
global.window = { matchMedia: () => ({ matches: false }) };
global.PMO = {};
for (const f of ['data', 'models']) eval(fs.readFileSync(path.join(__dirname, '..', 'src', 'js', f + '.js'), 'utf8'));
const D = PMO.data, M = PMO.models;
const del = M.delivery(null), rg = M.riskGov(null), fin = M.finance(null);
const BASE = { portfolio: 60, projects: 40, reduction: 25, rollout: 'phased' };
const sim = M.simulate(BASE), simBB = M.simulate({ ...BASE, rollout: 'bigbang' });
const aed = (m) => 'AED ' + (m >= 1 ? m.toFixed(m >= 10 ? 1 : 2) + 'M' : Math.round(m * 1000) + 'K');

const THEME = {
  name: 'PMO Portal Night',
  headFontFace: 'Calibri', bodyFontFace: 'Calibri',
  colors: { dk1: '0A111D', lt1: 'FFFFFF', dk2: '16233A', lt2: 'E8EEF8', accent1: '2DD4BF', accent2: '7CC4FF', accent3: 'F5B342', accent4: 'FF6B5E', accent5: '8B7CFF', accent6: '4ADE80', hlink: '7CC4FF', folHlink: '8B7CFF' }
};
const HEX = { bg: '0A111D', card: '16233A', card2: '1D2D49', line: '2A3A55', text: 'E8EEF8', text2: 'A7B6CC', text3: '70819B', teal: '2DD4BF', sky: '7CC4FF', amber: 'F5B342', coral: 'FF6B5E', violet: '8B7CFF', green: '4ADE80', slate: '4B5D78' };

const pres = new pptxgen();
pres.layout = 'LAYOUT_WIDE'; // 13.333 x 7.5 in
pres.title = 'PMO Portal — Investment Proposal';
pres.author = 'Abdulqader Alshawea';
pres.company = 'Sahel Holding Group (fictional)';
pres.theme = { headFontFace: THEME.headFontFace, bodyFontFace: THEME.bodyFontFace };
const C = pres.SchemeColor;
const W = 13.333, MX = 0.6;

/* ---------------- Layouts ---------------- */
pres.defineSlideMaster({
  title: 'TITLE_DARK',
  background: { color: HEX.bg },
  objects: [
    { placeholder: { options: { name: 'kicker', type: 'body', x: MX, y: 0.9, w: 8, h: 0.4, fontSize: 12, color: C.accent1, bold: true, charSpacing: 2, margin: 0 }, text: '' } },
    { placeholder: { options: { name: 'title', type: 'title', x: MX, y: 1.9, w: 7.4, h: 2.6, fontSize: 48, bold: true, color: C.background2, valign: 'top', align: 'left', margin: 0 }, text: '' } },
    { placeholder: { options: { name: 'body', type: 'body', x: MX, y: 4.7, w: 6.6, h: 1.2, fontSize: 16, color: C.background2, valign: 'top', margin: 0 }, text: '' } },
    { text: { text: 'Fictional organisation. All financial figures are illustrative assumptions.', options: { x: MX, y: 6.85, w: 8, h: 0.3, fontSize: 10, color: HEX.text3, margin: 0 } } }
  ]
});
pres.defineSlideMaster({
  title: 'CONTENT',
  background: { color: HEX.bg },
  margin: [0.5, 0.6, 0.6, 0.6],
  slideNumber: { x: W - 1.0, y: 7.0, w: 0.5, h: 0.3, fontSize: 10, color: HEX.text3, align: 'right' },
  objects: [
    { placeholder: { options: { name: 'kicker', type: 'body', x: MX, y: 0.4, w: 10, h: 0.3, fontSize: 12, color: C.accent1, bold: true, charSpacing: 2, margin: 0 }, text: '' } },
    { placeholder: { options: { name: 'title', type: 'title', x: MX, y: 0.72, w: W - 2 * MX, h: 0.8, fontSize: 30, bold: true, color: C.background2, valign: 'top', align: 'left', margin: 0 }, text: '' } },
    { text: { text: 'PMO Portal · Investment proposal · illustrative figures', options: { x: MX, y: 7.0, w: 6, h: 0.3, fontSize: 10, color: HEX.text3, margin: 0 } } }
  ]
});

/* ---------------- Helpers ---------------- */
function content(section, kicker, title) {
  const s = pres.addSlide({ masterName: 'CONTENT', sectionTitle: section });
  s.addText(kicker.toUpperCase(), { placeholder: 'kicker' });
  s.addText(title, { placeholder: 'title' });
  return s;
}
const card = (s, x, y, w, h, name, fill) => s.addShape(pres.shapes.ROUNDED_RECTANGLE, { x, y, w, h, rectRadius: 0.12, fill: { color: fill || HEX.card }, line: { color: HEX.line, width: 0.75 }, objectName: name });
const txt = (s, text, o) => s.addText(text, { isTextBox: true, margin: 0, color: HEX.text, fontSize: 14, valign: 'top', ...o });
const chartBase = () => ({
  catAxisLabelColor: HEX.text2, valAxisLabelColor: HEX.text2, catAxisLabelFontSize: 11, valAxisLabelFontSize: 11,
  catAxisLabelFontFace: '+mn-lt', valAxisLabelFontFace: '+mn-lt', titleFontFace: '+mn-lt', legendFontFace: '+mn-lt', dataLabelFontFace: '+mn-lt',
  valGridLine: { color: HEX.line, size: 0.5 }, catGridLine: { style: 'none' }, catAxisLineShow: false, valAxisLineShow: false,
  showTitle: true, titleColor: HEX.text, titleFontSize: 13, legendColor: HEX.text2, legendFontSize: 11, dataLabelColor: HEX.text, dataLabelFontSize: 10,
  plotArea: { fill: { color: HEX.card } }, chartArea: { fill: { color: HEX.card }, roundedCorners: true }
});

/* ===== 1 · Title ===== */
pres.addSection({ title: 'Opening' });
{
  const s = pres.addSlide({ masterName: 'TITLE_DARK', sectionTitle: 'Opening' });
  s.addText('SAHEL HOLDING GROUP · INVESTMENT COMMITTEE · OCTOBER 2026', { placeholder: 'kicker' });
  s.addText([{ text: 'Every dirham. Every milestone. ' }, { text: 'One source of truth', options: { color: C.accent1 } }], { placeholder: 'title' });
  s.addText('A proposal to run the group\'s transformation portfolio — delivery, risk, change and money — from one live platform with automation and AI built in.', { placeholder: 'body' });
  // Portfolio constellation motif: the 8 pilot projects as nodes on a ring.
  const cx = 10.35, cy = 3.7, R = 2.0;
  D.projects.forEach((p, i) => {
    const a = (i / D.projects.length) * Math.PI * 2 - Math.PI / 2;
    const x = cx + Math.cos(a) * R, y = cy + Math.sin(a) * R, r = 0.12 + p.budget * 0.035;
    const col = p.eac > p.budget * 1.05 ? HEX.coral : p.eac > p.budget ? HEX.amber : HEX.teal;
    s.addShape(pres.shapes.LINE, { x: Math.min(cx, x), y: Math.min(cy, y), w: Math.abs(x - cx), h: Math.abs(y - cy), flipH: (x < cx) !== (y < cy), line: { color: HEX.line, width: 1 }, objectName: 'link-' + p.id });
    s.addShape(pres.shapes.OVAL, { x: x - r, y: y - r, w: 2 * r, h: 2 * r, fill: { color: col }, line: { color: col, width: 0 }, objectName: 'node-' + p.id });
    txt(s, p.id, { x: x + r + 0.05, y: y - 0.32, w: 0.7, h: 0.25, fontSize: 11, color: HEX.text2, fontFace: 'Courier New' });
  });
  s.addShape(pres.shapes.OVAL, { x: cx - 0.75, y: cy - 0.75, w: 1.5, h: 1.5, fill: { color: HEX.card }, line: { color: HEX.teal, width: 2 }, objectName: 'hub' });
  txt(s, [{ text: 'PMO', options: { bold: true, fontSize: 20, breakLine: true } }, { text: 'Portal', options: { color: HEX.teal, fontSize: 14 } }], { x: cx - 0.75, y: cy - 0.42, w: 1.5, h: 0.84, align: 'center', valign: 'middle' });
  txt(s, 'Node size = budget · colour = forecast vs budget', { x: 8.6, y: 6.2, w: 3.6, h: 0.3, fontSize: 10, color: HEX.text3, align: 'center' });
  s.addNotes('Open with the hook: the group runs eight transformation projects worth AED 15M on spreadsheets and email. This proposal asks for investment to make the PMO Portal the single source of truth. The portal already runs in pilot — this is a scale-up decision, not a bet on an idea.');
}

/* ===== 2 · Problem ===== */
pres.addSection({ title: 'Case for change' });
{
  const s = content('Case for change', '02 · The problem', 'The portfolio is run on spreadsheets, status emails and memory');
  const stats = [
    [Math.round((1 - del.hitRate) * 100) + '%', 'of decided milestones were missed', `${D.milestones.filter(m => m.status === 'Missed').length} of ${D.milestones.filter(m => m.status !== 'Pending').length} · pilot data`, HEX.coral],
    [String(del.overdue), 'open tasks already overdue', `${del.blocked} more are blocked`, HEX.amber],
    [String(fin.overruns), 'of 8 projects heading over budget', 'seen only at quarter-end today', HEX.coral],
    ['34 days', 'from overrun to executive visibility', 'illustrative · manual monthly cycle', HEX.amber]
  ];
  stats.forEach(([n, l, f, col], i) => {
    const x = MX + (i % 2) * 3.55, y = 1.85 + Math.floor(i / 2) * 2.45;
    card(s, x, y, 3.35, 2.2, 'stat-' + i);
    txt(s, n, { x: x + 0.3, y: y + 0.25, w: 2.9, h: 0.95, fontSize: 48, bold: true, color: col });
    txt(s, l, { x: x + 0.3, y: y + 1.2, w: 2.9, h: 0.5, fontSize: 15, color: HEX.text });
    txt(s, f, { x: x + 0.3, y: y + 1.7, w: 2.9, h: 0.3, fontSize: 11, color: HEX.text3 });
  });
  const art = ['Status_Report_v7_FINAL(2).xlsx', 'RE: RE: FW: budget for Q3??', '"Was the vendor switch approved?"', 'Exec_pack_Oct — 3 days to build', 'Risk_log_old_DO_NOT_USE.xlsx'];
  art.forEach((a, i) => {
    const y = 1.95 + i * 0.92, x = 8.3 + (i % 2) * 0.55;
    s.addShape(pres.shapes.ROUNDED_RECTANGLE, { x, y, w: 4.0, h: 0.62, rectRadius: 0.08, rotate: [-4, 3, -2, 5, -3][i], fill: { color: '2A1A22' }, line: { color: HEX.coral, width: 0.75, transparency: 40 }, objectName: 'artefact-' + i });
    txt(s, a, { x: x + 0.2, y: y + 0.16, w: 3.6, h: 0.32, fontSize: 13, color: 'F3C9C4', fontFace: 'Courier New', rotate: [-4, 3, -2, 5, -3][i] });
  });
  s.addNotes('Delivery numbers come from the PMO Portal pilot dataset: half of decided milestones were missed and 14 tasks are already overdue. Financial overruns and the 34-day visibility lag are illustrative. The point: nobody sees problems until they are expensive.');
}

/* ===== 3 · Opportunity ===== */
{
  const s = content('Case for change', '03 · The opportunity', 'One live portfolio, five audiences, one version of the truth');
  const hub = { x: 3.4, y: 4.3 };
  const P = [['Executive committee', 'Decides & funds', 3.4, 2.35], ['Finance', 'Controls the money', 5.3, 3.6], ['PMO office', 'Runs the cadence', 4.6, 5.9], ['Project managers', 'Deliver the work', 2.2, 5.9], ['Delivery teams', 'Update progress', 1.5, 3.6]];
  P.forEach(([n, r, x, y], i) => {
    s.addShape(pres.shapes.LINE, { x: Math.min(hub.x, x), y: Math.min(hub.y, y), w: Math.abs(x - hub.x), h: Math.abs(y - hub.y), flipH: (x < hub.x) !== (y < hub.y), line: { color: HEX.teal, width: 1.25, dashType: 'dash' }, objectName: 'spoke-' + i });
  });
  s.addShape(pres.shapes.OVAL, { x: hub.x - 0.65, y: hub.y - 0.65, w: 1.3, h: 1.3, fill: { color: HEX.card }, line: { color: HEX.teal, width: 2 }, objectName: 'hub' });
  txt(s, [{ text: 'PMO', options: { bold: true, fontSize: 18, breakLine: true } }, { text: 'Portal', options: { color: HEX.teal, fontSize: 13 } }], { x: hub.x - 0.65, y: hub.y - 0.38, w: 1.3, h: 0.76, align: 'center', valign: 'middle' });
  P.forEach(([n, r, x, y], i) => {
    s.addShape(pres.shapes.OVAL, { x: x - 0.32, y: y - 0.32, w: 0.64, h: 0.64, fill: { color: HEX.card2 }, line: { color: HEX.line, width: 1 }, objectName: 'persona-' + i });
    txt(s, String(i + 1), { x: x - 0.32, y: y - 0.16, w: 0.64, h: 0.32, align: 'center', bold: true, fontSize: 14, color: HEX.teal });
    txt(s, n, { x: i === 0 ? x + 0.4 : x - 1.0, y: i === 0 ? y - 0.14 : y + 0.36, w: 2.0, h: 0.28, align: i === 0 ? 'left' : 'center', bold: true, fontSize: 12 });
  });
  const rows = [
    ['1 view', 'Executive committee', 'Portfolio health, money and risk on one screen — no pack to assemble'],
    ['−3 pts', 'Finance', 'EAC per project vs approved budget; overrun target 11% → 8% (illustrative)'],
    ['−80%', 'PMO office', 'Weekly reporting hours, via snapshots, rules and AI insights'],
    ['1 line', 'Project managers', 'of text and AI fills the risk or change-request form'],
    ['2 langs', 'Delivery teams', 'English and Arabic with full right-to-left layout']
  ];
  rows.forEach(([n, who, t], i) => {
    const y = 1.8 + i * 1.0;
    card(s, 6.6, y, 6.1, 0.85, 'outcome-' + i);
    txt(s, n, { x: 6.85, y: y + 0.18, w: 1.4, h: 0.5, fontSize: 22, bold: true, color: HEX.teal, valign: 'middle' });
    txt(s, [{ text: who, options: { bold: true, breakLine: true } }, { text: t, options: { color: HEX.text2, fontSize: 12 } }], { x: 8.35, y: y + 0.1, w: 4.2, h: 0.68, fontSize: 13, valign: 'middle' });
  });
  s.addNotes('Every audience gets a different benefit from the same data. In the live version, click a persona to see their outcomes. Emphasise that the numbers in this slide are targets, not guarantees.');
}

/* ===== 4 · Solution ===== */
pres.addSection({ title: 'The solution' });
{
  const s = content('The solution', '04 · Solution overview', 'Nine modules, one data model — built and running today');
  const mods = [
    ['Projects', 'Register, members, snapshots, forecasts'], ['Tasks', 'Weighted progress, overdue flags'], ['Milestones', 'Kickoff → design → build → UAT'],
    ['Risks & Issues', '5×5 scoring, mitigation, owners'], ['Change Requests', 'Approve / return / reject with history'], ['Finance', 'Costs, budget revisions, forecasts, EAC'],
    ['Executive Dashboard', 'Delivery · Risk · Financial tabs'], ['Automation & Logs', 'Rules, outbox, SignalR, audit trail'], ['AI Assistant', 'Chat, summaries, risk analysis, AI fill']
  ];
  mods.forEach(([n, t], i) => {
    const x = MX + (i % 3) * 2.95, y = 1.8 + Math.floor(i / 3) * 1.6;
    card(s, x, y, 2.75, 1.4, 'module-' + i, i === 6 ? '183C40' : HEX.card);
    s.addShape(pres.shapes.OVAL, { x: x + 0.25, y: y + 0.25, w: 0.42, h: 0.42, fill: { color: i === 6 ? HEX.teal : HEX.card2 }, line: { color: HEX.teal, width: 1 }, objectName: 'module-icon-' + i });
    txt(s, String(i + 1), { x: x + 0.25, y: y + 0.32, w: 0.42, h: 0.28, align: 'center', bold: true, fontSize: 12, color: i === 6 ? HEX.bg : HEX.teal });
    txt(s, n, { x: x + 0.8, y: y + 0.27, w: 1.85, h: 0.4, bold: true, fontSize: 15 });
    txt(s, t, { x: x + 0.25, y: y + 0.82, w: 2.35, h: 0.5, fontSize: 12, color: HEX.text2 });
  });
  card(s, 9.65, 1.8, 3.05, 4.6, 'stack');
  txt(s, 'Platform', { x: 9.9, y: 2.0, w: 2.6, h: 0.35, bold: true, fontSize: 15, color: HEX.teal });
  txt(s, ['React + TypeScript + Vite', 'ECharts dashboards', 'ASP.NET Core REST API', 'SQL Server', 'SignalR real-time', 'JWT + permission-key RBAC', 'English / العربية (RTL)', 'Light & dark themes'].map((t, i, a) => ({ text: t, options: { bullet: true, breakLine: i < a.length - 1 } })), { x: 9.9, y: 2.45, w: 2.65, h: 3.8, fontSize: 13, color: HEX.text, paraSpaceAfter: 6 });
  s.addNotes('Nine modules share one data model, so a change request, its cost and its risk are linked records — not three spreadsheets. In the live version each module is selectable with its live metrics.');
}

/* ===== 5 · Architecture ===== */
{
  const s = content('The solution', '05 · Architecture', 'A layered system you can trace end to end');
  const layers = [
    ['PRESENTATION · REACT + TYPESCRIPT', ['React SPA', 'ECharts dashboards', 'AI assistant UI', 'SignalR client']],
    ['API · ASP.NET CORE', ['JWT + RBAC', 'PMO controllers', 'Automation jobs', 'SignalR hub', 'AI gateway']],
    ['DATA · SQL SERVER', ['PMO schema', 'Audit & outbox', 'Identity & config']],
    ['EXTERNAL SERVICES', ['12 LLM providers (ordered failover)', 'SSO · ERP cost feed (phase 1)']]
  ];
  layers.forEach(([name, comps], i) => {
    const y = 1.75 + i * 1.28;
    s.addShape(pres.shapes.ROUNDED_RECTANGLE, { x: MX, y, w: 8.4, h: 1.12, rectRadius: 0.1, fill: { color: '0F1A2B' }, line: { color: HEX.line, width: 0.75 }, objectName: 'layer-' + i });
    txt(s, name, { x: MX + 0.2, y: y + 0.08, w: 6, h: 0.25, fontSize: 10, bold: true, color: HEX.text3, charSpacing: 1 });
    const cw = (8.0 - (comps.length - 1) * 0.15) / comps.length;
    comps.forEach((c, k) => {
      const x = MX + 0.2 + k * (cw + 0.15);
      s.addShape(pres.shapes.ROUNDED_RECTANGLE, { x, y: y + 0.4, w: cw, h: 0.58, rectRadius: 0.06, fill: { color: c === 'AI gateway' || c.startsWith('12') ? '2A2550' : HEX.card2 }, line: { color: HEX.line, width: 0.75 }, objectName: `comp-${i}-${k}` });
      txt(s, c, { x, y: y + 0.4, w: cw, h: 0.58, fontSize: 12, bold: true, align: 'center', valign: 'middle' });
    });
  });
  card(s, 9.3, 1.75, 3.4, 4.95, 'trace');
  txt(s, 'Trace: approve a change request', { x: 9.5, y: 1.95, w: 3.0, h: 0.35, bold: true, fontSize: 14, color: HEX.amber });
  const steps = ['Sponsor clicks Approve with a reason', 'JWT validated; decision permission checked', 'Status + history written; budget revision raised', 'Outbox event created for the requester', 'Requester notified instantly via SignalR'];
  steps.forEach((t, i) => {
    const y = 2.5 + i * 0.8;
    s.addShape(pres.shapes.OVAL, { x: 9.5, y, w: 0.36, h: 0.36, fill: { color: HEX.bg }, line: { color: HEX.amber, width: 1 }, objectName: 'step-' + i });
    txt(s, String(i + 1), { x: 9.5, y: y + 0.06, w: 0.36, h: 0.24, align: 'center', fontSize: 11, bold: true, color: HEX.amber });
    txt(s, t, { x: 10.0, y: y - 0.02, w: 2.55, h: 0.7, fontSize: 12, color: HEX.text2 });
  });
  s.addNotes('Walk one request down and back up the stack. The live version animates three traces: opening the dashboard, approving a change request, and asking the AI assistant. Key point: permissions are enforced at the API with keys like pmo:finance:list, not just hidden in the UI.');
}

/* ===== 6 · Use case ===== */
{
  const s = content('The solution', '06 · A real use case', 'A vendor switch, from one line of text to a governed decision');
  const st = [['Raise', 'PM types one sentence; AI fill completes the form'], ['Submit', 'Draft → Submitted; PMO notified in real time'], ['AI impact', '+AED 30K → EAC 960K; mitigates risk scored 25'], ['Decide', 'Sponsor approves, returns or rejects'], ['Outcome', 'Budget revised, risk 25 → 12, audit logged']];
  s.addShape(pres.shapes.LINE, { x: 1.2, y: 2.2, w: 10.9, h: 0, line: { color: HEX.teal, width: 2 }, objectName: 'rail' });
  st.forEach(([n, t], i) => {
    const x = 1.2 + i * 2.725;
    s.addShape(pres.shapes.OVAL, { x: x - 0.3, y: 1.9, w: 0.6, h: 0.6, fill: { color: HEX.teal }, line: { color: HEX.teal, width: 0 }, objectName: 'stage-' + i });
    txt(s, String(i + 1), { x: x - 0.3, y: 2.03, w: 0.6, h: 0.34, align: 'center', bold: true, fontSize: 16, color: HEX.bg });
    txt(s, n, { x: x - 1.25, y: 2.65, w: 2.5, h: 0.32, align: 'center', bold: true, fontSize: 15 });
    txt(s, t, { x: x - 1.25, y: 3.0, w: 2.5, h: 0.7, align: 'center', fontSize: 12, color: HEX.text2 });
  });
  // Before / after comparison for PRJ-HRP
  const cols = [['Before decision', HEX.card, [['Approved budget', 'AED 800,000'], ['Forecast (EAC)', 'AED 930,000'], ['High-exposure risk', 'Data quality gap · 25'], ['Time to decision', '~2 weeks of email']]],
    ['After approval', '183C40', [['Approved budget', 'AED 830,000'], ['Forecast (EAC)', 'AED 960,000'], ['High-exposure risk', 'Score 25 → 12'], ['Time to decision', '~1 hour, fully audited']]]];
  cols.forEach(([h, fill, rows], c) => {
    const x = MX + c * 6.15;
    card(s, x, 4.0, 5.95, 2.7, 'compare-' + c, fill);
    txt(s, (c ? '✓ ' : '') + h + ' · PRJ-HRP HR Self-Service Portal', { x: x + 0.3, y: 4.15, w: 5.4, h: 0.35, bold: true, fontSize: 14, color: c ? HEX.teal : HEX.text });
    rows.forEach(([k, v], r) => {
      txt(s, k, { x: x + 0.3, y: 4.65 + r * 0.48, w: 2.6, h: 0.35, fontSize: 13, color: HEX.text2 });
      txt(s, v, { x: x + 2.9, y: 4.65 + r * 0.48, w: 2.8, h: 0.35, fontSize: 13, bold: true, align: 'right', color: c && r < 3 ? HEX.teal : HEX.text });
    });
  });
  s.addNotes('In the live version the committee chooses the branch — approve, return or reject — and the project card, audit log and notifications change accordingly. Time-to-decision figures are illustrative.');
}

/* ===== 7 · Dashboard (native charts) ===== */
pres.addSection({ title: 'Intelligence & value' });
{
  const s = content('Intelligence & value', '07 · Web intelligence', 'The executive dashboard: one screen for delivery, risk and money');
  const k = [['Active projects', String(del.active)], ['Portfolio progress', Math.round(del.progress) + '%'], ['Overdue tasks', String(del.overdue)], ['High-exposure risks', String(rg.high.length)], ['Forecast EAC', aed(fin.eac)], ['Projected overruns', String(fin.overruns)]];
  const kc = [HEX.text, HEX.teal, HEX.coral, HEX.coral, HEX.amber, HEX.coral];
  k.forEach(([l, v], i) => {
    const x = MX + i * 2.05;
    card(s, x, 1.7, 1.9, 1.05, 'kpi-' + i);
    txt(s, l.toUpperCase(), { x: x + 0.15, y: 1.8, w: 1.65, h: 0.28, fontSize: 10, color: HEX.text3 });
    txt(s, v, { x: x + 0.15, y: 2.1, w: 1.65, h: 0.5, fontSize: 24, bold: true, color: kc[i] });
  });
  const ids = fin.perProject.map(p => p.id);
  s.addChart(pres.charts.BAR, [
    { name: 'Budget', labels: ids, values: fin.perProject.map(p => +p.budget.toFixed(2)) },
    { name: 'Actual', labels: ids, values: fin.perProject.map(p => +p.actual.toFixed(2)) },
    { name: 'EAC', labels: ids, values: fin.perProject.map(p => +p.eac.toFixed(2)) }
  ], { ...chartBase(), x: MX, y: 2.95, w: 6.0, h: 3.8, barDir: 'col', barGapWidthPct: 60, chartColors: [HEX.slate, HEX.sky, HEX.amber], title: 'Budget vs actual vs EAC (AED M)', showLegend: true, legendPos: 'b', valAxisLabelFormatCode: '0.0', objectName: 'chart-budget' });
  s.addChart(pres.charts.LINE, [{ name: 'Posted cost', labels: D.months, values: fin.monthly.map(v => +v.toFixed(2)) }],
    { ...chartBase(), x: 6.75, y: 2.95, w: 2.95, h: 3.8, chartColors: [HEX.teal], lineSize: 2.5, lineDataSymbol: 'none', title: 'Monthly cost (AED M)', showLegend: false, valAxisLabelFormatCode: '0.00', objectName: 'chart-trend' });
  const sp = del.statusByProject;
  s.addChart(pres.charts.BAR, ['Done', 'InProgress', 'ToDo', 'Blocked'].map((kk, i) => ({ name: ['Done', 'In progress', 'To do', 'Blocked'][i], labels: sp.map(r => r.id), values: sp.map(r => r[kk]) })),
    { ...chartBase(), x: 9.85, y: 2.95, w: 2.85, h: 3.8, barDir: 'bar', barGrouping: 'stacked', chartColors: [HEX.teal, HEX.sky, HEX.slate, HEX.coral], title: 'Task status', showLegend: true, legendPos: 'b', objectName: 'chart-tasks' });
  s.addNotes(`Live in the HTML version: three tabs (Delivery, Risk & governance, Financial), project slicers, click-to-filter bars, a 5×5 risk heatmap with drill-down, and an AI insight that rewrites itself for the selection. Core Banking Modernization drives the largest overrun (EAC ${aed(D.byId.CBM.eac)} vs ${aed(D.byId.CBM.budget)}). Financial figures are illustrative.`);
}

/* ===== 8 · Business case ===== */
{
  const s = content('Intelligence & value', '08 · The business case', `Payback in ${sim.payback} months, ${aed(sim.net3y)} net value over three years`);
  const k = [['Annual benefit', aed(sim.annualBenefit), 'at full adoption'], ['One-off investment', aed(sim.invest), '+ ' + aed(sim.annualRun) + '/yr to run'], ['Payback', sim.payback + ' months', 'from go-live'], ['3-year net value', aed(sim.net3y), 'ROI ' + Math.round(sim.roi * 100) + '%']];
  k.forEach(([l, v, sub], i) => {
    const y = 1.75 + i * 1.22;
    card(s, MX, y, 3.6, 1.05, 'bk-' + i, i === 2 ? '183C40' : HEX.card);
    txt(s, l.toUpperCase(), { x: MX + 0.25, y: y + 0.12, w: 3.2, h: 0.25, fontSize: 10, color: HEX.text3 });
    txt(s, v, { x: MX + 0.25, y: y + 0.38, w: 3.2, h: 0.45, fontSize: 24, bold: true, color: i === 2 ? HEX.teal : HEX.text });
    txt(s, sub, { x: MX + 0.25, y: y + 0.78, w: 3.2, h: 0.22, fontSize: 10, color: HEX.text3 });
  });
  const labels = sim.cash.map((_, i) => 'M' + (i + 1));
  s.addChart(pres.charts.LINE, [
    { name: 'Phased rollout', labels, values: sim.cash.map(v => +v.toFixed(2)) },
    { name: 'Big bang', labels, values: simBB.cash.map(v => +v.toFixed(2)) }
  ], { ...chartBase(), x: 4.45, y: 1.75, w: 8.25, h: 3.65, chartColors: [HEX.teal, HEX.violet], lineSize: 2.5, lineDataSymbol: 'none', title: 'Cumulative net cash position (AED M)', showLegend: true, legendPos: 'b', valAxisLabelFormatCode: '0.0', catAxisLabelFrequency: 6, objectName: 'chart-cash' });
  const A = M.ASSUMPTIONS;
  const rows = [
    [{ text: 'Assumption (illustrative)', options: { bold: true, color: HEX.text3 } }, { text: 'Base case', options: { bold: true, color: HEX.text3, align: 'right' } }],
    ['Portfolio · projects onboarded', `AED ${BASE.portfolio}M · ${BASE.projects}`], ['Overrun today → avoided', `${A.baselineOverrun * 100}% → ${BASE.reduction}% of it`],
    ['PMO reporting hours automated', `${A.hoursAutomated * 100}% of ${A.reportHoursPerProject} h/project/week`]
  ];
  s.addTable(rows.map(r => r.map(c => typeof c === 'string' ? { text: c, options: {} } : c)), { x: 4.45, y: 5.55, w: 8.25, colW: [5.0, 3.25], fontSize: 12, color: HEX.text, fill: { color: HEX.card }, border: { type: 'solid', pt: 0.5, color: HEX.line }, rowH: 0.3, margin: [2, 8, 2, 8], objectName: 'assumptions' });
  s.addNotes(`This is where the committee participates in the live version: sliders for portfolio size, projects onboarded and overrun avoided, plus a phased vs big-bang toggle. Big bang reaches payback in month ${simBB.payback} but costs 18% more and carries first-year friction. Everything is computed in models.js — no pre-recorded numbers.`);
}

/* ===== 9 · Automation & AI ===== */
{
  const s = content('Intelligence & value', '09 · Automation & AI approach', 'Machines do the chasing; people make the decisions');
  const cyc = M.weeklyCycle;
  const man = cyc.reduce((a, c) => a + c.manual, 0), au = cyc.reduce((a, c) => a + c.auto, 0);
  s.addChart(pres.charts.BAR, [
    { name: 'Today (manual)', labels: cyc.map(c => c.task), values: cyc.map(c => c.manual) },
    { name: 'With PMO Portal', labels: cyc.map(c => c.task), values: cyc.map(c => c.auto) }
  ], { ...chartBase(), x: MX, y: 1.75, w: 6.3, h: 4.25, barDir: 'bar', barGapWidthPct: 50, chartColors: [HEX.coral, HEX.teal], title: 'Weekly PMO hours per analyst', showLegend: true, legendPos: 'b', showValue: true, dataLabelPosition: 'outEnd', dataLabelFormatCode: '0.0', catAxisOrientation: 'maxMin', objectName: 'chart-hours' });
  txt(s, [{ text: `${man}h → ${au}h`, options: { bold: true, color: HEX.teal, fontSize: 22 } }, { text: '  per analyst, per week (illustrative)', options: { color: HEX.text2, fontSize: 12 } }], { x: MX, y: 6.15, w: 6.3, h: 0.45, valign: 'middle' });
  card(s, 7.15, 1.75, 5.55, 2.25, 'gateway');
  txt(s, 'AI gateway · 12 providers, ordered failover', { x: 7.4, y: 1.9, w: 5.1, h: 0.32, bold: true, fontSize: 14, color: HEX.violet });
  D.aiProviders.forEach((p, i) => {
    const x = 7.4 + (i % 4) * 1.28, y = 2.35 + Math.floor(i / 4) * 0.5;
    s.addShape(pres.shapes.ROUNDED_RECTANGLE, { x, y, w: 1.18, h: 0.38, rectRadius: 0.05, fill: { color: i === 0 ? '183C40' : HEX.card2 }, line: { color: i === 0 ? HEX.teal : HEX.line, width: 0.75 }, objectName: 'provider-' + i });
    txt(s, `${i + 1} ${p}`, { x, y, w: 1.18, h: 0.38, fontSize: 10, align: 'center', valign: 'middle', color: i === 0 ? HEX.text : HEX.text2 });
  });
  const caps = [['Portfolio chat', 'scoped to one, all or compared projects'], ['AI Summary & risk analysis', 'one-click briefs and mitigation ideas'], ['Dashboard insights', 'narrates what changed'], ['AI fill', 'one sentence → a complete form'], ['Guardrails', 'role-scoped data · humans approve · every call audited']];
  caps.forEach(([b, t], i) => {
    const y = 4.15 + i * 0.5;
    s.addShape(pres.shapes.OVAL, { x: 7.2, y: y + 0.1, w: 0.2, h: 0.2, fill: { color: i === 4 ? HEX.teal : HEX.violet }, line: { color: HEX.bg, width: 0 }, objectName: 'cap-dot-' + i });
    txt(s, [{ text: b + '  ', options: { bold: true } }, { text: t, options: { color: HEX.text2 } }], { x: 7.55, y, w: 5.15, h: 0.4, fontSize: 13, valign: 'middle' });
  });
  s.addNotes('Live demo: toggle "With PMO Portal" to collapse the weekly cycle, and switch on "Simulate provider outage" — Groq and OpenRouter fail and Cerebras answers, with no impact on the user. The AI never acts on its own: it drafts, summarises and recommends; people approve.');
}

/* ===== 10 · Roadmap & decision ===== */
pres.addSection({ title: 'Decision' });
{
  const s = content('Decision', '10 · Testing, delivery & the decision', 'Proven in pilot, ready to scale — the decision requested today');
  const ph = [['Pilot', 'Jul – Oct 2026', 'Delivered', true], ['Harden & integrate', 'Nov 2026 – Jan 2027', aed(sim.invest * 0.44)], ['Group rollout', 'Feb – Apr 2027', aed(sim.invest * 0.56)], ['Optimise', 'May – Sep 2027', 'Run budget']];
  s.addShape(pres.shapes.LINE, { x: 1.4, y: 2.05, w: 10.5, h: 0, line: { color: HEX.teal, width: 2.5 }, objectName: 'timeline' });
  ph.forEach(([n, d, c, done], i) => {
    const x = 1.4 + i * 3.5;
    s.addShape(pres.shapes.OVAL, { x: x - 0.25, y: 1.8, w: 0.5, h: 0.5, fill: { color: done ? HEX.teal : HEX.bg }, line: { color: HEX.teal, width: 2 }, objectName: 'phase-' + i });
    txt(s, done ? '✓' : String(i), { x: x - 0.25, y: 1.89, w: 0.5, h: 0.32, align: 'center', bold: true, fontSize: 14, color: done ? HEX.bg : HEX.teal });
    txt(s, [{ text: n, options: { bold: true, fontSize: 15, breakLine: true } }, { text: d, options: { color: HEX.text3, fontSize: 11, breakLine: true } }, { text: c, options: { color: HEX.sky, fontSize: 12 } }], { x: x - 1.4, y: 2.45, w: 2.8, h: 0.9, align: 'center' });
  });
  card(s, MX, 3.6, 5.9, 3.1, 'gates');
  txt(s, 'Quality gates on every release', { x: MX + 0.3, y: 3.75, w: 5.3, h: 0.35, bold: true, fontSize: 14, color: HEX.teal });
  const gates = ['Unit & integration tests on every pull request', 'Permission tests for every pmo:* key and role', 'UAT scripts signed off by the PMO per module', 'Performance budget: lists < 1 s; AI calls time out at 60 s', 'Pen-test, JWT expiry, audit trail on every change', 'Bilingual QA: every screen in Arabic RTL and English'];
  txt(s, gates.map((g, i) => ({ text: g, options: { bullet: true, breakLine: i < gates.length - 1 } })), { x: MX + 0.3, y: 4.2, w: 5.4, h: 2.4, fontSize: 13, color: HEX.text2, paraSpaceAfter: 5 });
  card(s, 6.75, 3.6, 5.95, 3.1, 'ask', '183C40');
  txt(s, 'The decision requested today', { x: 7.05, y: 3.75, w: 5.4, h: 0.35, bold: true, fontSize: 14, color: HEX.teal });
  txt(s, [
    { text: `Approve ${aed(sim.invest)} for Phases 1–2`, options: { bullet: { type: 'number' }, breakLine: true } },
    { text: 'Nominate an executive sponsor & PMO product owner', options: { bullet: { type: 'number' }, breakLine: true } },
    { text: 'Mandate the portal as the single source for portfolio reporting from Q2 2027', options: { bullet: { type: 'number' } } }
  ], { x: 7.05, y: 4.2, w: 5.4, h: 1.6, fontSize: 15, bold: true, color: HEX.text, paraSpaceAfter: 6 });
  txt(s, `Base case: ${aed(sim.annualBenefit)}/yr benefit · payback month ${sim.payback} · 3-yr net ${aed(sim.net3y)}`, { x: 7.05, y: 6.05, w: 5.4, h: 0.45, fontSize: 12, color: HEX.text2 });
  s.addNotes('Close with the ask. The amount on this slide is the base case; in the live version it updates from whatever inputs the committee chose in the simulator. End on: "Decide once. See everything."');
}

(async () => {
  const out = path.join(__dirname, '..', 'PMO_Portal_Management_Presentation.pptx');
  await pres.writeFile({ fileName: out });
  await applyTheme(out, THEME);
  console.log('wrote', out);
})();
