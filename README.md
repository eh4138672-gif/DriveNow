# DriveNow - שרת להשכרת רכבים

שרת ASP.NET Core Web API לניהול השכרת רכבים. לקוחות נרשמים, מחפשים רכב לפי דגם, מחיר ותכונות, ומזמינים אותו לטווח תאריכים. מנהל מנהל את צי הרכבים ואת הלקוחות, ורואה את כל ההזמנות. המערכת בנויה בארבע שכבות (Core, Data, Service, API), עובדת מול PostgreSQL דרך EF Core (Npgsql), ומאובטחת ב-JWT עם שני תפקידים.

## המשאב המוגבל והתחרות עליו

**המשאב:** רכב בטווח זמן (תאריך ושעה, בקפיצות של חצי שעה). אסור ששתי השכרות של אותו רכב יחפפו בזמן. השכרה יכולה להתחיל בדיוק ברגע שהקודמת מוחזרת. המחיר מחושב לפי שעה: לכל רכב מחיר לשעה בשקלים שלמים (מינימום ₪30), וכל שעה שהתחילה מחויבת כשעה מלאה. השכרה של יותר מיומיים (48 שעות) מקבלת 10% הנחה על המחיר לשעה, על כל השעות. המחיר אחרי ההנחה מעוגל לשקל שלם (`RentalPricing`).

**איפה התחרות:** שני לקוחות מזמינים את אותו רכב לאותם תאריכים באותו רגע. שתי הבקשות קוראות את הרכב, שתיהן בודקות ורואות שהוא פנוי, ושתיהן מנסות לשמור. בלי הגנה, שתיהן יקבלו אישור.

**איך המערכת מגיבה (optimistic concurrency):**
- לישות `Car` יש concurrency token בשם `Version` (`Guid`, מוגדר `IsConcurrencyToken()` ב-Fluent API). ב-PostgreSQL אין `rowversion`, ולכן זה token שהמערכת מנהלת בעצמה: `DataContext` דורס את `SaveChanges` ומחליף את ה-`Version` של כל רכב שנוסף או השתנה, כך שאף שירות לא צריך לזכור לעשות את זה.
- הזמנה (`RentalService.TryBookAsync`):
  1. טוענת את הרכב.
  2. בודקת שאין השכרה חופפת.
  3. מוסיפה את ההשכרה ומעדכנת את השדה `LastBookedAt` של הרכב.
- בדיקת החפיפה והשמירה רצות בתוך **טרנזקציה מפורשת אחת** (`BeginTransactionAsync`). אם יוצאים ממנה בלי `Commit`, הכול מתבטל.
- `SaveChanges` מריץ את ה-INSERT של ההשכרה ואת ה-UPDATE של הרכב באותה טרנזקציה, וה-UPDATE כולל `WHERE "Version" = <הערך שנקרא>`.
- אם הזמנה מקבילה נשמרה בינתיים, ה-Version כבר השתנה. ה-UPDATE לא מוצא שורה, EF זורק `DbUpdateConcurrencyException`, וכל הטרנזקציה מתבטלת, כולל ה-INSERT.
- ה-Repository עוטף את החריגה ב-`ConcurrencyConflictException` מ-Core, כדי ששכבת ה-Service לא תהיה תלויה ב-EF.
- ה-Service מחזיר **409 Conflict** עם הודעה ברורה, וההתנגשות נכתבת ללוג ברמת Warning.
- גם עדכון רכב מוגן: הלקוח שולח את ה-Version שקרא. אם הרכב השתנה מאז, מוחזר 409.
- גם ביטול השכרה ועדכון או מחיקה של לקוח שמתנגשים עם משתמש אחר מחזירים 409. אותו דבר קורה בשתי הרשמות מקבילות עם אותו אימייל: האינדקס הייחודי דוחה את השנייה, וה-Repository עוטף את השגיאה ב-`DuplicateKeyException`.

**ההוכחה:** ב-[Rental.Tests/ConcurrencyTests.cs](Rental.Tests/ConcurrencyTests.cs) יש שלוש בדיקות:
1. שני `DbContext` נפרדים קוראים את אותו רכב ומשנים אותו. הראשון שומר בהצלחה, והשני מקבל `DbUpdateConcurrencyException`.
2. תרחיש ה-race צעד אחר צעד: שתי הזמנות רואות שהרכב פנוי, ורק הראשונה נשמרת.
3. שתי הזמנות מקבילות אמיתיות דרך ה-Service, 5 פעמים. בכל פעם בדיוק אחת מצליחה, והשנייה מקבלת Conflict.

בלקוח, המנגנון פועל מאחורי הקלעים: מי שמנסה להזמין רכב שנתפס באותו רגע מקבל הודעה ברורה שהרכב תפוס (409).

## מבנה

| פרויקט | תוכן | תלוי ב |
|---|---|---|
| `Rental.Core` | ישויות, DTOs, ממשקים, `Result` | - |
| `Rental.Data` | `DataContext`, Repositories, Migrations | Core |
| `Rental.Service` | לוגיקה עסקית, AutoMapper, hash לסיסמאות | Core |
| `MyProject` (API) | Controllers, Middleware, JWT, `Program.cs` | Core, Service, ו-Data לרישום בלבד |
| `Rental.Tests` | xUnit ו-Moq, ובדיקות concurrency על PostgreSQL אמיתי | כולם |

## הרצה מקומית

