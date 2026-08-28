# Skill: Testing

## Rules
- Maintain at least 80% line and branch coverage for the application under test; track method coverage when the selected tool reports it.
- Test every meaningful business-rule case: success, invalid input, boundary conditions, expected failures, external-provider failures, and cancellation where the implementation performs asynchronous I/O.
- Test validators for malformed requests and boundary conditions.
- Test handlers as orchestration: verify the expected service method receives command/query data and the exact `CancellationToken`, and verify both success and failure mapping.
- Test feature services with deterministic mocks/fakes for repositories, identity infrastructure and external providers.
- Test resource adapters independently, including malformed JSON, duplicate keys and round-trip serialization where applicable.
- Test provider adapters at the HTTP/client boundary using deterministic fakes or a test server; do not require live Google credentials in normal unit tests.
- Prefer `async`/`await` in tests for asynchronous APIs and propagate cancellation tokens into the operation under test.
- The .NET 10 unit-test project runs through Microsoft.Testing.Platform (MTP) using `dotnet test` and xUnit v3.
- MTP is not compatible with the VSTest-only `coverlet.msbuild` or `coverlet.collector` integrations. Use `coverlet.MTP` for coverage and `Microsoft.Testing.Extensions.TrxReport` for TRX reports when the project runs natively on MTP.
- Keep the GitHub Actions summary structure stable. Adapt only the runner-specific commands and report file discovery required by MTP.
- If build/test commands are unavailable in the agent environment, inspect the workflow for the current branch and use its step summary and uploaded artifacts to validate test counts and coverage.
- Never reduce coverage scope or thresholds merely to make a test pass. Add tests or correct the implementation instead.
- Run the full solution build and relevant test projects before declaring completion.
