using System.Collections.Generic;

namespace WebResourceManager.Banners
{
    /// <summary>
    /// A built-in banner icon. Markup is the inner content of a 24x24 SVG drawn
    /// with strokes in currentColor, so the icon picks up the chosen icon colour.
    /// Shapes are based on Feather Icons (MIT licence, https://feathericons.com);
    /// the Law and Accounting shapes not in Feather are drawn in the same style.
    /// </summary>
    public class BannerIcon
    {
        public const string General = "General";
        public const string Law = "Law";
        public const string Accounting = "Accounting";
        public const string Sales = "Sales";

        public BannerIcon(string key, string label, string markup, string category = General)
        {
            Key = key;
            Label = label;
            Markup = markup;
            Category = category;
        }

        /// <summary>
        /// Heading the icon is grouped under in the icon gallery.
        /// </summary>
        public string Category { get; private set; }

        public string Key { get; private set; }

        public string Label { get; private set; }

        public string Markup { get; private set; }

        public override string ToString()
        {
            return Label;
        }

        public static readonly BannerIcon None = new BannerIcon("none", "(No icon)", string.Empty);

        public static readonly IList<BannerIcon> All = new List<BannerIcon>
        {
            None,
            new BannerIcon("document", "Document", "<path d=\"M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z\"/><polyline points=\"14 2 14 8 20 8\"/><line x1=\"16\" y1=\"13\" x2=\"8\" y2=\"13\"/><line x1=\"16\" y1=\"17\" x2=\"8\" y2=\"17\"/>"),
            new BannerIcon("info", "Information", "<circle cx=\"12\" cy=\"12\" r=\"10\"/><line x1=\"12\" y1=\"16\" x2=\"12\" y2=\"12\"/><line x1=\"12\" y1=\"8\" x2=\"12.01\" y2=\"8\"/>"),
            new BannerIcon("warning", "Warning", "<path d=\"M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z\"/><line x1=\"12\" y1=\"9\" x2=\"12\" y2=\"13\"/><line x1=\"12\" y1=\"17\" x2=\"12.01\" y2=\"17\"/>"),
            new BannerIcon("error", "Error", "<circle cx=\"12\" cy=\"12\" r=\"10\"/><line x1=\"15\" y1=\"9\" x2=\"9\" y2=\"15\"/><line x1=\"9\" y1=\"9\" x2=\"15\" y2=\"15\"/>"),
            new BannerIcon("success", "Success", "<path d=\"M22 11.08V12a10 10 0 1 1-5.93-9.14\"/><polyline points=\"22 4 12 14.01 9 11.01\"/>"),
            new BannerIcon("bell", "Notification", "<path d=\"M18 8A6 6 0 0 0 6 8c0 7-3 9-3 9h18s-3-2-3-9\"/><path d=\"M13.73 21a2 2 0 0 1-3.46 0\"/>"),
            new BannerIcon("user", "Person", "<path d=\"M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2\"/><circle cx=\"12\" cy=\"7\" r=\"4\"/>"),
            new BannerIcon("briefcase", "Business", "<rect x=\"2\" y=\"7\" width=\"20\" height=\"14\" rx=\"2\" ry=\"2\"/><path d=\"M16 21V5a2 2 0 0 0-2-2h-4a2 2 0 0 0-2 2v16\"/>"),
            new BannerIcon("home", "Home", "<path d=\"M3 9l9-7 9 7v11a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z\"/><polyline points=\"9 22 9 12 15 12 15 22\"/>"),
            new BannerIcon("calendar", "Calendar", "<rect x=\"3\" y=\"4\" width=\"18\" height=\"18\" rx=\"2\" ry=\"2\"/><line x1=\"16\" y1=\"2\" x2=\"16\" y2=\"6\"/><line x1=\"8\" y1=\"2\" x2=\"8\" y2=\"6\"/><line x1=\"3\" y1=\"10\" x2=\"21\" y2=\"10\"/>"),
            new BannerIcon("clock", "Clock", "<circle cx=\"12\" cy=\"12\" r=\"10\"/><polyline points=\"12 6 12 12 16 14\"/>"),
            new BannerIcon("mail", "Email", "<path d=\"M4 4h16c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z\"/><polyline points=\"22,6 12,13 2,6\"/>"),
            new BannerIcon("dollar", "Money", "<line x1=\"12\" y1=\"1\" x2=\"12\" y2=\"23\"/><path d=\"M17 5H9.5a3.5 3.5 0 0 0 0 7h5a3.5 3.5 0 0 1 0 7H6\"/>"),
            new BannerIcon("tag", "Tag", "<path d=\"M20.59 13.41l-7.17 7.17a2 2 0 0 1-2.83 0L2 12V2h10l8.59 8.59a2 2 0 0 1 0 2.82z\"/><line x1=\"7\" y1=\"7\" x2=\"7.01\" y2=\"7\"/>"),
            new BannerIcon("star", "Star", "<polygon points=\"12 2 15.09 8.26 22 9.27 17 14.14 18.18 21.02 12 17.77 5.82 21.02 7 14.14 2 9.27 8.91 8.26 12 2\"/>"),
            new BannerIcon("heart", "Heart", "<path d=\"M20.84 4.61a5.5 5.5 0 0 0-7.78 0L12 5.67l-1.06-1.06a5.5 5.5 0 0 0-7.78 7.78l1.06 1.06L12 21.23l7.78-7.78 1.06-1.06a5.5 5.5 0 0 0 0-7.78z\"/>"),
            new BannerIcon("flag", "Flag", "<path d=\"M4 15s1-1 4-1 5 2 8 2 4-1 4-1V3s-1 1-4 1-5-2-8-2-4 1-4 1z\"/><line x1=\"4\" y1=\"22\" x2=\"4\" y2=\"15\"/>"),
            new BannerIcon("lock", "Locked", "<rect x=\"3\" y=\"11\" width=\"18\" height=\"11\" rx=\"2\" ry=\"2\"/><path d=\"M7 11V7a5 5 0 0 1 10 0v4\"/>"),
            new BannerIcon("shield", "Shield", "<path d=\"M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z\"/>"),
            new BannerIcon("globe", "Globe", "<circle cx=\"12\" cy=\"12\" r=\"10\"/><line x1=\"2\" y1=\"12\" x2=\"22\" y2=\"12\"/><path d=\"M12 2a15.3 15.3 0 0 1 4 10 15.3 15.3 0 0 1-4 10 15.3 15.3 0 0 1-4-10 15.3 15.3 0 0 1 4-10z\"/>"),

            new BannerIcon("scales", "Justice", "<line x1=\"12\" y1=\"3\" x2=\"12\" y2=\"21\"/><line x1=\"7\" y1=\"21\" x2=\"17\" y2=\"21\"/><line x1=\"5\" y1=\"7\" x2=\"19\" y2=\"7\"/><path d=\"M5 7l-3 7a3 3 0 0 0 6 0z\"/><path d=\"M19 7l-3 7a3 3 0 0 0 6 0z\"/>", Law),
            new BannerIcon("gavel", "Gavel", "<polygon points=\"12.5 2.5 21.5 11.5 18 15 9 6\"/><line x1=\"13.5\" y1=\"10.5\" x2=\"4\" y2=\"20\"/><line x1=\"2\" y1=\"22\" x2=\"12\" y2=\"22\"/>", Law),
            new BannerIcon("courthouse", "Courthouse", "<polygon points=\"12 2 2 7 22 7\"/><line x1=\"5\" y1=\"10\" x2=\"5\" y2=\"18\"/><line x1=\"9.67\" y1=\"10\" x2=\"9.67\" y2=\"18\"/><line x1=\"14.33\" y1=\"10\" x2=\"14.33\" y2=\"18\"/><line x1=\"19\" y1=\"10\" x2=\"19\" y2=\"18\"/><line x1=\"2\" y1=\"21\" x2=\"22\" y2=\"21\"/>", Law),
            new BannerIcon("contract", "Contract", "<path d=\"M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z\"/><polyline points=\"14 2 14 8 20 8\"/><line x1=\"8\" y1=\"12\" x2=\"16\" y2=\"12\"/><path d=\"M8 17c1-2 2-2 3 0s2 2 3 0 2-1 2-1\"/>", Law),

            new BannerIcon("calculator", "Calculator", "<rect x=\"4\" y=\"2\" width=\"16\" height=\"20\" rx=\"2\" ry=\"2\"/><rect x=\"8\" y=\"6\" width=\"8\" height=\"4\"/><line x1=\"8\" y1=\"14\" x2=\"8.01\" y2=\"14\"/><line x1=\"12\" y1=\"14\" x2=\"12.01\" y2=\"14\"/><line x1=\"16\" y1=\"14\" x2=\"16.01\" y2=\"14\"/><line x1=\"8\" y1=\"18\" x2=\"8.01\" y2=\"18\"/><line x1=\"12\" y1=\"18\" x2=\"12.01\" y2=\"18\"/><line x1=\"16\" y1=\"18\" x2=\"16.01\" y2=\"18\"/>", Accounting),
            new BannerIcon("invoice", "Invoice", "<path d=\"M4 2v20l2-1 2 1 2-1 2 1 2-1 2 1 2-1 2 1V2l-2 1-2-1-2 1-2-1-2 1-2-1-2 1z\"/><line x1=\"8\" y1=\"8\" x2=\"16\" y2=\"8\"/><line x1=\"8\" y1=\"12\" x2=\"16\" y2=\"12\"/><line x1=\"8\" y1=\"16\" x2=\"13\" y2=\"16\"/>", Accounting),
            new BannerIcon("ledger", "Ledger", "<path d=\"M4 19.5A2.5 2.5 0 0 1 6.5 17H20\"/><path d=\"M6.5 2H20v20H6.5A2.5 2.5 0 0 1 4 19.5v-15A2.5 2.5 0 0 1 6.5 2z\"/><line x1=\"9\" y1=\"7\" x2=\"16\" y2=\"7\"/><line x1=\"9\" y1=\"11\" x2=\"16\" y2=\"11\"/>", Accounting),
            new BannerIcon("percent", "Tax", "<line x1=\"19\" y1=\"5\" x2=\"5\" y2=\"19\"/><circle cx=\"6.5\" cy=\"6.5\" r=\"2.5\"/><circle cx=\"17.5\" cy=\"17.5\" r=\"2.5\"/>", Accounting),
            new BannerIcon("piechart", "Report", "<path d=\"M21.21 15.89A10 10 0 1 1 8 2.83\"/><path d=\"M22 12A10 10 0 0 0 12 2v10z\"/>", Accounting),

            new BannerIcon("cart", "Cart", "<circle cx=\"9\" cy=\"21\" r=\"1\"/><circle cx=\"20\" cy=\"21\" r=\"1\"/><path d=\"M1 1h4l2.68 13.39a2 2 0 0 0 2 1.61h9.72a2 2 0 0 0 2-1.61L23 6H6\"/>", Sales),
            new BannerIcon("growth", "Growth", "<polyline points=\"23 6 13.5 15.5 8.5 10.5 1 18\"/><polyline points=\"17 6 23 6 23 12\"/>", Sales),
            new BannerIcon("barchart", "Pipeline", "<line x1=\"18\" y1=\"20\" x2=\"18\" y2=\"10\"/><line x1=\"12\" y1=\"20\" x2=\"12\" y2=\"4\"/><line x1=\"6\" y1=\"20\" x2=\"6\" y2=\"14\"/>", Sales),
            new BannerIcon("target", "Target", "<circle cx=\"12\" cy=\"12\" r=\"10\"/><circle cx=\"12\" cy=\"12\" r=\"6\"/><circle cx=\"12\" cy=\"12\" r=\"2\"/>", Sales),
            new BannerIcon("customers", "Customers", "<path d=\"M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2\"/><circle cx=\"9\" cy=\"7\" r=\"4\"/><path d=\"M23 21v-2a4 4 0 0 0-3-3.87\"/><path d=\"M16 3.13a4 4 0 0 1 0 7.75\"/>", Sales),
            new BannerIcon("phone", "Phone", "<path d=\"M22 16.92v3a2 2 0 0 1-2.18 2 19.79 19.79 0 0 1-8.63-3.07 19.5 19.5 0 0 1-6-6 19.79 19.79 0 0 1-3.07-8.67A2 2 0 0 1 4.11 2h3a2 2 0 0 1 2 1.72 12.84 12.84 0 0 0 .7 2.81 2 2 0 0 1-.45 2.11L8.09 9.91a16 16 0 0 0 6 6l1.27-1.27a2 2 0 0 1 2.11-.45 12.84 12.84 0 0 0 2.81.7A2 2 0 0 1 22 16.92z\"/>", Sales),
            new BannerIcon("payment", "Payment", "<rect x=\"1\" y=\"4\" width=\"22\" height=\"16\" rx=\"2\" ry=\"2\"/><line x1=\"1\" y1=\"10\" x2=\"23\" y2=\"10\"/>", Sales)
        };
    }
}
