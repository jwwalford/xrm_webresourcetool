using System.ComponentModel.Composition;
using XrmToolBox.Extensibility;
using XrmToolBox.Extensibility.Interfaces;

namespace WebResourceManager
{
    [Export(typeof(IXrmToolBoxPlugin)),
        ExportMetadata("Name", "Web Resource Manager"),
        ExportMetadata("Description", "Browse web resources by type and preview images, HTML, SVG, scripts and other text content."),
        ExportMetadata("SmallImageBase64", PluginIcons.Small),
        ExportMetadata("BigImageBase64", PluginIcons.Big),
        ExportMetadata("BackgroundColor", "Lavender"),
        ExportMetadata("PrimaryFontColor", "Black"),
        ExportMetadata("SecondaryFontColor", "Gray")]
    public class Plugin : PluginBase
    {
        public override IXrmToolBoxPluginControl GetControl()
        {
            return new WebResourceManagerControl();
        }
    }
}
