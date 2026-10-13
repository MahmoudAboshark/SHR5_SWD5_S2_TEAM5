const API_BASE_URL = "http://localhost:5000/api/contacts";
const USE_API = false;

/* Trash / "delete all" only exist in the local demo mode.
   With a real API, delete is permanent and handled by the server. */
const TRASH_ENABLED = !USE_API;

const STORE_CONTACTS = "contactbook.contacts";
const STORE_TRASH = "contactbook.trash";
const STORE_FAVS = "contactbook.favs";
const STORE_VIEW = "contactbook.view";
const STORE_THEME = "contactbook.theme";

/* ---------- safe storage ---------- */
function load(key, fallback) {
    try {
        const raw = localStorage.getItem(key);
        return raw ? JSON.parse(raw) : fallback;
    } catch { return fallback; }
}
function save(key, value) {
    try { localStorage.setItem(key, JSON.stringify(value)); } catch { /* ignore */ }
}

const DEFAULT_CONTACTS = [
    { id: 1, name: "Ahmed Mohamed", email: "ahmed@example.com", phone: "01012345678", category: "friends" },
    { id: 2, name: "Mariam Ali", email: "mariam@example.com", phone: "01123456789", category: "family" }
];

let contacts = USE_API ? [] : load(STORE_CONTACTS, DEFAULT_CONTACTS);
let trash = TRASH_ENABLED ? load(STORE_TRASH, []) : [];
let favorites = new Set(load(STORE_FAVS, []));
let view = load(STORE_VIEW, "list");
let tab = "all";
let editingContactId = null;

const formSection = document.getElementById("formSection");
const contactForm = document.getElementById("contactForm");
const formTitle = document.getElementById("formTitle");
const formSub = document.getElementById("formSub");
const tableBody = document.getElementById("contactsTableBody");
const gridView = document.getElementById("gridView");
const listView = document.getElementById("listView");
const contactsCount = document.getElementById("contactsCount");
const searchInput = document.getElementById("searchInput");
const emptyMessage = document.getElementById("emptyMessage");
const statusMessage = document.getElementById("statusMessage");
const confirmDialog = document.getElementById("confirmDialog");
const confirmTitle = document.getElementById("confirmTitle");
const confirmText = document.getElementById("confirmText");
const confirmOk = document.getElementById("confirmOk");
const detailsDialog = document.getElementById("detailsDialog");
const detailsBody = document.getElementById("detailsBody");
const bulkBtn = document.getElementById("bulkBtn");
const bulkLabel = document.getElementById("bulkLabel");
const tabsEl = document.getElementById("tabs");

function persist() {
    if (!USE_API) save(STORE_CONTACTS, contacts);
    if (TRASH_ENABLED) save(STORE_TRASH, trash);
    save(STORE_FAVS, [...favorites]);
}

