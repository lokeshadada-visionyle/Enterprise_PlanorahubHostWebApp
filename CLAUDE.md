# Planora Enterprise Host Web App

## Project Overview
Enterprise event management platform (Planorahub) — host portal for creating, managing, and monitoring events. Built as an ASP.NET Core Razor Pages frontend that communicates with an external Azure-hosted REST API. **No local database** — all data flows through `CommonHelper.cs` to the remote API.

## Tech Stack
- **.NET 8.0** — ASP.NET Core Razor Pages (not MVC)
- **Frontend:** Vanilla JS (no framework), Bootstrap 5, jQuery, custom CSS design system
- **Session:** In-memory distributed cache with 24-hour idle timeout, HttpOnly cookies
- **API Backend:** `https://jobnyle.azurewebsites.net/WorkNyleAPI/EventMGUnifiedAPI.svc`

## Build & Run
```bash
cd Planora_EnterproseHostWebApp
dotnet restore
dotnet run            # HTTP: localhost:5244, HTTPS: localhost:7245
```
No special build steps. No external NuGet packages beyond ASP.NET Core defaults.

## Project Structure
```
Planora_EnterproseHostWebApp/
├── CommonHelper.cs              # Central API layer (~50 methods, all REST calls)
├── Program.cs                   # Startup config (session, middleware, routing)
├── Extensions/SessionExtensions.cs  # Session helpers (event type get/set)
├── Models/
│   ├── LoginModel.cs            # Auth DTOs
│   ├── CreateEventModel.cs      # Event creation DTOs (largest)
│   ├── HostModel.cs             # Dashboard/reporting DTOs (320KB)
│   └── HostDashboardModel.cs    # Dashboard response models
├── Pages/
│   ├── Shared/_Layout.cshtml    # Main app shell (sidebar + topbar)
│   ├── CreateEvent/             # 12-step event wizard + _CreateEventLayout
│   ├── Events/EventHub/         # 16+ sub-pages + _EventHubLayout
│   ├── Dashboard/               # Host dashboard
│   ├── Login/                   # Auth pages (login, reset, profile)
│   ├── Plans/, Revenue/, Roles/ # Feature pages
│   └── _ViewImports.cshtml      # Tag helpers + namespace
└── wwwroot/
    ├── assets/css/styles.css    # Custom design system (~4000 lines)
    ├── assets/js/script.js      # 74+ vanilla JS modules
    └── lib/                     # jQuery, Bootstrap, validation libs
```

## Architecture & Data Flow
```
Razor Page → PageModel.OnGet/OnPost → CommonHelper method → REST API → JSON response
```
- Pages validate session first, redirect to `/Login/Login` if unauthenticated
- API auth uses header-based credentials (VenderId/VenderPwd) + SHA256 hash tokens
- All API responses deserialized with `PropertyNameCaseInsensitive = true`

## Key Conventions

### C# / Razor Pages
- **Namespace:** `Planora_EnterproseHostWebApp.Pages.[Feature]`
- **Page models:** `[FeatureName]Model : PageModel`
- **Model suffixes:** `*Req` for request DTOs, `*Resp` for response DTOs
- **Properties:** Auto-properties with default initializers (strings = `""`, collections = `new()`)
- **Nullable:** Enabled project-wide; handle null checks
- **Implicit usings:** Enabled
- **Private methods in CommonHelper:** Underscore-prefixed (e.g., `_download_serialized_json_data<T>`)

### Session Keys
- `"UserId"` (int), `"UserName"` (string), `"UserEmail"` (string)
- `"IsLoggedIn"` (string: `"true"/"false"`)
- `"EventType"` (string: `"private"/"public"`)
- `"createdEventId"` (int)

### ViewData Keys
- `PageName`, `PageDesc` — topbar display
- `Title` — browser tab
- `StepIndex`, `StepTotal`, `StepName`, `StepHint` — wizard steps
- `EventName`, `EventId`, `SlotId` — event context

### Auth Check Pattern (every OnGet/OnPost)
```csharp
var userId = HttpContext.Session.GetInt32("UserId");
if (!userId.HasValue || userId.Value <= 0 || ...)
    return RedirectToPage("/Login/Login");
```

### API Error Handling
```csharp
public string ApiError { get; set; } = string.Empty;
// API Response: Status == 1 → success, Status == 0 → failure with Message
```

### CSS
- BEM-like naming: `.sidenav__head`, `.btn--primary`, `.field__input`
- Design tokens as CSS custom properties (colors, spacing, typography)
- Font stack: Poppins (display) + Inter (body)
- Spacing scale: 4px base (`--space-1` through `--space-16`)

### JavaScript
- Pure vanilla JS — no frameworks
- Data-attribute driven initialization (e.g., `data-sidebar`, `data-choice-cards`)
- Event delegation patterns
- `localStorage` for persistent UI state (sidebar rail mode)

## Layout Hierarchy
```
_Layout.cshtml (main shell: sidebar + topbar + content area)
├── CreateEvent/_CreateEventLayout.cshtml (wizard wrapper with step progress)
├── Events/EventHub/_EventHubLayout.cshtml (event hub with stats + sub-nav)
└── Direct usage by all other pages
```

## Important Notes
- **No database migrations** — stateless frontend; all persistence via REST API
- **Session is in-memory** — lost on app restart
- **Model files are large** — HostModel.cs is 320KB; navigate by region/class name
- **CommonHelper.cs is ~94KB** — organized with `#region` blocks by feature area
- When adding new pages, follow the existing folder-based feature organization
- When adding API methods, add them to `CommonHelper.cs` following the existing pattern with `_download_serialized_json_data<T>` (GET) or `_serialized_json_data<T>` (POST)
