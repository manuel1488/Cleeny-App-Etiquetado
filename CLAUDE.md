# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**AppEtiquetado** is a cross-platform .NET MAUI + Blazor hybrid tablet application for labeling bulk products. It is a companion to the **Cleeny** POS system (`C:\repos\Cleeny`): it authenticates against Cleeny's REST API, searches the product catalog, and generates bulk product labels (PDF via Cleeny's label service).

**Cleeny API endpoints used:**
- `POST /api/Auth/Login` — form-encoded (`Input.UserName`, `Input.Password`); responds with 302 redirect
- `GET /api/products?search=&pageSize=20&isActive=true` — product search
- `POST /api/labels/bulk` — create label job (`CreateBulkLabelJobDto`)
- `GET /api/labels/bulk/{id}/pdf` — retrieve label PDF

**Key design decisions:**
- `AppApiService` wraps `HttpClient` with `CookieContainer` to persist Cleeny's auth cookie across requests
- All three services (`AppApiService`, `AuthService`, `LabelService`) are registered as **singletons** to survive page navigation
- `BlankLayout` is used for the login page (no AppBar); `MainLayout` is used for authenticated pages
- In `DEBUG` builds, SSL certificate validation is bypassed to allow Cleeny's dev cert
- The Cleeny server URL is persisted in `Preferences` (MAUI cross-platform settings)

**Cleeny theme colors (replicated in `AppTheme.cs`):**
- Primary: `#1A6868` (teal) | Secondary: `#7B3FA0` (purple)
- Font: Poppins (headings), Roboto (body)

## Brother Print SDK (Android)

**SDK version:** 4.13.0 (`BrotherPrintLibrary.aar`)
**AAR path in project:** `AppEtiquetado/Platforms/Android/libs/BrotherPrintLibrary.aar`
**Original source:** `C:\Users\Manuel Alfaro\Downloads\bpsdkaall4130\bpsdkaall4130\bpsdka4130\bpsdka4130\libs\`

The AAR is included via `<AndroidLibrary>` in the `.csproj` (Android target only). Binding transforms are in `Transforms/Metadata.xml`.

**Key classes** (C# namespace: `Com.Brother.Sdk.Lmprinter`):
- `Channel.NewBluetoothChannel(address, adapter)` — classic BT connection
- `PrinterDriverGenerator.OpenChannel(channel)` → `PrinterDriverGenerateResult`
- `QLPrintSettings(PrinterModel.Ql1110nwb)` — settings for QL-1110NWB
- `driver.PrintImage(bitmap, settings)` → `PrintError`

**Verified generated enum names** (binding uses PascalCase from Java names):
- `PrinterModel.Ql1110nwb`
- `QLPrintSettings.LabelSize.DieCutW62H100`, `.RollW62`, `.DieCutW103H164`, `.DieCutW62H29`, `.DieCutW62H60`, `.DieCutW102H152`
- `IPrintImageSettings.ScaleMode.FitPageAspect`
- `OpenChannelError.ErrorCode.NoError` / `PrintError.ErrorCode.NoError`

**Print pixel density (empirical, QL-1110NWB):** 200 px → 22.93 mm → `PxPerMm = 8.725`. Minimum print height: 25.40 mm (230 px). Label height is configurable and persisted via `Preferences` key `label_height_mm`.

**Printer connection test page:** `/conexion-impresora` (no auth required, uses `BlankLayout`). App startup is set to this page via `blazorWebView.StartPath` in `MainPage.xaml.cs`.

## VS Code

Required extensions (see `.vscode/extensions.json`): **ms-dotnettools.dotnet-maui**, **ms-dotnettools.csdevkit**, **ms-dotnettools.csharp**.

Debug configurations in `.vscode/launch.json` support Android (emulator and USB device), Windows, and iOS. Use `Ctrl+Shift+P` → `MAUI: Select Device` to pick the target before pressing F5. ADB is at `C:\Program Files (x86)\Android\android-sdk\platform-tools\`.

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

- **Language:** Code, comments, and documentation in English. UI text visible to end users in Spanish.
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