/* ---------- icons (static, trusted markup) ---------- */
const ICON = {
    star: '<svg viewBox="0 0 24 24"><path d="M12 2.5l2.9 6 6.6.9-4.8 4.6 1.2 6.5L12 17.3 6.1 20.5l1.2-6.5L2.5 9.4l6.6-.9z"/></svg>',
    copy: '<svg viewBox="0 0 24 24"><rect x="9" y="9" width="12" height="12" rx="2"/><path d="M5 15V5a2 2 0 0 1 2-2h10"/></svg>',
    check: '<svg viewBox="0 0 24 24"><path d="M5 12.5l4.5 4.5L19 7.5"/></svg>',
    ok: '<svg viewBox="0 0 24 24"><circle cx="12" cy="12" r="10"/><path d="m8 12.5 3 3 5-6"/></svg>',
    alert: '<svg viewBox="0 0 24 24"><circle cx="12" cy="12" r="10"/><path d="M12 8v4M12 16h.01"/></svg>',
    edit: '<svg viewBox="0 0 24 24"><path d="M12 20h9"/><path d="M16.5 3.5a2.1 2.1 0 0 1 3 3L7 19l-4 1 1-4z"/></svg>',
    trash: '<svg viewBox="0 0 24 24"><path d="M3 6h18M8 6V4h8v2M19 6l-1 14H6L5 6M10 11v6M14 11v6"/></svg>',
    mail: '<svg viewBox="0 0 24 24"><rect x="3" y="5" width="18" height="14" rx="2"/><path d="m3 7 9 6 9-6"/></svg>',
    phone: '<svg viewBox="0 0 24 24"><path d="M22 16.9v3a2 2 0 0 1-2.2 2 19.8 19.8 0 0 1-8.6-3.1 19.5 19.5 0 0 1-6-6A19.8 19.8 0 0 1 2.1 4.2 2 2 0 0 1 4.1 2h3a2 2 0 0 1 2 1.7c.1 1 .4 1.9.7 2.8a2 2 0 0 1-.5 2.1L8 9.9a16 16 0 0 0 6 6l1.3-1.3a2 2 0 0 1 2.1-.4c.9.3 1.8.6 2.8.7a2 2 0 0 1 1.7 2z"/></svg>',
    eye: '<svg viewBox="0 0 24 24"><path d="M2 12s3.5-7 10-7 10 7 10 7-3.5 7-10 7S2 12 2 12z"/><circle cx="12" cy="12" r="3"/></svg>',
    restore: '<svg viewBox="0 0 24 24"><path d="M3 12a9 9 0 1 0 3-6.7L3 8"/><path d="M3 3v5h5"/></svg>',
    home: '<svg viewBox="0 0 24 24"><path d="m3 11 9-8 9 8"/><path d="M5 10v10h14V10"/><path d="M10 20v-6h4v6"/></svg>',
    briefcase: '<svg viewBox="0 0 24 24"><rect x="3" y="7" width="18" height="13" rx="2"/><path d="M9 7V5a2 2 0 0 1 2-2h2a2 2 0 0 1 2 2v2M3 13h18"/></svg>',
    users: '<svg viewBox="0 0 24 24"><path d="M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M22 21v-2a4 4 0 0 0-3-3.9M16 3.1a4 4 0 0 1 0 7.8"/></svg>',
    calendar: '<svg viewBox="0 0 24 24"><rect x="3" y="4" width="18" height="18" rx="2"/><path d="M16 2v4M8 2v4M3 10h18"/></svg>'
};

const CATEGORIES = {
    family: { label: "Family", icon: ICON.home },
    work: { label: "Work", icon: ICON.briefcase },
    friends: { label: "Friends", icon: ICON.users }
};

/* ---------- toast ---------- */
let toastTimer;
function showMessage(message, isError = false) {
    statusMessage.replaceChildren();
    const icon = el("span", "toast-icon");
    icon.innerHTML = isError ? ICON.alert : ICON.ok;
    statusMessage.append(icon, message);
    statusMessage.classList.toggle("error", isError);
    statusMessage.classList.add("show");
    clearTimeout(toastTimer);
    toastTimer = setTimeout(() => statusMessage.classList.remove("show"), 3500);
}

/* ---------- helpers ---------- */
const AVATAR_COLORS = [
    "linear-gradient(135deg,#c8553d,#e0895a)",
    "linear-gradient(135deg,#7d8f68,#a9bd96)",
    "linear-gradient(135deg,#d9a441,#b8742f)",
    "linear-gradient(135deg,#b5707a,#e0a39b)",
    "linear-gradient(135deg,#8a6a52,#c4a484)"
];

function avatarColor(name) {
    let h = 0;
    for (const ch of name) h = (h * 31 + ch.charCodeAt(0)) >>> 0;
    return AVATAR_COLORS[h % AVATAR_COLORS.length];
}

function getInitials(name) {
    return name.split(/\s+/).filter(Boolean).slice(0, 2)
        .map(w => w[0].toUpperCase()).join("");
}

