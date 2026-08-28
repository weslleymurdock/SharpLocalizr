# Architecture

## Purpose and boundaries

SharpLocalizr is a .NET 10 application organized as a DDD-oriented layered solution. The host (`Localizr`) contains HTTP and Razor/Blazor presentation concerns; `Localizr.Application` contains use cases, requests, commands/queries, handlers, validators and application contracts; `Localizr.Domain` contains domain entities and rules; `Localizr.Infrastructure` contains persistence and external integrations; and `Localizr.Composition` owns dependency-injection composition.

The architecture must preserve these boundaries as new localization providers and resource formats are introduced. Provider SDKs, HTTP clients, authentication, provider-specific capability discovery, provider usage/quota semantics and provider-specific persistence/configuration details belong in Infrastructure. Application contracts must express localization intent without naming a concrete provider.

## Versioning

SharpLocalizr uses a four-component version in the form `A.B.C.D`.

| Component | Meaning |
| :--- | :--- |
| A | Stable Release |
| B | Release Candidate |
| C | Release of the current phase |
| D | Phase Number |

For example, `1.2.3.4` means Stable Release `1`, Release Candidate `2`, Phase Release `3`, Phase `4`.

The four components are **not** conventional SemVer `major.minor.patch` values. Agents and contributors must interpret them according to this project-specific model:

- **A — Stable Release:** identifies the stable product release generation. Increment A when the project starts a new stable release generation according to the release process. A stable release resets the release-candidate and phase-release progression as defined by the release plan; do not reinterpret A as an ordinary breaking-change counter.
- **B — Release Candidate:** identifies the release-candidate iteration within the current stable release generation. Increment B when a new release candidate is produced. Do not use B to represent a phase.
- **C — Phase Release:** identifies the release within the current implementation phase. Increment C when a new release is produced without moving to a different phase. C is therefore the release counter for the phase currently represented by D.
- **D — Phase Number:** identifies the implementation phase. Increment D when the project moves to a new phase. D is deliberately historical: once a phase number has been used, its meaning must never be reassigned or overwritten.

### Phase history

Every new phase must be recorded in a persistent version/phase history, including the phase number, its scope and the version range/release associated with it. The history is append-only from an architectural perspective: future agents may add new phase entries but must not rewrite an existing phase number to describe different work.

When a new phase begins, increment **D** and keep the new phase's releases distinguishable through **C**. Within that phase, **C** may advance for subsequent phase releases. **B** advances for release candidates and **A** advances for stable release generations. The exact release sequence is controlled by the project's release plan, but no component may be repurposed to mean another component's concept.

Agents must read the phase history before changing a version. They must identify the current phase from D, preserve all previous phase meanings, and update the history when introducing a new phase. A version such as `1.2.3.4` must therefore always be interpreted as Stable Release 1, Release Candidate 2, Phase Release 3, Phase 4—not as SemVer 1.2.3 with an unrelated fourth value.

## Request flow

`[Razor page || HTTP Controller](Request DTO) -> Mediator Command/Query -> Handler -> Feature Service -> Domain/Repository or external provider -> Service result -> Handler response -> [Controller response || UI]`

A Razor page may use `IMediator` directly when running in the same trusted ASP.NET process. For a public API boundary, prefer the controller route. Controllers translate transport DTOs into application messages; handlers remain thin orchestration components; feature services own use-case implementation.

## Provider-agnostic localization architecture

Localization is modeled as an application capability, not as a Google, Azure, DeepL, Amazon or Ollama feature. The Application layer must depend only on provider-agnostic contracts such as the translation service and provider capability/usage contracts. The selected provider is resolved by composition and implemented behind the Application boundary.

The application flow must remain compatible with the existing `Controller -> Request -> Command/Query -> Handler -> Service -> Provider` structure. A handler must not branch on provider SDK types or contain provider-specific HTTP/authentication logic.

### Interchangeable providers

Providers are interchangeable implementations of the same application-facing localization contract. The initial Google implementation and the future Azure AI Translator implementation are concrete Infrastructure concerns. The architecture must also permit future providers such as Azure AI Translator, Ollama models, DeepL API Free and Amazon Translate without changing the translation orchestration merely because a provider is added.

Provider selection is configuration/use-case policy. It must not require Application or UI code to reference provider SDK classes. Adding a provider should primarily add an Infrastructure adapter, capability metadata and composition/configuration rather than a new application workflow.

### Provider capabilities

A provider exposes capabilities that describe what it can actually perform. Capabilities may include, at minimum:

- supported target cultures;
- supported resource formats;
- batching or request-size constraints;
- authentication/configuration requirements;
- translation usage semantics;
- other provider-specific features required by a caller.

Capabilities are descriptive contracts at the Application boundary. Their provider-specific discovery and mapping belong in Infrastructure.

The UI must obtain target cultures from the selected provider's supported-culture capability rather than accepting an arbitrary culture string when provider validation is required. Application validation must reject unsupported provider/culture combinations before invoking provider I/O.

### Supported cultures

Cultures are represented using valid BCP-47/.NET culture identifiers such as `pt-BR` and `en-US`. A provider may support only a subset of cultures or may expose provider-specific culture metadata. The application must not assume that every provider supports every culture.

Culture discovery is therefore a provider capability query. The provider adapter translates its native culture catalog into the neutral application representation; the UI consumes that representation without knowing how the provider obtains it.

### Supported resource formats

