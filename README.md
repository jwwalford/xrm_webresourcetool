# Web Resource Manager for XrmToolBox

An XrmToolBox tool for browsing Dataverse web resources by type and previewing them without leaving XrmToolBox.

## Features

- **Type filter**: load all web resources, all images, or a single type (HTML, CSS, JScript, XML, PNG, JPG, GIF, XAP, XSL, ICO, SVG, RESX).
- **Options**: hide hidden (system) web resources (on by default) and show unmanaged only.
- **List view** with name, display name, type, managed state and modified date. Click a column to sort; type in **Search** to filter by name or display name.
- **Preview pane** on the right, loaded on demand when you select a row:
  - PNG, JPG, GIF, ICO: rendered image with dimensions and size. Large images shrink to fit.
  - HTML and SVG: rendered with WebView2 (Edge). Scripts are off by default; turn on **Options > Run scripts in HTML preview** if you need them. **View source** switches to the code view.
  - JScript, CSS, XML, XSL, RESX: read-only code view with syntax highlighting and line numbers.
  - XAP (Silverlight): no preview; use **Save to disk**.
- **Save to disk** and **Copy name** from the toolbar or right-click menu.

## Build

Requires the .NET SDK (any recent version) or Visual Studio 2022. The project targets .NET Framework 4.8.

```
dotnet build -c Release
```

The output is `src/WebResourceManager/bin/Release/net48/WebResourceManager.dll`. XrmToolBox libraries are reference-only, so only the plugin DLL is produced.

## Try it in XrmToolBox

Copy `WebResourceManager.dll` into your XrmToolBox `Plugins` folder (by default `%AppData%\MscrmTools\XrmToolBox\Plugins`) and restart XrmToolBox. To copy it automatically on every build:

```
dotnet build -p:XrmToolBoxPluginsPath="%AppData%\MscrmTools\XrmToolBox\Plugins"
```

## Project layout

| File | Purpose |
| --- | --- |
| `Plugin.cs` | MEF export and tool metadata shown in XrmToolBox |
| `WebResourceManagerControl.cs` | UI logic: loading, filtering, sorting, preview, save |
| `Services/WebResourceRepository.cs` | Dataverse queries (metadata list, content on demand) |
| `Models/WebResourceType.cs` | Web resource type option set values, labels and preview kinds |
| `CodeStyler.cs` | Scintilla syntax highlighting per type |