function el(tag, className, text) {
    const node = document.createElement(tag);
    if (className) node.className = className;
    if (text !== undefined) node.textContent = text;
    return node;
}

function makeAvatar(name) {
    const avatar = el("span", "avatar", getInitials(name));
    avatar.style.setProperty("--av", avatarColor(name));
    return avatar;
}

function plural(n, word) {
    return `${n} ${word}${n === 1 ? "" : "s"}`;
}

/* highlight search matches (safe: DOM nodes, no innerHTML) */
function highlight(text, query) {
    const frag = document.createDocumentFragment();
    const value = String(text);
    if (!query) { frag.append(value); return frag; }
    const lower = value.toLowerCase();
    let pos = 0;
    let idx;
    while ((idx = lower.indexOf(query, pos)) !== -1) {
        frag.append(value.slice(pos, idx));
        frag.append(el("mark", "", value.slice(idx, idx + query.length)));
        pos = idx + query.length;
    }
    frag.append(value.slice(pos));
    return frag;
}

function iconButton(className, svg, label, onClick) {
    const btn = el("button", className);
    btn.type = "button";
    btn.innerHTML = svg;
    btn.title = label;
    btn.setAttribute("aria-label", label);
    btn.addEventListener("click", onClick);
    return btn;
}

function copyButton(value, label) {
    const btn = iconButton("copy-btn", ICON.copy, `Copy ${label}`, async () => {
        try {
            await navigator.clipboard.writeText(value);
            btn.innerHTML = ICON.check;
            btn.classList.add("done");
            showMessage(`${label} copied.`);
            setTimeout(() => {
                btn.innerHTML = ICON.copy;
                btn.classList.remove("done");
            }, 1500);
        } catch {
            showMessage("Could not copy.", true);
        }
    });
    return btn;
}

function toggleFavorite(contact) {
    if (favorites.has(contact.id)) favorites.delete(contact.id);
    else favorites.add(contact.id);
    persist();
    renderContacts(false);
}

function starButton(contact, afterToggle) {
    const on = favorites.has(contact.id);
    return iconButton(
        "star-btn" + (on ? " on" : ""),
        ICON.star,
        on ? "Remove from favorites" : "Add to favorites",
        () => {
            toggleFavorite(contact);
            if (afterToggle) afterToggle();
        }
    );
}

function linkNode(href, text, query) {
    const a = el("a");
    a.href = href;
    a.append(highlight(text, query));
    return a;
}

function categoryBadge(category) {
    const cat = CATEGORIES[category];
    if (!cat) return null;
    const badge = el("span", `badge cat-${category}`);
    const icon = el("i");
    icon.innerHTML = cat.icon;
    badge.append(icon, cat.label);
    return badge;
}

function subLine(iconSvg, href, text, copyLabel, query) {
    const line = el("div", "sub-line");
    const icon = el("span", "sub-icon");
    icon.innerHTML = iconSvg;
    line.append(icon, linkNode(href, text, query), copyButton(text, copyLabel));
    return line;
}

function actionBtn(className, iconSvg, label, onClick) {
    const btn = el("button", `action-btn ${className}`);
    btn.type = "button";
    btn.innerHTML = iconSvg + "<span></span>";
    btn.lastChild.textContent = label;
    btn.addEventListener("click", onClick);
    return btn;
}

function actionButtons(contact, node, inTrash) {
    const wrap = el("div", "actions-cell");
    if (inTrash) {
        wrap.append(
            actionBtn("restore-btn", ICON.restore, "Restore", () => restoreContact(contact, node)),
            actionBtn("delete-btn", ICON.trash, "Delete forever", () => deleteForever(contact, node))
        );
    } else {
        wrap.append(
            actionBtn("view-btn", ICON.eye, "View", () => openDetails(contact)),
            actionBtn("edit-btn", ICON.edit, "Edit", () => openEditForm(contact)),
            actionBtn("delete-btn", ICON.trash, "Delete", () => deleteContact(contact, node))
        );
    }
    return wrap;
}

