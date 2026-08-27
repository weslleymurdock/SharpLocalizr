# Architecture

## Request flow
`Razor page -> HTTP Controller (Request DTO) -> Mediator Command/Query -> Handler -> Feature Service -> Domain/Repository or external provider -> Service result -> Handler response -> Controller response -> UI`

A Razor page can alternatively use `IMediator` directly when running in the same trusted ASP.NET process. For a public API boundary, prefer the controller route.

## Localization vertical slice
The first use case can be modeled as `TranslateResourceCommand`, containing the neutral resource entries and target culture, with a result containing the translated entries. The handler should only delegate to `ITranslatorService`.

`ITranslatorService` should express application-level intent, for example translating a resource and, where required, reading/writing resource formats. The Google-specific client, authentication and HTTP calls belong in Infrastructure.

## Resource formats
Do not make JSON parsing part of the Google provider. Introduce a format abstraction when more formats arrive, e.g. a resource reader/writer contract with a JSON implementation first and `.resx`/Java implementations later. The domain/use-case should operate on a format-neutral model.

## Data representation
Use `Dictionary<string,string>` for resource entries. A `HashSet<string>` cannot represent the key/value relationship and would require a secondary lookup structure, defeating its purpose. Preserve keys exactly; translation applies to values only. Decide explicitly how null, empty and duplicate JSON keys are handled and test those rules.

## API/UI considerations
For file uploads, avoid binding an arbitrarily large uploaded file directly to an unbounded dictionary. Apply request/file size limits, validate JSON, enforce a reasonable entry count/value length, and return actionable validation errors. For a first implementation, a JSON request body is simpler than multipart upload; if the UI uploads a file, parse it at the HTTP boundary into the request model while keeping the application command format independent of `IFormFile`.

## Translation concerns
Translation is external I/O and may have quotas, rate limits and transient failures. Use asynchronous APIs, cancellation, bounded batching where supported, retry only where safe, and avoid translating keys. Do not assume one provider request per dictionary entry is scalable. The service should preserve input order only if the output contract needs ordering; dictionary semantics themselves do not guarantee a meaningful presentation order.
