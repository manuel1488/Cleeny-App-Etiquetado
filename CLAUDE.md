# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**AppEtiquetado** is a cross-platform .NET MAUI + Blazor hybrid application targeting Android, iOS, macOS (Catalyst), and Windows. It embeds Blazor components inside a native MAUI shell via `BlazorWebView`.

## VS Code

Extensiones requeridas (en `.vscode/extensions.json`): **ms-dotnettools.dotnet-maui**, **ms-dotnettools.csdevkit**, **ms-dotnettools.csharp**.

Las configuraciones de depuración en `.vscode/launch.json` permiten depurar en Android, Windows e iOS directamente desde VS Code con F5 (requiere la extensión MAUI).

## Build Commands

```bash
# Build for a specific platform
dotnet build -f net10.0-android
dotnet build -f net10.0-windows10.0.19041.0
dotnet build -f net10.0-ios
dotnet build -f net10.0-maccatalyst

# Run on Windows
dotnet run -f net10.0-windows10.0.19041.0

# Publish for Android
dotnet publish -f net10.0-android
```

The active development target in `.csproj.user` is `net10.0-android` with an Android emulator (Pixel 7 - API 35).

## Architecture

### Hybrid App Model

MAUI provides the native shell; Blazor runs inside it via `BlazorWebView`:

```
App.xaml.cs (MAUI App)
  └── MainPage.xaml (BlazorWebView host)
        └── wwwroot/index.html (Blazor entry point)
              └── Components/Routes.razor (router)
                    └── Components/Layout/MainLayout.razor
                          └── @Body (pages under Components/Pages/)
```

### Service Registration

All DI registration happens in `MauiProgram.cs`. Add services there before `builder.Build()`. The `#if DEBUG` block enables developer tools and debug logging.

### Platform-Specific Code

Native code lives under `Platforms/{Android,iOS,MacCatalyst,Windows}/`. Avoid placing shared logic there; use `#if` preprocessor directives or MAUI's platform-specific API abstractions instead.

### Web Assets

Static assets (CSS, JS, images) belong in `wwwroot/`. Component-scoped styles use `.razor.css` files colocated with their component.

## Key Conventions

- **Nullable reference types** are enabled — always handle nullability.
- **Implicit usings** are enabled — common namespaces don't need explicit `using` statements.
- **XAML Source Generation** (`MauiXamlInflator=SourceGen`) is active for faster builds.
- `ApplicationId` is `com.companyname.appetiquetado` — update this before publishing.
- Windows packaging is unpackaged (`WindowsPackageType=None`).

## Platform Requirements

| Platform | Min Version |
|----------|-------------|
| Android | API 24 (7.0) |
| iOS | 15.0 |
| macOS Catalyst | 15.0 |
| Windows | 10.0.17763.0 |
