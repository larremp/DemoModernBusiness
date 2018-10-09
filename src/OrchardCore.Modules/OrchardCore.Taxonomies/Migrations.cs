using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;

namespace OrchardCore.Taxonomies
{
    public class Migrations : DataMigration
    {
        IContentDefinitionManager _contentDefinitionManager;

        public Migrations(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }

        public int Create()
        {
            _contentDefinitionManager.AlterTypeDefinition("Taxonomy", menu => menu
                .Draftable()
                .Versionable()
                .Creatable()
                .Listable()
                .WithPart("TitlePart", part => part.WithPosition("1"))
                .WithPart("AliasPart", part => part.WithPosition("2").WithSettings(new AliasPartSettings { Pattern = "{{ ContentItem | display_text | slugify }}" }))
            );

            return 1;
        }

        public int UpdateFrom1()
        {
            _contentDefinitionManager.AlterTypeDefinition("Taxonomy", menu => menu
                .WithPart("TaxonomyPart", part => part.WithPosition("3"))
                //.WithPart("TermsListPart", part => part.WithPosition("4"))
            );

            return 2;
        }
    }

    class AliasPartSettings
    {
        public string Pattern { get; set; }
    }
}
