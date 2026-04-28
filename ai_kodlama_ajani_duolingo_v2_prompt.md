# AI Kodlama Ajanı Görev Promptu — AI Eğitim Platformu V2

Sen ASP.NET Core Web API + EF Core + React/TypeScript bilen kıdemli bir full-stack yazılım ajanısın. Aşağıdaki hedefe göre projeyi geliştir:

## Ana Hedef

AI Uzmanlık Eğitim Platformu’nu klasik LMS olarak değil, Duolingo benzeri oyunlaştırılmış mikro öğrenme ürünü olarak inşa et. Platform web’de çalışacak; ileride React Native mobil uygulama aynı API’yi kullanacak.

## Girdi Dosyaları

- `ai_egitim_platformu_duolingo_stratejisi_v2.md`
- `ai_egitim_platformu_gamified_seed_v2.json`

Seed JSON’daki şu alanları veritabanı seed sürecine çevir:

- `resources`
- `learningPaths`
- `projects`
- `gamificationConfig.xpRules`
- `gamificationConfig.badges`
- `gamificationConfig.dailyQuests`
- `gamificationConfig.weeklyQuests`
- `notificationTemplates`
- `experimentation.candidateABTests`

## Öncelikli Teknoloji Kararı

Backend:

- ASP.NET Core Web API
- EF Core
- PostgreSQL veya SQL Server
- JWT access token + refresh token
- Background jobs için Hangfire, Quartz veya hosted service

Frontend:

- React + TypeScript
- React Router
- TanStack Query veya benzeri veri yönetimi
- CSS değişkenleri, Tailwind veya component-level CSS
- Görsel dil: açık, kart tabanlı, yuvarlatılmış, yumuşak gölgeli, mavi/turkuaz/turuncu aksanlı dashboard

## Mimari Modüller

Backend modülleri:

1. Identity
2. Learning
3. Progress
4. Assessment
5. Projects
6. Gamification
7. League
8. Notification
9. AI Mentor
10. Analytics
11. Experimentation
12. Billing hazırlığı
13. Admin

Frontend feature klasörleri:

```txt
features/auth
features/dashboard
features/learning
features/lessonPlayer
features/practice
features/projects
features/gamification
features/leagues
features/quests
features/portfolio
features/resources
features/aiMentor
features/notifications
features/admin
features/analytics
```

## MVP Akışları

Aşağıdaki akışlar çalışmadan MVP tamam sayılmaz:

1. Kullanıcı kayıt/giriş yapar.
2. Günlük XP hedefi seçer.
3. Başlangıç/orta/ileri yol haritasından birini seçer.
4. Dashboard’da bugünkü AI Byte kartını görür.
5. Mikro dersi LessonPlayer ile adım adım tamamlar.
6. Mini alıştırma gönderir.
7. XP kazanır.
8. Streak uzar.
9. Günlük görev tamamlanır.
10. Bir quiz çözer.
11. Bir proje adımı teslim eder.
12. Portföy kanıtı oluşur.
13. Lig sıralamasında görünür.
14. AI mentor dersle ilgili açıklama verir.
15. Admin yeni ders, kaynak, rozet, görev ve bildirim şablonu ekleyebilir.
16. Analytics event kayıtları oluşur.
17. En az bir A/B test assignment mekanizması çalışır.

## Backend Entity Gereksinimleri

Mutlaka şu entity’leri tasarla:

- ApplicationUser
- LearningPath
- Unit
- Lesson
- Exercise
- Resource
- LessonResource
- UserLessonProgress
- QuizAttempt
- Project
- ProjectSubmission
- ProjectRubricCriterion
- XpTransaction
- UserStreak
- Badge
- UserBadge
- Quest
- UserQuest
- LeagueSeason
- LeagueParticipant
- NotificationTemplate
- NotificationLog
- Experiment
- ExperimentVariant
- ExperimentAssignment
- AnalyticsEvent
- AiConversation
- AiMessage
- SubscriptionPlan
- UserSubscription

## Kritik İş Kuralları

### XP

XP doğrudan kullanıcı toplamına yazılıp geçilmemeli. Her XP kazanımı `XpTransaction` olarak kaydedilmeli. Kullanıcının toplam XP’si transaction toplamından veya cache alandan hesaplanabilir.

### Streak

Streak kullanıcının timezone’una göre hesaplanmalı. UTC günüyle doğrudan hesaplama yapma.

### Lig

Lig haftalık sezon mantığıyla çalışmalı. Kullanıcılar 30 kişilik gruplara yerleşmeli. İlk 10 üst lige çıkmalı, son 5 alt lige inmeli. Yeni kullanıcılarda ilk 2 hafta düşme kapalı olmalı.

### Ders

Ders uzun metin sayfası değil, adım adım akan mikro öğrenme deneyimi olmalı:

1. hedef
2. mini açıklama
3. örnek
4. alıştırma
5. geri bildirim
6. ödül ekranı
7. sonraki adım

### A/B Test

Experiment assignment deterministik olmalı:

