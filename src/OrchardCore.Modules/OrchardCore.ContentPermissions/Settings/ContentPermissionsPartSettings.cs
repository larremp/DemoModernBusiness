using System.Collections.Generic;
using System.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.MetaData;
using OrchardCore.ContentManagement.MetaData.Builders;
using OrchardCore.ContentManagement.MetaData.Models;
using OrchardCore.ContentManagement.ViewModels;
using OrchardCore.Roles.Models;
using OrchardCore.Roles.Services;
using OrchardCore.Security;
using OrchardCore.ContentPermissions.ViewModels;
using OrchardCore.Security.Services;
using Microsoft.AspNetCore.Authorization;

namespace OrchardCore.ContentPermissions.Settings {
    public class ContentPermissionsPartSettings {
        public string View { get; set; }
        public string ViewOwn { get; set; }
        public string Publish { get; set; }
        public string PublishOwn { get; set; }
        public string Edit { get; set; }
        public string EditOwn { get; set; }
        public string Delete { get; set; }
        public string DeleteOwn { get; set; }
        public string Preview { get; set; }
        public string PreviewOwn { get; set; }
        public string DisplayedRoles { get; set; }
    }

    public class ViewPermissionsSettingsHooks : ContentDefinitionEditorEventsBase {
        private readonly IAuthorizer _authorizer;
        private readonly IAuthorizationService _authorizationService;
        private readonly IRoleProvider _roleProvider;

        public ViewPermissionsSettingsHooks(
            IAuthorizer authorizer, 
            IAuthorizationService authorizationService,
            IRoleProvider roleProvider
            ) {
            _authorizer = authorizer;
            _authorizationService = authorizationService;
            _roleProvider = roleProvider;
        }