/* ---------- stats ---------- */
function countUp(node, to) {
    const from = Number(node.dataset.value || 0);
    node.dataset.value = to;
    if (from === to || matchMedia("(prefers-reduced-motion: reduce)").matches) {
        node.textContent = to;
        return;
    }
    const start = performance.now();
    const duration = 450;
    (function tick(now) {
        const t = Math.min((now - start) / duration, 1);
        const eased = 1 - Math.pow(1 - t, 3);
        node.textContent = Math.round(from + (to - from) * eased);
        if (t < 1) requestAnimationFrame(tick);
    })(start);
}

function renderStats(counts = countTabs()) {
    countUp(document.getElementById("statTotal"), counts.all);
    countUp(document.getElementById("statFav"), counts.favorites);
}

/* ---------- tabs ---------- */
const collator = new Intl.Collator();

function tabItems(name) {
    switch (name) {
        case "all": return contacts;
        case "favorites": return contacts.filter(c => favorites.has(c.id));
        case "trash": return trash;
        default: return contacts.filter(c => c.category === name);
    }
}

/* one pass over the data gives every tab's count */
function countTabs() {
    const counts = { all: contacts.length, favorites: 0, trash: trash.length, family: 0, work: 0, friends: 0 };
    for (const c of contacts) {
        if (favorites.has(c.id)) counts.favorites++;
        if (c.category in CATEGORIES) counts[c.category]++;
    }
    return counts;
}

function renderTabs(counts) {
    tabsEl.querySelectorAll(".tab").forEach(btn => {
        const name = btn.dataset.tab;
        btn.hidden = name === "trash" && !TRASH_ENABLED;
        btn.classList.toggle("active", name === tab);
        btn.setAttribute("aria-selected", name === tab);
        btn.querySelector(".tab-count").textContent = counts[name];
    });
}

tabsEl.addEventListener("click", event => {
    const btn = event.target.closest(".tab");
    if (!btn) return;
    tab = btn.dataset.tab;
    renderContacts(true);
});

const TAB_TITLES = {
    all: "All Contacts", favorites: "Favorites", family: "Family",
    work: "Work", friends: "Friends", trash: "Trash"
};

const EMPTY_TEXT = {
    all: ["No contacts yet", "Add your first contact to get started."],
    favorites: ["No favorites yet", "Tap the star on a contact to add it here."],
    family: ["No family contacts", "Choose the Family category when adding a contact."],
    work: ["No work contacts", "Choose the Work category when adding a contact."],
    friends: ["No friends added", "Choose the Friends category when adding a contact."],
    trash: ["Trash is empty", "Deleted contacts will appear here."]
};

/* ---------- render ---------- */
function matchesQuery(contact, query) {
    return query === "" || [contact.name, contact.email, contact.phone]
        .some(value => String(value).toLowerCase().includes(query));
}

function buildRows(list, query, inTrash) {
    const frag = document.createDocumentFragment();
    list.forEach((contact, index) => {
        const row = el("tr");
        row.style.setProperty("--i", Math.min(index, 10));

        const contactCell = el("td", "contact-cell");
        const nameWrap = el("div", "contact-name");

        const text = el("div", "contact-text");
        const nameLine = el("div", "name-line");
        const nameText = el(inTrash ? "span" : "button", "name-text");
        if (!inTrash) {
            nameText.type = "button";
            nameText.title = "View details";
            nameText.addEventListener("click", () => openDetails(contact));
        }
        nameText.append(highlight(contact.name, query));
        nameLine.append(nameText);
        const badge = categoryBadge(contact.category);
        if (badge) nameLine.append(badge);
        if (!inTrash) nameLine.append(starButton(contact));

        text.append(nameLine,
            subLine(ICON.mail, `mailto:${contact.email}`, contact.email, "Email", query),
            subLine(ICON.phone, `tel:${contact.phone}`, contact.phone, "Phone", query));
        nameWrap.append(makeAvatar(contact.name), text);
        contactCell.append(nameWrap);

        const actionsCell = el("td", "actions-td");
        actionsCell.append(actionButtons(contact, row, inTrash));

        row.append(contactCell, actionsCell);
        frag.append(row);
    });
    return frag;
}

