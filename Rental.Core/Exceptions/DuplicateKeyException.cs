namespace MyProject.Core.Exceptions
{
    // נזרקת מה-Repository כששמירה נכשלה על אינדקס ייחודי - למשל שני משתמשים שנרשמו
    // באותו רגע עם אותו אימייל. הבדיקה המוקדמת ב-Service עברה אצל שניהם, ובסיס הנתונים דחה את השני
    public class DuplicateKeyException : Exception
    {
        public DuplicateKeyException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
