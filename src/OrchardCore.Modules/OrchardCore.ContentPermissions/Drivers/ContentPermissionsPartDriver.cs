using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Drivers;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.Localization;
using OrchardCore.Roles.Models;
using OrchardCore.Roles.Services;
using OrchardCore.Security;
using OrchardCore.ContentPermissions.Models;
using OrchardCore.ContentPermissions.Settings;
using OrchardCore.ContentPermissions.ViewModels;
using OrchardCore.Security.Services;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using Microsoft.Extensions.Localization;
using Microsoft.AspNetCore.Authorization;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.DisplayManagement.ModelBinding;

namespace OrchardCore.ContentPermissions.Drivers {
    public class ContentPermissionsPartDriver : ContentPartDisplayDriver<ContentPermissionsPart> {

        private const string TemplateName = "Parts.ContentPermissions";
        private readonly IRoleProvider _roleProvider;
        private readonly IAuthorizer _authorizer;
        private readonly IAuthorizationService _authorizationService;

        public ContentPermissionsPartDriver(IRoleProvider roleProvider, IAuthorizer authorizer, IAuthorizationService authorizationService) {
            _roleProvider = roleProvider;
            _authorizer = authorizer;
            _authorizationService = authorizationService;
        }

        public IOrchardServices Services { get; set; }
        public IStringLocalizer T { get; set; }

        protected override string Prefix {
            get { return "ContentPermissionsPermissionPart"; }
        }

        protected override IDisplayResult Display(ContentPermissionsPart part, string displayType, dynamic shapeHelper) {
            if (contentPartDefinition == null)
            {
                throw new ArgumentNullException(nameof(contentPartDefinition));
            }

            var contentPartDefinitionShape = await CreateContentShapeAsync("Parts_ContentPermissions_SummaryAdmin");

            return ContentShape("Parts_ContentPermissions_SummaryAdmin", () => shapeHelper.Parts_ContentPermissions_SummaryAdmin());
        }