function buildCards(list, query, inTrash) {
    const frag = document.createDocumentFragment();
    list.forEach((contact, index) => {
        const card = el("article", "contact-card spot");
        card.style.setProperty("--i", Math.min(index, 10));

        const title = el("h3");
        title.append(highlight(contact.name, query));

        card.append(makeAvatar(contact.name), title);
        const cBadge = categoryBadge(contact.category);
        if (cBadge) card.append(cBadge);
        card.append(
            subLine(ICON.mail, `mailto:${contact.email}`, contact.email, "Email", query),
            subLine(ICON.phone, `tel:${contact.phone}`, contact.phone, "Phone", query),
            actionButtons(contact, card, inTrash));
        if (!inTrash) card.prepend(starButton(contact));
        frag.append(card);
    });
    return frag;
}

function renderContacts(animate = true) {
    const query = searchInput.value.trim().toLowerCase();
    const inTrash = tab === "trash";
    const isList = view === "list";

    const source = tabItems(tab);

    /* filter + sort in one go; favourite flag computed once per contact */
    const decorated = [];
    for (const contact of source) {
        if (!matchesQuery(contact, query)) continue;
        decorated.push({ contact, fav: favorites.has(contact.id) });
    }
    decorated.sort((a, b) => inTrash
        ? (b.contact.deletedAt || 0) - (a.contact.deletedAt || 0)
        : (Number(b.fav) - Number(a.fav)) || collator.compare(a.contact.name, b.contact.name));
    const filtered = decorated.map(d => d.contact);

    listView.classList.toggle("static", !animate);
    gridView.classList.toggle("static", !animate);

    contactsCount.previousElementSibling.textContent = TAB_TITLES[tab];
    contactsCount.textContent = inTrash
        ? plural(source.length, "item") + " in trash"
        : plural(source.length, "contact");

    /* empty state */
    emptyMessage.classList.toggle("hidden", filtered.length > 0);
    const [emptyTitle, emptySub] = query
        ? ["No contacts found", "Try a different search."]
        : EMPTY_TEXT[tab];
    emptyMessage.querySelector("strong").textContent = emptyTitle;
    emptyMessage.querySelector("p").textContent = emptySub;

    /* bulk button: "Delete all" on All, "Empty trash" on Trash */
    const showBulk = TRASH_ENABLED && (tab === "all" || inTrash) && source.length > 0;
    bulkBtn.hidden = !showBulk;
    bulkLabel.textContent = inTrash ? "Empty trash" : "Move all to Trash";

    const counts = countTabs();
    renderTabs(counts);
    renderStats(counts);

    /* only the visible view is built; the other is rebuilt when toggled */
    tableBody.replaceChildren();
    gridView.replaceChildren();
    if (isList) tableBody.append(buildRows(filtered, query, inTrash));
    else gridView.append(buildCards(filtered, query, inTrash));
}

/* ---------- view toggle ---------- */
function applyView(next) {
    view = next;
    save(STORE_VIEW, view);
    listView.classList.toggle("hidden", view !== "list");
    gridView.classList.toggle("hidden", view !== "grid");
    document.getElementById("viewList").classList.toggle("active", view === "list");
    document.getElementById("viewGrid").classList.toggle("active", view === "grid");
}

document.getElementById("viewList").addEventListener("click", () => { applyView("list"); renderContacts(true); });
document.getElementById("viewGrid").addEventListener("click", () => { applyView("grid"); renderContacts(true); });

