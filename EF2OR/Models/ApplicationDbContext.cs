using System;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity;
using System.Linq;
using System.Data.Entity.Infrastructure;
using EF2OR.Enums;
using System.Dynamic;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace EF2OR.Models
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<Template> Templates { get; set; }
        public DbSet<ApplicationSetting> ApplicationSettings { get; set; }
        public DbSet<AcademicSessionType> AcademicSessionTypes { get; set; }
        public DbSet<ErrorLog> ErrorLogs { get; set; }


        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        private void CreateAuditLog(string userName, string ipAddress, IEnumerable<DbEntityEntry> entities)
        {
            
        }


        public int SaveChanges(string userName, string ipAddress)
        {
            var changedEntities = ChangeTracker.Entries().Where(p => p.State == EntityState.Deleted || p.State == EntityState.Modified);
            var addedEntities = ChangeTracker.Entries().Where(p => p.State == EntityState.Added).ToList();

            foreach (var entry in changedEntities)
            {
                var auditLog = GetAuditLog(entry, userName, ipAddress);
                AuditLogs.Add(auditLog);
            }

            var objectsCount = base.SaveChanges(); //need to do a save here so the added entities can get an Identity Id

            foreach (var entry in addedEntities)
            {
                entry.State = EntityState.Added; //need to set back to added, because we did a savechanges, which makes it "Unchanged" now
                var auditLog = GetAuditLog(entry, userName, ipAddress);
                entry.State = EntityState.Unchanged; //back to "Unchanged" so it doesnt get double created
                AuditLogs.Add(auditLog);
            }

            objectsCount += base.SaveChanges();

            return objectsCount;
        }

        private string GetEntityType(DbEntityEntry entry)
        {
            var entityType = entry.Entity.GetType();
            var entityTypeName = entityType.Name;
            if (entityType.BaseType != null && String.IsNullOrEmpty(entityType.BaseType.Name) == false && entityType.BaseType.Name != "Object")
            {
                entityTypeName = entityType.BaseType.Name;
            }
            return entityTypeName;
        }

        private AuditLog GetAuditLog(DbEntityEntry entry, string userName, string ipAddress)
        {
            var entityType = entry.Entity.GetType();
            var entityTypeString = GetEntityType(entry);
            var actionType = "";
            switch (entry.State)
            {
                case EntityState.Added:
                    switch (entityTypeString)
                    {
                        case "Template":
                            actionType = ActionTypes.TemplateCreated;
                            break;
                    }
                    break;
                case EntityState.Modified:
                    switch (entityTypeString)
                    {
                        case "Template":
                            actionType = ActionTypes.TemplateModified;
                            break;
                        case "ApplicationSetting":
                        case "AcademicSessionType":
                            actionType = ActionTypes.SettingsModified;
                            break;
                    }
                    break;
                case EntityState.Deleted:
                    switch (entityTypeString)
                    {
                        case "Template":
                            actionType = ActionTypes.TemplateDeleted;
                            break;
                    }
                    break;
            }

            dynamic originals = new ExpandoObject();
            dynamic current = new ExpandoObject();

            string jsonOriginalValues = "";
            string jsonCurrentValues = "";

            var templateId = 0;

            if (entry.State == EntityState.Added)
            {
                templateId = entry.CurrentValues.PropertyNames.Contains("TemplateId")
                        ? (int)entry.CurrentValues.GetValue<object>("TemplateId")
                        : 0;

                foreach (string propertyName in entry.CurrentValues.PropertyNames)
                {
                    var prop = entityType.GetProperty(propertyName);
                    if (prop != null)// && Attribute.IsDefined(prop, typeof(ForceFieldForAuditAttribute)))
                    {
                        //columns.Add(propertyName);
                        var currentValue = entry.CurrentValues.GetValue<object>(propertyName) == null
                        ? string.Empty
                        : entry.CurrentValues.GetValue<object>(propertyName).ToString();
                        ((IDictionary<String, Object>)current).Add(propertyName, currentValue);
                    }
                }
                jsonCurrentValues = JsonConvert.SerializeObject(current);
            }

            if (entry.State == EntityState.Modified)
            {
                templateId = entry.CurrentValues.PropertyNames.Contains("TemplateId")
                        ? (int)entry.CurrentValues.GetValue<object>("TemplateId")
                        : 0;

                foreach (string propertyName in entry.CurrentValues.PropertyNames)
                {
                    var prop = entityType.GetProperty(propertyName);
                    if (prop != null)// && Attribute.IsDefined(prop, typeof(ForceFieldForAuditAttribute)))
                    {
                        var originalValue = entry.OriginalValues.GetValue<object>(propertyName);
                        var currentValue = entry.CurrentValues.GetValue<object>(propertyName);

                        if (!Object.Equals(originalValue, currentValue))
                        {
                            originalValue = originalValue == null ? null : originalValue.ToString();
                            currentValue = currentValue == null ? null : currentValue.ToString();

                            //columns.Add(propertyName);
                            ((IDictionary<String, Object>)originals).Add(propertyName, originalValue);
                            ((IDictionary<String, Object>)current).Add(propertyName, currentValue);
                        }
                    }
                }
                jsonOriginalValues = JsonConvert.SerializeObject(originals);
                jsonCurrentValues = JsonConvert.SerializeObject(current);
            }

            if (entry.State == EntityState.Deleted)
            {
                templateId = entry.OriginalValues.PropertyNames.Contains("TemplateId")
                        ? (int)entry.OriginalValues.GetValue<object>("TemplateId")
                        : 0;

                foreach (string propertyName in entry.OriginalValues.PropertyNames)
                {
                    var prop = entityType.GetProperty(propertyName);
                    if (prop != null)// && Attribute.IsDefined(prop, typeof(ForceFieldForAuditAttribute)))
                    {
                        //columns.Add(propertyName);
                        var originalValue = entry.OriginalValues.GetValue<object>(propertyName) == null
                           ? string.Empty
                           : entry.OriginalValues.GetValue<object>(propertyName).ToString();
                        ((IDictionary<String, Object>)originals).Add(propertyName, originalValue);
                    }
                }
                jsonOriginalValues = JsonConvert.SerializeObject(originals);
            }

            var auditLog = new AuditLog
            {
                User = userName,
                IpAddress = ipAddress,
                TemplateId = templateId,
                Success = true,
                DateTimeStamp = DateTime.Now,
                Type = actionType,
                OldValues = jsonOriginalValues,
                NewValues = jsonCurrentValues
            };

            return auditLog;
        }
    }
}