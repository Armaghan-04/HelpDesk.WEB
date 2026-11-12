using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace HelpDesk.WEB.TagHelpers
{
    /// <summary>
    /// Opt-in tag helper for textareas that need a character counter. It applies
    /// the shared maximum length, renders the live counter and keeps the
    /// validation message in the same row as the counter. Works with
    /// <c>asp-for</c> or with an explicit <c>id</c>/<c>name</c>.
    /// </summary>
    [HtmlTargetElement("textarea", Attributes = CountedAttribute)]
    public sealed class CountedTextareaTagHelper : TagHelper
    {
        private const string CountedAttribute = "hd-counted";

        [HtmlAttributeName(CountedAttribute)]
        public int MaxLength { get; set; }

        [HtmlAttributeName("asp-for")]
        public ModelExpression? For { get; set; }

        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext ViewContext { get; set; } = null!;

        // Run after the built-in TextAreaTagHelper has produced id/name/value.
        public override int Order => 100;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.Attributes.RemoveAll(CountedAttribute);

            if (MaxLength <= 0)
                return;

            output.Attributes.SetAttribute("maxlength", MaxLength);
            output.Attributes.SetAttribute("data-counter", "true");

            string id = output.Attributes["id"]?.Value?.ToString() ?? string.Empty;
            string name = output.Attributes["name"]?.Value?.ToString() ?? For?.Name ?? string.Empty;

            if (string.IsNullOrEmpty(id))
                return;

            TagBuilder footer = new("div");
            footer.AddCssClass("d-flex");
            footer.AddCssClass("justify-content-between");
            footer.AddCssClass("align-items-start");
            footer.AddCssClass("gap-3");
            footer.AddCssClass("mt-2");

            if (!string.IsNullOrEmpty(name))
                footer.InnerHtml.AppendHtml(BuildValidationMessage(name));

            TagBuilder counter = new("span");
            counter.AddCssClass("hd-counter");
            counter.AddCssClass("text-muted");
            counter.Attributes["data-counter-for"] = id;
            counter.InnerHtml.Append($"0 / {MaxLength}");
            footer.InnerHtml.AppendHtml(counter);

            output.PostElement.AppendHtml(footer);
        }

        private TagBuilder BuildValidationMessage(string name)
        {
            bool hasError = ViewContext.ModelState.TryGetValue(name, out ModelStateEntry? entry)
                && entry.Errors.Count > 0;

            TagBuilder validation = new("span");
            validation.AddCssClass(
                hasError ? "field-validation-error" : "field-validation-valid");
            validation.Attributes["data-valmsg-for"] = name;
            validation.Attributes["data-valmsg-replace"] = "true";

            if (hasError)
            {
                string message = string.Join(
                    " ",
                    entry!.Errors
                        .Select(error => error.ErrorMessage)
                        .Where(error => !string.IsNullOrWhiteSpace(error)));

                validation.InnerHtml.Append(message);
            }

            return validation;
        }
    }
}