/* ---------- confirm dialog ---------- */
function askConfirm({ title, text, ok = "Delete" }) {
    confirmTitle.textContent = title;
    confirmText.textContent = text;
    confirmOk.textContent = ok;
    return new Promise(resolve => {
        confirmDialog.addEventListener("close", () => {
            resolve(confirmDialog.returnValue === "ok");
        }, { once: true });
        confirmDialog.showModal();
    });
}

function animateRemoval(node, done) {
    if (!node) { done(); return; }
    node.classList.add("leaving");
    setTimeout(done, 220);
}

/* ---------- delete / trash / restore ---------- */
async function deleteContact(contact, node) {
    const confirmed = await askConfirm(TRASH_ENABLED
        ? {
            title: "Move to Trash?",
            text: `${contact.name} will be moved to the trash. You can restore it later.`,
            ok: "Move to Trash"
        }
        : {
            title: "Delete contact?",
            text: `${contact.name} will be permanently deleted.`,
            ok: "Delete"
        });
    if (!confirmed) return;

    if (USE_API) {
        try {
            const response = await fetch(
                `${API_BASE_URL}/${encodeURIComponent(contact.id)}`,
                { method: "DELETE" }
            );
            if (!response.ok) throw new Error("Could not delete contact.");
            animateRemoval(node, () => {
                contacts = contacts.filter(item => item.id !== contact.id);
                favorites.delete(contact.id);
                persist();
                renderContacts(false);
            });
            showMessage("Contact deleted successfully.");
        } catch (error) {
            showMessage(error.message, true);
        }
        return;
    }

    animateRemoval(node, () => {
        contacts = contacts.filter(item => item.id !== contact.id);
        trash.push({ ...contact, deletedAt: Date.now() });
        persist();
        renderContacts(false);
    });
    showMessage("Moved to trash.");
}

function restoreContact(contact, node) {
    animateRemoval(node, () => {
        trash = trash.filter(item => item.id !== contact.id);
        const { deletedAt, ...restored } = contact;
        contacts.push(restored);
        persist();
        renderContacts(false);
    });
    showMessage(`${contact.name} restored.`);
}

async function deleteForever(contact, node) {
    const confirmed = await askConfirm({
        title: "Delete permanently?",
        text: `${contact.name} will be deleted for good. This can't be undone.`,
        ok: "Delete forever"
    });
    if (!confirmed) return;
    animateRemoval(node, () => {
        trash = trash.filter(item => item.id !== contact.id);
        favorites.delete(contact.id);
        persist();
        renderContacts(false);
    });
    showMessage("Deleted permanently.");
}

bulkBtn.addEventListener("click", async () => {
    if (tab === "trash") {
        const n = trash.length;
        const confirmed = await askConfirm({
            title: "Empty trash?",
            text: `${plural(n, "contact")} will be deleted permanently. This can't be undone.`,
            ok: "Empty trash"
        });
        if (!confirmed) return;
        trash.forEach(c => favorites.delete(c.id));
        trash = [];
        persist();
        renderContacts(true);
        showMessage("Trash emptied.");
    } else {
        const n = contacts.length;
        const confirmed = await askConfirm({
            title: "Move all contacts to Trash?",
            text: `All ${plural(n, "contact")} will be moved to the trash. You can restore them later.`,
            ok: "Move all to Trash"
        });
        if (!confirmed) return;
        const now = Date.now();
        trash.push(...contacts.map(c => ({ ...c, deletedAt: now })));
        contacts = [];
        persist();
        renderContacts(true);
        showMessage("All contacts moved to trash.");
    }
});

/* ---------- details dialog ---------- */
let detailsId = null;

