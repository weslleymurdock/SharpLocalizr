# 🗺️ SharpLocalizr - A Localization tool

SharpLocalizr is a blazor web app to generate string resource files for the desired culture from a neutral strings resource file as input.

## 🔬 How it Works?

It uses the Google Translate API for generates the translations. 
The usage at cloud do not require an google translate api key, but it is
paid.
The user inputs an json as a neutral string resource file, where the json
properties are the keys and the property values are the key values for a neutral culture of the user app. So the user uploads it and select the desired culture of resource to be generated, and the final format (.resx only for now).

## 🚀 Getting Started

## 🛠️ Architecture

The `/src/` directory contains the main projects using the DDD pattern,
where each project is a DDD layer, being:

```text
/src/
  ├── Localizr                  # Blazor app, Presentation Layer 
  ├── Localizr.Application/     # classlib, Application Layer
  ├── Localizr.Composition/     # classlib, DI purposes Layer
  ├── Localizr.Domain/          # classlib, Domain Layer  
  └── Localizr.Infrastructure/  # classlib, Infrastructure Layer
```

The code at Application, Domain and Infrastructure Layers are structured by features, and are feature-aggregates, each feature containing the nested namespaces for each aggregate of the feature.  

## 🧑🏻‍💻 Contributing

## License

See [LICENSE](./LICENSE).