Resource parsing and serialization are independent from translation providers. The application operates on a format-neutral resource representation, preserving keys and translating only localizable values. Format adapters belong outside the provider implementation so JSON, `.resx`, XML and future formats can be added without coupling them to Azure or another provider.

A provider may declare that it supports only particular resource formats or translation modes. Such restrictions are expressed through capabilities rather than provider-specific checks in controllers, handlers or Razor components.

## Translation usage and quota semantics

**Translation usage** means the measurable amount of translation work performed through a provider, associated with the relevant provider, credential/API key and user where that information is available. Usage is not synonymous with remaining quota.

The application must not model provider usage as a universal `remaining monthly allowance`. Providers differ in billing, quota, rate-limit and metering semantics, and some do not expose an authoritative remaining allowance through the credentials used by the application.

The Application layer may expose a provider-agnostic usage contract describing observed/available translation usage. Infrastructure is responsible for mapping provider-specific counters, billing APIs, response metadata or local measurements into that contract. Provider-specific quota and remaining-allowance semantics remain entirely behind the Infrastructure boundary.

This separation allows one provider to report characters translated, another to report requests/tokens, and another to expose no authoritative remaining quota without forcing all providers into an incorrect common model. UI code must present the usage information supplied by the selected provider and must not derive a universal remaining allowance from assumptions about another provider.

## Resource representation

Use `Dictionary<string,string>` for the neutral resource entries. A `HashSet<string>` cannot represent the required key/value association. Preserve resource keys exactly and apply translation only to values. Format adapters are responsible for preserving format-specific structure and non-translatable metadata where applicable.

For uploads, transport concerns such as `IFormFile`, request-size limits and content validation remain at the host/application boundary. They must not leak into provider contracts.

## Secrets and configuration boundaries

Provider credentials must never be committed to source control and must never be persisted as raw plaintext. The per-user provider configuration model is represented by an `AggregateConfig` aggregate with a **1:1 relationship to `User` through a foreign key**. `AggregateConfig` is the future persistence boundary for configuration/API-key material for all supported providers while remaining extensible as providers are added.

Persisted credential values must be encrypted before they reach SQL persistence. The encryption mechanism must be exposed through an explicit application/infrastructure service abstraction rather than coupling Domain entities to Azure SDK types. Decryption must be tightly bounded and must never expose plaintext credentials through API responses, DTOs, logs, caches or unintended EF Core change tracking/projections.

The encryption key is obtained from **Azure App Configuration**. The Azure App Configuration connection/configuration value is supplied to dependency injection through an environment variable and must never itself be persisted by the application. Runtime configuration changes may cause the application's `IConfigurationRoot` to reload, but configuration reload does not authorize persistence of the Azure connection value.

### Azure App Configuration and Key Vault

Future Azure App Configuration integration is the application configuration source for the encryption material and runtime configuration model. DI initializes the configuration provider from environment-provided connection/configuration data. A future settings workflow may update the external configuration source and reload `IConfigurationRoot` so `IOptionsMonitor<T>`-based consumers observe changes without an application restart.

Azure Key Vault is the future secret-management boundary for Azure resources and related deployment secrets. Azure SDK types and authentication details remain in Infrastructure. User-facing configuration contracts must expose only safe metadata or masked state; they must never return decrypted secrets.

The architecture deliberately does **not** prescribe the concrete Azure App Configuration, Key Vault, encryption, or `AggregateConfig` implementation in this documentation task. Those belong to the subsequent secure-provider-configuration work.

## Deployment database

The deployment target is **PostgreSQL**, replacing SQL Server for the Render deployment target. EF Core mappings and migrations must remain compatible with PostgreSQL, and deployment connection settings must come from environment-provided configuration without committed credentials.

Render is the target deployment platform for the production/deployment database. This task establishes the architectural decision only; the PostgreSQL provider, migrations and deployment configuration are implemented by the dedicated database task.

## Future provider evolution

Future agents adding a provider must:

1. implement the provider behind the existing provider-agnostic Application contracts;
2. expose its supported cultures and resource formats through capabilities;
3. keep provider authentication, SDK/client types, HTTP calls, batching, limits, errors and usage/quota interpretation in Infrastructure;
4. report translation usage without converting it into a provider-independent remaining-quota value;
5. preserve cancellation through Application, service, provider and persistence/configuration operations;
6. keep resource format adapters independent from provider implementations;
7. use the per-user `AggregateConfig` boundary for persisted provider configuration when that feature is implemented;
8. keep credentials encrypted and out of API responses, logs, caches and source control;
9. preserve the existing DDD/layered boundaries and avoid adding provider-specific dependencies to Domain or Application.

No future provider should require a new application-layer abstraction solely because its vendor is different unless it introduces a genuinely different application capability that cannot be expressed by the established provider-neutral contract.

## Translation concerns

Translation is external I/O and may have quotas, rate limits and transient failures. Use asynchronous APIs, propagate cancellation, use bounded batching where supported, retry only where safe, and avoid translating keys. Do not assume one provider request per dictionary entry is scalable. Provider-specific reliability, throttling and usage policies belong in Infrastructure.

## Deliberate non-goals of this architecture task

This document establishes contracts and architectural decisions; it does not implement Azure AI Translator, PostgreSQL, provider settings persistence, Azure App Configuration/Key Vault integration, new resource-format adapters or the new localization UI. Those changes belong to their dedicated feature tasks and must conform to the boundaries documented here.
