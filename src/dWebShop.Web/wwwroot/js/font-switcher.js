/* ============================================================
   font-switcher.js — DEV-ONLY live font preview panel
   ------------------------------------------------------------
   Floating panel (bottom-right) that swaps the site's sans and
   mono fonts at runtime by:
     1. lazily injecting the chosen Google Fonts stylesheet, and
     2. overriding the --asg-sans / --asg-mono CSS variables.
   The choice is persisted in localStorage so it survives reloads
   and Blazor enhanced navigations.

   To REMOVE this tool: delete this file and the two lines that
   reference it in Components/App.razor.
   ============================================================ */
(function () {
    "use strict";

    // --- Font catalog -------------------------------------------------
    // googleName: the family= value for the Google Fonts API (null = system, no load)
    // stack:      the full CSS font-family stack applied to the variable
    var SANS = [
        { id: "inter-tight",   label: "Inter Tight (current)", googleName: "Inter+Tight:wght@400;500;600;700", stack: "'Inter Tight', 'Helvetica Neue', Helvetica, Arial, sans-serif" },
        { id: "space-grotesk", label: "Space Grotesk",         googleName: "Space+Grotesk:wght@400;500;600;700", stack: "'Space Grotesk', 'Helvetica Neue', Arial, sans-serif" },
        { id: "archivo",       label: "Archivo",               googleName: "Archivo:wght@400;500;600;700",       stack: "'Archivo', 'Helvetica Neue', Arial, sans-serif" },
        { id: "manrope",       label: "Manrope",               googleName: "Manrope:wght@400;500;600;700",       stack: "'Manrope', 'Helvetica Neue', Arial, sans-serif" },
        { id: "geist",         label: "Geist",                 googleName: "Geist:wght@400;500;600;700",         stack: "'Geist', 'Helvetica Neue', Arial, sans-serif" },
        { id: "ibm-plex-sans", label: "IBM Plex Sans",         googleName: "IBM+Plex+Sans:wght@400;500;600;700", stack: "'IBM Plex Sans', 'Helvetica Neue', Arial, sans-serif" }
    ];

    var MONO = [
        { id: "jetbrains-mono", label: "JetBrains Mono (current)", googleName: "JetBrains+Mono:wght@400;500;600", stack: "'JetBrains Mono', ui-monospace, Menlo, monospace" },
        { id: "space-mono",     label: "Space Mono",               googleName: "Space+Mono:wght@400;700",          stack: "'Space Mono', ui-monospace, Menlo, monospace" },
        { id: "ibm-plex-mono",  label: "IBM Plex Mono",            googleName: "IBM+Plex+Mono:wght@400;500;600",   stack: "'IBM Plex Mono', ui-monospace, Menlo, monospace" }
    ];

    // Curated pairings shown as one-click presets.
    var PAIRINGS = [
        { label: "1 · Technical (Space Grotesk + JetBrains)", sans: "space-grotesk", mono: "jetbrains-mono" },
        { label: "2 · Editorial (Archivo + Space Mono)",      sans: "archivo",       mono: "space-mono" },
        { label: "3 · Humanist (Manrope + IBM Plex Mono)",    sans: "manrope",       mono: "ibm-plex-mono" }
    ];

    var LS_SANS = "asg-pref-sans";
    var LS_MONO = "asg-pref-mono";

    function loadGoogleFont(googleName) {
        if (!googleName) return;
        var id = "asg-gfont-" + googleName.replace(/[^a-z0-9]/gi, "");
        if (document.getElementById(id)) return; // already loaded
        var link = document.createElement("link");
        link.id = id;
        link.rel = "stylesheet";
        link.href = "https://fonts.googleapis.com/css2?family=" + googleName + "&display=swap";
        document.head.appendChild(link);
    }

    function findById(list, id) {
        for (var i = 0; i < list.length; i++) if (list[i].id === id) return list[i];
        return null;
    }

    function applySans(id) {
        var f = findById(SANS, id) || SANS[0];
        loadGoogleFont(f.googleName);
        document.documentElement.style.setProperty("--asg-sans", f.stack);
        localStorage.setItem(LS_SANS, f.id);
    }

    function applyMono(id) {
        var f = findById(MONO, id) || MONO[0];
        loadGoogleFont(f.googleName);
        document.documentElement.style.setProperty("--asg-mono", f.stack);
        localStorage.setItem(LS_MONO, f.id);
    }

    // Apply saved prefs as early as possible (before panel is built).
    function applySaved() {
        var s = localStorage.getItem(LS_SANS);
        var m = localStorage.getItem(LS_MONO);
        if (s) applySans(s);
        if (m) applyMono(m);
    }

    function buildPanel() {
        if (document.getElementById("asg-font-switcher")) return;

        var wrap = document.createElement("div");
        wrap.id = "asg-font-switcher";
        wrap.innerHTML = [
            '<button type="button" id="asg-fs-toggle" title="Font preview">Aa</button>',
            '<div id="asg-fs-body">',
            '  <div class="asg-fs-title">Font preview <span>dev only</span></div>',
            '  <label>Pairings</label>',
            '  <div id="asg-fs-pairings"></div>',
            '  <label>Sans (headings + body)</label>',
            '  <select id="asg-fs-sans"></select>',
            '  <label>Mono (labels)</label>',
            '  <select id="asg-fs-mono"></select>',
            '  <button type="button" id="asg-fs-reset">Reset to default</button>',
            '</div>'
        ].join("");

        var css = document.createElement("style");
        css.textContent = [
            "#asg-font-switcher{position:fixed;right:16px;bottom:16px;z-index:99999;font-family:var(--asg-mono);}",
            "#asg-fs-toggle{width:44px;height:44px;border-radius:50%;border:1px solid rgba(0,0,0,.15);background:#00335e;color:#f4f1ea;font-size:16px;font-weight:700;cursor:pointer;box-shadow:0 6px 20px rgba(0,0,0,.25);}",
            "#asg-fs-body{display:none;position:absolute;right:0;bottom:54px;width:280px;background:#f4f1ea;color:#0e0e0c;border:1px solid rgba(0,0,0,.15);box-shadow:0 12px 40px rgba(0,0,0,.25);padding:16px;}",
            "#asg-font-switcher.open #asg-fs-body{display:block;}",
            "#asg-fs-body .asg-fs-title{font-size:11px;letter-spacing:.12em;text-transform:uppercase;margin-bottom:14px;display:flex;justify-content:space-between;align-items:center;}",
            "#asg-fs-body .asg-fs-title span{color:#6e6a62;font-size:9px;}",
            "#asg-fs-body label{display:block;font-size:10px;letter-spacing:.1em;text-transform:uppercase;color:#6e6a62;margin:12px 0 6px;}",
            "#asg-fs-body select{width:100%;padding:8px;border:1px solid rgba(0,0,0,.2);background:#fff;font-family:inherit;font-size:12px;}",
            "#asg-fs-pairings{display:flex;flex-direction:column;gap:6px;}",
            "#asg-fs-pairings button{text-align:left;padding:8px;border:1px solid rgba(0,0,0,.2);background:#fff;font-family:inherit;font-size:11px;cursor:pointer;}",
            "#asg-fs-pairings button:hover{border-color:#00335e;}",
            "#asg-fs-reset{margin-top:14px;width:100%;padding:8px;border:1px solid rgba(0,0,0,.2);background:transparent;font-family:inherit;font-size:11px;letter-spacing:.08em;text-transform:uppercase;cursor:pointer;}"
        ].join("");
        document.head.appendChild(css);
        document.body.appendChild(wrap);

        var sansSel = wrap.querySelector("#asg-fs-sans");
        var monoSel = wrap.querySelector("#asg-fs-mono");
        SANS.forEach(function (f) { var o = document.createElement("option"); o.value = f.id; o.textContent = f.label; sansSel.appendChild(o); });
        MONO.forEach(function (f) { var o = document.createElement("option"); o.value = f.id; o.textContent = f.label; monoSel.appendChild(o); });
        sansSel.value = localStorage.getItem(LS_SANS) || SANS[0].id;
        monoSel.value = localStorage.getItem(LS_MONO) || MONO[0].id;

        var pairWrap = wrap.querySelector("#asg-fs-pairings");
        PAIRINGS.forEach(function (p) {
            var b = document.createElement("button");
            b.type = "button";
            b.textContent = p.label;
            b.onclick = function () { applySans(p.sans); applyMono(p.mono); sansSel.value = p.sans; monoSel.value = p.mono; };
            pairWrap.appendChild(b);
        });

        wrap.querySelector("#asg-fs-toggle").onclick = function () { wrap.classList.toggle("open"); };
        sansSel.onchange = function () { applySans(sansSel.value); };
        monoSel.onchange = function () { applyMono(monoSel.value); };
        wrap.querySelector("#asg-fs-reset").onclick = function () {
            localStorage.removeItem(LS_SANS);
            localStorage.removeItem(LS_MONO);
            document.documentElement.style.removeProperty("--asg-sans");
            document.documentElement.style.removeProperty("--asg-mono");
            sansSel.value = SANS[0].id;
            monoSel.value = MONO[0].id;
        };
    }

    applySaved();
    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", buildPanel);
    } else {
        buildPanel();
    }
    // Rebuild after Blazor enhanced navigation (body gets replaced).
    document.addEventListener("enhancedload", function () { applySaved(); buildPanel(); });
})();
