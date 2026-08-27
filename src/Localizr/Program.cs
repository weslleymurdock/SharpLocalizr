using Localizr.Composition.Extensions;
using MudBlazor.Services;

await WebApplication
    .CreateBuilder(args)
    .RunLocalizrAsync<Program, Localizr.Components.App>(b=> b.Services.AddMudServices());
