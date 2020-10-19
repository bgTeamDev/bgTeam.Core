namespace bgTeam.DataAccess.Impl.PostgreSQL
{
    using System;
    using System.Data;
    using System.Threading.Tasks;
    using bgTeam.Helpers;
    using Npgsql;

    public class ConnectionFactoryPostgreSQL : IConnectionFactory
    {
        private readonly IAppLogger _logger;
        private readonly IConnectionSetting _setting;

        public ConnectionFactoryPostgreSQL(
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
                dialect.Init(SqlDialectEnum.PostgreSql);
            }
        }

        public ConnectionFactoryPostgreSQL(
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
                formatedConnectionString = HideHelper.HidePostgrePassword(connectionString);
            }

            _logger.Debug($"ConnectionFactoryPostgreSQL: {formatedConnectionString}");

            NpgsqlConnection dbConnection = new NpgsqlConnection(connectionString);
            dbConnection.Open();

            _logger.Debug($"ConnectionFactoryPostgreSQL: connect open");

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
                formatedConnectionString = HideHelper.HidePostgrePassword(connectionString);
            }

            _logger.Debug($"ConnectionFactoryPostgreSQL: {formatedConnectionString}");

            NpgsqlConnection dbConnection = new NpgsqlConnection(connectionString);
            await dbConnection.OpenAsync().ConfigureAwait(false);

            _logger.Debug($"ConnectionFactoryPostgreSQL: connect open");

            return dbConnection;
        }
    }
}
