using System;
using System.Linq;
using System.Collections.Generic;
using OrchardCore.ContentManagement.MetaData;
using OrchardCore.ContentPermissions.Settings;
using OrchardCore.Environment.Extensions.Models;
using OrchardCore.Security.Permissions;
using OrchardCore.ContentManagement;
using OrchardCore.Localization;

namespace OrchardCore.ContentPermissions.Services {
    public class DynamicPermissions : IPermissionProvider {
        private static readonly Permission PublishContent = new Permission { Description = "Publish or unpublish {0} for others", Name = "Publish_{0}", ImpliedBy = new[] { OrchardCore.Contents.Permissions.PublishContent } };
        private static readonly Permission PublishOwnContent = new Permission { Description = "Publish or unpublish {0}", Name = "PublishOwn_{0}", ImpliedBy = new[] { PublishContent, OrchardCore.Contents.Permissions.PublishOwnContent } };
        private static readonly Permission EditContent = new Permission { Description = "Edit {0} for others", Name = "Edit_{0}", ImpliedBy = new[] { PublishContent, OrchardCore.Contents.Permissions.EditContent } };
        private static readonly Permission EditOwnContent = new Permission { Description = "Edit {0}", Name = "EditOwn_{0}", ImpliedBy = new[] { EditContent, PublishOwnContent, OrchardCore.Contents.Permissions.EditOwnContent } };
        private static readonly Permission DeleteContent = new Permission { Description = "Delete {0} for others", Name = "Delete_{0}", ImpliedBy = new[] { OrchardCore.Contents.Permissions.DeleteContent } };
        private static readonly Permission DeleteOwnContent = new Permission { Description = "Delete {0}", Name = "DeleteOwn_{0}", ImpliedBy = new[] { DeleteContent, OrchardCore.Contents.Permissions.DeleteOwnContent } };
        private static readonly Permission ViewContent = new Permission { Description = "View {0} by others", Name = "View_{0}", ImpliedBy = new[] { EditContent, OrchardCore.Contents.Permissions.ViewContent } };
        private static readonly Permission ViewOwnContent = new Permission { Description = "View own {0}", Name = "ViewOwn_{0}", ImpliedBy = new[] { ViewContent, OrchardCore.Contents.Permissions.ViewOwnContent } };
        private static readonly Permission PreviewContent = new Permission { Description = "Preview {0} by others", Name = "Preview_{0}", ImpliedBy = new[] { EditContent, OrchardCore.Contents.Permissions.PreviewContent } };
        private static readonly Permission PreviewOwnContent = new Permission { Description = "Preview own {0}", Name = "PreviewOwn_{0}", ImpliedBy = new[] { PreviewContent, OrchardCore.Contents.Permissions.PreviewOwnContent } };


        public static readonly Dictionary<string, Permission> PermissionTemplates = new Dictionary<string, Permission> {
            {OrchardCore.Contents.Permissions.PublishContent.Name, PublishContent},
            {OrchardCore.Contents.Permissions.PublishOwnContent.Name, PublishOwnContent},
            {OrchardCore.Contents.Permissions.EditContent.Name, EditContent},
            {OrchardCore.Contents.Permissions.EditOwnContent.Name, EditOwnContent},
            {OrchardCore.Contents.Permissions.DeleteContent.Name, DeleteContent},
            {OrchardCore.Contents.Permissions.DeleteOwnContent.Name, DeleteOwnContent},
            {OrchardCore.Contents.Permissions.ViewContent.Name, ViewContent},
            {OrchardCore.Contents.Permissions.ViewOwnContent.Name, ViewOwnContent},
            {OrchardCore.Contents.Permissions.PreviewContent.Name, PreviewContent},
            {OrchardCore.Contents.Permissions.PreviewOwnContent.Name, PreviewOwnContent}
        };

        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IContentManager _contentManager;

        public virtual Feature Feature { get; set; }
        public Localizer T { get; set; }

        public DynamicPermissions(IContentDefinitionManager contentDefinitionManager, IContentManager contentManager) {
            _contentDefinitionManager = contentDefinitionManager;
            _contentManager = contentManager;

            T = NullLocalizer.Instance;
        }

        public IEnumerable<Permission> GetPermissions() {

            // manage rights only for Securable types
            var securableTypes = _contentDefinitionManager.ListTypeDefinitions()
                .Where(ctd => ctd.Settings.GetModel<ContentPermissionsContentTypeSettings>().SecurableContentItems)
                .ToList();

            foreach (var typeDefinition in securableTypes) {
                foreach (var content in _contentManager.Query(typeDefinition.Name).List()) {
                    foreach (var permissionTemplate in PermissionTemplates.Values) {
                        yield return CreateItemPermission(permissionTemplate, content, T, _contentManager);
                    }
                }
            }
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return Enumerable.Empty<PermissionStereotype>();
        }


        /// <summary>
        /// Generates a permission dynamically for a content item
        /// </summary>
        public static Permission CreateItemPermission(Permission template, IContent content, Localizer T, IContentManager contentManager) {
            var identity = contentManager.GetItemMetadata(content).Identity.ToString();
            var displayText = contentManager.GetItemMetadata(content).DisplayText;
            var typeDefinition = content.ContentItem.TypeDefinition;

            return new Permission {
                Name = String.Format(template.Name, identity),
                Description = String.Format(template.Description, typeDefinition.DisplayName),
                Category = T("{0} - {1}", typeDefinition.DisplayName, displayText).ToString(),
                ImpliedBy = (template.ImpliedBy ?? new Permission[0]).Select(t => CreateItemPermission(t, content, T, contentManager))
            };
        }

        /// <summary>
        /// Returns a dynamic permission for a content type, based on a global content permission template
        /// </summary>
        public static Permission ConvertToDynamicPermission(Permission permission) {
            if (PermissionTemplates.ContainsKey(permission.Name)) {
                return PermissionTemplates[permission.Name];
            }

            return null;
        }
    }
}
