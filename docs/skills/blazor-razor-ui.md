# Skill: Blazor/Razor UI

## Rules
- Keep UI-specific code in `src/Localizr`.
- Use the existing MudBlazor version and conventions already present in the host.
- For a file-based localization workflow, provide clear source-file selection, target-culture selection, validation/error feedback, progress state and translated output/download behavior.
- A page may inject `IMediator` and execute a command directly for an in-process workflow. Prefer HTTP controllers when testing or consuming the same public API contract is a requirement.
- Do not place Google Translate calls, repository access or Domain entities in Razor components.
- Never block the UI thread on translation I/O; use asynchronous operations and cancellation where the component lifecycle permits it.
- Avoid retaining uploaded file contents longer than necessary.