        public override IEnumerable<TemplateViewModel> TypePartEditor(ContentTypePartDefinition definition) {
            if (definition.PartDefinition.Name != "ContentPermissionsPart")
                yield break;

            // ensure the current user is allowed to define permissions
            if(!_authorizer.Authorize(Permissions.GrantPermission)) {
                yield break;
            }

            var settings = definition.Settings.TryGetModel<ContentPermissionsPartSettings>();

            var allRoles = _roleProvider.GetRoleNamesAsync().Result.ToList();

            // copy defaults if new type
            if(settings == null) {
                settings = new ContentPermissionsPartSettings {
                    View = ContentPermissionsPartViewModel.SerializePermissions(allRoles.Select(x => new RoleEntry { Role = x, Checked = _authorizationService.TryCheckAccess(OrchardCore.Contents.Permissions.ViewContent, UserSimulation.Create(x), null) })),
                    ViewOwn = ContentPermissionsPartViewModel.SerializePermissions(allRoles.Select(x => new RoleEntry { Role = x, Checked = _authorizationService.TryCheckAccess(OrchardCore.Contents.Permissions.ViewOwnContent, UserSimulation.Create(x), null) })),
                    Publish = ContentPermissionsPartViewModel.SerializePermissions(allRoles.Select(x => new RoleEntry { Role = x, Checked = _authorizationService.TryCheckAccess(OrchardCore.Contents.Permissions.PublishContent, UserSimulation.Create(x), null) })),
                    PublishOwn = ContentPermissionsPartViewModel.SerializePermissions(allRoles.Select(x => new RoleEntry { Role = x, Checked = _authorizationService.TryCheckAccess(OrchardCore.Contents.Permissions.PublishOwnContent, UserSimulation.Create(x), null) })),
                    Edit = ContentPermissionsPartViewModel.SerializePermissions(allRoles.Select(x => new RoleEntry { Role = x, Checked = _authorizationService.TryCheckAccess(OrchardCore.Contents.Permissions.EditContent, UserSimulation.Create(x), null) })),
                    EditOwn = ContentPermissionsPartViewModel.SerializePermissions(allRoles.Select(x => new RoleEntry { Role = x, Checked = _authorizationService.TryCheckAccess(OrchardCore.Contents.Permissions.EditOwnContent, UserSimulation.Create(x), null) })),
                    Delete = ContentPermissionsPartViewModel.SerializePermissions(allRoles.Select(x => new RoleEntry { Role = x, Checked = _authorizationService.TryCheckAccess(OrchardCore.Contents.Permissions.DeleteContent, UserSimulation.Create(x), null) })),
                    DeleteOwn = ContentPermissionsPartViewModel.SerializePermissions(allRoles.Select(x => new RoleEntry { Role = x, Checked = _authorizationService.TryCheckAccess(OrchardCore.Contents.Permissions.DeleteOwnContent, UserSimulation.Create(x), null) })),
                    Preview = ContentPermissionsPartViewModel.SerializePermissions(allRoles.Select(x => new RoleEntry { Role = x, Checked = _authorizationService.TryCheckAccess(OrchardCore.Contents.Permissions.PreviewContent, UserSimulation.Create(x), null) })),
                    PreviewOwn = ContentPermissionsPartViewModel.SerializePermissions(allRoles.Select(x => new RoleEntry { Role =x, Checked = _authorizationService.TryCheckAccess(OrchardCore.Contents.Permissions.PreviewOwnContent, UserSimulation.Create(x), null) })),
                    DisplayedRoles = ContentPermissionsPartViewModel.SerializePermissions(allRoles.Select(x => new RoleEntry { Role = x, Checked = true })),
                };
            }

            var model = new ContentPermissionsPartViewModel {
                ViewRoles = ContentPermissionsPartViewModel.ExtractRoleEntries(allRoles, settings.View),
                ViewOwnRoles = ContentPermissionsPartViewModel.ExtractRoleEntries(allRoles, settings.ViewOwn),
                PublishRoles = ContentPermissionsPartViewModel.ExtractRoleEntries(allRoles, settings.Publish),
                PublishOwnRoles = ContentPermissionsPartViewModel.ExtractRoleEntries(allRoles, settings.PublishOwn),
                EditRoles = ContentPermissionsPartViewModel.ExtractRoleEntries(allRoles, settings.Edit),
                EditOwnRoles = ContentPermissionsPartViewModel.ExtractRoleEntries(allRoles, settings.EditOwn),
                DeleteRoles = ContentPermissionsPartViewModel.ExtractRoleEntries(allRoles, settings.Delete),
                DeleteOwnRoles = ContentPermissionsPartViewModel.ExtractRoleEntries(allRoles, settings.DeleteOwn),
                PreviewRoles = ContentPermissionsPartViewModel.ExtractRoleEntries(allRoles, settings.Preview),
                PreviewOwnRoles = ContentPermissionsPartViewModel.ExtractRoleEntries(allRoles, settings.PreviewOwn),
                AllRoles = ContentPermissionsPartViewModel.ExtractRoleEntries(allRoles, settings.DisplayedRoles)
            };

            // disable permissions the current user doesn't have
            model.ViewRoles = model.ViewRoles.Select(x => new RoleEntry { Role = x.Role, Checked = x.Checked, Enabled = _authorizer.Authorize(OrchardCore.Contents.Permissions.ViewContent) }).ToList();
            model.ViewOwnRoles = model.ViewOwnRoles.Select(x => new RoleEntry { Role = x.Role, Checked = x.Checked, Enabled = _authorizer.Authorize(OrchardCore.Contents.Permissions.ViewOwnContent) }).ToList();
            model.PublishRoles = model.PublishRoles.Select(x => new RoleEntry { Role = x.Role, Checked = x.Checked, Enabled = _authorizer.Authorize(OrchardCore.Contents.Permissions.PublishContent) }).ToList();
            model.PublishOwnRoles = model.PublishOwnRoles.Select(x => new RoleEntry { Role = x.Role, Checked = x.Checked, Enabled = _authorizer.Authorize(OrchardCore.Contents.Permissions.PublishOwnContent) }).ToList();
            model.EditRoles = model.EditRoles.Select(x => new RoleEntry { Role = x.Role, Checked = x.Checked, Enabled = _authorizer.Authorize(OrchardCore.Contents.Permissions.EditContent) }).ToList();
            model.EditOwnRoles = model.EditOwnRoles.Select(x => new RoleEntry { Role = x.Role, Checked = x.Checked, Enabled = _authorizer.Authorize(OrchardCore.Contents.Permissions.EditOwnContent) }).ToList();
            model.DeleteRoles = model.DeleteRoles.Select(x => new RoleEntry { Role = x.Role, Checked = x.Checked, Enabled = _authorizer.Authorize(OrchardCore.Contents.Permissions.DeleteContent) }).ToList();
            model.DeleteOwnRoles = model.DeleteOwnRoles.Select(x => new RoleEntry { Role = x.Role, Checked = x.Checked, Enabled = _authorizer.Authorize(OrchardCore.Contents.Permissions.DeleteOwnContent) }).ToList();
            model.PreviewRoles = model.PreviewRoles.Select(x => new RoleEntry { Role = x.Role, Checked = x.Checked, Enabled = _authorizer.Authorize(OrchardCore.Contents.Permissions.PreviewContent) }).ToList();
            model.PreviewOwnRoles = model.PreviewOwnRoles.Select(x => new RoleEntry { Role = x.Role, Checked = x.Checked, Enabled = _authorizer.Authorize(OrchardCore.Contents.Permissions.PreviewOwnContent) }).ToList();

            // initialize default value
            model.ViewRoles = model.ViewRoles.Select(x => new RoleEntry { Role = x.Role, Checked = x.Checked, Enabled = x.Enabled, Default = _authorizationService.TryCheckAccess(OrchardCore.Contents.Permissions.ViewContent, UserSimulation.Create(x.Role), null) }).ToList();
            model.ViewOwnRoles = model.ViewOwnRoles.Select(x => new RoleEntry { Role = x.Role, Checked = x.Checked, Enabled = x.Enabled, Default = _authorizationService.TryCheckAccess(OrchardCore.Contents.Permissions.ViewOwnContent, UserSimulation.Create(x.Role), null) }).ToList();
            model.PublishRoles = model.PublishRoles.Select(x => new RoleEntry { Role = x.Role, Checked = x.Checked, Enabled = x.Enabled, Default = _authorizationService.TryCheckAccess(OrchardCore.Contents.Permissions.PublishContent, UserSimulation.Create(x.Role), null) }).ToList();
            model.PublishOwnRoles = model.PublishOwnRoles.Select(x => new RoleEntry { Role = x.Role, Checked = x.Checked, Enabled = x.Enabled, Default = _authorizationService.TryCheckAccess(OrchardCore.Contents.Permissions.PublishOwnContent, UserSimulation.Create(x.Role), null) }).ToList();
            model.EditRoles = model.EditRoles.Select(x => new RoleEntry { Role = x.Role, Checked = x.Checked, Enabled = x.Enabled, Default = _authorizationService.TryCheckAccess(OrchardCore.Contents.Permissions.EditContent, UserSimulation.Create(x.Role), null) }).ToList();
            model.EditOwnRoles = model.EditOwnRoles.Select(x => new RoleEntry { Role = x.Role, Checked = x.Checked, Enabled = x.Enabled, Default = _authorizationService.TryCheckAccess(OrchardCore.Contents.Permissions.EditOwnContent, UserSimulation.Create(x.Role), null) }).ToList();
            model.DeleteRoles = model.DeleteRoles.Select(x => new RoleEntry { Role = x.Role, Checked = x.Checked, Enabled = x.Enabled, Default = _authorizationService.TryCheckAccess(OrchardCore.Contents.Permissions.DeleteContent, UserSimulation.Create(x.Role), null) }).ToList();
            model.DeleteOwnRoles = model.DeleteOwnRoles.Select(x => new RoleEntry { Role = x.Role, Checked = x.Checked, Enabled = x.Enabled, Default = _authorizationService.TryCheckAccess(OrchardCore.Contents.Permissions.DeleteOwnContent, UserSimulation.Create(x.Role), null) }).ToList();
            model.PreviewRoles = model.PreviewRoles.Select(x => new RoleEntry { Role = x.Role, Checked = x.Checked, Enabled = x.Enabled, Default = _authorizationService.TryCheckAccess(OrchardCore.Contents.Permissions.PreviewContent, UserSimulation.Create(x.Role), null) }).ToList();
            model.PreviewOwnRoles = model.PreviewOwnRoles.Select(x => new RoleEntry { Role = x.Role, Checked = x.Checked, Enabled = x.Enabled, Default = _authorizationService.TryCheckAccess(OrchardCore.Contents.Permissions.PreviewOwnContent, UserSimulation.Create(x.Role), null) }).ToList();

            yield return DefinitionTemplate(model);
        }

