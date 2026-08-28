# Localization Implementation Brief

## Phase 1: JSON translation
Implement one complete vertical slice for translating a neutral JSON resource into a target culture.

### Contract
Request: source resource entries plus target culture. The transport may be JSON or multipart upload, but the Application layer must not depend on ASP.NET file types.

Command: `TranslateResourceCommand` containing the neutral entries and target culture.

Result: a response containing the translated `Dictionary<string,string>` and, if useful, source/target culture metadata.

### Application
Create the feature under a `Localization` (or equivalent) vertical folder. Add Request, Command, Response, Validator, service abstraction and handler following the existing Identity organization. The handler should do little more than call `ITranslatorService.TranslateToCultureAsync(...)` (prefer async naming for external I/O).

### Infrastructure
Implement the translator service using the official Google Cloud Translation API/client. Configure credentials and project/location through options/configuration. Keep the Google integration behind the service abstraction. If the service also reads/writes resource formats, introduce separate format reader/writer abstractions so Google translation is not responsible for parsing `.resx`, Java or JSON.

### Host
Add a versioned controller matching the current controller conventions. Bind the Request DTO, send the command with `IMediator`, and map the result to the existing response/status-code style. Add a Razor page for upload/input and culture selection. The page uses Mediator directly because it is a trusted in-process Blazor Server UI; the controller remains available as the HTTP boundary.

## Provider settings

Provider configuration is implemented as a second localization vertical slice so the application can evolve from one translation provider to multiple providers without creating a separate settings surface for each provider.

### Contract

Provider settings are exposed through `ILocalizationSettingsService` using provider-neutral operations for reading, updating, and querying usage. Commands and queries carry a provider identifier, while provider-specific implementation remains in Infrastructure.

### Application

`UpdateLocalizationProviderSettingsCommand`, `GetLocalizationProviderSettingsQuery`, and `GetLocalizationProviderUsageQuery` are handled by `LocalizationSettingsHandlers`. Controllers and Razor components do not perform provider-specific work.

### Infrastructure

`LocalizationSettingsService` currently supports Google Cloud Translation. It updates the server configuration, reloads the configuration root, and relies on `IOptionsMonitor<GoogleTranslateOptions>` so `TranslatorService` observes an updated API key without an application restart.

The API key is returned to the UI only in masked form. Credentials must never be committed to source control.

The initial usage implementation tracks characters submitted by the current application instance and calculates the estimated remaining monthly free character allowance. This is intentionally distinguished from authoritative Google Cloud Billing or project-wide quota data: API-key-only Cloud Translation Basic calls do not provide the application's billing balance, and authoritative billing/quota APIs require separate authenticated access. A future provider settings implementation can add that authenticated integration without changing the Application contract.

### Host

`LocalizationSettingsController` centralizes HTTP operations for all providers. The `/settings` Razor page uses MudBlazor tabs, with one tab per provider. Google Cloud Translation is the first provider tab; additional providers should add their own tab/configuration implementation while reusing the centralized settings workflow.

## Important design choices
- Keep `Dictionary<string,string>`. A `HashSet` cannot encode the required key/value association and offers no useful optimization here.
- Do not persist translations in Phase 1 unless there is a concrete requirement. The translation operation is naturally stateless.
- Provider settings are configuration state rather than Domain entities in this phase. If persistence is later required, inject the generic repository into the feature service and persist Domain entities; do not put repository calls in handlers/controllers.
- Treat culture as a validated BCP-47/.NET culture identifier such as `pt-BR`, `en-US`, etc. Validate it before provider calls.
- Preserve keys exactly and translate only values.
- Keep provider-specific configuration and usage concerns in Infrastructure.

## Phase 2: resource format abstraction
Introduce a format-neutral reader/writer contract. Implement JSON first, then `.resx` and Java formats as separate adapters. The controller/page selects or infers the format; the application translation use case remains format-neutral.

## Phase 3: scalable jobs
For large files, replace synchronous request/response translation with a job model: create job, persist status, process in background, expose status/progress, and produce downloadable output. Add batching, rate limiting, quota handling and retry policy at the Infrastructure/provider boundary.

## Phase 4: provider abstraction
If additional providers are desired, make the provider abstraction explicit and select implementations through configuration/strategy rather than branching on Google-specific logic in the feature service.
