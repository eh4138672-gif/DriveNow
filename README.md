# DriveNow - שרת להשכרת רכבים

שרת ASP.NET Core Web API לניהול השכרת רכבים. לקוחות נרשמים, מחפשים רכב לפי דגם, מחיר ותכונות, ומזמינים אותו לטווח תאריכים. מנהל מנהל את צי הרכבים ואת הלקוחות, ורואה את כל ההזמנות. המערכת בנויה בארבע שכבות (Core, Data, Service, API), עובדת מול SQL Server דרך EF Core, ומאובטחת ב-JWT עם שני תפקידים.

## המשאב המוגבל והתחרות עליו

**המשאב:** רכב בטווח תאריכים. אסור ששתי השכרות של אותו רכב יחפפו בזמן.

**איפה התחרות:** שני לקוחות מזמינים את אותו רכב לאותם תאריכים באותו רגע. שתי הבקשות קוראות את הרכב, שתיהן בודקות ורואות שהוא פנוי, ושתיהן מנסות לשמור. בלי הגנה, שתיהן יקבלו אישור.

**איך המערכת מגיבה (optimistic concurrency):**
- לישות `Car` יש עמודת `RowVersion` מסוג `rowversion`, שמתעדכנת אוטומטית בכל שינוי בשורה.
- הזמנה (`RentalService.TryBookAsync`):
  1. טוענת את הרכב.
  2. בודקת שאין השכרה חופפת.
  3. מוסיפה את ההשכרה ומעדכנת את השדה `LastBookedAt` של הרכב.
- `SaveChanges` מריץ את ה-INSERT של ההשכרה ואת ה-UPDATE של הרכב **באותה טרנזקציה**, וה-UPDATE כולל `WHERE RowVersion = <הערך שנקרא>`.
- אם הזמנה מקבילה נשמרה בינתיים, ה-RowVersion כבר השתנה. ה-UPDATE לא מוצא שורה, EF זורק `DbUpdateConcurrencyException`, וכל הטרנזקציה מתבטלת, כולל ה-INSERT.
- ה-Repository עוטף את החריגה ב-`ConcurrencyConflictException` מ-Core, כדי ששכבת ה-Service לא תהיה תלויה ב-EF.
- ה-Service מחזיר **409 Conflict** עם הודעה ברורה, וההתנגשות נכתבת ללוג ברמת Warning.
- גם עדכון רכב מוגן: הלקוח שולח את ה-RowVersion שקרא. אם הרכב השתנה מאז, מוחזר 409.

**ההוכחה:** ב-[Rental.Tests/ConcurrencyTests.cs](Rental.Tests/ConcurrencyTests.cs) יש שלוש בדיקות:
1. שני `DbContext` נפרדים קוראים את אותו רכב ומשנים אותו. הראשון שומר בהצלחה, והשני מקבל `DbUpdateConcurrencyException`.
2. תרחיש ה-race צעד אחר צעד: שתי הזמנות רואות שהרכב פנוי, ורק הראשונה נשמרת.
3. שתי הזמנות מקבילות אמיתיות דרך ה-Service, 5 פעמים. בכל פעם בדיוק אחת מצליחה, והשנייה מקבלת Conflict.

בלקוח, בחלון ההזמנה, הכפתור **"הדגמת תחרות על הרכב"** שולח שתי הזמנות בו-זמנית ומציג את ה-409.

## מבנה

| פרויקט | תוכן | תלוי ב |
|---|---|---|
| `Rental.Core` | ישויות, DTOs, ממשקים, `Result` | - |
| `Rental.Data` | `DataContext`, Repositories, Migrations | Core |
| `Rental.Service` | לוגיקה עסקית, AutoMapper, hash לסיסמאות | Core |
| `MyProject` (API) | Controllers, Middleware, JWT, `Program.cs` | Core, Service, ו-Data לרישום בלבד |
| `Rental.Tests` | xUnit ו-Moq, ובדיקות concurrency על LocalDB | כולם |

## הרצה מקומית

**דרישות:**
- .NET SDK 6 ומעלה. הפרויקט מכוון ל-net6.0, ורץ גם על 8 ו-10 בזכות `RollForward`.
- SQL Server LocalDB. מגיע עם Visual Studio, וה-instance הוא `MSSQLLocalDB`.