        protected override IDisplayResult Editor(ContentPermissionsPart part, dynamic shapeHelper) {
            return ContentShape("Parts_ContentPermissions_Edit", () => {

                // ensure the current user is allowed to define permissions
                if (!_authorizer.Authorize(Permissions.GrantPermission)) {
                    return null;
                }

                var settings = part.Settings.TryGetModel<ContentPermissionsPartSettings>();

                var allRoles = _roleProvider.GetRoleNamesAsync().Result.ToList();

                if(settings == null) {
                    settings = new ContentPermissionsPartSettings {
                        View = ContentPermissionsPartViewModel.SerializePermissions(allRoles.Select(x => new RoleEntry { Role = x, Checked = _authorizationService.AuthorizeAsync(OrchardCore.Contents.Permissions.ViewContent, UserSimulation.Create(x), null) }).ToList()),
                        ViewOwn = ContentPermissionsPartViewModel.SerializePermissions(allRoles.Select(x => new RoleEntry { Role = x, Checked = _authorizationService.TryCheckAccess(OrchardCore.Contents.Permissions.ViewOwnContent, UserSimulation.Create(x), null) }).ToList()),
                        Publish = ContentPermissionsPartViewModel.SerializePermissions(allRoles.Select(x => new RoleEntry { Role = x, Checked = _authorizationService.TryCheckAccess(OrchardCore.Contents.Permissions.PublishContent, UserSimulation.Create(x), null) }).ToList()),
                        PublishOwn = ContentPermissionsPartViewModel.SerializePermissions(allRoles.Select(x => new RoleEntry { Role = x, Checked = _authorizationService.TryCheckAccess(OrchardCore.Contents.Permissions.PublishOwnContent, UserSimulation.Create(x), null) }).ToList()),
                        Edit = ContentPermissionsPartViewModel.SerializePermissions(allRoles.Select(x => new RoleEntry { Role = x, Checked = _authorizationService.TryCheckAccess(OrchardCore.Contents.Permissions.EditContent, UserSimulation.Create(x), null) }).ToList()),
                        EditOwn = ContentPermissionsPartViewModel.SerializePermissions(allRoles.Select(x => new RoleEntry { Role = x, Checked = _authorizationService.TryCheckAccess(OrchardCore.Contents.Permissions.EditOwnContent, UserSimulation.Create(x), null) }).ToList()),
                        Delete = ContentPermissionsPartViewModel.SerializePermissions(allRoles.Select(x => new RoleEntry { Role = x, Checked = _authorizationService.TryCheckAccess(OrchardCore.Contents.Permissions.DeleteContent, UserSimulation.Create(x), null) }).ToList()),
                        DeleteOwn = ContentPermissionsPartViewModel.SerializePermissions(allRoles.Select(x => new RoleEntry { Role = x, Checked = _authorizationService.TryCheckAccess(OrchardCore.Contents.Permissions.DeleteOwnContent, UserSimulation.Create(x), null) }).ToList()),
                        Preview = ContentPermissionsPartViewModel.SerializePermissions(allRoles.Select(x => new RoleEntry { Role = x, Checked = _authorizationService.TryCheckAccess(OrchardCore.Contents.Permissions.PreviewContent, UserSimulation.Create(x), null) }).ToList()),
                        PreviewOwn = ContentPermissionsPartViewModel.SerializePermissions(allRoles.Select(x => new RoleEntry { Role = x, Checked = _authorizationService.TryCheckAccess(OrchardCore.Contents.Permissions.PreviewOwnContent, UserSimulation.Create(x), null) }).ToList()),
                        DisplayedRoles = ContentPermissionsPartViewModel.SerializePermissions(allRoles.Select(x => new RoleEntry { Role = x, Checked = true }).ToList()),
                    };
                }

                ContentPermissionsPartViewModel model;

                // copy defaults settings if new content item
                if (!part.Enabled && !part.ContentItem.HasDraft() && !part.ContentItem.HasPublished()) {
                    model = new ContentPermissionsPartViewModel {
                        ViewRoles = ContentPermissionsPartViewModel.ExtractRoleEntries(allRoles, settings.View),
                        ViewOwnRoles = ContentPermissionsPartViewModel.ExtractRoleEntries(allRoles, settings.ViewOwn),
                        PublishRoles= ContentPermissionsPartViewModel.ExtractRoleEntries(allRoles, settings.Publish),
                        PublishOwnRoles= ContentPermissionsPartViewModel.ExtractRoleEntries(allRoles, settings.PublishOwn),
                        EditRoles= ContentPermissionsPartViewModel.ExtractRoleEntries(allRoles, settings.Edit),
                        EditOwnRoles= ContentPermissionsPartViewModel.ExtractRoleEntries(allRoles, settings.EditOwn),
                        DeleteRoles= ContentPermissionsPartViewModel.ExtractRoleEntries(allRoles, settings.Delete),
                        DeleteOwnRoles= ContentPermissionsPartViewModel.ExtractRoleEntries(allRoles, settings.DeleteOwn),
                        PreviewRoles = ContentPermissionsPartViewModel.ExtractRoleEntries(allRoles, settings.Preview),
                        PreviewOwnRoles = ContentPermissionsPartViewModel.ExtractRoleEntries(allRoles, settings.PreviewOwn),
                        AllRoles = ContentPermissionsPartViewModel.ExtractRoleEntries(allRoles, settings.DisplayedRoles)
                    };
                }
                else {
                    model = new ContentPermissionsPartViewModel {
                        ViewRoles = ContentPermissionsPartViewModel.ExtractRoleEntries(allRoles, part.ViewContent),
                        ViewOwnRoles = ContentPermissionsPartViewModel.ExtractRoleEntries(allRoles, part.ViewOwnContent),
                        PublishRoles = ContentPermissionsPartViewModel.ExtractRoleEntries(allRoles, part.PublishContent),
                        PublishOwnRoles = ContentPermissionsPartViewModel.ExtractRoleEntries(allRoles, part.PublishOwnContent),
                        EditRoles = ContentPermissionsPartViewModel.ExtractRoleEntries(allRoles, part.EditContent),
                        EditOwnRoles = ContentPermissionsPartViewModel.ExtractRoleEntries(allRoles, part.EditOwnContent),
                        DeleteRoles = ContentPermissionsPartViewModel.ExtractRoleEntries(allRoles, part.DeleteContent),
                        DeleteOwnRoles = ContentPermissionsPartViewModel.ExtractRoleEntries(allRoles, part.DeleteOwnContent),
                        PreviewRoles = ContentPermissionsPartViewModel.ExtractRoleEntries(allRoles, part.PreviewContent),
                        PreviewOwnRoles = ContentPermissionsPartViewModel.ExtractRoleEntries(allRoles, part.PreviewOwnContent),
                        AllRoles = ContentPermissionsPartViewModel.ExtractRoleEntries(allRoles, settings.DisplayedRoles)
                    };
                }

                // disable permissions the current user doesn't have
                model.ViewRoles = model.ViewRoles.Select(x => new RoleEntry { Role = x.Role, Checked = x.Checked, Enabled = _authorizer.Authorize(OrchardCore.Contents.Permissions.ViewContent, part.ContentItem), Default = _authorizationService.TryCheckAccess(OrchardCore.Contents.Permissions.ViewContent, UserSimulation.Create(x.Role), null) }).ToList();
                model.ViewOwnRoles = model.ViewOwnRoles.Select(x => new RoleEntry { Role = x.Role, Checked = x.Checked, Enabled = _authorizer.Authorize(OrchardCore.Contents.Permissions.ViewOwnContent, part.ContentItem), Default = _authorizationService.TryCheckAccess(OrchardCore.Contents.Permissions.ViewOwnContent, UserSimulation.Create(x.Role), null) }).ToList();
                model.PublishRoles = model.PublishRoles.Select(x => new RoleEntry { Role = x.Role, Checked = x.Checked, Enabled = _authorizer.Authorize(OrchardCore.Contents.Permissions.PublishContent, part.ContentItem), Default = _authorizationService.TryCheckAccess(OrchardCore.Contents.Permissions.PublishContent, UserSimulation.Create(x.Role), null) }).ToList();
                model.PublishOwnRoles = model.PublishOwnRoles.Select(x => new RoleEntry { Role = x.Role, Checked = x.Checked, Enabled = _authorizer.Authorize(OrchardCore.Contents.Permissions.PublishOwnContent, part.ContentItem), Default = _authorizationService.TryCheckAccess(OrchardCore.Contents.Permissions.PublishOwnContent, UserSimulation.Create(x.Role), null) }).ToList();
                model.EditRoles = model.EditRoles.Select(x => new RoleEntry { Role = x.Role, Checked = x.Checked, Enabled = _authorizer.Authorize(OrchardCore.Contents.Permissions.EditContent, part.ContentItem), Default = _authorizationService.TryCheckAccess(OrchardCore.Contents.Permissions.EditContent, UserSimulation.Create(x.Role), null) }).ToList();
                model.EditOwnRoles = model.EditOwnRoles.Select(x => new RoleEntry { Role = x.Role, Checked = x.Checked, Enabled = _authorizer.Authorize(OrchardCore.Contents.Permissions.EditOwnContent, part.ContentItem), Default = _authorizationService.TryCheckAccess(OrchardCore.Contents.Permissions.EditOwnContent, UserSimulation.Create(x.Role), null) }).ToList();
                model.DeleteRoles = model.DeleteRoles.Select(x => new RoleEntry { Role = x.Role, Checked = x.Checked, Enabled = _authorizer.Authorize(OrchardCore.Contents.Permissions.DeleteContent, part.ContentItem), Default = _authorizationService.TryCheckAccess(OrchardCore.Contents.Permissions.DeleteContent, UserSimulation.Create(x.Role), null) }).ToList();
                model.DeleteOwnRoles = model.DeleteOwnRoles.Select(x => new RoleEntry { Role = x.Role, Checked = x.Checked, Enabled = _authorizer.Authorize(OrchardCore.Contents.Permissions.DeleteOwnContent, part.ContentItem), Default = _authorizationService.TryCheckAccess(OrchardCore.Contents.Permissions.DeleteOwnContent, UserSimulation.Create(x.Role), null) }).ToList();
                model.PreviewRoles = model.PreviewRoles.Select(x => new RoleEntry { Role = x.Role, Checked = x.Checked, Enabled = _authorizer.Authorize(OrchardCore.Contents.Permissions.PreviewContent, part.ContentItem), Default = _authorizationService.TryCheckAccess(OrchardCore.Contents.Permissions.PreviewContent, UserSimulation.Create(x.Role), null) }).ToList();
                model.PreviewOwnRoles = model.PreviewOwnRoles.Select(x => new RoleEntry { Role = x.Role, Checked = x.Checked, Enabled = _authorizer.Authorize(OrchardCore.Contents.Permissions.PreviewOwnContent, part.ContentItem), Default = _authorizationService.TryCheckAccess(OrchardCore.Contents.Permissions.PreviewOwnContent, UserSimulation.Create(x.Role), null) }).ToList();

                model.Enabled = part.Enabled;

                return shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: model, Prefix: Prefix);
            });
        }

