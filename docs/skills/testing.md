# Skill: Testing

## Rules
- Test validators for malformed/invalid requests and boundary conditions.
- Test handlers as orchestration: verify the expected service method receives the command/query data and cancellation token, and that its result is returned.
- Test localization services with representative dictionaries and verify keys are unchanged while values are translated.
- Test format adapters independently, including malformed JSON, duplicate keys and round-trip serialization where applicable.
- Test provider adapters at the HTTP/client boundary using deterministic fakes or a test server; do not require live Google credentials in normal unit tests.
- Include cancellation and provider failure behavior where the service performs external I/O.
- Run the full solution build and relevant test projects before declaring completion.