`hash(userId + experimentKey) -> variant`

Kullanıcı aynı deneyde her zaman aynı varyantı görmeli.

### AI Mentor

AI çağrıları sadece backend üzerinden yapılmalı. API anahtarı frontend veya mobil uygulamada bulunmamalı. Kullanıcı girdisi, ilgili ders context’i ve rubrik backend’de hazırlanıp AI servisine gönderilmeli.

## API Endpoint Taslağı

Auth:

- POST `/api/v1/auth/register`
- POST `/api/v1/auth/login`
- POST `/api/v1/auth/refresh`
- POST `/api/v1/auth/logout`

Learning:

- GET `/api/v1/learning-paths`
- GET `/api/v1/learning-paths/{slug}`
- GET `/api/v1/lessons/{slug}`
- POST `/api/v1/lessons/{slug}/start`
- POST `/api/v1/lessons/{slug}/complete`

Exercises:

- POST `/api/v1/exercises/{id}/submit`
- POST `/api/v1/quizzes/{lessonSlug}/attempts`
- GET `/api/v1/review/mistakes`

Gamification:

- GET `/api/v1/gamification/me`
- GET `/api/v1/quests/daily`
- POST `/api/v1/quests/{id}/claim`
- GET `/api/v1/badges/me`
- GET `/api/v1/leagues/current`

Projects:

- GET `/api/v1/projects`
- GET `/api/v1/projects/{slug}`
- POST `/api/v1/projects/{slug}/submissions`
- GET `/api/v1/portfolio/me`

AI Mentor:

- POST `/api/v1/ai/chat`
- POST `/api/v1/ai/explain-lesson`
- POST `/api/v1/ai/review-submission`
- POST `/api/v1/ai/generate-practice`

Analytics:

- POST `/api/v1/events/track`
- GET `/api/v1/admin/analytics/funnel`
- GET `/api/v1/admin/analytics/retention`

Experimentation:

- GET `/api/v1/experiments/assignments`
- POST `/api/v1/admin/experiments`
- PATCH `/api/v1/admin/experiments/{id}`

Admin:

- CRUD learning paths
- CRUD units
- CRUD lessons
- CRUD resources
- CRUD badges
- CRUD quests
- CRUD notification templates

## UI Sayfaları

1. Login/Register
2. Onboarding: hedef seçimi + yol seçimi
3. Dashboard
4. Bugünkü AI Byte
5. Yol Haritası
6. Modül Detay
7. LessonPlayer
8. Pratik Alanı
9. Quiz Sonuç
10. Projeler
11. Proje Teslim
12. Ligler
13. Görevler
14. Portföy
15. Kaynaklar
16. AI Mentor
17. Bildirimler
18. Admin Dashboard
19. Admin İçerik Yönetimi
20. Admin Deney Yönetimi

## Tasarım Kuralları

- Açık arka plan: `#F7F9FD`
- Kart yüzeyi: `#FFFFFF`
- Primary: `#5E7CFB`
- Secondary: `#6FD3C3`
- Accent: `#F8A23D`
- Border: `#E6EAF2`
- Radius: 16-24px
- Shadow: yumuşak, düşük opaklık
- Sidebar sabit ve temiz olmalı.
- Dashboard kartları görseldeki gibi nefes alan grid yapısında olmalı.
- Başlangıç mavi, orta turkuaz, ileri turuncu aksanla temsil edilmeli.

## Uygulama Sırası

1. Backend solution + frontend app iskeleti kur.
2. Auth ve kullanıcı modeli oluştur.
3. Seed JSON import mekanizması yaz.
4. LearningPath/Unit/Lesson ekranlarını kur.
5. LessonPlayer geliştir.
6. Exercise submit ve quiz attempt geliştir.
7. XP transaction ve streak sistemini ekle.
8. Daily quests ekle.
9. League MVP ekle.
10. Project submission ve portfolio ekle.
11. Notification template/log ekle.
12. Analytics events ekle.
13. Experiment assignment ekle.
14. AI mentor endpoint’lerini ekle.
15. Admin CRUD ekranlarını tamamla.
16. E2E testlerle MVP akışlarını doğrula.

## Test Gereksinimleri

Unit test:

- XP rule calculation
- Streak calculation by timezone
- League promotion/demotion
- Experiment assignment determinism
- Lesson completion validation

Integration test:

- Register -> select path -> complete lesson -> XP -> streak
- Complete quiz -> wrong answers -> review list
- Submit project -> portfolio evidence
- Event tracking writes AnalyticsEvent

Frontend test:

- Dashboard renders cards
- LessonPlayer step navigation
- Exercise submit states
- League card ranking
- Admin lesson creation form

## Çıktı Beklentisi

Kod üretirken her modülü küçük PR mantığıyla tamamla. Önce çalışan sade MVP, sonra polish. Veritabanı migration’larını ve seed importunu unutma. Kullanıcı deneyiminde amaç: “Bugün sadece 3 dakikalık bir AI Byte bitireyim” hissini oluşturmak.
