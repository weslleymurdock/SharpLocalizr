# Skill: Resource Formats

## Goal
Keep localization independent from a particular file format.

## Rules
- Represent a neutral resource as key/value entries; use `Dictionary<string,string>` for the initial JSON implementation.
- A `HashSet` is inappropriate because the operation needs a unique key mapped to a value.
- Isolate parsing and serialization behind an Application abstraction when format support is introduced.
- JSON is the first adapter. Future `.resx`, Java properties/XML and other formats must be adapters, not branches inside the translation service.
- Validate duplicate keys, malformed JSON, empty keys and unsupported/null values explicitly.
- Preserve keys byte-for-byte/semantically exactly as supplied; only values are translated.
- Do not couple the format abstraction to `IFormFile`, ASP.NET, Blazor or a provider SDK.

## Suggested conceptual model
Use a format-neutral resource document/entry abstraction if requirements grow beyond a dictionary. Keep the first implementation simple rather than prematurely creating a complex domain model.