        protected override IDisplayResult Editor(ContentPermissionsPart part, IUpdateModel updater, dynamic shapeHelper) {
            // ensure the current user is allowed to define permissions
            if (!_authorizer.Authorize(Permissions.GrantPermission)) {
                return null;
            }

            var model = new ContentPermissionsPartViewModel();

            if (!updater.TryUpdateModel(model, Prefix, null, null)) {
                updater.AddModelError(String.Empty, T("Could not update permissions"));
            }
            else {
                part.Enabled = model.Enabled;
                part.ViewContent = ContentPermissionsPartViewModel.SerializePermissions(model.ViewRoles);
                part.ViewOwnContent = ContentPermissionsPartViewModel.SerializePermissions(model.ViewOwnRoles);
                part.PublishContent = ContentPermissionsPartViewModel.SerializePermissions(model.PublishRoles);
                part.PublishOwnContent = ContentPermissionsPartViewModel.SerializePermissions(model.PublishOwnRoles);
                part.EditContent = ContentPermissionsPartViewModel.SerializePermissions(model.EditRoles);
                part.EditOwnContent = ContentPermissionsPartViewModel.SerializePermissions(model.EditOwnRoles);
                part.DeleteContent = ContentPermissionsPartViewModel.SerializePermissions(model.DeleteRoles);
                part.DeleteOwnContent = ContentPermissionsPartViewModel.SerializePermissions(model.DeleteOwnRoles);
                part.PreviewContent = ContentPermissionsPartViewModel.SerializePermissions(model.PreviewRoles);
                part.PreviewOwnContent = ContentPermissionsPartViewModel.SerializePermissions(model.PreviewOwnRoles);

                var settings = part.Settings.TryGetModel<ContentPermissionsPartSettings>();

                var allRoles = _roleProvider.GetRoleNamesAsync().Result.ToList();

                OverrideDefaultPermissions(part, allRoles, settings);
            }

            return Editor(part, shapeHelper);
        }

