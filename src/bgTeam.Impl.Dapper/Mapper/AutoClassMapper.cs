namespace DapperExtensions.Mapper
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using System.Reflection;
    using bgTeam.DataAccess.Impl.Dapper;

    /// <summary>
    /// Automatically maps an entity to a table using a combination of reflection and naming conventions for keys.
    /// </summary>
    public class AutoClassMapper<T> : ClassMapper<T>
        where T : class
    {
        private readonly string _identityColumn;

        public AutoClassMapper()
        {
            _identityColumn = DapperHelper.IdentityColumn;

            AutoMap();
        }

        public override void Table(string tableName)
        {
            if (Attribute.IsDefined(EntityType, typeof(TableNameAttribute)))
            {
                tableName = ((TableNameAttribute)EntityType.GetCustomAttribute(typeof(TableNameAttribute))).Name;
            }
            else if (Attribute.IsDefined(EntityType, typeof(TableAttribute)))
            {
                var attr = EntityType.GetCustomAttribute(typeof(TableAttribute)) as TableAttribute;
                tableName = attr.Name;
            }

            base.Table(tableName);
        }

        protected override void Schema(string schemaName)
        {
            if (Attribute.IsDefined(EntityType, typeof(SchemaAttribute)))
            {
                schemaName = ((SchemaAttribute)EntityType.GetCustomAttribute(typeof(SchemaAttribute))).Name;
            }
            else if (Attribute.IsDefined(EntityType, typeof(TableAttribute)))
            {
                var attr = EntityType.GetCustomAttribute(typeof(TableAttribute)) as TableAttribute;
                schemaName = attr.Schema;
            }

            base.Schema(schemaName);
        }

        private void AutoMap()
        {
            Type type = typeof(T);

            string columnPrefix = string.Empty;

            if (Attribute.IsDefined(type, typeof(PrefixForColumnsAttribute)))
            {
                columnPrefix = ((PrefixForColumnsAttribute)type.GetCustomAttribute(typeof(PrefixForColumnsAttribute))).Prefix;
            }

            foreach (PropertyInfo propertyInfo in EntityType.GetProperties())
            {
                PropertyMap propertyMap;
                if (Attribute.IsDefined(propertyInfo, typeof(IgnoreAttribute))
                    || Attribute.IsDefined(propertyInfo, typeof(NotMappedAttribute)))
                {
                    propertyMap = Map(propertyInfo, false).Ignore();
                }
                else if (Attribute.IsDefined(propertyInfo, typeof(ColumnNameAttribute)))
                {
                    propertyMap = Map(propertyInfo, false).Column(((ColumnNameAttribute)propertyInfo.GetCustomAttribute(typeof(ColumnNameAttribute))).Name);
                }
                else if (Attribute.IsDefined(propertyInfo, typeof(ColumnAttribute)))
                {
                    propertyMap = Map(propertyInfo, false).Column(((ColumnAttribute)propertyInfo.GetCustomAttribute(typeof(ColumnAttribute))).Name);
                }
                ////else if (Attribute.IsDefined(propertyInfo, typeof(MapToAttribute)))
                ////    propertyMap = Map(propertyInfo, false).Column(((MapToAttribute)propertyInfo.GetCustomAttribute(typeof(MapToAttribute))).DatabaseColumn);
                else if (!string.IsNullOrEmpty(columnPrefix))
                {
                    propertyMap = Map(propertyInfo, false).Column(string.Format("{0}{1}", columnPrefix, propertyInfo.Name));
                }
                else
                {
                    propertyMap = Map(propertyInfo, false);
                }

                if (Properties.Any(e => e.KeyType != KeyType.NotAKey))
                {
                    continue;
                }

                if (Attribute.IsDefined(propertyInfo, typeof(PrimaryKeyAttribute))
                    || Attribute.IsDefined(propertyInfo, typeof(KeyAttribute)))
                {
                    propertyMap.Key(KeyType.PrimaryKey);
                }

                if (Attribute.IsDefined(propertyInfo, typeof(IdentityAttribute)))
                {
                    propertyMap.Key(KeyType.Identity);
                }

                if (Attribute.IsDefined(propertyInfo, typeof(DatabaseGeneratedAttribute)))
                {
                    var attrs = propertyInfo.GetCustomAttributes(typeof(DatabaseGeneratedAttribute));
                    foreach (var attr in attrs)
                    {
                        var _attr = attr as DatabaseGeneratedAttribute;
                        if (_attr.DatabaseGeneratedOption == DatabaseGeneratedOption.Identity)
                        {
                            propertyMap.Key(KeyType.Identity);
                            break;
                        }
                    }
                }

                if (!string.IsNullOrEmpty(_identityColumn) && string.Equals(propertyMap.PropertyInfo.Name, _identityColumn, StringComparison.OrdinalIgnoreCase))
                {
                    propertyMap.Key(PropertyTypeKeyTypeMapping.ContainsKey(propertyMap.PropertyInfo.PropertyType) ?
                        PropertyTypeKeyTypeMapping[propertyMap.PropertyInfo.PropertyType] :
                        KeyType.Assigned);
                }
            }
        }

    }
}