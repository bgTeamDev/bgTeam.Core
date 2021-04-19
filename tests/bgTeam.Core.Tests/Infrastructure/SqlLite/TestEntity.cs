using bgTeam.DataAccess.Impl.Dapper;

namespace bgTeam.Core.Tests.Infrastructure
{
    [TableName("TestEntity")]
    public class TestEntity
    {
        [Identity]
        [PrimaryKey]
        public int? Id { get; set; }

        public string Name { get; set; }
    }
}
