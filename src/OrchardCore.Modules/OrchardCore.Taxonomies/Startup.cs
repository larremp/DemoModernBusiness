using Microsoft.Extensions.DependencyInjection;
using Fluid;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.Data.Migration;
using OrchardCore.Modules;
using OrchardCore.Security.Permissions;
using OrchardCore.Taxonomies.Drivers;
using OrchardCore.Taxonomies.Fields;
using OrchardCore.Taxonomies.Handlers;
using OrchardCore.Taxonomies.Models;
using OrchardCore.Taxonomies.ViewModels;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.Indexing;
using OrchardCore.Taxonomies.Settings;
using OrchardCore.Taxonomies.Indexing;

namespace OrchardCore.Taxonomies
{
    public class Startup : StartupBase
    {
        static Startup()
        {
            // Registering both field types and shape types are necessary as they can 
            // be accessed from inner properties.

            TemplateContext.GlobalMemberAccessStrategy.Register<TaxonomyField>();
            TemplateContext.GlobalMemberAccessStrategy.Register<DisplayTaxonomyFieldViewModel>();
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IDataMigration, Migrations>();
            services.AddScoped<IPermissionProvider, Permissions>();

            // Taxonomy Part
            services.AddScoped<IContentHandler, TaxonomyContentHandler>();
            services.AddScoped<IContentPartDisplayDriver, TaxonomyPartDisplayDriver>();
            services.AddSingleton<ContentPart, TaxonomyPart>();

            // Taxonomy Field
            services.AddSingleton<ContentField, TaxonomyField>();
            services.AddScoped<IContentFieldDisplayDriver, TaxonomyFieldDisplayDriver>();
            services.AddScoped<IContentPartFieldDefinitionDisplayDriver, TaxonomyFieldSettingsDriver>();
            services.AddScoped<IContentFieldIndexHandler, TaxonomyFieldIndexHandler>();
        }
    }
}
