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

## Veritabanı ve Migration

- Varsayılan SQL Server hedefi `appsettings.json` içindeki LocalDB connection string'dir.
- LocalDB başlatılamayan development makinelerinde `DatabaseProvider=Sqlite` override'ı kullanılabilir.
- SQLite development fallback `EnsureCreated` + uyumluluk şeması + seed importer ile çalışır.
- SQL Server migration dosyaları `src/AiEducation.Api/Data/Migrations` altındadır.
- Production ortamında `EnsureCreated` kullanılmamalıdır; migration uygulama stratejisi ayrıca yönetilmelidir.
- Production JWT signing key mutlaka environment variable veya secret store üzerinden verilmelidir.

SQL Server migration update:

```powershell
$env:ASPNETCORE_ENVIRONMENT="Migration"
$env:Jwt__SigningKey="local-migration-signing-key-change-before-prod-32chars"
dotnet ef database update --project src/AiEducation.Api/AiEducation.Api.csproj --startup-project src/AiEducation.Api/AiEducation.Api.csproj
```

SQLite development örneği:

```powershell
$env:ASPNETCORE_ENVIRONMENT="Development"
$env:DatabaseProvider="Sqlite"
$env:ConnectionStrings__SqliteConnection="Data Source=ai_education_platform_v2_dev.db"
dotnet run --project src/AiEducation.Api/AiEducation.Api.csproj --launch-profile http
```

## Development Admin

Development seed çalıştığında roller (`Admin`, `Learner`) ve dev-only admin hesabı oluşturulur:

- E-posta: `admin@example.com`
- Şifre: `Admin123!`

Bu bilgiler yalnızca yerel development içindir. Production ortamında dev admin ve varsayılan signing key kullanılmamalıdır.

## Test Komutları

Backend:

```powershell
dotnet restore
dotnet build AiEducationPlatform.sln
dotnet test tests/AiEducation.Api.Tests/AiEducation.Api.Tests.csproj
```

Frontend unit/component:

```powershell
cd src/AiEducation.Web
npm install
npm run build
npm test
npm run lint
```

Playwright E2E:

```powershell
cd src/AiEducation.Web
npx playwright install chromium
npx playwright test
```

E2E config kendi SQLite dosyasını (`src/AiEducation.Web/ai_education_platform_v2_e2e.db`) her koşuda temizler, API'yi `DatabaseProvider=Sqlite` ile başlatır ve Vite web server'ını `http://localhost:5173` üzerinde çalıştırır.

## MVP Kapsamı

- Student flow: kayıt, onboarding, AI Byte tamamlama, exercise submit, XP, streak, quest ve league görünürlüğü.
- Admin Content Studio: lesson list, create/edit, status transition, preview, resource picker ve kalite skoru.
- Quiz editor: 4 seçenekli çoktan seçmeli soru oluşturma, tek doğru cevap validation'ı ve backend scoring.
- League rollover: idempotent season close, promotion/demotion/protection eventleri ve admin manuel tetikleme endpointleri.
