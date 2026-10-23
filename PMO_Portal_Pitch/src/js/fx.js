/* ==========================================================================
   fx.js — reusable motion primitives built on the Web Animations API.

   Every animation, timer and listener a scene starts is registered in a
   "scope". When the controller leaves a scene it calls scope.dispose(),
   which cancels animations, clears timers and removes listeners. This is
   what prevents runaway loops and stale state when presenters jump around.
   ========================================================================== */
PMO.fx = (function () {
  'use strict';
  const state = { reduced: window.matchMedia('(prefers-reduced-motion: reduce)').matches, paused: false };
  const EASE = 'cubic-bezier(.2,.75,.15,1)';

  function createScope() {
    const anims = new Set(), timers = new Set(), offs = new Set(), rafs = new Set();
    return {
      anim(a) { anims.add(a); a.finished.then(() => anims.delete(a), () => anims.delete(a)); return a; },
      timeout(fn, ms) { const id = setTimeout(() => { timers.delete(id); fn(); }, state.reduced ? Math.min(ms, 60) : ms); timers.add(id); return id; },
      interval(fn, ms) { const id = setInterval(fn, ms); timers.add(id); return id; },
      raf(fn) {
        let id; const loop = (t) => { if (!state.paused) fn(t); id = requestAnimationFrame(loop); rafs.add(id); };
        id = requestAnimationFrame(loop); rafs.add(id);
      },
      on(target, ev, fn, opt) { target.addEventListener(ev, fn, opt); offs.add(() => target.removeEventListener(ev, fn, opt)); },
      dispose() {
        anims.forEach(a => { try { a.cancel(); } catch (e) { /* already gone */ } });
        timers.forEach(id => { clearTimeout(id); clearInterval(id); });
        rafs.forEach(id => cancelAnimationFrame(id));
        offs.forEach(off => off());
        anims.clear(); timers.clear(); offs.clear(); rafs.clear();
      }
    };
  }

  function animate(scope, node, frames, opts) {
    if (!node) return null;
    const o = Object.assign({ duration: 700, easing: EASE, fill: 'both' }, opts);
    if (state.reduced) { o.duration = Math.min(o.duration, 1); o.delay = 0; }
    const a = node.animate(frames, o);
    return scope ? scope.anim(a) : a;
  }

  /* Kinetic typography: split a heading into words (and optionally chars)
     and reveal them with a stagger. Words keep their spaces so the line
     wraps naturally. Accent words are marked with *asterisks* in the HTML. */
  function splitWords(node) {
    if (node._split) return node._split;
    const raw = node.dataset.text || node.textContent;
    node.dataset.text = raw;
    node.innerHTML = '';
    const words = [];
    raw.trim().split(/\s+/).forEach((w, i) => {
      const accent = /^\*.*\*[.,!?]?$/.test(w);
      const clean = w.replace(/\*/g, '');
      const mask = document.createElement('span'); mask.className = 'w-mask';
      const span = document.createElement('span'); span.className = 'w' + (accent ? ' accent' : '');
      span.textContent = clean;
      mask.appendChild(span); node.appendChild(mask);
      node.appendChild(document.createTextNode(' '));
      words.push(span);
    });
    node._split = words;
    return words;
  }

  function revealWords(scope, node, { delay = 0, stagger = 70, duration = 900, from = 'up' } = {}) {
    const words = splitWords(node);
    const f0 = from === 'up' ? 'translateY(105%) rotate(4deg)' : 'translateY(-105%)';
    words.forEach((w, i) => animate(scope, w,
      [{ transform: f0, opacity: 0 }, { transform: 'translateY(0) rotate(0)', opacity: 1 }],
      { duration, delay: delay + i * stagger }));
    return delay + words.length * stagger + duration;
  }

  // Generic "rise in" for a list of nodes.
  function rise(scope, nodes, { delay = 0, stagger = 80, y = 28, duration = 700, scale = 1 } = {}) {
    [...nodes].forEach((n, i) => animate(scope, n,
      [{ opacity: 0, transform: `translateY(${y}px) scale(${scale})` }, { opacity: 1, transform: 'none' }],
      { duration, delay: delay + i * stagger }));
  }

  // Number counter. format(v) returns the display string.
  function countUp(scope, node, to, { from = 0, duration = 1400, delay = 0, format = (v) => Math.round(v) } = {}) {
    if (state.reduced) { node.textContent = format(to); return; }
    const t0 = performance.now() + delay;
    const step = (t) => {
      const k = Math.min(1, Math.max(0, (t - t0) / duration));
      const e = 1 - Math.pow(1 - k, 4);
      node.textContent = format(from + (to - from) * e);
      if (k < 1) node._cu = requestAnimationFrame(step);
    };
    cancelAnimationFrame(node._cu);
    node._cu = requestAnimationFrame(step);
    if (scope) scope.on(window, 'pmo:dispose', () => cancelAnimationFrame(node._cu), { once: true });
  }

  // SVG path drawing via stroke-dash animation.
  function drawPath(path, duration = 1200, delay = 0, scope) {
    let len = 0;
    try { len = path.getTotalLength(); } catch (e) { return; }
    if (!len) return;
    path.style.strokeDasharray = len;
    const a = animate(scope, path, [{ strokeDashoffset: len }, { strokeDashoffset: 0 }], { duration, delay, easing: 'cubic-bezier(.6,.05,.2,1)' });
    if (a) a.finished.then(() => { path.style.strokeDasharray = ''; }, () => { path.style.strokeDasharray = ''; });
  }

  // Move a dot along a path (used for "data packet" animations).
  function travel(scope, dot, path, { duration = 1400, delay = 0, iterations = 1 } = {}) {
    const len = path.getTotalLength();
    const frames = [];
    for (let i = 0; i <= 24; i++) {
      const p = path.getPointAtLength((len * i) / 24);
      frames.push({ transform: `translate(${p.x}px, ${p.y}px)`, opacity: i === 0 || i === 24 ? 0 : 1 });
    }
    return animate(scope, dot, frames, { duration, delay, iterations, easing: 'linear', fill: 'none' });
  }

  /* Responsive layout mode.
     "stage": the fixed 1920×1080 stage, uniformly scaled (desktop, large tablets).
     "flow":  phones, small tablets and very short windows — the stage fills the
              window at 1:1, scenes reflow and scroll vertically (styles.css, .flow).
     The scaled stage is kept wherever it renders text at a readable size, so
     1366×768 and 1920×1080 (including browser chrome) are untouched. */
  function isFlow() { return window.innerWidth < 1100 || window.innerHeight < 520; }

  function setReduced(v) { state.reduced = v; document.documentElement.classList.toggle('reduced', v); }
  function setPaused(v) {
    state.paused = v;
    document.documentElement.classList.toggle('paused', v);
    document.getAnimations().forEach(a => { if (a.effect && a.effect.getTiming().iterations === Infinity) { v ? a.pause() : a.play(); } });
  }

  return { state, EASE, createScope, animate, splitWords, revealWords, rise, countUp, drawPath, travel, setReduced, setPaused, isFlow };
})();