        protected override void Exporting(ContentPermissionsPart part, ExportContentContext context) {
            context.Element(part.PartDefinition.Name).SetAttributeValue("Enabled", part.Enabled);
            context.Element(part.PartDefinition.Name).SetAttributeValue("ViewContent", part.ViewContent);
            context.Element(part.PartDefinition.Name).SetAttributeValue("EditContent", part.EditContent);
            context.Element(part.PartDefinition.Name).SetAttributeValue("PublishContent", part.PublishContent);
            context.Element(part.PartDefinition.Name).SetAttributeValue("DeleteContent", part.DeleteContent);
            context.Element(part.PartDefinition.Name).SetAttributeValue("PreviewContent", part.PreviewContent);
            context.Element(part.PartDefinition.Name).SetAttributeValue("ViewOwnContent", part.ViewOwnContent);
            context.Element(part.PartDefinition.Name).SetAttributeValue("EditOwnContent", part.EditOwnContent);
            context.Element(part.PartDefinition.Name).SetAttributeValue("PublishOwnContent", part.PublishOwnContent);
            context.Element(part.PartDefinition.Name).SetAttributeValue("DeleteOwnContent", part.DeleteOwnContent);
            context.Element(part.PartDefinition.Name).SetAttributeValue("PreviewOwnContent", part.PreviewOwnContent);
        }

        protected override void Importing(ContentPermissionsPart part, ImportContentContext context) {
            // Don't do anything if the tag is not specified.
            if (context.Data.Element(part.PartDefinition.Name) == null) {
                return;
            }

            context.ImportAttribute(part.PartDefinition.Name, "Enabled", s => part.Enabled = XmlConvert.ToBoolean(s));
            context.ImportAttribute(part.PartDefinition.Name, "ViewContent", s => part.ViewContent = s);
            context.ImportAttribute(part.PartDefinition.Name, "EditContent", s => part.EditContent = s);
            context.ImportAttribute(part.PartDefinition.Name, "PublishContent", s => part.PublishContent = s);
            context.ImportAttribute(part.PartDefinition.Name, "DeleteContent", s => part.DeleteContent = s);
            context.ImportAttribute(part.PartDefinition.Name, "PreviewContent", s => part.PreviewContent = s);
            context.ImportAttribute(part.PartDefinition.Name, "ViewOwnContent", s => part.ViewOwnContent = s);
            context.ImportAttribute(part.PartDefinition.Name, "EditOwnContent", s => part.EditOwnContent = s);
            context.ImportAttribute(part.PartDefinition.Name, "PublishOwnContent", s => part.PublishOwnContent = s);
            context.ImportAttribute(part.PartDefinition.Name, "DeleteOwnContent", s => part.DeleteOwnContent = s);
            context.ImportAttribute(part.PartDefinition.Name, "PreviewOwnContent", s => part.PreviewOwnContent = s);
        }

