# AI Eğitim Platformu V2 MVP

ASP.NET Core Web API + EF Core + React/TypeScript tabanlı oyunlaştırılmış mikro öğrenme MVP'si.

## Çalıştırma

API:

```powershell
dotnet run --project src/AiEducation.Api/AiEducation.Api.csproj --launch-profile http
```

Frontend:

```powershell
cd src/AiEducation.Web
npm install
npm run dev
```

Frontend varsayılan olarak `http://localhost:5076/api/v1` API adresini kullanır. Farklı API adresi için:

```powershell
$env:VITE_API_BASE_URL="http://localhost:5076/api/v1"
npm run dev
```

## Veritabanı

- Üretim/SQL Server migration hedefi: `appsettings.json` içindeki SQL Server LocalDB connection string.
- Development profili: LocalDB başlatılamayan makinelerde çalışabilmesi için SQLite fallback kullanır ve seed verisini otomatik içeri aktarır.
- SQL Server migration dosyaları `src/AiEducation.Api/Data/Migrations` altındadır.

SQL Server LocalDB çalışıyorsa:

```powershell
dotnet ef database update --project src/AiEducation.Api/AiEducation.Api.csproj --startup-project src/AiEducation.Api/AiEducation.Api.csproj
```

## Testler

Backend:

```powershell
dotnet test tests/AiEducation.Api.Tests/AiEducation.Api.Tests.csproj
```

Frontend:

```powershell
cd src/AiEducation.Web
npm test
npm run build
```
