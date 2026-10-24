# Assisted Web Resource Generator

An XrmToolBox plugin for generating HTML/CSS and Image web resources: build banners, buttons and custom HTML/CSS with live previews, and browse existing HTML, CSS and image web resources.

## Layout

- **Left panel** with two tabs:
  - **Tools**: large buttons grouped under black **HTML Tools** and **Image Tools** headers. The open tool is highlighted with a blue accent bar, and a **Resource explorer** footer button switches tabs. The left panel takes about 28% of the width on this tab and half on the explorer; each tab remembers where you drag the splitter. **Create a banner...**, **Create a button...**, **Create custom HTML/CSS...** and **Create an icon...** open in the right panel; the other buttons are placeholders for future tools.
  - **Resource explorer**: the list of loaded web resources, with a coloured type badge (HTML, CSS, PNG, SVG, ...), name, display name, type, managed state and modified date. Click a column to sort; type in **Search** to filter by name or display name. **Search** and **Save to disk** show in the toolbar only while this tab is open. The type dropdown marks each type with its badge, "HTML, CSS and images" with a document icon and "All images" with a picture icon.
- **Right panel**: follows the left tab.
  - On **Tools**, it hosts the open tool (banner designer, button designer or custom HTML/CSS editor). Tools keep what you've entered when you switch between them.
  - On **Resource explorer**, it shows details of the selected web resource, the **live preview** on top and the **code preview** below (drag the splitter to resize).

## Features

- **Type filter**: HTML, CSS and images (default), HTML only, CSS only, all images, or a single image type (PNG, JPG, GIF, ICO, SVG). **Load web resources** switches to the Resource explorer tab.
- **Options**: hide hidden (system) web resources (on by default) and show unmanaged only.
- **Preview**, loaded on demand when you select a row:
  - HTML: rendered with WebView2 (Edge), with the source below. Scripts are off by default; turn on **Options > Run scripts in HTML preview** if you need them.
  - CSS: applied to a sample page of headings, text, links, a list, form controls and a table, with the stylesheet below.
  - SVG: rendered image, with the SVG markup below.
  - PNG, JPG, GIF, ICO: rendered image with dimensions and size. Large images shrink to fit. There's no code pane for these.
- **Save to disk** and **Copy name** from the toolbar or right-click menu.
- **Create a button...** (Tools tab): a single-page designer for a button to place on a form.
  - **Button**: text, tooltip, text and background colours, font size, bold, icon and icon colour (same gallery as banners), corner roundness (0 is square, 50+ is a pill), and size: fill the web resource control or a fixed width and height positioned left, centre or right.
  - **When clicked**: run JavaScript (with `Xrm` and `formContext` for the form), open a URL in a new tab, save the record, or do nothing. Scripts reach the form through `parent.Xrm`.
  - The preview shows the button in a control of the height you choose; clicking it there doesn't run the action.
- **Create custom HTML/CSS...** (Tools tab): write an HTML or CSS web resource by hand.
  - **Web resource** details (top left) work the same way as the banner designer's.
  - **Preview** (top right) updates as you type. HTML is rendered as a page; CSS is applied to a sample page.
  - **Code** (below) is a syntax-highlighted editor. Switch between HTML and CSS with the Type option; untouched starter code is swapped for the other type's.
  - **Save to Dataverse** creates or replaces the web resource, adds it to the solution and publishes it. **Export...** saves the file to disk.
- **Create an icon...** (Tools tab, Image Tools): turn an image into icon web resources.
  - **Source image**: choose a PNG, JPG, GIF, BMP or ICO file. **Fit** pads a non-square image with transparency; **Crop** fills the square from the centre.
  - **Sizes**: 16, 24, 32, 48, 64, 80 and 128 px, each shown at actual size on a light and a dark background. Tick the ones to keep (16 and 32 by default).
  - **Save as**: PNG creates one web resource per size (`my_icon_16.png`, `my_icon_32.png`, ...); ICO creates one `.ico` holding every ticked size.
  - **Save to Dataverse** asks once before replacing existing web resources, then saves and publishes them together. **Export...** saves the files to disk.
- **Create a banner...** (Tools tab): a single-page designer for form banners (an HTML web resource with a coloured bar, an icon and a line of text), shown in the right panel.
  - **Web resource**: pick a solution (its publisher prefix is added to the name), then enter the name, display name and description.
  - **Banner**: text, text colour, background colour, font size, bold, alignment, and an icon with its own colour (white outline by default). Pick from 36 icons in the clickable gallery, grouped into General, Law, Accounting and Sales. A live preview updates as you change things.
  - **Save to Dataverse** creates the web resource (or replaces an existing HTML one after confirming), adds it to the chosen solution and publishes it.
  - **Export HTML...** saves the same HTML file to disk. This works without a connection.
  - The banner fills its frame, so set its height on the form with the web resource's row count, and turn off the label and border.

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
| `ToolsPanel.cs` | The Tools tab: group headers and tool buttons |
| `Buttons/ButtonDesignerPanel.cs` | Button designer UI: details, look, icon gallery, click action, preview |
| `Buttons/ButtonHtmlBuilder.cs` | Generates the button HTML and click handler |
| `Banners/IconGallery.cs` | Clickable icon grid shared by the banner and button designers |
| `Images/IconGeneratorPanel.cs` | Icon tool UI: source image, fit or crop, size previews, PNG or ICO |
| `Images/IconResizer.cs` | High-quality square resizing and PNG / multi-size ICO encoding |
| `Tools/CustomCodePanel.cs` | Custom HTML/CSS tool: details, live preview and code editor |
| `Tools/WebResourceDetailsBox.cs` | Shared web resource details group (solution, name, display name, description) |
| `Tools/WebResourcePublisher.cs` | Shared save: replace check, create or update, add to solution, publish (single or several at once) |
| `CssSamplePage.cs` | Sample page used for the CSS live preview |
| `CodeStyler.cs` | Scintilla syntax highlighting per type |
| `Banners/BannerDesignerPanel.cs` | Banner designer UI: details, design, icon gallery, live preview, save and export |
| `Banners/BannerHtmlBuilder.cs` | Generates the banner HTML |
| `Banners/BannerIcon.cs` | Built-in SVG icons (based on [Feather Icons](https://feathericons.com), MIT) |
