using System.ComponentModel.Composition;
using XrmToolBox.Extensibility;
using XrmToolBox.Extensibility.Interfaces;

namespace WebResourceManager
{
    [Export(typeof(IXrmToolBoxPlugin)),
        ExportMetadata("Name", "Assisted Web Resource Generator"),
        ExportMetadata("Description", "Generate HTML/CSS and image web resources: design form banners and buttons, write custom HTML/CSS with a live preview, and browse existing web resources."),
        ExportMetadata("SmallImageBase64", PluginIcons.Small),
        ExportMetadata("BigImageBase64", PluginIcons.Big),
        ExportMetadata("BackgroundColor", "#C8B8F0"),
        ExportMetadata("PrimaryFontColor", "Black"),
        ExportMetadata("SecondaryFontColor", "#4A4A4A")]
    public class Plugin : PluginBase
    {
        public override IXrmToolBoxPluginControl GetControl()
        {
            return new WebResourceManagerControl();
        }
    }
}
