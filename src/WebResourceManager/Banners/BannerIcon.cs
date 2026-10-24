using System.Collections.Generic;
using System.Linq;

namespace WebResourceManager.Banners
{
    /// <summary>
    /// A built-in banner icon. Markup is the inner content of a 24x24 SVG drawn
    /// with strokes in currentColor, so the icon picks up the chosen icon colour.
    /// Shapes are based on Feather Icons (MIT licence, https://feathericons.com);
    /// the Law and Accounting shapes not in Feather are drawn in the same style.
    /// Some Hardware and Buildings shapes are based on Lucide (ISC licence,
    /// https://lucide.dev), Feather's continuation.
    /// </summary>
    public class BannerIcon
    {
        public const string General = "General";
        public const string Law = "Law";
        public const string Accounting = "Accounting";
        public const string Sales = "Sales";
        public const string Hardware = "Hardware";
        public const string Buildings = "Buildings";
        public const string IT = "IT";
        public const string Emojis = "Emojis";
        public const string Numbers = "Numbers";

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
            new BannerIcon("payment", "Payment", "<rect x=\"1\" y=\"4\" width=\"22\" height=\"16\" rx=\"2\" ry=\"2\"/><line x1=\"1\" y1=\"10\" x2=\"23\" y2=\"10\"/>", Sales),

            new BannerIcon("wrench", "Wrench", "<path d=\"M14.7 6.3a1 1 0 0 0 0 1.4l1.6 1.6a1 1 0 0 0 1.4 0l3.77-3.77a6 6 0 0 1-7.94 7.94l-6.91 6.91a2.12 2.12 0 0 1-3-3l6.91-6.91a6 6 0 0 1 7.94-7.94l-3.76 3.76z\"/>", Hardware),
            new BannerIcon("hammer", "Hammer", "<path d=\"M15 12l-8.5 8.5a2.12 2.12 0 0 1-3-3L12 9\"/><path d=\"M17.64 15L22 10.64\"/><path d=\"M20.91 11.7l-1.25-1.25c-.6-.6-.93-1.4-.93-2.25v-.86L16.01 4.6a5.56 5.56 0 0 0-3.94-1.64H9l.92.82A6.18 6.18 0 0 1 12 8.4v1.56l2 2h2.47l2.26 1.91\"/>", Hardware),
            new BannerIcon("cog", "Settings", "<circle cx=\"12\" cy=\"12\" r=\"3\"/><path d=\"M19.4 15a1.65 1.65 0 0 0 .33 1.82l.06.06a2 2 0 0 1 0 2.83 2 2 0 0 1-2.83 0l-.06-.06a1.65 1.65 0 0 0-1.82-.33 1.65 1.65 0 0 0-1 1.51V21a2 2 0 0 1-2 2 2 2 0 0 1-2-2v-.09A1.65 1.65 0 0 0 9 19.4a1.65 1.65 0 0 0-1.82.33l-.06.06a2 2 0 0 1-2.83 0 2 2 0 0 1 0-2.83l.06-.06a1.65 1.65 0 0 0 .33-1.82 1.65 1.65 0 0 0-1.51-1H3a2 2 0 0 1-2-2 2 2 0 0 1 2-2h.09A1.65 1.65 0 0 0 4.6 9a1.65 1.65 0 0 0-.33-1.82l-.06-.06a2 2 0 0 1 0-2.83 2 2 0 0 1 2.83 0l.06.06a1.65 1.65 0 0 0 1.82.33H9a1.65 1.65 0 0 0 1-1.51V3a2 2 0 0 1 2-2 2 2 0 0 1 2 2v.09a1.65 1.65 0 0 0 1 1.51 1.65 1.65 0 0 0 1.82-.33l.06-.06a2 2 0 0 1 2.83 0 2 2 0 0 1 0 2.83l-.06.06a1.65 1.65 0 0 0-.33 1.82V9a1.65 1.65 0 0 0 1.51 1H21a2 2 0 0 1 2 2 2 2 0 0 1-2 2h-.09a1.65 1.65 0 0 0-1.51 1z\"/>", Hardware),
            new BannerIcon("ruler", "Ruler", "<path d=\"M21.3 15.3a2.4 2.4 0 0 1 0 3.4l-2.6 2.6a2.4 2.4 0 0 1-3.4 0L2.7 8.7a2.41 2.41 0 0 1 0-3.4l2.6-2.6a2.41 2.41 0 0 1 3.4 0z\"/><path d=\"M14.5 12.5l2-2\"/><path d=\"M11.5 9.5l2-2\"/><path d=\"M8.5 6.5l2-2\"/><path d=\"M17.5 15.5l2-2\"/>", Hardware),
            new BannerIcon("toolbox", "Toolbox", "<rect x=\"2\" y=\"8\" width=\"20\" height=\"13\" rx=\"2\"/><path d=\"M8 8V5a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v3\"/><line x1=\"2\" y1=\"13\" x2=\"22\" y2=\"13\"/><line x1=\"10\" y1=\"13\" x2=\"10\" y2=\"15\"/><line x1=\"14\" y1=\"13\" x2=\"14\" y2=\"15\"/>", Hardware),
            new BannerIcon("plug", "Plug", "<path d=\"M12 22v-5\"/><path d=\"M9 8V2\"/><path d=\"M15 8V2\"/><path d=\"M18 8v5a4 4 0 0 1-4 4h-4a4 4 0 0 1-4-4V8z\"/>", Hardware),
            new BannerIcon("battery", "Battery", "<rect x=\"1\" y=\"6\" width=\"18\" height=\"12\" rx=\"2\" ry=\"2\"/><line x1=\"23\" y1=\"13\" x2=\"23\" y2=\"11\"/>", Hardware),

