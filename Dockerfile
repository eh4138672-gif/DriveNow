# בנייה והרצה של השרת בענן (Render מריץ .NET דרך Docker)

# שלב 1: בנייה
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish MyProject.API/MyProject.API.csproj -c Release -o /app

# שלב 2: הרצה, על תמונה קטנה שכוללת רק את ה-runtime
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=build /app .

# Render קובע את הפורט במשתנה PORT. ה-HTTPS מטופל אצל Render, ולכן השרת מאזין ב-HTTP
ENV ASPNETCORE_ENVIRONMENT=Production
CMD ASPNETCORE_URLS=http://0.0.0.0:${PORT:-8080} dotnet MyProject.API.dll
