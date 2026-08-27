# SharpLocalizr Agent Guide

## Purpose
SharpLocalizr is a .NET 10 application for localizing resource data. The current solution is organized around Domain, Application, Infrastructure, Composition, and the ASP.NET/Blazor host under `src/`. Preserve this architecture when implementing localization features.

## Repository structure
- `src/Localizr.Domain`: domain entities and domain rules. Keep infrastructure concerns out.
- `src/Localizr.Application`: feature abstractions, Requests, Commands/Queries, handlers, validators, common responses, repositories and unit-of-work abstractions.
- `src/Localizr.Infrastructure`: concrete services, persistence and external integrations.
- `src/Localizr.Composition`: dependency-injection/composition extensions.
- `src/Localizr`: ASP.NET Core host, controllers and Razor/Blazor UI.

The existing Identity flow is the reference implementation: controllers receive feature Requests, construct Mediator messages, handlers call an application abstraction/service, and the service owns the feature operation. The current controller follows this pattern directly, and the handler delegates to `IIdentityService`. Do not introduce a parallel CQRS style without a strong reason.

## Architectural rules
1. HTTP input belongs in a feature `Requests` DTO; do not expose domain entities from controllers.
2. Controllers translate Requests into `Command`/`Query` objects and call `IMediator.Send`.
3. Command/query handlers are thin orchestration components. They validate through the existing pipeline and call the feature service.
4. Feature services contain the use-case implementation. If persistence is needed, inject `IRepository<T>` (and `IUnitOfWork` when appropriate) and work with Domain entities.
5. Infrastructure implements Application abstractions. External providers such as Google Translate belong behind an Application abstraction and are implemented in Infrastructure.
6. Responses should follow the existing `Response<T>`/feature-response conventions instead of inventing a second response envelope.
7. Use `CancellationToken` end-to-end for asynchronous I/O.
8. Keep Blazor/Razor UI concerns in the host. A page may call Mediator directly when it is a trusted in-process UI, or call the HTTP controller when the page is intended to behave like an API client. Pick one deliberately and document it.
9. Never put Google credentials, API keys, or provider-specific secrets in Domain/Application or source-controlled configuration.
10. Keep the initial JSON implementation extensible: parsing and writing must be isolated so future .NET `.resx`, Java, and other resource formats can be added without changing translation orchestration.

## Localization feature direction
The first vertical slice should accept a neutral JSON resource represented as `Dictionary<string,string>`, a target culture, translate values while preserving keys, and return the translated dictionary. A `HashSet` is not a replacement for a dictionary because key-to-value association is fundamental to the use case. Use `Dictionary<string,string>` unless profiling demonstrates a different representation is required.

Prefer an abstraction such as `ITranslatorService` for resource translation/read/write responsibilities. The concrete Google Translate integration should live in Infrastructure and be replaceable. Avoid scraping the public Google Translate web page with HTML parsing for the production implementation; use an official supported translation API/client and isolate provider-specific behavior.

## Quality gates
Before considering a feature complete, build the solution, run the relevant tests, and add unit tests for validators, handlers and service behavior. Add integration tests for provider/persistence boundaries when practical. Do not silently change unrelated files.
