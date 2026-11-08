# Emergency Health System

An ASP.NET Core MVC prototype for ambulance requests, patient booking history, and MOH fleet/booking administration. The app uses ASP.NET Identity and a local SQLite database. Patients can optionally add a phone number and cancel requests that are still pending and unassigned. Tracking in this prototype uses booking status and ETA updates, not live GPS or emergency dispatch.

## Run locally

From this directory:

```powershell
dotnet run
```

The app applies its EF Core migrations at startup and creates `emergency-health.db` in the app content directory.

## Create the first MOH administrator

Set administrator credentials using .NET User Secrets (never commit passwords to `appsettings.json` or source control). Replace the example values with your own:

```powershell
dotnet user-secrets set "Admin:Email" "admin@example.com" --project ".\Emergency_Health_System.csproj"
dotnet user-secrets set "Admin:Password" "ChooseYourOwnStrongPassword2026" --project ".\Emergency_Health_System.csproj"
dotnet run
```

After signing in, use **MOH admin** in the navigation to manage ambulances, review requests, assign compatible available vehicles, and update request status and ETA. Admin request views show the account's registered name, email, and optional phone number for coordination. Status changes follow the workflow Pending → Confirmed → Dispatched → InTransit → Arrived → Complete; cancellation remains available before completion. A vehicle must be assigned before a request can advance beyond Pending.

Signed-in patients can open **Track** on their requests to see status, ETA, and staff-reported location updates. Tracking is updated by staff and does not provide live GPS.

## End-to-end smoke test

1. Register a patient account, optionally add a contact phone number, and submit a Road or Air request.
2. Confirm a pending request can be cancelled by its owner, but not by another account.
3. Sign in with the configured administrator and open **MOH admin**.
4. Add an ambulance with the same transport type as the request, then assign it.
5. Advance the booking through the allowed status steps and update the ETA. Confirm invalid status jumps are rejected and the patient sees updates on **Track**.
6. Set the request to Complete and confirm the ambulance is available again.

For deployment, supply `Admin__Email` and `Admin__Password` through the hosting platform's secret configuration instead of User Secrets. This prototype is not a real emergency dispatch service.
