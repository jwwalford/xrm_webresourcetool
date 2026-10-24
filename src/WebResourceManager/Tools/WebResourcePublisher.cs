using System;
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
    /// Saves a tool's output as a text web resource: checks for an existing one with
    /// the same name, asks before replacing it, then creates or updates, adds it to
    /// the solution and publishes. Errors are left to the caller.
    /// </summary>
    internal static class WebResourcePublisher
    {
        /// <param name="what">How the content is described in the replace prompt, e.g. "this banner".</param>
        public static async Task<SaveOutcome> SaveAsync(IWin32Window owner, string caption, Microsoft.Xrm.Sdk.IOrganizationService service,
            WebResourceDetailsBox details, WebResourceType type, string content, string what, Action<string> reportStatus)
        {
            var name = details.FullName;
            var displayName = details.DisplayName;
            var description = details.Description;
            var solution = details.SelectedSolution;
            var repository = new WebResourceRepository(service);

            reportStatus("Checking for an existing web resource...");
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

            reportStatus("Saving and publishing...");
            var existingId = existing != null ? existing.Id : (Guid?)null;
            await Task.Run(() => repository.SaveText(existingId, name, displayName, description, content, type, solution.UniqueName));

            return existing != null ? SaveOutcome.Updated : SaveOutcome.Created;
        }
    }
}
