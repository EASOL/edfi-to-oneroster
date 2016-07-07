using System;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity;
using System.Linq;
using System.Data.Entity.Infrastructure;
using System.ComponentModel.DataAnnotations.Schema;
using ED2OR.Enums;
using System.Dynamic;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ED2OR.Models
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<Template> Templates { get; set; }
        public DbSet<MappingSetting> MappingSettings { get; set; }
        public DbSet<AcademicSessionType> AcademicSessionTypes { get; set; }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        private void CreateAuditLog(string userName, string ipAddress, IEnumerable<DbEntityEntry> entities)
        {
            
        }


        public int SaveChanges(string userName, string ipAddress)
        {
            //var changedEntities = ChangeTracker.Entries().Where(p => p.State == EntityState.Deleted || p.State == EntityState.Modified);
            var changedEntities = ChangeTracker.Entries().Where(p => p.State == EntityState.Deleted || p.State == EntityState.Modified || p.State == EntityState.Added);

            foreach (var entry in changedEntities)
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
                            case "ApplicationUser":
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

                //http://stackoverflow.com/questions/17904631/how-can-i-log-all-entities-change-during-savechanges-using-ef-code-first
                //https://jmdority.wordpress.com/2011/07/20/using-entity-framework-4-1-dbcontext-change-tracking-for-audit-logging/
                //string primaryKeyName = dbEntry.GetAuditRecordKeyName();
                //int primaryKeyId = 0;
                //object primaryKeyValue;
                //primaryKeyValue = dbEntry.GetPropertyValue(primaryKeyName, true);
                //if (primaryKeyValue != null)
                //{
                //    Int32.TryParse(primaryKeyValue.ToString(), out primaryKeyId);
                //}

                dynamic originals = new ExpandoObject();
                dynamic current = new ExpandoObject();

                string jsonOriginalValues = "";
                string jsonCurrentValues = "";

                if (entry.State == EntityState.Modified || entry.State == EntityState.Deleted)
                {
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

                if (entry.State == EntityState.Modified || entry.State == EntityState.Added)
                {
                    foreach (string propertyName in entry.CurrentValues.PropertyNames)
                    {
                        var prop = entityType.GetProperty(propertyName);
                        if (prop != null)// && Attribute.IsDefined(prop, typeof(ForceFieldForAuditAttribute)))
                        {
                            var currentValue = entry.CurrentValues.GetValue<object>(propertyName) == null
                            ? string.Empty
                            : entry.CurrentValues.GetValue<object>(propertyName).ToString();
                            ((IDictionary<String, Object>)current).Add(propertyName, currentValue);
                        }
                    }
                    jsonCurrentValues = JsonConvert.SerializeObject(current);
                }

                var auditLog = new AuditLog
                {
                    User = userName,
                    IpAddress = ipAddress,
                    Success = true,
                    DateTimeStamp = DateTime.Now,
                    Type = actionType,
                    OldValues = jsonOriginalValues,
                    NewValues = jsonCurrentValues
                };
                AuditLogs.Add(auditLog);
            }
            return base.SaveChanges();
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

        //private static Type GetEntityType(DbEntityEntry dbEntry, out string tableName)
        //{
        //    // Get the Table() attribute, if one exists
        //    var entityType = dbEntry.Entity.GetType();
        //    var tableAttr = entityType.GetCustomAttributes(typeof(TableAttribute), false).SingleOrDefault() as TableAttribute;
        //    var entityTypeName = entityType.Name;
        //    if (entityType.BaseType != null && String.IsNullOrEmpty(entityType.BaseType.Name) == false && entityType.BaseType.Name != "Object")
        //    {
        //        entityTypeName = entityType.BaseType.Name;
        //    }

        //    // Get table name (if it has a Table attribute, use that, otherwise get the pluralized name)
        //    tableName = tableAttr != null ? tableAttr.Name : entityTypeName;
        //    return entityType;
        //}
        

    }
}