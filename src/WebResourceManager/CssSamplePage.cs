namespace WebResourceManager
{
    /// <summary>
    /// A page of common elements to show what a CSS web resource looks like when applied.
    /// </summary>
    internal static class CssSamplePage
    {
        private const string Body = @"
<h1>Heading 1</h1>
<h2>Heading 2</h2>
<h3>Heading 3</h3>
<p>A paragraph with <a href=""#"">a link</a>, <strong>bold</strong>, <em>italic</em> and <code>code</code>.</p>
<ul><li>List item one</li><li>List item two</li></ul>
<p><button>Button</button> <input type=""text"" placeholder=""Text input""> <select><option>Option</option></select></p>
<table>
  <thead><tr><th>Column A</th><th>Column B</th></tr></thead>
  <tbody><tr><td>Cell 1</td><td>Cell 2</td></tr><tr><td>Cell 3</td><td>Cell 4</td></tr></tbody>
</table>";

        public static string Build(string css)
        {
            // Keep the stylesheet from closing its own style tag early
            var safeCss = (css ?? string.Empty).Replace("</style", "<\\/style");
            return "<!DOCTYPE html><html><head><meta charset=\"utf-8\"><style>" + safeCss + "</style></head><body>" + Body + "</body></html>";
        }
    }
}
