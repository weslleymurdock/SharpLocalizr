# Skill: External Translation Provider

## Goal
Implement translation through a replaceable provider without leaking provider details into Application or Domain.

## Rules
- Define the provider-facing capability through an Application abstraction such as `ITranslatorService`.
- Implement Google Translate integration in Infrastructure.
- Prefer an official supported Google Cloud Translation API/client rather than scraping the public Google Translate website or parsing HTML with HTMLAgilityPack.
- Keep credentials in the host's supported configuration/secret mechanism; never commit secrets.
- Make translation asynchronous and cancellation-aware.
- Batch entries where the provider API supports it and respect provider limits/quotas.
- Do not translate resource keys.
- Handle transient provider failures deliberately; do not add unbounded retries.
- Map provider-specific errors to application-level failures so controllers/UI do not depend on Google exception types.
- Make the provider implementation independently testable by keeping HTTP/client creation injectable.
