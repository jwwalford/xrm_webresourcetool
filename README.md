<img width="800" height="100" alt="Banner" src="https://github.com/user-attachments/assets/632d3107-4492-4951-8625-04584efa3eb0" />


An XrmToolBox plugin for generating HTML/CSS and Image web resources: build banners, buttons and custom HTML/CSS with live previews. Create icons, SVG and upload other image types. Browse existing HTML, CSS and image web resources.

<img width="1908" height="920" alt="webresourcegen" src="https://github.com/user-attachments/assets/ad78f6ff-9848-44ba-b6a1-52d22b0e42f0" />

## Features

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
- **Create an SVG icon...** (Tools tab, Image Tools): make an SVG web resource from the icon gallery used by the banner and button designers.
  - **Appearance**: icon colour, fill, line width, an optional background (circle, rounded square or square) with its colour, and the size written into the SVG.
  - **Icons**: click an icon in the gallery (General, Law, Accounting, Sales, Hardware, Buildings, IT, Emojis and circled Numbers 1 to 10). The gallery keeps its scroll position when you pick an icon.
  - **Preview** shows the icon at 16, 24, 32, 48 and 64 px (plus the saved size) on light and dark backgrounds.
  - Colours are written into the file (no `currentColor`), so it looks the same in an `img` tag or as an entity or command icon. **Save to Dataverse** creates or replaces the SVG web resource; **Export SVG...** saves the file.
- **Upload an image...** (Tools tab, Image Tools): save any image file as a web resource.
  - **File**: choose a PNG, JPG, GIF, BMP, ICO or SVG. The web resource name and display name default to the file name.
  - **Save as**: keep the file's own format (saved byte for byte unless resized, so GIF animation and JPG quality are kept) or convert to PNG, JPG or GIF. BMP isn't a Dataverse type, so it is converted (PNG by default). SVG is saved as it is.
  - **Size**: original, or resize to a width and height with proportions kept. **JPG quality** and the fill colour for transparent areas apply when saving as JPG.
  - **Result** shows the format, size and file size, and warns above Dataverse's default 5 MB limit. The preview shows the image at actual size on a checkerboard and a dark background.
- **Create a banner...** (Tools tab): a single-page designer for form banners (an HTML web resource with a coloured bar, an icon and a line of text), shown in the right panel.
  - **Web resource**: pick a solution (its publisher prefix is added to the name), then enter the name, display name and description. Typing a display name fills in the name in lowercase with underscores ("Form Banner" becomes `form_banner`, keeping any folder such as `banners/`) until you edit the name yourself; this works the same on every tool page.
  - **Banner**: text, text colour, background colour, font size, bold, alignment, and an icon with its own colour (white outline by default). Pick from 78 icons in the clickable gallery, grouped into General, Law, Accounting, Sales, Hardware, Buildings, IT, Emojis and Numbers (circled 1 to 10). A live preview updates as you change things.
  - **Save to Dataverse** creates the web resource (or replaces an existing HTML one after confirming), adds it to the chosen solution and publishes it.
  - **Export HTML...** saves the same HTML file to disk. This works without a connection.
  - The banner fills its frame, so set its height on the form with the web resource's row count, and turn off the label and border.

## Build

Requires the .NET SDK (any recent version) or Visual Studio 2022. The project targets .NET Framework 4.8.

```
dotnet build -c Release
```

The output is `src/WebResourceManager/bin/Release/net48/webresource_generator.dll`. XrmToolBox libraries are reference-only, so only the plugin DLL is produced.

## Try it in XrmToolBox

Copy `webresource_generator.dll` into your XrmToolBox `Plugins` folder (by default `%AppData%\MscrmTools\XrmToolBox\Plugins`) and restart XrmToolBox. Builds before October 2026 were called `WebResourceManager.dll`; delete that file if it is still there, or the tool shows up twice. To copy it automatically on every build:

