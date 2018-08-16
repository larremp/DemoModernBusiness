using OrchardCore.ContentManagement;

namespace OrchardCore.ContentPermissions.Models
{
    public class ContentPermissionsPart : ContentPart
    {
        /// <summary>
        /// Whether the access control should be applied for the content item
        /// </summary>
        public bool Enabled { get; set; }

        public string ViewContent { get; set; }

        public string ViewOwnContent { get; set; }

        public string PublishContent { get; set; }

        public string PublishOwnContent { get; set; }

        public string EditContent { get; set; }

        public string EditOwnContent { get; set; }

        public string DeleteContent { get; set; }

        public string DeleteOwnContent { get; set; }

        public string PreviewContent { get; set; }

        public string PreviewOwnContent { get; set; }
    }
}
