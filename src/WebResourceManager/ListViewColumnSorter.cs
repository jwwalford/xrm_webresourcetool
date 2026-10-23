using System;
using System.Collections;
using System.Windows.Forms;
using WebResourceManager.Models;

namespace WebResourceManager
{
    internal class ListViewColumnSorter : IComparer
    {
        private const int ModifiedOnColumn = 4;

        public int SortColumn { get; set; }

        public SortOrder Order { get; set; } = SortOrder.Ascending;

        public int Compare(object x, object y)
        {
            var left = (ListViewItem)x;
            var right = (ListViewItem)y;

            int result;
            if (SortColumn == ModifiedOnColumn)
            {
                // Sort dates chronologically, not by their display text
                result = Nullable.Compare(((WebResourceItem)left.Tag).ModifiedOn, ((WebResourceItem)right.Tag).ModifiedOn);
            }
            else
            {
                result = string.Compare(left.SubItems[SortColumn].Text, right.SubItems[SortColumn].Text, StringComparison.CurrentCultureIgnoreCase);
            }

            return Order == SortOrder.Descending ? -result : result;
        }
    }
}