            new BannerIcon("office", "Office", "<rect x=\"4\" y=\"2\" width=\"16\" height=\"20\" rx=\"2\"/><path d=\"M9 22v-4h6v4\"/><path d=\"M8 6h.01\"/><path d=\"M12 6h.01\"/><path d=\"M16 6h.01\"/><path d=\"M8 10h.01\"/><path d=\"M12 10h.01\"/><path d=\"M16 10h.01\"/><path d=\"M8 14h.01\"/><path d=\"M12 14h.01\"/><path d=\"M16 14h.01\"/>", Buildings),
            new BannerIcon("factory", "Factory", "<path d=\"M2 20a2 2 0 0 0 2 2h16a2 2 0 0 0 2-2V8l-7 5V8l-7 5V4a2 2 0 0 0-2-2H4a2 2 0 0 0-2 2z\"/><path d=\"M17 18h1\"/><path d=\"M12 18h1\"/><path d=\"M7 18h1\"/>", Buildings),
            new BannerIcon("warehouse", "Warehouse", "<path d=\"M22 8.35V20a2 2 0 0 1-2 2H4a2 2 0 0 1-2-2V8.35A2 2 0 0 1 3.26 6.5l8-3.2a2 2 0 0 1 1.48 0l8 3.2A2 2 0 0 1 22 8.35z\"/><path d=\"M6 18h12\"/><path d=\"M6 14h12\"/><rect x=\"6\" y=\"10\" width=\"12\" height=\"12\"/>", Buildings),
            new BannerIcon("store", "Store", "<path d=\"M3 9l1.5-5h15L21 9\"/><path d=\"M3 9a3 3 0 0 0 6 0 3 3 0 0 0 6 0 3 3 0 0 0 6 0\"/><path d=\"M5 11v10h14V11\"/><path d=\"M10 21v-6h4v6\"/>", Buildings),
            new BannerIcon("hospital", "Hospital", "<rect x=\"3\" y=\"3\" width=\"18\" height=\"18\" rx=\"2\"/><line x1=\"12\" y1=\"8\" x2=\"12\" y2=\"16\"/><line x1=\"8\" y1=\"12\" x2=\"16\" y2=\"12\"/>", Buildings),
            new BannerIcon("school", "School", "<path d=\"M22 10L12 5 2 10l10 5 10-5z\"/><path d=\"M6 12v5c3 3 9 3 12 0v-5\"/>", Buildings),
            new BannerIcon("city", "City", "<rect x=\"3\" y=\"7\" width=\"8\" height=\"14\"/><rect x=\"11\" y=\"3\" width=\"10\" height=\"18\"/><line x1=\"14\" y1=\"7\" x2=\"14.01\" y2=\"7\"/><line x1=\"18\" y1=\"7\" x2=\"18.01\" y2=\"7\"/><line x1=\"14\" y1=\"11\" x2=\"14.01\" y2=\"11\"/><line x1=\"18\" y1=\"11\" x2=\"18.01\" y2=\"11\"/><line x1=\"14\" y1=\"15\" x2=\"14.01\" y2=\"15\"/><line x1=\"18\" y1=\"15\" x2=\"18.01\" y2=\"15\"/><line x1=\"6\" y1=\"11\" x2=\"6.01\" y2=\"11\"/><line x1=\"6\" y1=\"15\" x2=\"6.01\" y2=\"15\"/>", Buildings),

