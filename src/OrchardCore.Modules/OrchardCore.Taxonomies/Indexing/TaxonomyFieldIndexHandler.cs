using System.Threading.Tasks;
using OrchardCore.Indexing;
using OrchardCore.Taxonomies.Fields;

namespace OrchardCore.Taxonomies.Indexing
{
    public class TaxonomyFieldIndexHandler : ContentFieldIndexHandler<TaxonomyField>
    {
        public override Task BuildIndexAsync(TaxonomyField field, BuildFieldIndexContext context)
        {
            // TODO: Also add the parents of each term, probably as a separate field

            var options = context.Settings.ToOptions();
            options |= DocumentIndexOptions.Store;

            foreach (var contentItemId in field.TermContentItemIds)
            {
                context.DocumentIndex.Entries.Add(context.Key, new DocumentIndex.DocumentIndexEntry(contentItemId, DocumentIndex.Types.Text, options));
            }

            return Task.CompletedTask;
        }
    }
}