```
dotnet build -p:XrmToolBoxPluginsPath="%AppData%\MscrmTools\XrmToolBox\Plugins"
```

XrmToolBox caches each plugin's name and images in `Plugins\manifest.json` and only refreshes them when a DLL's assembly version changes. Every build gets a new version (`1.1.<days since 2026-01-01>.<minute of the day, UTC>`), so a new DLL is picked up on restart. If the tool list still shows an old name or logo, close XrmToolBox, delete `Plugins\manifest.json` and start it again.

## Publish to the XrmToolBox Tool Library

```
dotnet pack -c Release
```

This builds the plugin and writes `artifacts/webresource_generator.<version>.nupkg` from `src/WebResourceManager/webresource_generator.nuspec`. The package follows the Tool Library rules:

- Id `webresource_generator` (the assembly name) and title "Assisted Web Resource Generator".
- The package version is the assembly version of the same build, so the Tool Library doesn't keep offering an update.
- Icon URL points at `assets/plugin-80x80.png` in this repository (not embedded, so nuget.org keeps the URL the XrmToolBox site needs), project URL at the repository.
- Depends on `XrmToolBox` 1.2025.10.74 (the version the plugin is built against), not `XrmToolBoxPackage`.
- Contains only `lib/net48/Plugins/webresource_generator.dll`.

Update the release notes in the nuspec before each release.

### Publishing from GitHub (Trusted Publishing)

`.github/workflows/publish-nuget.yml` packs the plugin and pushes it to nuget.org when a GitHub release is published, or when run by hand from the Actions tab. It uses NuGet Trusted Publishing, so no API key is stored: GitHub gives the job a short-lived token and `NuGet/login` exchanges it for a one-hour key. One-off setup:

1. On nuget.org, open your user menu > **Trusted Publishing** and add a policy with Repository Owner `jwwalford`, Repository `xrm_webresourcetool` and Workflow File `publish-nuget.yml`. Leave Environment empty.
2. In the GitHub repository, add an Actions secret `NUGET_USER` with your nuget.org profile name (not your email address).

To publish by hand instead, push the package with your own API key:

```
dotnet nuget push artifacts/webresource_generator.<version>.nupkg --api-key <your key> --source https://api.nuget.org/v3/index.json
```

Then submit the tool on the XrmToolBox website so it is listed in the Tool Library.

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
| `Banners/IconGalleryView.cs` | Loads the gallery into its WebView2, keeping the scroll position when it reloads after a selection |
| `Images/IconGeneratorPanel.cs` | Icon tool UI: source image, fit or crop, size previews, PNG or ICO |
| `Images/SvgIconPanel.cs` | SVG icon tool UI: details, appearance, icon gallery, preview |
| `Images/SvgIconBuilder.cs` | Builds the SVG and its multi-size preview page |
| `Images/ImageUploadPanel.cs` | Image upload tool UI: file, format, resize, preview |
| `Images/ImageFileConverter.cs` | Resizes and re-encodes images as PNG, JPG or GIF |
| `Images/IconResizer.cs` | High-quality square resizing and PNG / multi-size ICO encoding |
| `Tools/CustomCodePanel.cs` | Custom HTML/CSS tool: details, live preview and code editor |
| `Tools/WebResourceDetailsBox.cs` | Shared web resource details group (solution, name, display name, description) |
| `Tools/WebResourcePublisher.cs` | Shared save: replace check, create or update, add to solution, publish (single or several at once) |
| `CssSamplePage.cs` | Sample page used for the CSS live preview |
| `CodeStyler.cs` | Scintilla syntax highlighting per type |
| `Banners/BannerDesignerPanel.cs` | Banner designer UI: details, design, icon gallery, live preview, save and export |
| `Banners/BannerHtmlBuilder.cs` | Generates the banner HTML |
| `Banners/BannerIcon.cs` | Built-in SVG icons (based on [Feather Icons](https://feathericons.com), MIT) |

## Licence

MIT, see [LICENSE](LICENSE).
