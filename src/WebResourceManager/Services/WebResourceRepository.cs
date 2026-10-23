using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using WebResourceManager.Models;

namespace WebResourceManager.Services
{
    public class WebResourceRepository
    {
        private readonly IOrganizationService service;

        public WebResourceRepository(IOrganizationService service)
        {
            this.service = service;
        }

        /// <summary>
        /// Retrieves web resource metadata (without content) for the given types.
        /// An empty types array returns every type. Hidden web resources are the
        /// system ones Dataverse keeps out of the solution explorer.
        /// </summary>
        public List<WebResourceItem> GetWebResources(WebResourceType[] types, bool hideHidden, bool unmanagedOnly)
        {
            var query = new QueryExpression("webresource")
            {
                ColumnSet = new ColumnSet("name", "displayname", "description", "webresourcetype", "ismanaged", "modifiedon", "modifiedby"),
                PageInfo = new PagingInfo { Count = 5000, PageNumber = 1 }
            };
            query.AddOrder("name", OrderType.Ascending);

            if (types != null && types.Length > 0)
            {
                query.Criteria.AddCondition("webresourcetype", ConditionOperator.In, types.Select(t => (object)(int)t).ToArray());
            }

            if (hideHidden)
            {
                query.Criteria.AddCondition("ishidden", ConditionOperator.Equal, false);
            }

            if (unmanagedOnly)
            {
                query.Criteria.AddCondition("ismanaged", ConditionOperator.Equal, false);
            }

            var results = new List<WebResourceItem>();
            while (true)
            {
                var page = service.RetrieveMultiple(query);
                results.AddRange(page.Entities.Select(ToItem));

                if (!page.MoreRecords)
                {
                    break;
                }

                query.PageInfo.PageNumber++;
                query.PageInfo.PagingCookie = page.PagingCookie;
            }

            return results;
        }

        public string GetContent(Guid id)
        {
            var record = service.Retrieve("webresource", id, new ColumnSet("content"));
            return record.GetAttributeValue<string>("content");
        }

        public static byte[] DecodeBytes(string base64)
        {
            return string.IsNullOrEmpty(base64) ? new byte[0] : Convert.FromBase64String(base64);
        }

        public static string DecodeText(string base64)
        {
            var bytes = DecodeBytes(base64);

            // Strip a UTF-8 byte order mark if present
            if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
            {
                return Encoding.UTF8.GetString(bytes, 3, bytes.Length - 3);
            }

            return Encoding.UTF8.GetString(bytes);
        }

        private static WebResourceItem ToItem(Entity entity)
        {
            var type = entity.GetAttributeValue<OptionSetValue>("webresourcetype");
            var modifiedBy = entity.GetAttributeValue<EntityReference>("modifiedby");

            return new WebResourceItem
            {
                Id = entity.Id,
                Name = entity.GetAttributeValue<string>("name"),
                DisplayName = entity.GetAttributeValue<string>("displayname"),
                Description = entity.GetAttributeValue<string>("description"),
                Type = type != null ? (WebResourceType)type.Value : 0,
                IsManaged = entity.GetAttributeValue<bool>("ismanaged"),
                ModifiedOn = entity.GetAttributeValue<DateTime?>("modifiedon")?.ToLocalTime(),
                ModifiedBy = modifiedBy?.Name
            };
        }
    }
}
