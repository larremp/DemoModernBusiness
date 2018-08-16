using System.Collections.Generic;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.MetaData;
using OrchardCore.ContentManagement.MetaData.Builders;
using OrchardCore.ContentManagement.MetaData.Models;
using OrchardCore.ContentManagement.ViewModels;
using OrchardCore.ContentPermissions.ViewModels;

namespace OrchardCore.ContentPermissions.Settings {
    public class SecurableContentItemsEditorEvents : ContentDefinitionEditorEventsBase {

        public override IEnumerable<TemplateViewModel> TypeEditor(ContentTypeDefinition definition) {
            var settings = definition.Settings.GetModel<ContentPermissionsContentTypeSettings>();
            var model = new SecurableContentItemsSettingsViewModel {
                SecurableContentItems = settings.SecurableContentItems,
            };

            yield return DefinitionTemplate(model);
        }

        public override IEnumerable<TemplateViewModel> TypeEditorUpdate(ContentTypeDefinitionBuilder builder, IUpdateModel updateModel) {
            var model = new SecurableContentItemsSettingsViewModel();
            updateModel.TryUpdateModel(model, "SecurableContentItemsSettingsViewModel", null, null);

            builder.WithSetting("ContentPermissionsTypeSettings.SecurableContentItems", model.SecurableContentItems.ToString());

            yield return DefinitionTemplate(model);
        }
    }
}