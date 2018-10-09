using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using OrchardCore;
using OrchardCore.ContentManagement;

public static class TaxonomyOrchardHelperExtensions
{
    /// <summary>
    /// Returns a the term from its content item id.
    /// </summary>
    /// <param name="taxonomyContentItemId">The taxonomy content item id.</param>
    /// <param name="termContentItemId">The term content item id.</param>
    /// <returns>A content item id <c>null</c> if it was not found.</returns>
    public static async Task<ContentItem> GetTaxonomyTermAsync(this IOrchardHelper orchardHelper, string taxonomyContentItemId, string termContentItemId)
    {
        var contentManager = orchardHelper.HttpContext.RequestServices.GetService<IContentManager>();
        var taxonomy = await contentManager.GetAsync(taxonomyContentItemId);

        if (taxonomy == null)
        {
            return null;
        }

        return FindTerm(taxonomy.Content.TaxonomyPart.Terms as JArray, termContentItemId);
    }

    /// <summary>
    /// Returns the list of terms including its parents.
    /// </summary>
    /// <param name="taxonomyContentItemId">The taxonomy content item id.</param>
    /// <param name="termContentItemId">The term content item id.</param>
    /// <returns>A list content items.</returns>
    public static async Task<List<ContentItem>> GetTaxonomyAllTermsAsync(this IOrchardHelper orchardHelper, string taxonomyContentItemId, string termContentItemId)
    {
        var contentManager = orchardHelper.HttpContext.RequestServices.GetService<IContentManager>();
        var taxonomy = await contentManager.GetAsync(taxonomyContentItemId);

        if (taxonomy == null)
        {
            return null;
        }

        var terms = new List<ContentItem>();

        FindTermHierarchy(taxonomy.Content.Terms as JArray, termContentItemId, terms);

        return terms;
    }

    private static ContentItem FindTerm(JArray termsArray, string termContentItemId)
    {
        foreach(JObject term in termsArray)
        {
            string contentItemId = term.GetValue("ContentItemId").ToString();

            if (contentItemId == termContentItemId)
            {
                return term.ToObject<ContentItem>();
            }

            if (term.GetValue("Terms") is JArray children)
            {
                var found = FindTerm(children, termContentItemId);

                if (found != null)
                {
                    return found;
                }
            }
        }

        return null;
    }

    private static bool FindTermHierarchy(JArray termsArray, string termContentItemId, List<ContentItem> terms)
    {
        foreach (JObject term in termsArray)
        {
            string contentItemId = term.GetValue("ContentItemId").ToString();

            if (contentItemId == termContentItemId)
            {
                terms.Add(term.ToObject<ContentItem>());

                return true;
            }

            if (term.GetValue("Terms") is JArray children)
            {
                var found = FindTermHierarchy(children, termContentItemId, terms);

                if (found)
                {
                    terms.Add(term.ToObject<ContentItem>());

                    return true;
                }
            }
        }

        return false;
    }

}
