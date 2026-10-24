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

        /// <summary>
        /// Unmanaged solutions the user can add a web resource to, with their
        /// publisher prefix. The Default solution is listed first.
        /// </summary>
        public List<SolutionOption> GetUnmanagedSolutions()
        {
            var query = new QueryExpression("solution")
            {
                ColumnSet = new ColumnSet("uniquename", "friendlyname")
            };
            query.Criteria.AddCondition("ismanaged", ConditionOperator.Equal, false);
            var visible = new FilterExpression(LogicalOperator.Or);
            visible.AddCondition("isvisible", ConditionOperator.Equal, true);
            visible.AddCondition("uniquename", ConditionOperator.Equal, "Default");
            query.Criteria.AddFilter(visible);
            query.AddOrder("friendlyname", OrderType.Ascending);

            var publisher = query.AddLink("publisher", "publisherid", "publisherid");
            publisher.EntityAlias = "pub";
            publisher.Columns = new ColumnSet("customizationprefix");

            return service.RetrieveMultiple(query).Entities
                .Select(e => new SolutionOption
                {
                    UniqueName = e.GetAttributeValue<string>("uniquename"),
                    FriendlyName = e.GetAttributeValue<string>("friendlyname"),
                    Prefix = e.GetAttributeValue<AliasedValue>("pub.customizationprefix")?.Value as string
                })
                .OrderBy(s => s.IsDefault ? 0 : 1)
                .ToList();
        }

        /// <summary>
        /// Looks up a web resource by its unique name. Returns null if there isn't one.
        /// </summary>
        public WebResourceItem FindByName(string name)
        {
            var query = new QueryExpression("webresource")
            {
                ColumnSet = new ColumnSet("name", "displayname", "description", "webresourcetype", "ismanaged", "modifiedon", "modifiedby"),
                TopCount = 1
            };
            query.Criteria.AddCondition("name", ConditionOperator.Equal, name);

            var entity = service.RetrieveMultiple(query).Entities.FirstOrDefault();
            return entity != null ? ToItem(entity) : null;
        }

        /// <summary>
        /// Creates a text web resource (HTML, CSS, ...), or updates the existing one
        /// when an id is given, then adds it to the solution (unless it's the Default
        /// solution) and publishes it. Returns the web resource id.
        /// </summary>
        public Guid SaveText(Guid? existingId, string name, string displayName, string description, string text, WebResourceType type, string solutionUniqueName)
        {
            var entity = new Entity("webresource");
            entity["displayname"] = displayName;
            entity["description"] = description;
            entity["content"] = EncodeText(text);

            Guid id;
            if (existingId.HasValue)
            {
                id = existingId.Value;
                entity.Id = id;
                service.Update(entity);
            }
            else
            {
                entity["name"] = name;
                entity["webresourcetype"] = new OptionSetValue((int)type);
                id = service.Create(entity);
            }

            if (!string.IsNullOrEmpty(solutionUniqueName) && solutionUniqueName != "Default")
            {
                var add = new OrganizationRequest("AddSolutionComponent");
                add["ComponentId"] = id;
                add["ComponentType"] = 61; // Web resource
                add["SolutionUniqueName"] = solutionUniqueName;
                add["AddRequiredComponents"] = false;
                service.Execute(add);
            }

            var publish = new OrganizationRequest("PublishXml");
            publish["ParameterXml"] = string.Format(
                "<importexportxml><webresources><webresource>{0}</webresource></webresources></importexportxml>", id.ToString("B"));
            service.Execute(publish);

            return id;
        }

        public static string EncodeText(string text)
        {
            return Convert.ToBase64String(new UTF8Encoding(false).GetBytes(text ?? string.Empty));
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
