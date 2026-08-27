# Agent Workflow

1. Read `AGENTS.md` and the relevant files under `docs/` before modifying code.
2. Identify the feature vertical slice and inspect an existing feature with the same architectural role before creating new conventions.
3. Keep Request DTOs, Commands/Queries, handlers, services, responses and abstractions in their established Application locations.
4. Put concrete provider, HTTP, persistence and serialization implementations in Infrastructure unless the host is explicitly responsible for them.
5. Register new services through the existing Composition extensions rather than scattering registrations through feature code.
6. For UI work, inspect the current Razor/Blazor structure and MudBlazor conventions before adding components.
7. Add or update tests with the implementation. Prefer deterministic unit tests with mocked Application abstractions and separate integration tests for external providers.
8. Build and test after changes. Fix warnings/errors introduced by the feature before completion.
9. Keep commits focused. Do not rewrite unrelated architecture merely because a different pattern is personally preferred.
10. When an external SDK/API is required, consult its current official documentation and verify package/API versions before coding against it.