function renderDetails() {
    const contact = contacts.find(c => c.id === detailsId);
    if (!contact) { if (detailsDialog.open) detailsDialog.close(); return; }

    detailsBody.replaceChildren();

    const top = el("div", "details-top");
    const avatar = makeAvatar(contact.name);
    avatar.classList.add("big");
    const name = el("h3", "", contact.name);
    top.append(avatar, name);
    const badge = categoryBadge(contact.category);
    if (badge) top.append(badge);

    const list = el("div", "details-list");
    list.append(
        subLine(ICON.mail, `mailto:${contact.email}`, contact.email, "Email", ""),
        subLine(ICON.phone, `tel:${contact.phone}`, contact.phone, "Phone", ""));
    if (contact.createdAt) {
        const added = el("div", "sub-line muted-line");
        const icon = el("span", "sub-icon");
        icon.innerHTML = ICON.calendar;
        added.append(icon, "Added " + new Date(contact.createdAt).toLocaleDateString(
            undefined, { day: "numeric", month: "short", year: "numeric" }));
        list.append(added);
    }

    const actions = el("div", "details-actions");
    const copyAll = el("button", "btn primary");
    copyAll.type = "button";
    copyAll.innerHTML = ICON.copy + "<span>Copy all</span>";
    copyAll.addEventListener("click", async () => {
        const text = `${contact.name}\n${contact.email}\n${contact.phone}`;
        try {
            await navigator.clipboard.writeText(text);
            showMessage("Contact details copied.");
        } catch {
            showMessage("Could not copy.", true);
        }
    });

    const fav = el("button", "btn secondary" + (favorites.has(contact.id) ? " fav-on" : ""));
    fav.type = "button";
    fav.innerHTML = ICON.star + "<span>" + (favorites.has(contact.id) ? "Favorited" : "Favorite") + "</span>";
    fav.addEventListener("click", () => { toggleFavorite(contact); renderDetails(); });

    const edit = el("button", "btn secondary");
    edit.type = "button";
    edit.innerHTML = ICON.edit + "<span>Edit</span>";
    edit.addEventListener("click", () => { detailsDialog.close(); openEditForm(contact); });

    const del = el("button", "btn secondary danger-text");
    del.type = "button";
    del.innerHTML = ICON.trash + "<span>Delete</span>";
    del.addEventListener("click", () => { detailsDialog.close(); deleteContact(contact, null); });

    actions.append(copyAll, fav, edit, del);
    detailsBody.append(top, list, actions);
}

function openDetails(contact) {
    detailsId = contact.id;
    renderDetails();
    if (!detailsDialog.open) detailsDialog.showModal();
}

document.getElementById("closeDetails").addEventListener("click", () => detailsDialog.close());
detailsDialog.addEventListener("mousedown", event => {
    if (event.target === detailsDialog) detailsDialog.close();
});

/* ---------- form (modal) ---------- */
function openForm(title, sub) {
    formTitle.textContent = title;
    formSub.textContent = sub;
    if (!formSection.open) formSection.showModal();
}

function openEditForm(contact) {
    editingContactId = contact.id;
    contactForm.reset();
    document.getElementById("name").value = contact.name;
    document.getElementById("email").value = contact.email;
    document.getElementById("phone").value = contact.phone;
    document.getElementById("category").value = contact.category || "";
    openForm("Edit Contact", "Update the details and save.");
}

document.getElementById("showFormBtn").addEventListener("click", () => {
    editingContactId = null;
    contactForm.reset();
    /* preselect the category of the tab you are on */
    if (CATEGORIES[tab]) document.getElementById("category").value = tab;
    openForm("Add New Contact", "Fill in the details below.");
});

function closeForm() {
    if (formSection.open) formSection.close();
}

formSection.addEventListener("close", () => {
    contactForm.reset();
    editingContactId = null;
});

document.getElementById("cancelBtn").addEventListener("click", closeForm);
document.getElementById("closeFormBtn").addEventListener("click", closeForm);

formSection.addEventListener("mousedown", event => {
    if (event.target === formSection) closeForm();
});