        public override IEnumerable<TemplateViewModel> TypePartEditorUpdate(ContentTypePartDefinitionBuilder builder, IUpdateModel updateModel) {
            if (builder.Name != "ContentPermissionsPart")
                yield break;

            if (!_authorizer.Authorize(Permissions.GrantPermission)) {
                yield break;
            }

            var allRoles = _roleProvider.GetRoleNamesAsync().Result.ToList();

            var model = new ContentPermissionsPartViewModel();

            updateModel.TryUpdateModel(model, "ContentPermissionsPartViewModel", null, null);
            
            // update permissions only for those the current user is granted
            if ( _authorizer.Authorize(OrchardCore.Contents.Permissions.ViewContent)) {
                builder.WithSetting("ContentPermissionsPartSettings.View", ContentPermissionsPartViewModel.SerializePermissions(model.ViewRoles));
            }

            if (_authorizer.Authorize(OrchardCore.Contents.Permissions.ViewOwnContent)) {
                builder.WithSetting("ContentPermissionsPartSettings.ViewOwn", ContentPermissionsPartViewModel.SerializePermissions(model.ViewOwnRoles));
            }

            if (_authorizer.Authorize(OrchardCore.Contents.Permissions.PublishContent)) {
                builder.WithSetting("ContentPermissionsPartSettings.Publish", ContentPermissionsPartViewModel.SerializePermissions(model.PublishRoles));
            }

            if (_authorizer.Authorize(OrchardCore.Contents.Permissions.PublishOwnContent)) {
                builder.WithSetting("ContentPermissionsPartSettings.PublishOwn", ContentPermissionsPartViewModel.SerializePermissions(model.PublishOwnRoles));
            }

            if (_authorizer.Authorize(OrchardCore.Contents.Permissions.EditContent)) {
                builder.WithSetting("ContentPermissionsPartSettings.Edit", ContentPermissionsPartViewModel.SerializePermissions(model.EditRoles));
            }

            if (_authorizer.Authorize(OrchardCore.Contents.Permissions.EditOwnContent)) {
                builder.WithSetting("ContentPermissionsPartSettings.EditOwn", ContentPermissionsPartViewModel.SerializePermissions(model.EditOwnRoles));
            }

            if (_authorizer.Authorize(OrchardCore.Contents.Permissions.DeleteContent)) {
                builder.WithSetting("ContentPermissionsPartSettings.Delete", ContentPermissionsPartViewModel.SerializePermissions(model.DeleteRoles));
            }

            if (_authorizer.Authorize(OrchardCore.Contents.Permissions.DeleteOwnContent)) {
                builder.WithSetting("ContentPermissionsPartSettings.DeleteOwn", ContentPermissionsPartViewModel.SerializePermissions(model.DeleteOwnRoles));
            }

            if (_authorizer.Authorize(OrchardCore.Contents.Permissions.PreviewContent)) {
                builder.WithSetting("ContentPermissionsPartSettings.Preview", ContentPermissionsPartViewModel.SerializePermissions(model.PreviewRoles));
            }

            if (_authorizer.Authorize(OrchardCore.Contents.Permissions.PreviewOwnContent)) {
                builder.WithSetting("ContentPermissionsPartSettings.PreviewOwn", ContentPermissionsPartViewModel.SerializePermissions(model.PreviewOwnRoles));
            }

            builder.WithSetting("ContentPermissionsPartSettings.DisplayedRoles", ContentPermissionsPartViewModel.SerializePermissions(model.AllRoles));

            // disable permissions the current user doesn't have
            model.ViewRoles = model.ViewRoles.Select(x => new RoleEntry { Role = x.Role, Checked = x.Checked, Enabled = _authorizer.Authorize(OrchardCore.Contents.Permissions.ViewContent) }).ToList();
            model.ViewOwnRoles = model.ViewOwnRoles.Select(x => new RoleEntry { Role = x.Role, Checked = x.Checked, Enabled = _authorizer.Authorize(OrchardCore.Contents.Permissions.ViewOwnContent) }).ToList();
            model.PublishRoles = model.PublishRoles.Select(x => new RoleEntry { Role = x.Role, Checked = x.Checked, Enabled = _authorizer.Authorize(OrchardCore.Contents.Permissions.PublishContent) }).ToList();
            model.PublishOwnRoles = model.PublishOwnRoles.Select(x => new RoleEntry { Role = x.Role, Checked = x.Checked, Enabled = _authorizer.Authorize(OrchardCore.Contents.Permissions.PublishOwnContent) }).ToList();
            model.EditRoles = model.EditRoles.Select(x => new RoleEntry { Role = x.Role, Checked = x.Checked, Enabled = _authorizer.Authorize(OrchardCore.Contents.Permissions.EditContent) }).ToList();
            model.EditOwnRoles = model.EditOwnRoles.Select(x => new RoleEntry { Role = x.Role, Checked = x.Checked, Enabled = _authorizer.Authorize(OrchardCore.Contents.Permissions.EditOwnContent) }).ToList();
            model.DeleteRoles = model.DeleteRoles.Select(x => new RoleEntry { Role = x.Role, Checked = x.Checked, Enabled = _authorizer.Authorize(OrchardCore.Contents.Permissions.DeleteContent) }).ToList();
            model.DeleteOwnRoles = model.DeleteOwnRoles.Select(x => new RoleEntry { Role = x.Role, Checked = x.Checked, Enabled = _authorizer.Authorize(OrchardCore.Contents.Permissions.DeleteOwnContent) }).ToList();
            model.PreviewRoles = model.PreviewRoles.Select(x => new RoleEntry { Role = x.Role, Checked = x.Checked, Enabled = _authorizer.Authorize(OrchardCore.Contents.Permissions.PreviewContent) }).ToList();
            model.PreviewOwnRoles = model.PreviewOwnRoles.Select(x => new RoleEntry { Role = x.Role, Checked = x.Checked, Enabled = _authorizer.Authorize(OrchardCore.Contents.Permissions.PreviewOwnContent) }).ToList();

            // initialize default value
            model.ViewRoles = model.ViewRoles.Select(x => new RoleEntry { Role = x.Role, Checked = x.Checked, Enabled = x.Enabled, Default = _authorizationService.TryCheckAccess(OrchardCore.Contents.Permissions.ViewContent, UserSimulation.Create(x.Role), null) }).ToList();
            model.ViewOwnRoles = model.ViewOwnRoles.Select(x => new RoleEntry { Role = x.Role, Checked = x.Checked, Enabled = x.Enabled, Default = _authorizationService.TryCheckAccess(OrchardCore.Contents.Permissions.ViewOwnContent, UserSimulation.Create(x.Role), null) }).ToList();
            model.PublishRoles = model.PublishRoles.Select(x => new RoleEntry { Role = x.Role, Checked = x.Checked, Enabled = x.Enabled, Default = _authorizationService.TryCheckAccess(OrchardCore.Contents.Permissions.PublishContent, UserSimulation.Create(x.Role), null) }).ToList();
            model.PublishOwnRoles = model.PublishOwnRoles.Select(x => new RoleEntry { Role = x.Role, Checked = x.Checked, Enabled = x.Enabled, Default = _authorizationService.TryCheckAccess(OrchardCore.Contents.Permissions.PublishOwnContent, UserSimulation.Create(x.Role), null) }).ToList();
            model.EditRoles = model.EditRoles.Select(x => new RoleEntry { Role = x.Role, Checked = x.Checked, Enabled = x.Enabled, Default = _authorizationService.TryCheckAccess(OrchardCore.Contents.Permissions.EditContent, UserSimulation.Create(x.Role), null) }).ToList();
            model.EditOwnRoles = model.EditOwnRoles.Select(x => new RoleEntry { Role = x.Role, Checked = x.Checked, Enabled = x.Enabled, Default = _authorizationService.TryCheckAccess(OrchardCore.Contents.Permissions.EditOwnContent, UserSimulation.Create(x.Role), null) }).ToList();
            model.DeleteRoles = model.DeleteRoles.Select(x => new RoleEntry { Role = x.Role, Checked = x.Checked, Enabled = x.Enabled, Default = _authorizationService.TryCheckAccess(OrchardCore.Contents.Permissions.DeleteContent, UserSimulation.Create(x.Role), null) }).ToList();
            model.DeleteOwnRoles = model.DeleteOwnRoles.Select(x => new RoleEntry { Role = x.Role, Checked = x.Checked, Enabled = x.Enabled, Default = _authorizationService.TryCheckAccess(OrchardCore.Contents.Permissions.DeleteOwnContent, UserSimulation.Create(x.Role), null) }).ToList();
            model.PreviewRoles = model.PreviewRoles.Select(x => new RoleEntry { Role = x.Role, Checked = x.Checked, Enabled = x.Enabled, Default = _authorizationService.TryCheckAccess(OrchardCore.Contents.Permissions.PreviewContent, UserSimulation.Create(x.Role), null) }).ToList();
            model.PreviewOwnRoles = model.PreviewOwnRoles.Select(x => new RoleEntry { Role = x.Role, Checked = x.Checked, Enabled = x.Enabled, Default = _authorizationService.TryCheckAccess(OrchardCore.Contents.Permissions.PreviewOwnContent, UserSimulation.Create(x.Role), null) }).ToList();

            yield return DefinitionTemplate(model);
        }
    }
}