        private void OverrideDefaultPermissions(ContentPermissionsPart part, List<string> allRoles, ContentPermissionsPartSettings settings) {
            // reset permissions the user can't change
            if (!_authorizer.Authorize(OrchardCore.Contents.Permissions.ViewContent, part.ContentItem)) {
                part.ViewContent = settings == null ? ContentPermissionsPartViewModel.SerializePermissions(allRoles.Select(x => new RoleEntry {Role = x, Checked = _authorizationService.TryCheckAccess(OrchardCore.Contents.Permissions.ViewContent, UserSimulation.Create(x), null)})) : settings.View;
            }

            if (!_authorizer.Authorize(OrchardCore.Contents.Permissions.ViewOwnContent, part.ContentItem)) {
                part.ViewOwnContent = settings == null ? ContentPermissionsPartViewModel.SerializePermissions(allRoles.Select(x => new RoleEntry {Role = x, Checked = _authorizationService.TryCheckAccess(OrchardCore.Contents.Permissions.ViewOwnContent, UserSimulation.Create(x), null)})) : settings.ViewOwn;
            }

            if (!_authorizer.Authorize(OrchardCore.Contents.Permissions.PublishContent, part.ContentItem)) {
                part.PublishContent = settings == null ? ContentPermissionsPartViewModel.SerializePermissions(allRoles.Select(x => new RoleEntry {Role = x, Checked = _authorizationService.TryCheckAccess(OrchardCore.Contents.Permissions.PublishContent, UserSimulation.Create(x), null)})) : settings.Publish;
            }

            if (!_authorizer.Authorize(OrchardCore.Contents.Permissions.PublishOwnContent, part.ContentItem)) {
                part.PublishOwnContent = settings == null ? ContentPermissionsPartViewModel.SerializePermissions(allRoles.Select(x => new RoleEntry {Role = x, Checked = _authorizationService.TryCheckAccess(OrchardCore.Contents.Permissions.PublishOwnContent, UserSimulation.Create(x), null)})) : settings.PublishOwn;
            }

            if (!_authorizer.Authorize(OrchardCore.Contents.Permissions.EditContent, part.ContentItem)) {
                part.EditContent = settings == null ? ContentPermissionsPartViewModel.SerializePermissions(allRoles.Select(x => new RoleEntry {Role = x, Checked = _authorizationService.TryCheckAccess(OrchardCore.Contents.Permissions.EditContent, UserSimulation.Create(x), null)})) : settings.Edit;
            }

            if (!_authorizer.Authorize(OrchardCore.Contents.Permissions.EditOwnContent, part.ContentItem)) {
                part.EditOwnContent = settings == null ? ContentPermissionsPartViewModel.SerializePermissions(allRoles.Select(x => new RoleEntry {Role = x, Checked = _authorizationService.TryCheckAccess(OrchardCore.Contents.Permissions.EditOwnContent, UserSimulation.Create(x), null)})) : settings.EditOwn;
            }

            if (!_authorizer.Authorize(OrchardCore.Contents.Permissions.DeleteContent, part.ContentItem)) {
                part.DeleteContent = settings == null ? ContentPermissionsPartViewModel.SerializePermissions(allRoles.Select(x => new RoleEntry {Role = x, Checked = _authorizationService.TryCheckAccess(OrchardCore.Contents.Permissions.DeleteContent, UserSimulation.Create(x), null)})) : settings.Delete;
            }

            if (!_authorizer.Authorize(OrchardCore.Contents.Permissions.DeleteOwnContent, part.ContentItem)) {
                part.DeleteOwnContent = settings == null ? ContentPermissionsPartViewModel.SerializePermissions(allRoles.Select(x => new RoleEntry {Role = x, Checked = _authorizationService.TryCheckAccess(OrchardCore.Contents.Permissions.DeleteOwnContent, UserSimulation.Create(x), null)})) : settings.DeleteOwn;
            }

            if (!_authorizer.Authorize(OrchardCore.Contents.Permissions.PreviewContent, part.ContentItem)) {
                part.PreviewContent = settings == null ? ContentPermissionsPartViewModel.SerializePermissions(allRoles.Select(x => new RoleEntry {Role = x, Checked = _authorizationService.TryCheckAccess(OrchardCore.Contents.Permissions.PreviewContent, UserSimulation.Create(x), null)})) : settings.Preview;
            }

            if (!_authorizer.Authorize(OrchardCore.Contents.Permissions.PreviewOwnContent, part.ContentItem)) {
                part.PreviewOwnContent = settings == null ? ContentPermissionsPartViewModel.SerializePermissions(allRoles.Select(x => new RoleEntry {Role = x, Checked = _authorizationService.TryCheckAccess(OrchardCore.Contents.Permissions.PreviewOwnContent, UserSimulation.Create(x), null)})) : settings.PreviewOwn;
            }
        }
    }
}