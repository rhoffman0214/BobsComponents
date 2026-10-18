// Browser helpers for the BobsComponents docs site: live DOM reporting for the DOM map, the part inspector,
// element highlighting from the DOM map, and clipboard copy. Loaded once through BobJs from
// ./_content/BobsComponents.Docs/js/bobdocs.js.

const frames = new Map();       // key -> { el, observer, inspector }
let tip = null;
let highlightBox = null;

// ---- DOM map ---------------------------------------------------------------------------------------------

/** Registers a demo frame and reports its markup to .NET (debounced) whenever it changes. */
export function observeFrame(key, el, dotnetRef) {
    disposeFrame(key);
    let timer = 0;
    const report = () => {
        clearTimeout(timer);
        timer = setTimeout(() => dotnetRef.invokeMethodAsync("OnDomChanged", el.innerHTML), 80);
    };
    const observer = new MutationObserver(report);
    observer.observe(el, { subtree: true, childList: true, attributes: true, characterData: true });
    frames.set(key, { el, observer, inspector: null, dotnetRef });
    dotnetRef.invokeMethodAsync("OnDomChanged", el.innerHTML);
}

export function disposeFrame(key) {
    const f = frames.get(key);
    if (!f) return;
    f.observer.disconnect();
    setInspectorEnabled(key, false);
    frames.delete(key);
    hideTip();
}

/** Elements counted exactly like DomMap.Build in C#: document order, skipping the insides of <svg>. */
function countedElements(root) {
    const list = [];
    const walk = (node) => {
        for (const child of node.children) {
            list.push(child);
            if (child.tagName.toLowerCase() !== "svg") walk(child);
        }
    };
    walk(root);
    return list;
}

export function highlightElement(key, index) {
    const f = frames.get(key);
    if (!f) return;
    const el = countedElements(f.el)[index];
    if (!el) return;
    showHighlight(el);
}

export function clearHighlight() {
    if (highlightBox) highlightBox.style.display = "none";
}

function showHighlight(el) {
    if (!highlightBox) {
        highlightBox = document.createElement("div");
        highlightBox.className = "bobdoc-highlight";
        document.body.appendChild(highlightBox);
    }
    const r = el.getBoundingClientRect();
    Object.assign(highlightBox.style, {
        display: "block",
        left: r.left + "px",
        top: r.top + "px",
        width: Math.max(r.width, 2) + "px",
        height: Math.max(r.height, 2) + "px",
    });
}

// ---- Part inspector --------------------------------------------------------------------------------------

/** Hover a part (or any bob- element) in the demo to see its classes, the variables that style it and its states. */
export function setInspectorEnabled(key, enabled, indexJson) {
    const f = frames.get(key);
    if (!f) return;
    if (f.inspector) {
        f.el.removeEventListener("pointerover", f.inspector.over);
        f.el.removeEventListener("pointerleave", f.inspector.leave);
        f.inspector = null;
        hideTip();
    }
    if (!enabled) return;

    const index = JSON.parse(indexJson);
    const over = (e) => {
        const target = e.target.closest ? e.target.closest("[data-bob-part], [class*='bob-']") : null;
        if (!target || !f.el.contains(target)) { hideTip(); return; }
        showTip(target, index, e);
        showHighlight(target);
    };
    const leave = () => { hideTip(); clearHighlight(); };
    f.el.addEventListener("pointerover", over);
    f.el.addEventListener("pointerleave", leave);
    f.inspector = { over, leave };
}

function ensureTip() {
    if (tip) return tip;
    tip = document.createElement("div");
    tip.className = "bobdoc-tip";
    tip.setAttribute("role", "tooltip");
    document.body.appendChild(tip);
    return tip;
}

function hideTip() {
    if (tip) tip.style.display = "none";
}

function add(parent, tag, text, cls) {
    const el = document.createElement(tag);
    if (cls) el.className = cls;
    el.textContent = text;
    parent.appendChild(el);
    return el;
}

function showTip(target, index, event) {
    const t = ensureTip();
    t.replaceChildren();

    const part = target.getAttribute("data-bob-part");
    const classes = [...target.classList].filter((c) => c.startsWith("bob-"));
    const infos = classes.map((c) => ({ name: c, info: index[c] })).filter((x) => x.info);
    const component = infos.length ? infos[0].info.component : "";

    const head = add(t, "div", "", "bobdoc-tip__head");
    add(head, "strong", (component ? component + " " : "") + (part ? "part: " + part : "<" + target.tagName.toLowerCase() + ">"));
    add(head, "span", "<" + target.tagName.toLowerCase() + ">", "bobdoc-tip__tag");

    if (classes.length) {
        const row = add(t, "div", "", "bobdoc-tip__row");
        add(row, "span", "classes", "bobdoc-tip__label");
        add(row, "code", classes.map((c) => "." + c).join(" "));
    }

    const vars = new Map();
    const states = new Map();
    for (const { info } of infos) {
        for (const [name, prop, state] of info.vars) {
            const label = prop + (state ? " when " + state : "");
            vars.set(name, (vars.get(name) ? vars.get(name) + ", " : "") + label);
        }
        for (const [state, props] of info.states) states.set(state, props);
    }

    if (vars.size) {
        add(t, "div", "variables", "bobdoc-tip__label");
        const ul = add(t, "ul", "");
        for (const [name, effect] of vars) {
            const li = add(ul, "li", "");
            add(li, "code", name);
            add(li, "span", " sets " + effect);
        }
    } else {
        add(t, "div", "No custom properties on this element; style it with a selector.", "bobdoc-tip__muted");
    }

    if (states.size) {
        add(t, "div", "states", "bobdoc-tip__label");
        const ul = add(t, "ul", "");
        for (const [state, props] of states) {
            const li = add(ul, "li", "");
            add(li, "code", state);
            add(li, "span", " changes " + props);
        }
    }

    t.style.display = "block";
    const margin = 14;
    const w = t.offsetWidth;
    const h = t.offsetHeight;
    let x = event.clientX + margin;
    let y = event.clientY + margin;
    if (x + w > window.innerWidth - 8) x = Math.max(8, event.clientX - w - margin);
    if (y + h > window.innerHeight - 8) y = Math.max(8, event.clientY - h - margin);
    t.style.left = x + "px";
    t.style.top = y + "px";
}

// ---- Clipboard -------------------------------------------------------------------------------------------

export async function copyText(text) {
    try {
        await navigator.clipboard.writeText(text);
        return true;
    } catch {
        const area = document.createElement("textarea");
        area.value = text;
        area.setAttribute("readonly", "");
        area.style.position = "fixed";
        area.style.opacity = "0";
        document.body.appendChild(area);
        area.select();
        let ok = false;
        try { ok = document.execCommand("copy"); } catch { ok = false; }
        area.remove();
        return ok;
    }
}

export function scrollToId(id) {
    const el = document.getElementById(id);
    if (el) el.scrollIntoView({ block: "start" });
}
