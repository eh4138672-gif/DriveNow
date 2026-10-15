using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MyProject.Auth;
using MyProject.Core.DTOs;
using MyProject.Core.Interfaces;
using MyProject.Data;
using MyProject.Middleware;
using MyProject.Services;
using NLog;
using NLog.Web;

// לוגר לשלב ההפעלה, לפני שה-host קיים. ההגדרות נטענות מ-nlog.config
var startupLogger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // הקוד ממשיך לעבוד מול ILogger של .NET, ו-NLog מתחבר מתחת
    builder.Logging.ClearProviders();
    builder.Host.UseNLog();

    // DbContext הוא scoped: מופע אחד לכל בקשה, משותף ל-Repositories של אותה בקשה.
    // לכן גם ה-Repositories וה-Services הם scoped - singleton שמחזיק DbContext היה משתף אותו בין בקשות
    // PostgreSQL. בפיתוח מחרוזת החיבור נמצאת ב-User Secrets, ובענן במשתנה סביבה ConnectionStrings__RentalDb
    var connectionString = builder.Configuration.GetConnectionString("RentalDb");
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        throw new InvalidOperationException(
            "ConnectionStrings:RentalDb is missing. Set it with: " +
            "dotnet user-secrets set \"ConnectionStrings:RentalDb\" \"Host=localhost;Database=rentaldb;Username=postgres;Password=...\" --project MyProject.API");
    }
    builder.Services.AddDbContext<DataContext>(options =>
        options.UseNpgsql(PostgresConnectionString.Normalize(connectionString)));

    builder.Services.AddScoped<ICarRepository, CarRepository>();
    builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
    builder.Services.AddScoped<IRentalRepository, RentalRepository>();
    builder.Services.AddScoped<IFeatureRepository, FeatureRepository>();
    builder.Services.AddScoped<IUserRepository, UserRepository>();

    builder.Services.AddScoped<ICarService, CarService>();
    builder.Services.AddScoped<ICustomerService, CustomerService>();
    builder.Services.AddScoped<IRentalService, RentalService>();
    builder.Services.AddScoped<IAuthService, AuthService>();

    // ללא מצב פנימי, ולכן singleton
    builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();
    builder.Services.AddSingleton<ITokenService, JwtTokenService>();

    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<ICurrentUser, CurrentUser>();

    builder.Services.AddAutoMapper(typeof(MappingProfile));

    // JWT
    var jwtSection = builder.Configuration.GetSection(JwtSettings.SectionName);
    builder.Services.Configure<JwtSettings>(jwtSection);
    var jwt = jwtSection.Get<JwtSettings>() ?? new JwtSettings();
    if (jwt.Key.Length < 32)
    {
        throw new InvalidOperationException(
            "Jwt:Key is missing or shorter than 32 characters. Set it with: " +
            "dotnet user-secrets set \"Jwt:Key\" \"<random key>\" --project MyProject.API");
    }

    builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwt.Issuer,
                ValidateAudience = true,
                ValidAudience = jwt.Audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(1)
            };

            // ברירת המחדל מחזירה 401/403 עם גוף ריק. כאן הם מקבלים את אותו מבנה JSON כמו שאר השגיאות
            options.Events = new JwtBearerEvents
            {
                OnChallenge = context =>
                {
                    context.HandleResponse();
                    return ProblemResponse.WriteAsync(context.HttpContext, StatusCodes.Status401Unauthorized,
                        "נדרשת התחברות", "יש להתחבר ולשלוח token תקף ב-header Authorization.");
                },
                OnForbidden = context =>
                    ProblemResponse.WriteAsync(context.HttpContext, StatusCodes.Status403Forbidden,
                        "אין הרשאה", "לתפקיד שלך אין הרשאה לפעולה הזאת.")
            };
        });
    builder.Services.AddAuthorization();

    // CORS: בפיתוח הלקוח עובר דרך ה-proxy של Vite ולא צריך את זה. בענן הלקוח יושב בכתובת אחרת,
    // והכתובות המותרות נטענות מהקונפיגורציה (Cors:AllowedOrigins, למשל דרך משתני סביבה ב-Render)
    var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
    builder.Services.AddCors(options =>
        options.AddDefaultPolicy(policy => policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .WithExposedHeaders(CorrelationIdMiddleware.HeaderName)));

    builder.Services.AddControllers().ConfigureApiBehaviorOptions(options =>
    {
        // תשובת 400 האוטומטית של ModelState, עם traceId שתואם ל-CorrelationId
        options.InvalidModelStateResponseFactory = context =>
        {
            var problem = new ValidationProblemDetails(context.ModelState)
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "אחד או יותר מהשדות אינם תקינים"
            };
            problem.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
            return new BadRequestObjectResult(problem) { ContentTypes = { "application/problem+json" } };
        };
    });
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo { Title = "Rental API", Version = "v1" });

        // כפתור Authorize ב-Swagger: מדביקים את ה-token שמתקבל מ-/api/auth/login
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "הדבק את ה-token שהתקבל מ-/api/auth/login"
        });
        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                },
                Array.Empty<string>()
            }
        });
    });

    var app = builder.Build();

    // מריץ migrations ויוצר משתמשי דמו בהפעלה, כך שהמערכת עולה בלי התערבות ידנית בבסיס הנתונים
    using (var scope = app.Services.CreateScope())
    {
        await scope.ServiceProvider.GetRequiredService<DataContext>().Database.MigrateAsync();

        var demoUsers = app.Configuration.GetSection("DemoUsers").Get<List<DemoUserOptions>>();
        if (demoUsers != null)
        {
            await scope.ServiceProvider.GetRequiredService<IAuthService>().SeedDemoUsersAsync(demoUsers, CancellationToken.None);
        }
    }

    // סדר ה-pipeline: טיפול בשגיאות ראשון (עוטף את כל השאר), אחריו CorrelationId,
    // ו-authentication לפני authorization
    app.UseMiddleware<ErrorHandlingMiddleware>();
    app.UseMiddleware<CorrelationIdMiddleware>();

    // Swagger תמיד בפיתוח, ובענן רק אם הופעל במפורש (Swagger:Enabled)
    if (app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("Swagger:Enabled"))
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    app.UseCors();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
// כלי ה-migrations של EF עוצר את ה-host בכוונה עם החריגה הזאת. היא לא שגיאה
catch (Exception ex) when (ex.GetType().Name is not ("StopTheHostException" or "HostAbortedException"))
{
    startupLogger.Error(ex, "Application stopped because of an exception");
    throw;
}
finally
{
    LogManager.Shutdown();
}
