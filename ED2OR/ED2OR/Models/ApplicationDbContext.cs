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

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }



/*
        public int SaveChanges(string actionType)
        {
            // Get all Added/Deleted/Modified entities (not Unmodified or Detached)
            var changedEntities = ChangeTracker.Entries().Where(p => p.State == EntityState.Deleted || p.State == EntityState.Modified || p.State == EntityState.Added);
            if (actionType == ActionTypes.TemplateCreated)
            {
                foreach (var dbEntry in changedEntities)
                {
                    //dynamic originals = new ExpandoObject();
                    dynamic current = new ExpandoObject();
                    var entityType = dbEntry.Entity.GetType();

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




                    foreach (string propertyName in dbEntry.CurrentValues.PropertyNames)
                    {
                        var prop = entityType.GetProperty(propertyName);
                        if (prop != null)// && Attribute.IsDefined(prop, typeof(ForceFieldForAuditAttribute)))
                        {
                            //columns.Add(propertyName);

                            var currentValue = dbEntry.CurrentValues.GetValue<object>(propertyName) == null
                                ? string.Empty
                                : dbEntry.CurrentValues.GetValue<object>(propertyName).ToString();

                            //((IDictionary<String, Object>)originals).Add(propertyName, null);
                            ((IDictionary<String, Object>)current).Add(propertyName, currentValue);
                        }
                    }
                        //string jsonOriginalValues = JsonConvert.SerializeObject(originals);
                    string jsonCurrentValues = JsonConvert.SerializeObject(current);

                    var auditLog = new AuditLog
                    {
                        IpAddress = "123",
                        Type = actionType,
                        DateTimeStamp = System.DateTime.Now,
                        //OldValues = jsonOriginalValues,
                        NewValues = jsonCurrentValues
                    };
                    AuditLogs.Add(auditLog);

                }
            }
            return base.SaveChanges();
        }*/

        /*
        public override int SaveChanges()
        {
         

            //var addedEntities = ChangeTracker.Entries().Where(p => p.State == EntityState.Added).ToList();
            var changedEntities = ChangeTracker.Entries().Where(p => p.State == EntityState.Deleted || p.State == EntityState.Modified);

            foreach (var entry in changedEntities)
            {
                var entityType = GetEntityType(entry);

                var actionType = "";
                switch(entityType)
                {
                    case "Template":
                        actionType = ActionTypes.TemplateModified;
                        break;
                    case "ApplicationUser":
                        actionType = ActionTypes.SettingsModified;
                        break;
                }

                var auditLog = new AuditLog
                {
                    IpAddress = "123",
                    DateTimeStamp = System.DateTime.Now
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
        */

    }
}