using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.Data.Migration;
using OrchardCore.Modules;
using OrchardCore.Security.Permissions;
using OrchardCore.Taxonomies.Drivers;
using OrchardCore.Taxonomies.Handlers;
using OrchardCore.Taxonomies.Models;

namespace OrchardCore.Taxonomies
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IDataMigration, Migrations>();
            services.AddScoped<IPermissionProvider, Permissions>();

            // MenuPart
            services.AddScoped<IContentHandler, TaxonomyContentHandler>();
            services.AddScoped<IContentPartDisplayDriver, TaxonomyPartDisplayDriver>();
            services.AddSingleton<ContentPart, TaxonomyPart>();
        }
    }
}
