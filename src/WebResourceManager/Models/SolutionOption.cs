namespace WebResourceManager.Models
{
    public class SolutionOption
    {
        public string UniqueName { get; set; }

        public string FriendlyName { get; set; }

        /// <summary>
        /// Customization prefix of the solution's publisher, e.g. "new".
        /// </summary>
        public string Prefix { get; set; }

        public bool IsDefault
        {
            get { return UniqueName == "Default"; }
        }

        public override string ToString()
        {
            return string.Format("{0} ({1}_)", FriendlyName, Prefix);
        }
    }
}
