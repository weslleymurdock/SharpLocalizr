# Architecture

## Versioning

SharpLocalizr uses a four-component version in the form `A.B.C.D`.

| Component | Meaning |
|---|---|
| A | Stable Release |
| B | Release Candidate |
| C | Release of the current phase |
| D | Phase number |

For example, `1.2.3.4` means Stable Release `1`, Release Candidate `2`, Phase Release `3`, Phase `4`.

The phase number is intentionally part of the product version. When a new implementation phase is introduced, increment `D` and document the phase in the version history. Phase releases increment `C`; release candidates increment `B`; stable releases increment `A` according to the project's release process. Agents must preserve this scheme and must not reinterpret the four components as a conventional SemVer `major.minor.patch` triplet.

### Phase history

Maintain a historical record of phase numbers and their scope as phases are added. Do not overwrite the meaning of an existing phase number.

## Request flow
`Razor page -> HTTP Controller (Request DTO) -> Mediator Command/Query -> Handler -> Feature Service -> Domain/Repository or external provider -> Service result -> Handler response -> Controller response -> UI`

A Razor page can alternatively use `IMediator` directly when running in the same trusted ASP.NET process. For a public API boundary, prefer the controller route.

## Localization vertical slice
The first use case can be modeled as `TranslateResourceCommand`, containing the neutral resource entries and target culture, with a result containing the translated entries. The handler should only delegate to `ITranslatorService`.

`ITranslatorService` should express application-level intent. Translation providers must remain replaceable behind provider-agnostic Application contracts. Provider-specific clients, authentication, HTTP calls, capabilities, supported cultures, formats and usage semantics belong in Infrastructure.

## Translation provider capabilities

Providers may differ in supported cultures, resource formats, batching, authentication and quota/billing semantics. Do not expose a universal provider-independent "remaining usage" calculation. The Application layer should consume provider-agnostic capability and usage contracts; Infrastructure translates those contracts to provider-specific APIs.

The UI must obtain target cultures from the selected provider's supported-culture capability instead of accepting arbitrary culture strings when provider validation is required.

## Resource formats
Do not make JSON parsing part of a translation provider. Use a format abstraction with independent adapters for JSON, `.resx`, XML and future formats. The domain/use-case should operate on a format-neutral representation.

## Data representation
Use `Dictionary<string,string>` for resource entries. A `HashSet<string>` cannot represent the key/value relationship and would require a secondary lookup structure, defeating its purpose. Preserve keys exactly; translation applies to values only. Decide explicitly how null, empty and duplicate JSON keys are handled and test those rules.

## API/UI considerations
For file uploads, avoid binding an arbitrarily large uploaded file directly to an unbounded dictionary. Apply request/file size limits, validate the resource format, enforce a reasonable entry count/value length, and return actionable validation errors. Parse uploaded files at the HTTP/UI boundary into application-level resource models; do not leak `IFormFile` or UI concerns into application contracts.

## Secrets and configuration

Provider credentials must never be committed and must never be persisted as raw text. Per-user provider configuration belongs to an `AggregateConfig` aggregate with a 1:1 relationship to `User` through a foreign key.

Persisted provider API keys are encrypted before storage. The encryption key is supplied through Azure App Configuration and is not stored in SQL. The Azure configuration/connection value is supplied through an environment variable during dependency-injection setup and must not itself be persisted by the application.

Azure Key Vault may be used as the secret-management boundary for Azure resources. Keep Azure SDK details in Infrastructure. Do not return decrypted secrets through API responses, logging, caching or UI models. Runtime configuration changes may reload the application's `IConfigurationRoot`; reloading configuration must not imply persistence of the Azure connection value.

## Deployment database

The deployment target uses PostgreSQL rather than SQL Server. EF Core mappings and migrations must remain provider-compatible, and deployment configuration must use environment-provided PostgreSQL connection settings. Render is the target deployment platform for this database configuration.

## Translation concerns
Translation is external I/O and may have quotas, rate limits and transient failures. Use asynchronous APIs, cancellation, bounded batching where supported, retry only where safe, and avoid translating keys. Do not assume one provider request per dictionary entry is scalable. The service should preserve input order only if the output contract needs ordering; dictionary semantics themselves do not guarantee a meaningful presentation order.
