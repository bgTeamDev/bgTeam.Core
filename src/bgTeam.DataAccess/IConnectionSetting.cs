namespace bgTeam.DataAccess
{
    /// <summary>
    /// Используеться для подключения к БД
    /// </summary>
    public interface IConnectionSetting
    {
        string ConnectionString { get; set; }

        bool HidePassword { get; set; }
    }
}
