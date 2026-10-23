using System.Drawing;
using ScintillaNET;
using WebResourceManager.Models;

namespace WebResourceManager
{
    /// <summary>
    /// Sets up Scintilla syntax highlighting for each text-based web resource type.
    /// </summary>
    internal static class CodeStyler
    {
        private const string JavaScriptKeywords =
            "async await break case catch class const continue debugger default delete do else export extends " +
            "finally for function if import in instanceof let new of return static super switch this throw try " +
            "typeof var void while with yield";

        private const string JavaScriptLiterals = "true false null undefined NaN Infinity Xrm window document console";

        private static readonly Color CommentColor = Color.FromArgb(0, 128, 0);
        private static readonly Color StringColor = Color.FromArgb(163, 21, 21);
        private static readonly Color KeywordColor = Color.FromArgb(0, 0, 255);
        private static readonly Color NumberColor = Color.FromArgb(9, 134, 88);
        private static readonly Color TagColor = Color.FromArgb(128, 0, 0);
        private static readonly Color AttributeColor = Color.FromArgb(255, 0, 0);

        public static void Apply(Scintilla editor, WebResourceType type)
        {
            editor.StyleResetDefault();
            editor.Styles[Style.Default].Font = "Consolas";
            editor.Styles[Style.Default].Size = 10;
            editor.StyleClearAll();
            editor.Styles[Style.LineNumber].ForeColor = Color.Gray;

            switch (type)
            {
                case WebResourceType.JavaScript:
                    ApplyJavaScript(editor);
                    break;
                case WebResourceType.Css:
                    ApplyCss(editor);
                    break;
                case WebResourceType.Html:
                    ApplyHtml(editor);
                    break;
                case WebResourceType.Xml:
                case WebResourceType.Xsl:
                case WebResourceType.Resx:
                case WebResourceType.Svg:
                    ApplyXml(editor);
                    break;
                default:
                    editor.Lexer = Lexer.Null;
                    break;
            }
        }

        private static void ApplyJavaScript(Scintilla editor)
        {
            editor.Lexer = Lexer.Cpp;
            editor.SetKeywords(0, JavaScriptKeywords);
            editor.SetKeywords(1, JavaScriptLiterals);
            editor.Styles[Style.Cpp.Comment].ForeColor = CommentColor;
            editor.Styles[Style.Cpp.CommentLine].ForeColor = CommentColor;
            editor.Styles[Style.Cpp.CommentDoc].ForeColor = CommentColor;
            editor.Styles[Style.Cpp.String].ForeColor = StringColor;
            editor.Styles[Style.Cpp.Character].ForeColor = StringColor;
            editor.Styles[Style.Cpp.StringEol].ForeColor = StringColor;
            editor.Styles[Style.Cpp.Regex].ForeColor = StringColor;
            editor.Styles[Style.Cpp.Number].ForeColor = NumberColor;
            editor.Styles[Style.Cpp.Word].ForeColor = KeywordColor;
            editor.Styles[Style.Cpp.Word2].ForeColor = Color.FromArgb(43, 145, 175);
        }

        private static void ApplyCss(Scintilla editor)
        {
            editor.Lexer = Lexer.Css;
            editor.Styles[Style.Css.Comment].ForeColor = CommentColor;
            editor.Styles[Style.Css.Tag].ForeColor = TagColor;
            editor.Styles[Style.Css.Class].ForeColor = TagColor;
            editor.Styles[Style.Css.Id].ForeColor = TagColor;
            editor.Styles[Style.Css.Identifier].ForeColor = AttributeColor;
            editor.Styles[Style.Css.Value].ForeColor = KeywordColor;
            editor.Styles[Style.Css.DoubleString].ForeColor = StringColor;
            editor.Styles[Style.Css.SingleString].ForeColor = StringColor;
            editor.Styles[Style.Css.Important].ForeColor = KeywordColor;
        }

        private static void ApplyHtml(Scintilla editor)
        {
            editor.Lexer = Lexer.Html;
            editor.Styles[Style.Html.Tag].ForeColor = TagColor;
            editor.Styles[Style.Html.TagUnknown].ForeColor = TagColor;
            editor.Styles[Style.Html.Attribute].ForeColor = AttributeColor;
            editor.Styles[Style.Html.AttributeUnknown].ForeColor = AttributeColor;
            editor.Styles[Style.Html.DoubleString].ForeColor = KeywordColor;
            editor.Styles[Style.Html.SingleString].ForeColor = KeywordColor;
            editor.Styles[Style.Html.Comment].ForeColor = CommentColor;
            editor.Styles[Style.Html.Entity].ForeColor = NumberColor;
        }

        private static void ApplyXml(Scintilla editor)
        {
            editor.Lexer = Lexer.Xml;
            editor.Styles[Style.Xml.Tag].ForeColor = TagColor;
            editor.Styles[Style.Xml.TagEnd].ForeColor = TagColor;
            editor.Styles[Style.Xml.Attribute].ForeColor = AttributeColor;
            editor.Styles[Style.Xml.DoubleString].ForeColor = KeywordColor;
            editor.Styles[Style.Xml.SingleString].ForeColor = KeywordColor;
            editor.Styles[Style.Xml.Comment].ForeColor = CommentColor;
            editor.Styles[Style.Xml.CData].ForeColor = StringColor;
            editor.Styles[Style.Xml.Entity].ForeColor = NumberColor;
        }
    }
}
