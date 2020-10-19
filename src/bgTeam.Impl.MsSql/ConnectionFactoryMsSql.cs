namespace bgTeam.DataAccess.Impl.MsSql
{
    using bgTeam.Helpers;
    using System;
    using System.Data;
    using System.Data.SqlClient;
    using System.Threading.Tasks;

    public class ConnectionFactoryMsSql : IConnectionFactory
    {
        private readonly IAppLogger _logger;
        private readonly IConnectionSetting _setting;

        public ConnectionFactoryMsSql(
            IAppLogger logger,
            IConnectionSetting setting,
            ISqlDialect dialect = null)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _setting = setting ?? throw new ArgumentNullException(nameof(setting));
            if (string.IsNullOrEmpty(_setting.ConnectionString))
            {
                throw new ArgumentOutOfRangeException(nameof(setting), "Connection string should not be null or empty");
            }

            if (dialect != null)
            {
                dialect.Init(SqlDialectEnum.MsSql);
            }
        }

        public ConnectionFactoryMsSql(
            IAppLogger logger,
            string connectionString,
            ISqlDialect dialect = null)
            : this(logger, new ConnectionSettingDefault(connectionString), dialect)
        {
        }

        public IDbConnection Create()
        {
            return Create(_setting.ConnectionString);
        }

        public IDbConnection Create(string connectionString)
        {
            string formatedConnectionString = connectionString;
            if (_setting.HidePassword)
            {
                formatedConnectionString = HideHelper.HideMsSqlPassword(connectionString);
            }
            _logger.Debug($"ConnectionFactoryMsSql: {formatedConnectionString}");

            SqlConnection dbConnection = new SqlConnection(connectionString);
            dbConnection.Open();

            _logger.Debug($"ConnectionFactoryMsSql: connect open");

            return dbConnection;
        }

        public async Task<IDbConnection> CreateAsync()
        {
            return await CreateAsync(_setting.ConnectionString);
        }

        public async Task<IDbConnection> CreateAsync(string connectionString)
        {
            string formatedConnectionString = connectionString;
            if (_setting.HidePassword)
            {
                formatedConnectionString = HideHelper.HideMsSqlPassword(connectionString);
            }
            _logger.Debug($"ConnectionFactoryMsSql: {formatedConnectionString}");

            SqlConnection dbConnection = new SqlConnection(connectionString);
            await dbConnection.OpenAsync().ConfigureAwait(false);

            _logger.Debug($"ConnectionFactoryMsSql: connect open");

            return dbConnection;
        }

        ////DbConnection dbConnection = MiniProfiler.Current == null
        ////                                ? (DbConnection)sqlConnection
        ////                                : new ProfiledDbConnection(sqlConnection, MiniProfiler.Current);
    }
}
