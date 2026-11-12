using Microsoft.AspNetCore.Razor.TagHelpers;

namespace HelpDesk.WEB.TagHelpers
{
    /// <summary>
    /// Shared chrome for GET filter forms: card styling, the search role and the
    /// Bootstrap row that the entity-specific filter columns live in.
    /// </summary>
    [HtmlTargetElement("form", Attributes = FilterAttribute)]
    public sealed class FilterFormTagHelper : TagHelper
    {
        private const string FilterAttribute = "hd-filter";

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.Attributes.RemoveAll(FilterAttribute);
            output.Attributes.SetAttribute("method", "get");
            output.Attributes.SetAttribute("role", "search");
            output.Attributes.SetAttribute("class", "hd-card hd-filter mb-4");
            output.PreContent.AppendHtml("<div class=\"row g-3 align-items-end\">");
            output.PostContent.AppendHtml("</div>");
        }
    }
}