**1. מפתח ה-JWT ב-User Secrets.** זו פעולה חד-פעמית, כי המפתח לא נמצא ב-repo:

```bash
dotnet user-secrets set "Jwt:Key" "<מחרוזת אקראית של 32 תווים לפחות>" --project MyProject
```

**2. הרצת השרת:**

```bash
dotnet run --project MyProject --launch-profile MyProject
```

בהפעלה הראשונה השרת:
- יוצר את בסיס הנתונים `RentalDb` ומריץ את כל ה-migrations.
- מכניס נתוני התחלה: רכבים, לקוחות, תכונות והשכרות.
- יוצר את משתמשי הדמו.

אין צורך בשום פעולה ידנית בבסיס הנתונים.

**כתובות:**
- Swagger: https://localhost:7017/swagger. כדי לבדוק endpoints מוגנים, מתחברים דרך `/api/auth/login` ומדביקים את ה-token בכפתור **Authorize**.
- הלקוח (repository נפרד): http://localhost:5173. ה-proxy שלו מפנה ל-`https://localhost:7017`.

**3. הבדיקות:** הן יוצרות ומוחקות בעצמן בסיס נתונים נפרד, `RentalDb_Tests`.

```bash
dotnet test
```

## משתמשי דמו

| תפקיד | אימייל | סיסמה | הרשאות |
|---|---|---|---|
| Admin | admin@rental.local | Admin123! | ניהול רכבים ולקוחות, צפייה וביטול של כל ההשכרות, הזמנה עבור כל לקוח |
| Customer | israel@rental.local | Customer123! | הזמנה לעצמו, צפייה וביטול רק של ההשכרות שלו |
| Customer | dana@rental.local | Customer123! | כנ"ל |

הפרטים נטענים מ-`appsettings.Development.json` (`DemoUsers`), ונשמרים בבסיס הנתונים עם hash מסוג PBKDF2.

## Endpoints עיקריים

| Method | נתיב | הרשאה |
|---|---|---|
| POST | `/api/auth/register`, `/api/auth/login` | פתוח |
| GET | `/api/cars?page=&pageSize=&search=&featureId=&sortBy=&descending=` | פתוח |
| GET | `/api/features` | פתוח |
| POST, PUT, DELETE | `/api/cars/{id}` | Admin |
| GET, POST, PUT, DELETE | `/api/customers/{id}` | Admin |
| GET, POST, PUT, DELETE | `/api/rentals/{id}` | מחובר. לקוח רק את שלו (403 על השכרה של אחר) |

**הסטטוסים:**
- 200 ו-201 (עם Location) בהצלחה, ו-204 בעדכון או מחיקה.
- 400 בולידציה.
- 401 בלי token, ו-403 בלי הרשאה.
- 404 כשמשאב לא קיים.
- 409 בהתנגשות: רכב תפוס, מספר רישוי כפול, או שינוי מקביל.

**מבנה תשובת שגיאה:** כל שגיאה חוזרת כ-ProblemDetails, עם `traceId` ששווה ל-CorrelationId של הבקשה.

## לוגים

- **הספרייה:** NLog (`NLog.Web.AspNetCore`). ההגדרות ב-[MyProject/nlog.config](MyProject/nlog.config).
- **היעדים:** הקונסול, וקובץ `logs/rental-<date>.log` בתיקיית הפלט.
- **CorrelationId:** `CorrelationIdMiddleware` נותן מזהה לכל בקשה (או משתמש ב-header ‏`X-Correlation-ID` אם נשלח), ומחזיר אותו ב-header. המזהה מופיע בכל שורת לוג של הבקשה.
- **רמות:**
  - Debug: בפיתוח בלבד.
  - Information: זרימה רגילה.
  - Warning: התנגשות או ניסיון התחברות שנכשל.
  - Error: חריגה שלא טופלה. `ErrorHandlingMiddleware` כותב אותה ומחזיר 500 אחיד.
- **מה לא נכתב:** סיסמאות, tokens ופרטי הרשמה.
