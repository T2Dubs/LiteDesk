# 📞 LiteDesk - Lightweight Client & Call Tracking

**LiteDesk** is a simple and modern desktop application for tracking clients and logging interactions. Built for clarity and long-term stability, LiteDesk is ideal for teams who want a fast, reliable solution without cloud dependency.

---

## 🚀 Features

- Add, view, and manage client profiles.
- Record call logs with date, contact method, and summary.
- View call logs grouped by **year and month**.
- Lightweight WPF interface, dark-mode optimized.
- Supports multiple databases:
  - SQLite (default)
  - SQL Server
  - PostgreSQL
  - MySQL
- Easy-to-use installer with GUI.
- Inline duplicate detection for client names and phone numbers.
- Deactivation flow with confirmation prompt.

---

## 🛠️ Installation

1. Download the latest `LiteDeskInstaller.exe` from the [Output page](https://github.com/T2Dubs/LiteDesk/Output).
2. Run the installer.
3. Choose your database type.
4. (Optional) Enter a connection string (defaults are pre-configured for SQLite).
5. Finish installation and launch LiteDesk.

----
## ⚙️ Configuration

LiteDesk reads its database configuration from `appsettings.json`:
```json
{
  "Database": {
    "Type": "Sqlite",
    "ConnectionString": "Data Source=LiteDesk.db"
  }
}
```
Manually edit this file if you need to:
 - Switch database types. Supported types are:
   - Sqlite
   - SqlServer
   - Postgres
   - MySql
 - Update connection strings.

----
## 📚 Using LiteDesk

➕ Adding a Client
 - Click "Add Client" to open the slide-in panel.
 - Fill in the fields. Optionally add address and notes.
 - Click Save.

  ⚠️ Duplicate Detection
   - If a client name or phone number already exists, LiteDesk will show an inline warning.

🔍 Editing a Client
 - Double-click a client to view and edit details.

🧼 Deactivating a Client
 - Click "Deactivate Client" from the edit panel.
 - Confirm by typing the client’s name.
 - Deactivated clients will no longer appear in the list.

☎️ Logging a Call
 - Select a client.
 - Use the Add New Call Log section.
 - Choose contact method, set date, write a short summary.
 - Click Add Call Log.

🔍 Viewing Logs
 - Call logs are grouped by year and month.
 - Click to expand/collapse.
 - Double-click a call log to view more details.

----
## 📄 License
LiteDesk is licensed under the MIT License.
