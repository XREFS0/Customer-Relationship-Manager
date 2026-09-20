# Customer Relationship Manager (Enterprise CRM)

A modular Windows desktop Customer Relationship Management system developed with .NET 8, Windows Forms, and SQLite.

Repository: https://github.com/XREFS0/Customer-Relationship-Manager

---

## Overview

Customer Relationship Manager is an offline-first management suite built for operations and sales teams. It provides full customer lifecycle management, deal pipeline tracking, task scheduling, communication auditing, data analytics, and local database backup utilities.

---

## Application Screenshots

### Main Interface & Analytics

| Dashboard Overview | Sales Pipeline |
| :---: | :---: |
| ![Dashboard Overview](ScreenShot/02_Dashboard_View.png) | ![Sales Pipeline](ScreenShot/05_Sales_View.png) |

| Customer Directory | Reports & Business Intelligence |
| :---: | :---: |
| ![Customer Directory](ScreenShot/03_Customers_View.png) | ![Reports & BI](ScreenShot/07_Reports_View.png) |

### Operations & Management

| Communication & Interaction History | Task Execution & Backlog |
| :---: | :---: |
| ![Interaction History](ScreenShot/04_Contacts_View.png) | ![Tasks Execution](ScreenShot/06_Tasks_View.png) |

| System Configuration & Maintenance | Authentication |
| :---: | :---: |
| ![System Configuration](ScreenShot/08_Settings_View.png) | ![Login Screen](ScreenShot/01_Login_Screen.png) |

### Modal Dialogs & Dark Theme

| Customer Profile Dialog | Sales Opportunity Dialog |
| :---: | :---: |
| ![Customer Dialog](ScreenShot/09_Customer_Edit_Dialog.png) | ![Sale Dialog](ScreenShot/10_Sale_Edit_Dialog.png) |

| Dashboard Dark Mode | Sales Pipeline Dark Mode |
| :---: | :---: |
| ![Dashboard Dark Mode](ScreenShot/13_Dashboard_Dark_Mode.png) | ![Sales Dark Mode](ScreenShot/14_Sales_Dark_Mode.png) |

---

## Core Capabilities

- **Executive Dashboard**: Real-time sales metrics, opportunity stage breakdown charts, active leads count, and live activity audit streams.
- **Customer Directory**: Customer profiles, search and filtering by lifecycle status, full 360-degree customer detail views, and CSV export.
- **Interaction History**: Chronological logging of calls, meetings, emails, and notes linked to customer records.
- **Sales Pipeline**: Deal tracking across stages (New Lead, Contacted, Negotiation, Won, Lost), win probability calculation, and revenue forecasting.
- **Task & Activity Execution**: Priority-based task management with due dates, assignment tracking, and completion status.
- **Reporting & Business Intelligence**: Summary reports with data grid views, CSV exports, and styled standalone HTML export for reporting.
- **Security & Data Maintenance**: Role-based access (Admin / Employee), password hashing with PBKDF2/SHA256 and unique salt, transactional SQLite database backup and restoration.
- **Theme Support**: Built-in Light and Dark themes.

---

## System Architecture

```
EnterpriseCRM/
├── Data/
│   ├── Repositories/       # Data Access Objects (CRUD per entity)
│   ├── DatabaseContext.vb  # SQLite connection factory & WAL configuration
│   └── DbInitializer.vb    # Schema creation & initial seed data
├── Models/
│   ├── Entities.vb         # Domain entities (User, Customer, SaleOpportunity, etc.)
│   └── Enums.vb            # Domain enumerations (CustomerStatus, PipelineStage, etc.)
├── Services/
│   ├── AppLogger.vb        # File-based logging and exception auditing
│   ├── SecurityService.vb  # PBKDF2 password hashing & verification
│   └── Services.vb         # Application business services (Auth, Customer, Sales, Reports)
├── UI/
│   ├── Controls/           # Custom reusable controls (KPI cards, bar chart)
│   ├── Forms/              # Dialogs (Login, Customer, Sale, Task, Detail views)
│   ├── Theme/              # Theme palette management (Light & Dark modes)
│   └── Views/              # Modular user control pages for the main viewport
├── ScreenShot/             # Application screenshots
└── Program.vb              # Application bootstrap & entry point
```

---

## Technical Specifications

- **Target Framework**: .NET 8.0 Windows (`net8.0-windows`)
- **UI Platform**: Windows Forms (WinForms)
- **Database Engine**: SQLite via `Microsoft.Data.Sqlite`
- **Architecture Pattern**: Layered Repository & Service Architecture

---

## Requirements

- Windows 10 or Windows 11 (64-bit)
- .NET 8.0 SDK or Desktop Runtime

---

## Installation & Execution

### 1. Clone Repository
```bash
git clone https://github.com/XREFS0/Customer-Relationship-Manager.git
cd Customer-Relationship-Manager
```

### 2. Build Solution
```bash
dotnet build
```

### 3. Run Application
```bash
dotnet run
```

---

## Default Access Credentials

On first run, the SQLite database is automatically generated with initial seed accounts:

| Username | Password | Role |
| :--- | :--- | :--- |
| `admin` | `Admin@12345` | Administrator |
| `sara` | `Employee@123` | Employee |

Credentials and system settings can be updated directly within the **Settings** view.

---

## License & Copyright

Copyright (c) 2026 XREFS0. All rights reserved.

Licensed under the [MIT License](LICENSE).

---

### 🌐 Connect with Me

<p align="left">
  <a href="http://xrefs0.com/" target="_blank">
    <img src="https://img.icons8.com/bubbles/60/000000/domain.png" title="Website" width="45" height="45"/>
  </a>
  <a href="https://www.facebook.com/XREFS0" target="_blank">
    <img src="https://img.icons8.com/bubbles/60/000000/facebook-new.png" title="Facebook Page" width="45" height="45"/>
  </a>
  <a href="https://t.me/MrMasaOfficial" target="_blank">
    <img src="https://img.icons8.com/bubbles/60/000000/telegram-app.png" title="Telegram Contact" width="45" height="45"/>
  </a>
  <a href="https://t.me/XREFS0_CHANNEL" target="_blank">
    <img src="https://img.icons8.com/bubbles/60/000000/telegram-app.png" title="Telegram Channel" width="45" height="45"/>
  </a>
  <a href="https://www.youtube.com/@XREFS0" target="_blank">
    <img src="https://img.icons8.com/bubbles/60/000000/youtube-play.png" title="YouTube" width="45" height="45"/>
  </a>
</p>