            new BannerIcon("monitor", "Monitor", "<rect x=\"2\" y=\"3\" width=\"20\" height=\"14\" rx=\"2\" ry=\"2\"/><line x1=\"8\" y1=\"21\" x2=\"16\" y2=\"21\"/><line x1=\"12\" y1=\"17\" x2=\"12\" y2=\"21\"/>", IT),
            new BannerIcon("laptop", "Laptop", "<rect x=\"4\" y=\"4\" width=\"16\" height=\"11\" rx=\"1\"/><path d=\"M2 19h20l-2-4H4z\"/>", IT),
            new BannerIcon("server", "Server", "<rect x=\"2\" y=\"2\" width=\"20\" height=\"8\" rx=\"2\" ry=\"2\"/><rect x=\"2\" y=\"14\" width=\"20\" height=\"8\" rx=\"2\" ry=\"2\"/><line x1=\"6\" y1=\"6\" x2=\"6.01\" y2=\"6\"/><line x1=\"6\" y1=\"18\" x2=\"6.01\" y2=\"18\"/>", IT),
            new BannerIcon("database", "Database", "<ellipse cx=\"12\" cy=\"5\" rx=\"9\" ry=\"3\"/><path d=\"M21 12c0 1.66-4 3-9 3s-9-1.34-9-3\"/><path d=\"M3 5v14c0 1.66 4 3 9 3s9-1.34 9-3V5\"/>", IT),
            new BannerIcon("wifi", "Wi-Fi", "<path d=\"M5 12.55a11 11 0 0 1 14.08 0\"/><path d=\"M1.42 9a16 16 0 0 1 21.16 0\"/><path d=\"M8.53 16.11a6 6 0 0 1 6.95 0\"/><line x1=\"12\" y1=\"20\" x2=\"12.01\" y2=\"20\"/>", IT),
            new BannerIcon("cloud", "Cloud", "<path d=\"M18 10h-1.26A8 8 0 1 0 9 20h9a5 5 0 0 0 0-10z\"/>", IT),
            new BannerIcon("code", "Code", "<polyline points=\"16 18 22 12 16 6\"/><polyline points=\"8 6 2 12 8 18\"/>", IT),
            new BannerIcon("cpu", "Processor", "<rect x=\"4\" y=\"4\" width=\"16\" height=\"16\" rx=\"2\" ry=\"2\"/><rect x=\"9\" y=\"9\" width=\"6\" height=\"6\"/><line x1=\"9\" y1=\"1\" x2=\"9\" y2=\"4\"/><line x1=\"15\" y1=\"1\" x2=\"15\" y2=\"4\"/><line x1=\"9\" y1=\"20\" x2=\"9\" y2=\"23\"/><line x1=\"15\" y1=\"20\" x2=\"15\" y2=\"23\"/><line x1=\"20\" y1=\"9\" x2=\"23\" y2=\"9\"/><line x1=\"20\" y1=\"14\" x2=\"23\" y2=\"14\"/><line x1=\"1\" y1=\"9\" x2=\"4\" y2=\"9\"/><line x1=\"1\" y1=\"14\" x2=\"4\" y2=\"14\"/>", IT),
            new BannerIcon("printer", "Printer", "<polyline points=\"6 9 6 2 18 2 18 9\"/><path d=\"M6 18H4a2 2 0 0 1-2-2v-5a2 2 0 0 1 2-2h16a2 2 0 0 1 2 2v5a2 2 0 0 1-2 2h-2\"/><rect x=\"6\" y=\"14\" width=\"12\" height=\"8\"/>", IT),
            new BannerIcon("smartphone", "Mobile", "<rect x=\"5\" y=\"2\" width=\"14\" height=\"20\" rx=\"2\" ry=\"2\"/><line x1=\"12\" y1=\"18\" x2=\"12.01\" y2=\"18\"/>", IT),

