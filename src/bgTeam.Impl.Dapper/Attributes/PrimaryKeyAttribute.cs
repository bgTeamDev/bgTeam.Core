namespace bgTeam.DataAccess.Impl.Dapper
{
    using System;

    [AttributeUsage(AttributeTargets.Property)]
    public sealed class PrimaryKeyAttribute : Attribute
    {
    }
}
