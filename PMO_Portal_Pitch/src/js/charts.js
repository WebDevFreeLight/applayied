/* ==========================================================================
   charts.js — a tiny SVG chart toolkit (no external chart library).

   Why hand-rolled: it keeps the deliverable dependency-free under file://
   and lets every visual animate between states. Elements are keyed and
   REUSED across renders, and geometry is written as CSS properties
   (height, y, d: path()) so the CSS transitions in styles.css tween them
   when a slicer or simulator input changes — instead of the chart blinking.
   ========================================================================== */
PMO.charts = (function () {
  'use strict';
  const NS = 'http://www.w3.org/2000/svg';

  function el(tag, attrs, parent) {
    const n = document.createElementNS(NS, tag);
    for (const k in attrs) n.setAttribute(k, attrs[k]);
    if (parent) parent.appendChild(n);
    return n;
  }

  // One SVG per container, created once. Returns { svg, keyed(key, tag, layer) }.
  function surface(container, w, h) {
    if (!container._s) {
      const svg = el('svg', { viewBox: `0 0 ${w} ${h}`, class: 'chart', preserveAspectRatio: 'none' });
      container.innerHTML = '';
      container.appendChild(svg);
      const layers = {};
      container._s = {
        svg, pool: new Map(),
        layer(name) { return layers[name] || (layers[name] = el('g', { class: 'layer-' + name }, svg)); },
        keyed(key, tag, layerName, attrs) {
          let n = this.pool.get(key);
          if (!n) { n = el(tag, attrs || {}, this.layer(layerName || 'main')); this.pool.set(key, n); n._new = true; }
          else n._new = false;
          n._seen = true;
          return n;
        },
        begin() { this.pool.forEach(n => { n._seen = false; }); },
        end() { this.pool.forEach((n, k) => { if (!n._seen) { n.remove(); this.pool.delete(k); } }); }
      };
    }
    return container._s;
  }

  const fmt = {
    m: (v) => 'AED ' + v.toFixed(v >= 10 ? 1 : 2) + 'M',
    pct: (v) => Math.round(v * 100) + '%',
    k: (v) => v >= 1e6 ? (v / 1e6).toFixed(2) + 'M' : v >= 1e3 ? Math.round(v / 1e3) + 'K' : String(Math.round(v))
  };

  function text(s, key, x, y, str, cls, layer) {
    const t = s.keyed(key, 'text', layer || 'labels');
    t.setAttribute('x', x); t.setAttribute('y', y); t.setAttribute('class', cls || 'axis-label');
    t.textContent = str;
    return t;
  }

  /* Grouped bars: series = [{name, cls}], rows = [{label, values:[...]}] */
  function groupedBars(container, rows, series, opts = {}) {
    const W = opts.w || 640, H = opts.h || 300, pad = { l: 48, r: 12, t: 16, b: 34 };
    const s = surface(container, W, H); s.begin();
    const max = opts.max || Math.max(0.0001, ...rows.flatMap(r => r.values)) * 1.12;
    const iw = W - pad.l - pad.r, ih = H - pad.t - pad.b;
    const gw = iw / Math.max(1, rows.length), bw = Math.min(26, (gw * 0.72) / series.length);
    for (let i = 0; i <= 4; i++) {
      const y = pad.t + ih - (ih * i) / 4;
      const g = s.keyed('grid' + i, 'line', 'grid'); g.setAttribute('class', 'gridline');
      g.setAttribute('x1', pad.l); g.setAttribute('x2', W - pad.r); g.setAttribute('y1', y); g.setAttribute('y2', y);
      text(s, 'gl' + i, pad.l - 8, y + 4, opts.tick ? opts.tick((max * i) / 4) : ((max * i) / 4).toFixed(1), 'axis-label end');
    }
    rows.forEach((r, i) => {
      const gx = pad.l + gw * i + (gw - bw * series.length) / 2;
      r.values.forEach((v, j) => {
        const b = s.keyed(`b-${r.label}-${j}`, 'rect', 'bars');
        const h = Math.max(1, (v / max) * ih);
        b.setAttribute('class', 'bar ' + series[j].cls + (r.dim ? ' dim' : ''));
        b.setAttribute('rx', 3); b.setAttribute('x', gx + j * bw); b.setAttribute('width', bw - 3);
        if (b._new) { b.style.y = (pad.t + ih) + 'px'; b.style.height = '0px'; b.getBoundingClientRect(); }
        b.style.y = (pad.t + ih - h) + 'px'; b.style.height = h + 'px';
        b.dataset.tip = `${r.label} · ${series[j].name}: ${opts.tick ? opts.tick(v) : v}`;
        if (opts.onClick) { b.onclick = () => opts.onClick(r.label); b.style.cursor = 'pointer'; }
      });
      text(s, 'x-' + r.label, pad.l + gw * i + gw / 2, H - 10, r.label, 'axis-label mid' + (r.dim ? ' dim' : ''));
    });
    s.end();
  }

  /* Multi-line chart. lines = [{key, values, cls}], labels = x labels */
  function lines(container, linesData, labels, opts = {}) {
    const W = opts.w || 640, H = opts.h || 280, pad = { l: 44, r: 16, t: 14, b: 30 };
    const s = surface(container, W, H); s.begin();
    const all = linesData.flatMap(l => l.values);
    const max = opts.max !== undefined ? opts.max : Math.max(...all) * 1.1;
    const min = opts.min !== undefined ? opts.min : Math.min(0, ...all);
    const span = (max - min) || 1;
    const iw = W - pad.l - pad.r, ih = H - pad.t - pad.b;
    const X = (i) => pad.l + (iw * i) / Math.max(1, labels.length - 1);
    const Y = (v) => pad.t + ih - ((v - min) / span) * ih;
    for (let i = 0; i <= 4; i++) {
      const y = pad.t + ih - (ih * i) / 4, v = min + (span * i) / 4;
      const g = s.keyed('grid' + i, 'line', 'grid'); g.setAttribute('class', 'gridline');
      g.setAttribute('x1', pad.l); g.setAttribute('x2', W - pad.r); g.setAttribute('y1', y); g.setAttribute('y2', y);
      text(s, 'gl' + i, pad.l - 8, y + 4, opts.tick ? opts.tick(v) : Math.round(v), 'axis-label end');
    }
    labels.forEach((lb, i) => text(s, 'x' + i, X(i), H - 8, lb, 'axis-label mid'));
    linesData.forEach(l => {
      const d = l.values.map((v, i) => (i ? 'L' : 'M') + X(i).toFixed(1) + ' ' + Y(v).toFixed(1)).join(' ');
      if (opts.area) {
        const a = s.keyed('a-' + l.key, 'path', 'areas');
        const base = Y(Math.max(min, 0)).toFixed(1);
        const ad = d + ` L${X(l.values.length - 1).toFixed(1)} ${base} L${X(0).toFixed(1)} ${base} Z`;
        a.setAttribute('class', 'area ' + (l.cls || ''));
        a.style.d = `path("${ad}")`;
      }
      const p = s.keyed('l-' + l.key, 'path', 'lines');
      p.setAttribute('class', 'line ' + (l.cls || '') + (l.dim ? ' dim' : ''));
      if (p._new) { p.setAttribute('d', d); PMO.fx.drawPath(p, 900); }
      p.style.d = `path("${d}")`;
      const last = s.keyed('d-' + l.key, 'circle', 'dots');
      last.setAttribute('r', 4); last.setAttribute('class', 'dot ' + (l.cls || '') + (l.dim ? ' dim' : ''));
      last.style.cx = X(l.values.length - 1) + 'px'; last.style.cy = Y(l.values[l.values.length - 1]) + 'px';
      if (opts.endLabels) {
        const t = text(s, 'el-' + l.key, X(l.values.length - 1) - 8, Y(l.values[l.values.length - 1]) - 9, l.key, 'axis-label end strong' + (l.dim ? ' dim' : ''));
        t.style.transition = 'opacity .4s';
      }
    });
    if (opts.zero !== undefined) {
      const z = s.keyed('zero', 'line', 'grid'); z.setAttribute('class', 'zeroline');
      z.setAttribute('x1', pad.l); z.setAttribute('x2', W - pad.r); z.style.transform = '';
      z.setAttribute('y1', Y(opts.zero)); z.setAttribute('y2', Y(opts.zero));
    }
    s.end();
    return { X, Y, pad, W, H, ih };
  }

  /* Donut. parts = [{label, value, cls}] */
  function donut(container, parts, opts = {}) {
    const W = 260, H = 260, cx = 130, cy = 130, R = 112, r = 74;
    const s = surface(container, W, H); s.begin();
    const total = parts.reduce((a, p) => a + p.value, 0) || 1;
    let a0 = -Math.PI / 2;
    parts.forEach(p => {
      const a1 = a0 + (p.value / total) * Math.PI * 2 - 0.012;
      const large = a1 - a0 > Math.PI ? 1 : 0;
      const pt = (rad, a) => `${(cx + rad * Math.cos(a)).toFixed(2)} ${(cy + rad * Math.sin(a)).toFixed(2)}`;
      const d = `M${pt(R, a0)} A${R} ${R} 0 ${large} 1 ${pt(R, a1)} L${pt(r, a1)} A${r} ${r} 0 ${large} 0 ${pt(r, a0)} Z`;
      const seg = s.keyed('s-' + p.label, 'path', 'segs');
      seg.setAttribute('class', 'seg ' + p.cls);
      seg.setAttribute('d', d);
      seg.dataset.tip = `${p.label}: ${opts.fmt ? opts.fmt(p.value) : p.value} (${Math.round((p.value / total) * 100)}%)`;
      a0 = a1 + 0.012;
    });
    const t1 = text(s, 'c1', cx, cy - 2, opts.center || '', 'donut-center');
    const t2 = text(s, 'c2', cx, cy + 22, opts.sub || '', 'donut-sub');
    s.end();
  }

  /* 5×5 risk heatmap. grid[prob-1][imp-1] = items. onCell(prob, imp, items) */
  function heatmap(container, grid, opts = {}) {
    const W = 420, H = 360, pad = { l: 46, b: 40, t: 8, r: 8 };
    const s = surface(container, W, H); s.begin();
    const cw = (W - pad.l - pad.r) / 5, ch = (H - pad.t - pad.b) / 5;
    for (let p = 1; p <= 5; p++) for (let i = 1; i <= 5; i++) {
      const score = p * i, items = grid[p - 1][i - 1];
      const x = pad.l + (i - 1) * cw, y = pad.t + (5 - p) * ch;
      const c = s.keyed(`c${p}${i}`, 'rect', 'cells');
      c.setAttribute('x', x + 2); c.setAttribute('y', y + 2); c.setAttribute('width', cw - 4); c.setAttribute('height', ch - 4); c.setAttribute('rx', 6);
      const band = score >= 15 ? 'hot' : score >= 8 ? 'warm' : 'cool';
      c.setAttribute('class', `cell ${band} ${items.length ? 'has' : 'empty'} ${opts.selected === `${p}${i}` ? 'sel' : ''}`);
      c.onclick = () => opts.onCell && opts.onCell(p, i, items);
      c.dataset.tip = `P${p} × I${i} = ${score} · ${items.length} open`;
      const n = text(s, `n${p}${i}`, x + cw / 2, y + ch / 2 + 7, items.length ? String(items.length) : '', 'cell-count');
      n.style.pointerEvents = 'none';
    }
    for (let k = 1; k <= 5; k++) {
      text(s, 'xi' + k, pad.l + (k - 0.5) * cw, H - 18, String(k), 'axis-label mid');
      text(s, 'yp' + k, pad.l - 14, pad.t + (5 - k + 0.5) * ch + 5, String(k), 'axis-label mid');
    }
    text(s, 'xt', pad.l + (W - pad.l) / 2, H - 1, 'Impact →', 'axis-label mid strong');
    const yt = text(s, 'yt', 12, pad.t + (H - pad.b) / 2, 'Probability →', 'axis-label mid strong');
    yt.setAttribute('transform', `rotate(-90 12 ${pad.t + (H - pad.b) / 2})`);
    s.end();
  }

  /* Horizontal stacked bars. rows = [{label, parts:{k:v}}], keys = [{k, cls, name}] */
  function stacked(container, rows, keys, opts = {}) {
    const W = opts.w || 600, H = opts.h || 300, pad = { l: 56, r: 16, t: 6, b: 6 };
    const s = surface(container, W, H); s.begin();
    const max = Math.max(1, ...rows.map(r => keys.reduce((a, k) => a + (r.parts[k.k] || 0), 0)));
    const rh = (H - pad.t - pad.b) / Math.max(1, rows.length);
    rows.forEach((r, i) => {
      let x = pad.l; const y = pad.t + i * rh + rh * 0.18, h = rh * 0.64;
      text(s, 'l' + r.label, pad.l - 10, y + h / 2 + 5, r.label, 'axis-label end' + (r.dim ? ' dim' : ''));
      keys.forEach(k => {
        const v = r.parts[k.k] || 0, w = ((W - pad.l - pad.r) * v) / max;
        const b = s.keyed(`s-${r.label}-${k.k}`, 'rect', 'bars');
        b.setAttribute('class', 'bar ' + k.cls + (r.dim ? ' dim' : '')); b.setAttribute('y', y); b.setAttribute('height', h); b.setAttribute('rx', 2);
        if (b._new) { b.style.x = pad.l + 'px'; b.style.width = '0px'; b.getBoundingClientRect(); }
        b.style.x = x + 'px'; b.style.width = Math.max(0, w - 2) + 'px';
        b.dataset.tip = `${r.label} · ${k.name}: ${v}`;
        if (opts.onClick) { b.onclick = () => opts.onClick(r.label); b.style.cursor = 'pointer'; }
        x += w;
      });
    });
    s.end();
  }

  // Shared floating tooltip for any element with data-tip inside the stage.
  function initTooltips(root) {
    const tip = document.createElement('div'); tip.className = 'tooltip'; root.appendChild(tip);
    root.addEventListener('pointermove', (e) => {
      const t = e.target.closest && e.target.closest('[data-tip]');
      if (!t) { tip.classList.remove('on'); return; }
      tip.textContent = t.dataset.tip;
      const r = root.getBoundingClientRect(), k = r.width / 1920;
      tip.style.transform = `translate(${(e.clientX - r.left) / k + 14}px, ${(e.clientY - r.top) / k + 14}px)`;
      tip.classList.add('on');
    });
    root.addEventListener('pointerleave', () => tip.classList.remove('on'));
  }

  return { groupedBars, lines, donut, heatmap, stacked, initTooltips, fmt };
})();
