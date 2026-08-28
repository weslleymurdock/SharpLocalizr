---
name: blazor-razor-ui
description: Guidance for Razor/Blazor localization workflow UI in the SharpLocalizr host.
---

# Blazor/Razor UI

Use `docs/skills/blazor-razor-ui.md`. Keep UI concerns in the host and invoke the application use case through Mediator or the controller; never call provider or repository implementations directly.

## Rules

- All razor pages and controls must not have `@using ...` instructions. These instructions belongs to the `_Imports.razor` file.
This is also valid for `@inject ...` instructions. Only the `@implements` adn `@page` instrcutions can be found at these files.

- All razor pages and components must use MudBlazor (9.9.0).

- The razor pages and components must not have splited files for code-behind, css or js as well, the implementation must be done all in the singlo *.razor file