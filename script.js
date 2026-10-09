
const API_BASE_URL = "http://localhost:5000/api/contacts";
const USE_API = false;

let contacts = [
    {
        id: 1,
        name: "Ahmed Mohamed",
        email: "ahmed@example.com",
        phone: "01012345678"
    },
    {
        id: 2,
        name: "Mariam Ali",
        email: "mariam@example.com",
        phone: "01123456789"
    }
];

const formSection = document.getElementById("formSection");
const contactForm = document.getElementById("contactForm");
const tableBody = document.getElementById("contactsTableBody");
const contactsCount = document.getElementById("contactsCount");
const searchInput = document.getElementById("searchInput");
const emptyMessage = document.getElementById("emptyMessage");
const statusMessage = document.getElementById("statusMessage");

function showMessage(message, isError = false) {
    statusMessage.textContent = message;
    statusMessage.style.color = isError ? "#d9364f" : "#16845b";
}

function renderContacts() {
    const query = searchInput.value.trim().toLowerCase();

    const filteredContacts = contacts.filter(contact =>
        [contact.name, contact.email, contact.phone].some(value =>
            String(value).toLowerCase().includes(query)
        )
    );

    tableBody.replaceChildren();

    contactsCount.textContent = `${contacts.length} contacts`;

    emptyMessage.classList.toggle(
        "hidden",
        filteredContacts.length > 0
    );

    filteredContacts.forEach(contact => {
        const row = document.createElement("tr");

        [contact.name, contact.email, contact.phone].forEach(value => {
            const cell = document.createElement("td");
            cell.textContent = value;
            row.appendChild(cell);
        });

        const actionsCell = document.createElement("td");
        const deleteButton = document.createElement("button");

        deleteButton.type = "button";
        deleteButton.className = "action-btn";
        deleteButton.textContent = "Delete";

        deleteButton.addEventListener("click", async () => {
            if (USE_API) {
                try {
                    const response = await fetch(
                        `${API_BASE_URL}/${encodeURIComponent(contact.id)}`,
                        { method: "DELETE" }
                    );

                    if (!response.ok) {
                        throw new Error("Could not delete contact.");
                    }

                    contacts = contacts.filter(item => item.id !== contact.id);
                    renderContacts();
                    showMessage("Contact deleted successfully.");
                } catch (error) {
                    showMessage(error.message, true);
                }
            } else {
                contacts = contacts.filter(item => item.id !== contact.id);
                renderContacts();
                showMessage("Deleted from demo data only.");
            }
        });

        actionsCell.appendChild(deleteButton);
        row.appendChild(actionsCell);
        tableBody.appendChild(row);
    });
}

document.getElementById("showFormBtn").addEventListener("click", () => {
    formSection.classList.remove("hidden");
    document.getElementById("name").focus();
});

function closeForm() {
    formSection.classList.add("hidden");
    contactForm.reset();
}

document.getElementById("cancelBtn").addEventListener("click", closeForm);

contactForm.addEventListener("submit", async event => {
    event.preventDefault();

    const newContact = {
        name: document.getElementById("name").value.trim(),
        email: document.getElementById("email").value.trim(),
        phone: document.getElementById("phone").value.trim()
    };

    if (!newContact.name || !newContact.email || !newContact.phone) {
        showMessage("Please fill in all fields.", true);
        return;
    }

    try {
        if (USE_API) {
            const response = await fetch(API_BASE_URL, {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify(newContact)
            });

            if (!response.ok) {
                throw new Error("Could not save contact.");
            }

            await loadContacts();
        } else {
            newContact.id = Date.now();
            contacts.push(newContact);
            renderContacts();
        }

        closeForm();
        showMessage(
            USE_API
                ? "Contact saved successfully."
                : "Contact added to demo data only."
        );
    } catch (error) {
        showMessage(error.message, true);
    }
});

searchInput.addEventListener("input", renderContacts);

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
    renderContacts();
}

async function initializeApp() {
    if (USE_API) {
        try {
            await loadContacts();
        } catch (error) {
            showMessage(error.message, true);
        }
    } else {
        renderContacts();
    }
}

initializeApp();