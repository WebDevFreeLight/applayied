/* ==========================================================================
   controller.js — the scene controller.

   Responsibilities
     • scale the fixed 1920×1080 stage to any window (letterboxed)
     • own the current scene index and run enter/leave lifecycles
     • choreograph transitions (generic depth push + named set pieces)
     • presenter controls: keys, HUD, progress, full screen, pause,
       reduced motion, help, and resume-from-last-scene

   Lifecycle of a scene change (go(i)):
     1. If a transition is still running, finish it instantly (no stuck states).
     2. Dispose the outgoing scene's scope → its timers/animations/listeners stop.
     3. Move the background constellation to the new scene's layout.
     4. Play the outgoing + incoming choreography (controller-owned animations).
     5. Call scene.enter(ctx) with a fresh scope.
   ========================================================================== */
PMO.controller = (function () {
  'use strict';
  const fx = PMO.fx, scenes = PMO.scenes;
  const $ = (s) => document.querySelector(s);
  const stage = $('#stage');
  let index = -1, scope = null, transition = null, idleTimer = null;

  const store = {
    get(k) { try { return localStorage.getItem('pmo.' + k); } catch (e) { return null; } },
    set(k, v) { try { localStorage.setItem('pmo.' + k, v); } catch (e) { /* blocked */ } }
  };
  const savedIndex = parseInt(store.get('scene') || '0', 10) || 0;

  /* ---------- Stage scaling / responsive layout ----------
     Desktop: the 1920×1080 stage is uniformly scaled (letterboxed), exactly as before.
     Phones & small tablets ("flow", see fx.isFlow): the stage fills the window at 1:1,
     scenes reflow via styles.css and scroll vertically inside themselves. */
  let lastFlow = null, lastW = 0, relayoutTimer = null;
  function fit() {
    const flow = fx.isFlow();
    document.documentElement.classList.toggle('flow', flow);
    if (flow) { stage.style.transform = ''; return; }
    const k = Math.min(window.innerWidth / 1920, window.innerHeight / 1080);
    stage.style.transform = `scale(${k})`;
  }
  // Resize / orientation change: rescale at once; when the flow layout changes width
  // (or the mode flips), redraw the background and quietly re-render the current
  // scene so JS-built SVGs and charts are rebuilt for the new size.
  function onResize() {
    fit();
    clearTimeout(relayoutTimer);
    relayoutTimer = setTimeout(() => {
      const flow = fx.isFlow(), w = window.innerWidth;
      const modeChanged = flow !== lastFlow, widthChanged = flow && w !== lastW;
      lastFlow = flow; lastW = w;
      if (flow || modeChanged) PMO.constellation.resize();
      if ((modeChanged || widthChanged) && index >= 0) go(index, { force: true, quiet: true });
    }, 180);
  }

  /* ---------- Transitions ----------
     Each returns a Promise that resolves when the incoming scene is in place.
     All animations here are tracked in `transition.anims` so they can be
     finished instantly if the presenter navigates again mid-transition. */
  const T = {
    // Generic: outgoing recedes into depth with blur; incoming rises from below/above.
    push(out, inc, dir) {
      const a = [];
      if (out) a.push(out.animate([{ opacity: 1, transform: 'none', filter: 'blur(0)' }, { opacity: 0, transform: `scale(.94) translateY(${dir * -40}px)`, filter: 'blur(8px)' }], { duration: 650, easing: fx.EASE, fill: 'forwards' }));
      a.push(inc.animate([{ opacity: 0, transform: `scale(1.04) translateY(${dir * 60}px)` }, { opacity: 1, transform: 'none' }], { duration: 800, delay: out ? 250 : 0, easing: fx.EASE, fill: 'backwards' }));
      return a;
    },
    // 02 → 03: every chaotic artefact is pulled into the PMO hub, then the hub scene blooms.
    converge(out, inc) {
      const a = [];
      if (out) {
        out.querySelectorAll('.artefact').forEach((n, i) => {
          const flow = fx.isFlow();
          const r = n.getBoundingClientRect(), s = stage.getBoundingClientRect(), k = flow ? 1 : s.width / 1920;
          const hx = flow ? window.innerWidth / 2 : 820, hy = flow ? window.innerHeight * 0.45 : 600;
          const dx = hx - ((r.left - s.left) / k + r.width / k / 2), dy = hy - ((r.top - s.top) / k + r.height / k / 2);
          a.push(n.animate([{ transform: getComputedStyle(n).transform, opacity: 1 }, { transform: `translate(${dx}px, ${dy}px) scale(.1) rotate(0)`, opacity: 0 }], { duration: 800, delay: i * 50, easing: 'cubic-bezier(.6,0,.4,1)', fill: 'forwards' }));
        });
        out.querySelectorAll('.scene-head, .problem-grid, .problem-foot').forEach(n => a.push(n.animate([{ opacity: 1 }, { opacity: 0, transform: 'translateX(-60px)' }], { duration: 500, easing: fx.EASE, fill: 'forwards' })));
      }
      a.push(inc.animate([{ opacity: 0 }, { opacity: 1 }], { duration: 400, delay: out ? 700 : 0, fill: 'backwards' }));
      const hub = inc.querySelector('.hub');
      if (hub) a.push(hub.animate([{ transform: 'scale(0)', opacity: 0 }, { transform: 'scale(1.15)', opacity: 1, offset: .7 }, { transform: 'scale(1)', opacity: 1 }], { duration: 900, delay: out ? 750 : 0, easing: fx.EASE, fill: 'backwards' }));
      if (hub) hub.style.transformOrigin = '820px 600px', hub.style.transformBox = 'view-box';
      return a;
    },
    // 03 → 04: camera flies INTO the hub; it becomes the solution's core disc.
    zoom(out, inc) {
      const a = [];
      if (out) {
        const map = out.querySelector('.value-map');
        const vb = map.viewBox && map.viewBox.baseVal;
        map.style.transformOrigin = fx.isFlow() && vb && vb.width ? `${(820 - vb.x) / vb.width * 100}% ${(600 - vb.y) / vb.height * 100}%` : `${820 / 19.2}% ${600 / 10.8}%`;
        a.push(map.animate([{ transform: 'scale(1)', opacity: 1 }, { transform: 'scale(4)', opacity: 0 }], { duration: 900, easing: 'cubic-bezier(.7,0,.3,1)', fill: 'forwards' }));
        out.querySelectorAll('.scene-head, .persona-panel, .value-hint').forEach(n => a.push(n.animate([{ opacity: 1 }, { opacity: 0 }], { duration: 300, fill: 'forwards' })));
      }
      a.push(inc.animate([{ opacity: 0 }, { opacity: 1 }], { duration: 500, delay: out ? 500 : 0, fill: 'backwards' }));
      const core = inc.querySelector('.core-disc');
      if (core) a.push(core.animate([{ transform: 'scale(3.2)', opacity: 0 }, { transform: 'scale(1)', opacity: 1 }], { duration: 1000, delay: out ? 500 : 0, easing: fx.EASE, fill: 'backwards' }));
      return a;
    },
    // Any → dashboard: a circular iris opens from the centre like a screen powering on.
    wipe(out, inc) {
      const a = [];
      if (out) a.push(out.animate([{ opacity: 1, filter: 'brightness(1)' }, { opacity: 0, filter: 'brightness(.3)' }], { duration: 600, fill: 'forwards' }));
      a.push(inc.animate([{ clipPath: 'circle(0% at 50% 55%)' }, { clipPath: 'circle(120% at 50% 55%)' }], { duration: 1000, delay: out ? 150 : 0, easing: 'cubic-bezier(.7,0,.2,1)', fill: 'backwards' }));
      return a;
    }
  };

  function finishTransition() {
    if (!transition) return;
    transition.anims.forEach(a => { try { a.finish(); } catch (e) { /* ignore */ } });
    transition.cleanup();
    transition = null;
  }

  /* ---------- Navigation ---------- */
  function go(i, opts = {}) {
    i = Math.max(0, Math.min(scenes.length - 1, i));
    if (i === index && !opts.force) return;
    finishTransition();
    const prev = index, dir = i >= prev ? 1 : -1;
    const outScene = scenes[prev], inScene = scenes[i];
    const out = outScene ? document.getElementById(outScene.id) : null;
    const inc = document.getElementById(inScene.id);

    if (scope) { scope.dispose(); window.dispatchEvent(new Event('pmo:dispose')); }
    index = i; store.set('scene', String(i));
    if (i > 0) store.set('resume', String(i));
    PMO.constellation.setLayout(inScene.layout[0], inScene.layout[1]);
    PMO.constellation.setVisible(i === 6 ? 0.25 : i === 7 ? 0.45 : 1);

    if (out) { out.classList.remove('active'); out.classList.add('leaving'); }
    inc.classList.add('active');
    // Named set pieces only play forward; going back always uses the reversible push.
    const kind = dir > 0 && inScene.transIn && T[inScene.transIn] ? inScene.transIn : 'push';
    const anims = fx.state.reduced || opts.quiet ? [] : T[kind](out, inc, dir);
    const cleanup = () => {
      if (out) { out.classList.remove('leaving'); out.getAnimations({ subtree: true }).forEach(a => a.cancel()); }
      anims.forEach(a => { try { a.cancel(); } catch (e) { /* ignore */ } });
      inc.style.clipPath = '';
    };
    transition = { anims, cleanup };
    Promise.all(anims.map(a => a.finished.catch(() => null))).then(() => { if (transition && transition.anims === anims) { cleanup(); transition = null; } });

    scope = fx.createScope();
    inScene.enter({ el: inc, scope, dir, from: prev });
    updateHud();
    if (location.hash !== '#' + (i + 1)) history.replaceState(null, '', '#' + (i + 1));
  }

  /* ---------- HUD ---------- */
  function buildHud() {
    const prog = $('#hud-progress');
    scenes.forEach((s, i) => {
      const b = document.createElement('button');
      const title = document.getElementById(s.id).dataset.title;
      b.title = `${i + 1}. ${title}`; b.setAttribute('aria-label', `Go to scene ${i + 1}: ${title}`);
      b.addEventListener('click', () => go(i));
      prog.appendChild(b);
    });
  }
  const seen = new Set();
  function updateHud() {
    seen.add(index);
    [...$('#hud-progress').children].forEach((b, i) => { b.classList.toggle('cur', i === index); b.classList.toggle('seen', seen.has(i)); });
    const title = document.getElementById(scenes[index].id).dataset.title;
    $('#hud-label').innerHTML = `<b>${String(index + 1).padStart(2, '0')}</b> / ${scenes.length}<span class="hud-title"> · ${title}</span>`;
    document.querySelector('[data-action="prev"].hud-btn').disabled = index === 0;
    document.querySelector('[data-action="next"].hud-btn').disabled = index === scenes.length - 1;
  }
  function poke() {
    $('#hud').classList.remove('idle');
    clearTimeout(idleTimer);
    idleTimer = setTimeout(() => $('#hud').classList.add('idle'), 3500);
  }

  /* ---------- Actions (buttons and keys share one table) ---------- */
  const actions = {
    next: () => go(index + 1),
    prev: () => go(index - 1),
    home: () => go(0),
    end: () => go(scenes.length - 1),
    fullscreen: () => {
      try {
        if (document.fullscreenElement) document.exitFullscreen();
        else document.documentElement.requestFullscreen().catch(() => { /* not permitted: keep windowed */ });
      } catch (e) { /* API unavailable */ }
    },
    pause: () => { fx.setPaused(!fx.state.paused); $('#btn-pause').classList.toggle('on', fx.state.paused); $('#btn-pause').textContent = fx.state.paused ? '▶' : '❚❚'; },
    reduce: () => {
      fx.setReduced(!fx.state.reduced); $('#btn-reduce').classList.toggle('on', fx.state.reduced);
      store.set('reduced', fx.state.reduced ? '1' : '0');
      if (fx.state.reduced) PMO.constellation.stop(); else PMO.constellation.start();
      PMO.constellation.drawFrame(performance.now(), true);
      go(index, { force: true }); // re-enter so the scene renders in the new motion mode
    },
    help: () => { const h = $('#help'); h.hidden = !h.hidden; if (!h.hidden) h.querySelector('.btn').focus(); }
  };

  function onKey(e) {
    // Text fields keep their keys; range sliders stop propagation themselves (scenes.js).
    // Checkboxes/buttons must NOT swallow navigation, or the presenter gets "stuck".
    if (e.target.matches('textarea, select, input[type="text"], input[type="number"]')) return;
    if (e.target.matches('input[type="checkbox"]') && e.key === ' ') return;
    const helpOpen = !$('#help').hidden;
    if (e.key === 'Escape' && helpOpen) return actions.help();
    if (helpOpen) return;
    const k = e.key;
    const map = { ArrowRight: 'next', PageDown: 'next', ' ': 'next', ArrowLeft: 'prev', PageUp: 'prev', Home: 'home', End: 'end', f: 'fullscreen', F: 'fullscreen', p: 'pause', P: 'pause', m: 'reduce', M: 'reduce', '?': 'help', h: 'help' };
    if (k === ' ' && e.target.closest('button, [role="button"]')) return; // let space press focused buttons
    if (map[k]) { e.preventDefault(); actions[map[k]](); poke(); return; }
    if (/^[0-9]$/.test(k)) { e.preventDefault(); go(k === '0' ? 9 : +k - 1); poke(); }
  }

  /* ---------- Touch: swipe left / right to change scene ----------
     Passive touch listeners, so vertical scrolling inside a scene stays native.
     A swipe must be mostly horizontal, long enough and quick; it is ignored when it
     starts on a slider, inside something that scrolls sideways, near the screen
     edges (OS back/forward gestures), with two fingers, or while help is open. */
  function blocksSwipe(node) {
    for (let n = node; n && n !== stage; n = n.parentElement) {
      if (n.matches && n.matches('input[type="range"], textarea, select')) return true;
      if (n.scrollWidth > n.clientWidth + 1) { const ox = getComputedStyle(n).overflowX; if (ox === 'auto' || ox === 'scroll') return true; }
    }
    return false;
  }
  function initSwipe() {
    let t0 = null;
    stage.addEventListener('touchstart', (e) => {
      poke();
      t0 = null;
      if (e.touches.length !== 1 || !$('#help').hidden) return;
      const p = e.touches[0];
      if (p.clientX < 16 || p.clientX > window.innerWidth - 16 || blocksSwipe(e.target)) return;
      t0 = { x: p.clientX, y: p.clientY, t: performance.now() };
    }, { passive: true });
    stage.addEventListener('touchmove', (e) => { if (e.touches.length > 1) t0 = null; }, { passive: true });
    stage.addEventListener('touchcancel', () => { t0 = null; }, { passive: true });
    stage.addEventListener('touchend', (e) => {
      if (!t0) return;
      const p = e.changedTouches[0], dx = p.clientX - t0.x, dy = p.clientY - t0.y, dt = performance.now() - t0.t;
      t0 = null;
      const min = Math.max(50, Math.min(120, window.innerWidth * 0.15));
      if (dt > 900 || Math.abs(dx) < min || Math.abs(dx) < Math.abs(dy) * 1.5) return;
      if (dx < 0) actions.next(); else actions.prev();
    }, { passive: true });
  }

  /* ---------- Boot ---------- */
  function boot() {
    fit();
    lastFlow = fx.isFlow(); lastW = window.innerWidth;
    window.addEventListener('resize', onResize);
    window.addEventListener('orientationchange', onResize);
    if (!(document.fullscreenEnabled || document.webkitFullscreenEnabled)) document.querySelector('.hud-btn[data-action="fullscreen"]').hidden = true;
    initSwipe();
    PMO.constellation.init($('#constellation'));
    PMO.charts.initTooltips(stage);
    buildHud();
    // Reduced motion: an explicit presenter choice wins; otherwise follow the OS setting.
    const pref = store.get('reduced');
    if (pref === '1' || (pref === null && fx.state.reduced)) { fx.setReduced(true); $('#btn-reduce').classList.add('on'); PMO.constellation.stop(); PMO.constellation.drawFrame(performance.now(), true); }
    else if (pref === '0') fx.setReduced(false);
    document.addEventListener('click', (e) => { const b = e.target.closest('[data-action]'); if (b && actions[b.dataset.action]) { actions[b.dataset.action](); poke(); } });
    document.addEventListener('keydown', onKey);
    document.addEventListener('pointermove', poke);
    window.addEventListener('hashchange', () => { const n = parseInt(location.hash.slice(1), 10); if (n >= 1 && n <= scenes.length) go(n - 1); });
    const fromHash = parseInt(location.hash.slice(1), 10);
    go(fromHash >= 1 && fromHash <= scenes.length ? fromHash - 1 : 0);
    poke();
    $('#loader').classList.add('done');
    setTimeout(() => $('#loader').remove(), 800);
  }

  // Wait for bundled fonts so kinetic text is split with final metrics (no reflow pop-in).
  const ready = document.fonts && document.fonts.ready ? Promise.race([document.fonts.ready, new Promise(r => setTimeout(r, 1500))]) : Promise.resolve();
  ready.then(boot);

  return { go, resumeIndex: () => parseInt(store.get('resume') || '0', 10) || 0, get index() { return index; } };
})();
