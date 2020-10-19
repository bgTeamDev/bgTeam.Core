using bgTeam.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace bgTeam.Core.Tests.Tests.Core.Helpers
{
    public class HideHelperTest
    {
        [Theory]
        [InlineData("Server=10.0.0.1;Database=db;Username=user_dbo;Password=pass_dbo", "Server=10.0.0.1;Database=db;Username=user_dbo;Password=********")]
        [InlineData("Server=10.0.0.2;Database=db;Username=user_dbo;Password=pass_dbo;", "Server=10.0.0.2;Database=db;Username=user_dbo;Password=********;")]
        [InlineData("Server=10.0.0.3;Database=db;Password=pass_dbo;Username=user_dbo;", "Server=10.0.0.3;Database=db;Password=********;Username=user_dbo;")]
        [InlineData("Server=10.0.0.4;Database=db;Password=pass_dbo;Username=user_dbo", "Server=10.0.0.4;Database=db;Password=********;Username=user_dbo")]
        [InlineData("Server=10.0.0.5;Database=password;Password=pass_dbo;Username=user_dbo;", "Server=10.0.0.5;Database=password;Password=********;Username=user_dbo;")]
        [InlineData("Server=10.0.0.7;Database=Password;Password=pass_dbo;Username=user_dbo;", "Server=10.0.0.7;Database=Password;Password=********;Username=user_dbo;")]
        [InlineData("Server=10.0.0.6;Database=password;Username=user_dbo;", "Server=10.0.0.6;Database=password;Username=user_dbo;")]
        public void HidePostgrePasswordTest(string input, string expected)
        {
            string hided = HideHelper.HidePostgrePassword(input);

            Assert.Equal(expected, hided);
        }

        [Theory]
        [InlineData("Server=myServerName,1234;Database=myDataBase;UID=myUsername;PWD=myPassword", "Server=myServerName,1234;Database=myDataBase;UID=myUsername;PWD=**********")]
        [InlineData("Server=myServerName,1234;Database=myDataBase;UID=myUsername;PWD=myPassword;", "Server=myServerName,1234;Database=myDataBase;UID=myUsername;PWD=**********;")]
        [InlineData("Server=myServerName,1234;Database=Password;UID=myUsername;PWD=myPassword;", "Server=myServerName,1234;Database=Password;UID=myUsername;PWD=**********;")]
        [InlineData("Server=myServerName,1234;Database=Password;UID=myUsername;PWD=myPassword", "Server=myServerName,1234;Database=Password;UID=myUsername;PWD=**********")]
        [InlineData("Server=myServerName,1234;Database=myDataBase;PWD=myPassword;UID=myUsername;", "Server=myServerName,1234;Database=myDataBase;PWD=**********;UID=myUsername;")]
        public void HideMsSqlPasswordTest(string input, string expected)
        {
            string hided = HideHelper.HideMsSqlPassword(input);

            Assert.Equal(expected, hided);
        }
    }
}
