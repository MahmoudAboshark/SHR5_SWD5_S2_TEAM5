# SHR5_SWD5_S2_TEAM5

# Contact Book – Frontend Development

## Overview

Contact Book is a web app for managing contacts entirely in the browser, with no server or build step required. It supports adding, editing, searching, categories, favorites, a trash bin, and dark mode.

## Features

- **Contacts page:** View contacts as a table (List) or cards (Grid).
- **Add / Edit form:** A dialog form with validation:
  - Name: 2 to 100 characters.
  - Phone: Egyptian format, 11 digits starting with 010, 011, 012, or 015.
  - Email: must be a valid email address.
- **Search:** Instant search by name, email, or phone, with matching text highlighted. Press `/` to focus the search box.
- **Tabs:** All, Favorites, Family, Work, Friends, and Trash, each showing its count.
- **Favorites:** Star any contact; favorites are listed first.
- **Details:** A dialog showing name, email, phone, and date added, with Copy, Favorite, Edit, and Delete actions.
- **Copy & links:** Copy email or phone, plus `mailto:` and `tel:` links.
- **Trash:** Deleting moves a contact to Trash, where you can restore it or delete it permanently. Every delete asks for confirmation first.
- **Dark mode:** Toggle light/dark theme; your choice is saved.
- **Responsive & Arabic-ready:** Works on mobile and desktop, and Arabic text is automatically aligned right-to-left.

## Tech Stack

HTML5 · CSS3 · Vanilla JavaScript

## Project Structure

```
ContactBook/
├── index.html    # Page structure, form, and dialogs
├── style.css     # Styles, dark mode, and animations
├── script.js     # Application logic
└── fonts/        # Fonts (Poppins, Fraunces, Cairo)
```

## How to Run

Unzip `ContactBook-v17.zip` and open `index.html` in your browser. No installation needed.

## Data Storage

Data is saved in the browser's `localStorage` (contacts, trash, favorites, view mode, and theme).

## API Integration

API integration is planned for a future stage, as the API is currently under development.

The code is ready for it later: at the top of `script.js` there are `USE_API` and `API_BASE_URL`. While `USE_API = false` (the default), the app runs locally using localStorage.

## Team

SHR5_SWD5_S2_TEAM5