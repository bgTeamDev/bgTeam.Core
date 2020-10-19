namespace bgTeam.Helpers
{
    using System.Text;

    public class HideHelper
    {
        public static string HidePostgrePassword(string text)
        {
            const string PasswordPattern = "Password=";
            const string EndPasswordPattern = ";";

            return HidePassword(text, PasswordPattern, EndPasswordPattern);
        }

        public static string HideMsSqlPassword(string text)
        {
            const string PasswordPattern = "PWD=";
            const string EndPasswordPattern = ";";

            return HidePassword(text, PasswordPattern, EndPasswordPattern);
        }

        private static string HidePassword(
            string text,
            string passwordPattern,
            string endPasswordPattern)
        {
            int passIndex = text.IndexOf(passwordPattern);
            if (passIndex == -1)
            {
                return text;
            }

            passIndex = passIndex + passwordPattern.Length;

            int endIndex = text.IndexOf(endPasswordPattern, passIndex);
            if (endIndex == -1)
            {
                endIndex = text.Length;
            }

            StringBuilder sb = new StringBuilder(text.Substring(0, passIndex));
            for (int i = 0; i < endIndex - passIndex; i++)
            {
                sb.Append("*");
            }
            sb.Append(text.Substring(endIndex));

            var res = sb.ToString();
            return res;
        }
    }
}
