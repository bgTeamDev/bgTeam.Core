namespace bgTeam.DataAccess.Impl
{
    /// <summary>
    /// Реализация получения настроек подключения к БД по-умолчанию
    /// </summary>
    public class ConnectionSettingDefault : IConnectionSetting
    {
        public ConnectionSettingDefault(string connectionString)
        {
            ConnectionString = connectionString;
            HidePassword = true;
        }

        public string ConnectionString { get; set; }

        public bool HidePassword { get; set; }
    }
}
