using System;

namespace WebResourceManager.Models
{
    public class WebResourceItem
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string DisplayName { get; set; }

        public string Description { get; set; }

        public WebResourceType Type { get; set; }

        public bool IsManaged { get; set; }

        public DateTime? ModifiedOn { get; set; }

        public string ModifiedBy { get; set; }

        /// <summary>
        /// Base64 content. Loaded on demand because it can be large.
        /// </summary>
        public string Content { get; set; }

        public string TypeLabel
        {
            get { return WebResourceTypeInfo.GetLabel(Type); }
        }

        public PreviewKind PreviewKind
        {
            get { return WebResourceTypeInfo.GetPreviewKind(Type); }
        }
    }
}
