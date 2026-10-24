using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using WebResourceManager.Models;
using WebResourceManager.Services;

namespace WebResourceManager.Tools
{
    internal enum SaveOutcome
    {
        Cancelled,
        Created,
        Updated
    }

    /// <summary>
    /// One web resource in a multi-save. Content is base64.
    /// </summary>
    internal class WebResourceContent
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Base64Content { get; set; }
        public WebResourceType Type { get; set; }
    }

    /// <summary>
    /// Saves a tool's output as a text web resource: checks for an existing one with
    /// the same name, asks before replacing it, then creates or updates, adds it to
    /// the solution and publishes. Errors are left to the caller.
    /// </summary>
    internal static class WebResourcePublisher
    {
        /// <param name="what">How the content is described in the replace prompt, e.g. "this banner".</param>
        public static Task<SaveOutcome> SaveAsync(IWin32Window owner, string caption, Microsoft.Xrm.Sdk.IOrganizationService service,
            WebResourceDetailsBox details, WebResourceType type, string content, string what, Action<string, int> reportStatus)
        {
            return SaveBase64Async(owner, caption, service, details, type, WebResourceRepository.EncodeText(content), what, reportStatus);
        }

        /// <summary>
        /// As <see cref="SaveAsync"/>, for binary content such as images.
        /// </summary>
        public static Task<SaveOutcome> SaveBytesAsync(IWin32Window owner, string caption, Microsoft.Xrm.Sdk.IOrganizationService service,
            WebResourceDetailsBox details, WebResourceType type, byte[] content, string what, Action<string, int> reportStatus)
        {
            return SaveBase64Async(owner, caption, service, details, type, Convert.ToBase64String(content), what, reportStatus);
        }

        private static async Task<SaveOutcome> SaveBase64Async(IWin32Window owner, string caption, Microsoft.Xrm.Sdk.IOrganizationService service,
            WebResourceDetailsBox details, WebResourceType type, string base64Content, string what, Action<string, int> reportStatus)
        {
            var name = details.FullName;
            var displayName = details.DisplayName;
            var description = details.Description;
            var solution = details.SelectedSolution;
            var repository = new WebResourceRepository(service);

            reportStatus("Checking for an existing web resource...", 10);
            var existing = await Task.Run(() => repository.FindByName(name));
            if (existing != null)
            {
                if (existing.Type != type)
                {
                    MessageBox.Show(owner, string.Format("A {0} web resource named {1} already exists. Choose another name.", existing.TypeLabel, name),
                        caption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return SaveOutcome.Cancelled;
                }

                var prompt = string.Format("A web resource named {0} already exists{1}.\r\n\r\nReplace its content with {2}?",
                    name, existing.IsManaged ? " and is managed, so this adds an unmanaged customisation on top" : string.Empty, what);
                if (MessageBox.Show(owner, prompt, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                {
                    return SaveOutcome.Cancelled;
                }
            }

            reportStatus(existing != null ? "Updating " + name + "..." : "Creating " + name + "...", 35);
            var existingId = existing != null ? existing.Id : (Guid?)null;
            var id = await Task.Run(() => repository.SaveContent(existingId, name, displayName, description, base64Content, type, solution.UniqueName, false));

            reportStatus("Publishing...", 75);
            await Task.Run(() => repository.Publish(new[] { id }));

            return existing != null ? SaveOutcome.Updated : SaveOutcome.Created;
        }

        /// <summary>
        /// Saves several web resources at once: checks every name first, asks once
        /// before replacing any that exist, saves them all and publishes them with a
        /// single request. Description and solution come from the details box.
        /// Returns the number created and updated, or null if cancelled.
        /// </summary>
        public static async Task<Tuple<int, int>> SaveManyAsync(IWin32Window owner, string caption, Microsoft.Xrm.Sdk.IOrganizationService service,
            WebResourceDetailsBox details, IList<WebResourceContent> items, Action<string, int> reportStatus)
        {
            var description = details.Description;
            var solution = details.SelectedSolution;
            var repository = new WebResourceRepository(service);

            reportStatus("Checking for existing web resources...", 5);
            var existing = await Task.Run(() => items.ToDictionary(i => i.Name, i => repository.FindByName(i.Name), StringComparer.OrdinalIgnoreCase));

            var wrongType = items.FirstOrDefault(i => existing[i.Name] != null && existing[i.Name].Type != i.Type);
            if (wrongType != null)
            {
                MessageBox.Show(owner, string.Format("A {0} web resource named {1} already exists. Choose another name.", existing[wrongType.Name].TypeLabel, wrongType.Name),
                    caption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return null;
            }

            var replacing = items.Where(i => existing[i.Name] != null).ToList();
            if (replacing.Count > 0)
            {
                var prompt = string.Format("{0} already exist{1}:\r\n\r\n{2}\r\n\r\nReplace {3}?",
                    replacing.Count == 1 ? "This web resource" : "These web resources",
                    replacing.Count == 1 ? "s" : string.Empty,
                    string.Join("\r\n", replacing.Select(i => i.Name + (existing[i.Name].IsManaged ? " (managed)" : string.Empty))),
                    replacing.Count == 1 ? "it" : "them");
                if (MessageBox.Show(owner, prompt, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                {
                    return null;
                }
            }

            var ids = new List<Guid>();
            for (var n = 0; n < items.Count; n++)
            {
                var item = items[n];
                reportStatus(string.Format("Saving {0} of {1}...", n + 1, items.Count), 10 + 70 * n / items.Count);
                var existingId = existing[item.Name] != null ? existing[item.Name].Id : (Guid?)null;
                ids.Add(await Task.Run(() => repository.SaveContent(existingId, item.Name, item.DisplayName, description, item.Base64Content, item.Type, solution.UniqueName, false)));
            }

            reportStatus("Publishing...", 85);
            await Task.Run(() => repository.Publish(ids));

            return Tuple.Create(items.Count - replacing.Count, replacing.Count);
        }
    }
}