**דרישות:**
- .NET SDK 6 ומעלה. הפרויקט מכוון ל-net6.0, ורץ גם על 8 ו-10 בזכות `RollForward`.
- PostgreSQL 13 ומעלה, שרץ מקומית (למשל PostgreSQL 16 מ-https://www.postgresql.org/download/windows/, פורט 5432).

**1. סודות ב-User Secrets.** פעולה חד-פעמית, כי הם לא נמצאים ב-repo:

```bash
dotnet user-secrets set "Jwt:Key" "<מחרוזת אקראית של 32 תווים לפחות>" --project MyProject
```
```bash
dotnet user-secrets set "ConnectionStrings:RentalDb" "Host=localhost;Port=5432;Database=rentaldb;Username=postgres;Password=<הסיסמה של postgres>" --project MyProject
```

**2. הרצת השרת:**

```bash
dotnet run --project MyProject --launch-profile MyProject
```

בהפעלה הראשונה השרת:
- יוצר את בסיס הנתונים `rentaldb` ומריץ את ה-migrations (`InitialSchema` לטבלאות, `SeedData` לנתוני ההתחלה).
- מכניס נתוני התחלה: רכבים, לקוחות, תכונות והשכרות.
- יוצר את משתמשי הדמו.

אין צורך בשום פעולה ידנית בבסיס הנתונים.

**כתובות:**
- Swagger: https://localhost:7017/swagger. כדי לבדוק endpoints מוגנים, מתחברים דרך `/api/auth/login` ומדביקים את ה-token בכפתור **Authorize**.
- הלקוח ([repository נפרד](https://github.com/eh4138672-gif/DriveNow-client)): http://localhost:5173. ה-proxy שלו מפנה ל-`https://localhost:7017`.

**3. הבדיקות:** בדיקות ה-concurrency יוצרות ומוחקות בעצמן בסיס נתונים נפרד. את מחרוזת החיבור אליו קובעים במשתנה הסביבה `RENTAL_TEST_DB`:

```bash
setx RENTAL_TEST_DB "Host=localhost;Port=5432;Database=rentaldb_tests;Username=postgres;Password=<הסיסמה של postgres>"
```
```bash
dotnet test
```

## העלאה לענן (Render)

1. **בסיס נתונים:** ב-Render, ‏New ← Postgres, ‏Free. אחרי היצירה מעתיקים את ה-**Internal Database URL**.
2. **השרת:** ‏New ← Web Service ← ה-repository הזה, ‏Language: Docker (יש `Dockerfile` בשורש), ‏Free. משתני סביבה:
   - `ConnectionStrings__RentalDb`: ה-Internal Database URL (השרת ממיר בעצמו כתובת `postgres://...` לפורמט של Npgsql).
   - `Jwt__Key`: ערך אקראי (כפתור Generate).
   - `DemoUsers__0__Email`, `DemoUsers__0__Password`, `DemoUsers__0__Role` = `Admin`: מנהל עם סיסמה פרטית.
   - `Swagger__Enabled` = `true` (אופציונלי).
   - `Cors__AllowedOrigins__0`: כתובת הלקוח ב-Render.
3. **הלקוח:** ‏New ← Static Site ← ה-repository של הלקוח. ‏Build: `npm ci && npm run build`, ‏Publish directory: `dist`, ומשתנה `VITE_API_URL` = כתובת השרת.

ה-migrations רצים אוטומטית בעליית השרת, כך שאין צורך בפעולה ידנית בבסיס הנתונים.

## משתמשי דמו

| תפקיד | אימייל | סיסמה | הרשאות |
|---|---|---|---|
| Admin | admin@rental.local | Admin123! | ניהול רכבים ולקוחות, צפייה וביטול של כל ההשכרות, הזמנה עבור לקוח לפי מספר רישיון הנהיגה שלו |
| Customer | israel@rental.local | Customer123! | הזמנה על שמו (ברירת המחדל) או על שם לקוח אחר לפי מספר רישיון הנהיגה, צפייה וביטול רק של ההשכרות שעל שמו |
| Customer | dana@rental.local | Customer123! | כנ"ל |

הפרטים נטענים מ-`appsettings.Development.json` (`DemoUsers`), ונשמרים בבסיס הנתונים עם hash מסוג PBKDF2. בלקוח אין כפתורי כניסה מהירה: נכנסים רק עם אימייל וסיסמה.

**על שם מי ההזמנה:** ההזמנה נרשמת כברירת מחדל על שם המשתמש המחובר. כדי להזמין עבור מישהו אחר, לוחצים "שינוי" ומקלידים את מספר רישיון הנהיגה שלו. השרת מאתר את הלקוח לפי התאמה מדויקת, ואין בלקוח שום מסך שמציג את רשימת הלקוחות במערכת.

## Endpoints עיקריים

| Method | נתיב | הרשאה |
|---|---|---|
| POST | `/api/auth/register`, `/api/auth/login` | פתוח |
| GET | `/api/cars?page=&pageSize=&search=&featureId=&sortBy=&descending=` | פתוח |
| GET | `/api/features` | פתוח |
| POST, PUT, DELETE | `/api/cars/{id}` | Admin |
| PATCH | `/api/cars/{id}/availability` (`{ "isActive": false }`) | Admin. הוצאת רכב מההשכרה או החזרתו: רכב שהוצא לא מוצג ללקוחות ולא ניתן להזמנה. רכב עם השכרות אי אפשר למחוק (409), אבל אפשר להוציא אותו מההשכרה |
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