            new BannerIcon("smile", "Happy", "<circle cx=\"12\" cy=\"12\" r=\"10\"/><path d=\"M8 14s1.5 2 4 2 4-2 4-2\"/><line x1=\"9\" y1=\"9\" x2=\"9.01\" y2=\"9\"/><line x1=\"15\" y1=\"9\" x2=\"15.01\" y2=\"9\"/>", Emojis),
            new BannerIcon("laugh", "Laughing", "<circle cx=\"12\" cy=\"12\" r=\"10\"/><path d=\"M7 13h10a5 5 0 0 1-10 0z\"/><line x1=\"9\" y1=\"9\" x2=\"9.01\" y2=\"9\"/><line x1=\"15\" y1=\"9\" x2=\"15.01\" y2=\"9\"/>", Emojis),
            new BannerIcon("wink", "Wink", "<circle cx=\"12\" cy=\"12\" r=\"10\"/><path d=\"M8 14s1.5 2 4 2 4-2 4-2\"/><line x1=\"9\" y1=\"9\" x2=\"9.01\" y2=\"9\"/><path d=\"M14 9h3\"/>", Emojis),
            new BannerIcon("meh", "Neutral", "<circle cx=\"12\" cy=\"12\" r=\"10\"/><line x1=\"8\" y1=\"15\" x2=\"16\" y2=\"15\"/><line x1=\"9\" y1=\"9\" x2=\"9.01\" y2=\"9\"/><line x1=\"15\" y1=\"9\" x2=\"15.01\" y2=\"9\"/>", Emojis),
            new BannerIcon("surprised", "Surprised", "<circle cx=\"12\" cy=\"12\" r=\"10\"/><circle cx=\"12\" cy=\"15.5\" r=\"2\"/><line x1=\"9\" y1=\"9\" x2=\"9.01\" y2=\"9\"/><line x1=\"15\" y1=\"9\" x2=\"15.01\" y2=\"9\"/>", Emojis),
            new BannerIcon("frown", "Sad", "<circle cx=\"12\" cy=\"12\" r=\"10\"/><path d=\"M16 16s-1.5-2-4-2-4 2-4 2\"/><line x1=\"9\" y1=\"9\" x2=\"9.01\" y2=\"9\"/><line x1=\"15\" y1=\"9\" x2=\"15.01\" y2=\"9\"/>", Emojis),
            new BannerIcon("thumbsup", "Thumbs up", "<path d=\"M14 9V5a3 3 0 0 0-3-3l-4 9v11h11.28a2 2 0 0 0 2-1.7l1.38-9a2 2 0 0 0-2-2.3zM7 22H4a2 2 0 0 1-2-2v-7a2 2 0 0 1 2-2h3\"/>", Emojis),
            new BannerIcon("thumbsdown", "Thumbs down", "<path d=\"M10 15v4a3 3 0 0 0 3 3l4-9V2H5.72a2 2 0 0 0-2 1.7l-1.38 9a2 2 0 0 0 2 2.3zm7-13h2.67A2.31 2.31 0 0 1 22 4v7a2.31 2.31 0 0 1-2.33 2H17\"/>", Emojis)
        }.Concat(CircledNumbers()).ToList();

        /// <summary>
        /// 1 to 10 in circles. The digit is text filled in the icon colour, so it
        /// stays solid whether or not the shape is filled.
        /// </summary>
        private static IEnumerable<BannerIcon> CircledNumbers()
        {
            for (var n = 1; n <= 10; n++)
            {
                yield return new BannerIcon("number" + n, "Number " + n,
                    "<circle cx=\"12\" cy=\"12\" r=\"10\"/><text x=\"12\" y=\"16\" text-anchor=\"middle\" font-family=\"Segoe UI, Arial, sans-serif\" font-weight=\"600\" font-size=\"" +
                    (n < 10 ? "11" : "9.5") + "\" fill=\"currentColor\" fill-opacity=\"1\" stroke=\"none\">" + n + "</text>", Numbers);
            }
        }
    }
}