contactForm.addEventListener("submit", async event => {
    event.preventDefault();

    const contactData = {
        name: document.getElementById("name").value.trim(),
        email: document.getElementById("email").value.trim(),
        phone: document.getElementById("phone").value.trim(),
        category: document.getElementById("category").value
    };

    if (!contactData.name || !contactData.email || !contactData.phone) {
        showMessage("Please fill in all fields.", true);
        return;
    }

    const wasEditing = Boolean(editingContactId);

    try {
        if (USE_API) {
            if (editingContactId) {
                const response = await fetch(`${API_BASE_URL}/${encodeURIComponent(editingContactId)}`, {
                    method: "PUT",
                    headers: { "Content-Type": "application/json" },
                    body: JSON.stringify(contactData)
                });
                if (!response.ok) throw new Error("Could not update contact.");
            } else {
                const response = await fetch(API_BASE_URL, {
                    method: "POST",
                    headers: { "Content-Type": "application/json" },
                    body: JSON.stringify(contactData)
                });
                if (!response.ok) throw new Error("Could not save contact.");
            }

            await loadContacts();
        } else {
            if (editingContactId) {
                const index = contacts.findIndex(item => item.id === editingContactId);
                if (index !== -1) {
                    contacts[index] = { ...contacts[index], ...contactData };
                }
            } else {
                contactData.id = Date.now();
                contactData.createdAt = contactData.id;
                contacts.push(contactData);
            }
            persist();
            renderContacts(true);
        }

        closeForm();
        showMessage(wasEditing ? "Contact updated successfully." : "Contact saved successfully.");
    } catch (error) {
        showMessage(error.message, true);
    }
});

let searchTimer;
searchInput.addEventListener("input", () => {
    clearTimeout(searchTimer);
    searchTimer = setTimeout(() => renderContacts(false), 90);
});

/* ---------- theme ---------- */
document.getElementById("themeToggle").addEventListener("click", () => {
    const root = document.documentElement;
    const next = root.getAttribute("data-theme") === "dark" ? "light" : "dark";
    root.setAttribute("data-theme", next);
    try { localStorage.setItem(STORE_THEME, next); } catch { /* ignore */ }
});

/* ---------- keyboard shortcut ---------- */
document.addEventListener("keydown", event => {
    const typing = /^(INPUT|TEXTAREA|SELECT)$/.test(document.activeElement.tagName);
    if (event.key === "/" && !typing) {
        event.preventDefault();
        searchInput.focus();
    }
});

/* ---------- cursor spotlight (throttled, mouse only) ---------- */
let spotFrame = 0, spotEvent = null;
document.addEventListener("pointermove", event => {
    if (event.pointerType !== "mouse") return;
    spotEvent = event;
    if (spotFrame) return;
    spotFrame = requestAnimationFrame(() => {
        spotFrame = 0;
        const target = spotEvent.target.closest && spotEvent.target.closest(".spot");
        if (!target) return;
        const rect = target.getBoundingClientRect();
        target.style.setProperty("--mx", `${spotEvent.clientX - rect.left}px`);
        target.style.setProperty("--my", `${spotEvent.clientY - rect.top}px`);
    });
}, { passive: true });

/* ---------- API + init ---------- */
async function loadContacts() {
    const response = await fetch(API_BASE_URL);

    if (!response.ok) {
        throw new Error("Could not load contacts.");
    }

    const data = await response.json();

    if (!Array.isArray(data)) {
        throw new Error("Unexpected API response.");
    }

    contacts = data;
    renderContacts(true);
}

async function initializeApp() {
    applyView(view === "grid" ? "grid" : "list");
    if (USE_API) {
        try {
            await loadContacts();
        } catch (error) {
            showMessage(error.message, true);
        }
    } else {
        renderContacts(true);
    }
}

initializeApp();

/* pause background animation when the tab is hidden */
document.addEventListener("visibilitychange", () => {
    document.body.classList.toggle("hidden-tab", document.hidden);
});
