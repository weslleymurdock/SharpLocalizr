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
Implement the translator service using the official Google Cloud Translation API/client. Configure credentials and project/location through options/configuration. Keep the Google SDK behind the service abstraction. If the service also reads/writes resource formats, introduce separate format reader/writer abstractions so Google translation is not responsible for parsing `.resx`, Java or JSON.

### Host
Add a versioned controller matching the current controller conventions. Bind the Request DTO, send the command with `IMediator`, and map the result to the existing response/status-code style. Add a Razor page for upload/input and culture selection. The page can call the controller through HTTP or use Mediator directly; use the former if the API is intended as the stable public boundary.

## Important design choices
- Keep `Dictionary<string,string>`. A `HashSet` cannot encode the required key/value association and offers no useful optimization here.
- Do not persist anything in Phase 1 unless there is a concrete requirement. The translation operation is naturally stateless.
- If persistence is later required (projects, translation jobs, provider usage, cached translations), inject the generic repository into the feature service and persist Domain entities; do not put repository calls in handlers/controllers.
- Treat culture as a validated BCP-47/.NET culture identifier such as `pt-BR`, `en-US`, etc. Validate it before provider calls.
- Preserve keys exactly and translate only values.
- Consider a result model that can report entries that were skipped/failed rather than losing the entire batch on one provider issue, if partial translation is a desired feature.

## Phase 2: resource format abstraction
Introduce a format-neutral reader/writer contract. Implement JSON first, then `.resx` and Java formats as separate adapters. The controller/page selects or infers the format; the application translation use case remains format-neutral.

## Phase 3: scalable jobs
For large files, replace synchronous request/response translation with a job model: create job, persist status, process in background, expose status/progress, and produce downloadable output. Add batching, rate limiting, quota handling and retry policy at the Infrastructure/provider boundary.

## Phase 4: provider abstraction
If additional providers are desired, make the provider abstraction explicit and select implementations through configuration/strategy rather than branching on Google-specific logic in the feature service.
