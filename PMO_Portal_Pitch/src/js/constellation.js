/* ==========================================================================
   constellation.js — the persistent background canvas.

   The eight portfolio projects are drawn as glowing nodes that SURVIVE
   every scene change. Each scene asks for a layout ("chaos", "ring",
   "orbit" …) and the nodes glide to their new positions. This is the
   cross-scene continuity device: the audience watches the same eight
   projects go from scattered and alarming to organised and healthy.

   Rendering is a single requestAnimationFrame loop at a fixed 1920×1080
   internal resolution (the stage is scaled with CSS). It pauses when the
   tab is hidden, when motion is paused, and is skipped entirely in
   reduced-motion mode (one static frame is drawn instead).
   ========================================================================== */
PMO.constellation = (function () {
  'use strict';
  const W = 1920, H = 1080;
  let ctx, canvas, layout = 'sphere', mode = 'neutral', t = 0, raf = null;
  const mouse = { x: 0, y: 0, tx: 0, ty: 0 };
  const COLORS = { neutral: [124, 196, 255], alarm: [255, 107, 94], warn: [245, 179, 66], healthy: [45, 212, 191], ai: [139, 124, 255] };

  const nodes = PMO.data.projects.map((p, i) => ({ id: p.id, x: W / 2, y: H / 2, tx: W / 2, ty: H / 2, r: 6 + p.budget * 1.4, phase: i * 0.8, col: [124, 196, 255], tcol: [124, 196, 255], alpha: 1, talpha: 1 }));
  const dust = Array.from({ length: 150 }, (_, i) => ({ a: Math.random() * Math.PI * 2, d: 0.15 + Math.random() * 0.85, z: Math.random(), s: 0.4 + Math.random() * 1.6, sp: 0.0003 + Math.random() * 0.0009, i }));

  // Target positions per layout. Returns [x,y] for node index i.
  const LAYOUTS = {
    sphere: (i, n, tm) => { const a = (i / n) * Math.PI * 2 + tm * 0.00012; return [W * 0.68 + Math.cos(a) * 300, H * 0.5 + Math.sin(a) * 300 * Math.cos(a * 0.5 + 0.6)]; },
    chaos:  (i) => { const pts = [[0.58,0.2],[0.88,0.34],[0.66,0.78],[0.94,0.7],[0.54,0.52],[0.8,0.12],[0.72,0.46],[0.9,0.9]]; return [pts[i][0] * W, pts[i][1] * H]; },
    ring:   (i, n) => { const a = (i / n) * Math.PI * 2 - Math.PI / 2; return [W * 0.5 + Math.cos(a) * 360, H * 0.53 + Math.sin(a) * 360]; },
    orbit:  (i, n, tm) => { const a = (i / n) * Math.PI * 2 + tm * 0.00006; return [W * 0.5 + Math.cos(a) * 470, H * 0.52 + Math.sin(a) * 300]; },
    top:    (i, n) => [W * 0.30 + (W * 0.62 * i) / (n - 1), 34],
    left:   (i, n) => [34, H * 0.2 + (H * 0.62 * i) / (n - 1)],
    line:   (i, n) => [W * 0.1 + (W * 0.8 * i) / (n - 1), H * 0.9],
    core:   () => [820, 600]
  };

  function init(el) {
    canvas = el; canvas.width = W; canvas.height = H; ctx = canvas.getContext('2d');
    document.addEventListener('pointermove', (e) => { mouse.tx = (e.clientX / window.innerWidth - 0.5); mouse.ty = (e.clientY / window.innerHeight - 0.5); });
    document.addEventListener('visibilitychange', () => { document.hidden ? stop() : start(); });
    setLayout('sphere', 'neutral', true);
    start();
  }

  function setLayout(name, colorMode, snap) {
    layout = name; mode = colorMode || mode;
    nodes.forEach((n, i) => {
      const p = PMO.data.projects[i];
      // Health colouring: in "alarm" mode over-budget projects glow red; in "healthy" they settle to teal.
      let c = COLORS[mode] || COLORS.neutral;
      if (mode === 'alarm') c = p.eac > p.budget * 1.02 ? COLORS.alarm : (i % 2 ? COLORS.warn : COLORS.neutral);
      if (mode === 'health') c = p.eac > p.budget * 1.05 ? COLORS.alarm : p.eac > p.budget ? COLORS.warn : COLORS.healthy;
      // Side/edge layouts are a quiet reminder of the portfolio, so they are dimmed.
      n.tcol = c.slice(); n.talpha = name === 'core' ? 0 : (name === 'top' || name === 'left' || name === 'line') ? 0.35 : 1;
      if (snap) { const [x, y] = LAYOUTS[name](i, nodes.length, t); n.x = n.tx = x; n.y = n.ty = y; n.col = c.slice(); }
    });
    if (PMO.fx.state.reduced) drawFrame(performance.now(), true);
  }

  function setVisible(level) { canvas.style.opacity = level; }

  function start() { if (!raf && !PMO.fx.state.reduced) raf = requestAnimationFrame(loop); }
  function stop() { cancelAnimationFrame(raf); raf = null; }

  function loop(now) {
    raf = requestAnimationFrame(loop);
    if (PMO.fx.state.paused) return;
    drawFrame(now, false);
  }

  function drawFrame(now, snap) {
    t = now;
    mouse.x += (mouse.tx - mouse.x) * 0.04; mouse.y += (mouse.ty - mouse.y) * 0.04;
    const px = mouse.x * 40, py = mouse.y * 30;
    ctx.clearRect(0, 0, W, H);

    // Ambient dust with parallax depth (z) — gives the stage its sense of depth.
    for (const d of dust) {
      d.a += d.sp;
      const x = W / 2 + Math.cos(d.a) * d.d * W * 0.62 + px * (0.3 + d.z);
      const y = H / 2 + Math.sin(d.a * 1.3) * d.d * H * 0.55 + py * (0.3 + d.z);
      ctx.fillStyle = `rgba(160,190,230,${0.08 + d.z * 0.22})`;
      ctx.fillRect(x, y, d.s, d.s);
    }

    // Ease nodes toward their layout targets.
    const f = snap ? 1 : 0.055;
    nodes.forEach((n, i) => {
      const [x, y] = LAYOUTS[layout](i, nodes.length, now);
      n.tx = x; n.ty = y;
      n.x += (n.tx - n.x) * f; n.y += (n.ty - n.y) * f;
      for (let k = 0; k < 3; k++) n.col[k] += (n.tcol[k] - n.col[k]) * f;
      n.alpha += (n.talpha - n.alpha) * f;
    });

    // Links between consecutive projects (the "portfolio" fabric).
    ctx.lineWidth = 1;
    for (let i = 0; i < nodes.length; i++) for (let j = i + 1; j < nodes.length; j++) {
      const a = nodes[i], b = nodes[j];
      const dist = Math.hypot(a.x - b.x, a.y - b.y);
      if (dist > 520) continue;
      const al = (1 - dist / 520) * 0.22 * Math.min(a.alpha, b.alpha);
      ctx.strokeStyle = `rgba(${a.col.map(Math.round).join(',')},${al})`;
      ctx.beginPath(); ctx.moveTo(a.x + px * 0.6, a.y + py * 0.6); ctx.lineTo(b.x + px * 0.6, b.y + py * 0.6); ctx.stroke();
    }

    // Nodes: soft glow + core + code label.
    nodes.forEach(n => {
      if (n.alpha < 0.02) return;
      const pulse = 1 + Math.sin(now * 0.002 + n.phase) * 0.12;
      const x = n.x + px * 0.6, y = n.y + py * 0.6, c = n.col.map(Math.round).join(',');
      const g = ctx.createRadialGradient(x, y, 0, x, y, n.r * 5 * pulse);
      g.addColorStop(0, `rgba(${c},${0.45 * n.alpha})`); g.addColorStop(1, `rgba(${c},0)`);
      ctx.fillStyle = g; ctx.beginPath(); ctx.arc(x, y, n.r * 5 * pulse, 0, Math.PI * 2); ctx.fill();
      ctx.fillStyle = `rgba(${c},${n.alpha})`; ctx.beginPath(); ctx.arc(x, y, n.r * 0.55, 0, Math.PI * 2); ctx.fill();
      ctx.font = '600 15px "JetBrains Mono", monospace';
      ctx.fillStyle = `rgba(220,232,250,${0.7 * n.alpha})`;
      ctx.fillText(n.id, x + n.r + 6, y - n.r - 4);
    });
  }

  return { init, setLayout, setVisible, start, stop, drawFrame };
})();
