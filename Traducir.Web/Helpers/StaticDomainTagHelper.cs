using System;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Configuration;
using StackExchange.Exceptional.Internal;

namespace Traducir.Web.Helpers
{
    [HtmlTargetElement("script", Attributes = "use-static-domain", TagStructure = TagStructure.WithoutEndTag)]
    [HtmlTargetElement("link", Attributes = "use-static-domain", TagStructure = TagStructure.WithoutEndTag)]
    public class StaticDomainTagHelper : TagHelper
    {
        public StaticDomainTagHelper(IConfiguration configuration)
        {
            StaticDomain = configuration.GetValue<string>("STATIC_DOMAIN");
        }

        public override int Order => 1000;

        private string StaticDomain { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (output == null)
            {
                throw new ArgumentNullException(nameof(output));
            }

            if (StaticDomain.HasValue())
            {
                void processAttribute(string attrName)
                {
                    if (context.AllAttributes.ContainsName(attrName))
                    {
                        output.CopyHtmlAttribute(attrName, context);
                        if (output.Attributes.TryGetAttribute(attrName, out var srcAttr))
                        {
                            output.Attributes.SetAttribute(attrName, "https://" + StaticDomain + srcAttr.Value);
                        }
                    }
                }

                processAttribute("src");
                processAttribute("href");
            }

            if (output.Attributes.TryGetAttribute("use-static-domain", out var customAttr))
            {
                output.Attributes.Remove(customAttr);
            }
        }
    }
}