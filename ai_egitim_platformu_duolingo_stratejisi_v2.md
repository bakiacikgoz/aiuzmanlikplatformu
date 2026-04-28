# AI Uzmanlık Eğitim Platformu — Duolingo Esinli V2 Ürün, İçerik ve Teknik Plan

**Sürüm:** 2.0  
**Amaç:** Hazırlanan eğitim platformunu yalnızca klasik LMS olmaktan çıkarıp, günlük alışkanlık yaratan, oyunlaştırılmış, veriyle gelişen ve proje/portföy kanıtı üreten bir AI öğrenme platformuna dönüştürmek.  
**Teknik varsayım:** ASP.NET Core Web API + EF Core + React/TypeScript. Sonraki fazda React Native mobil uygulama aynı API’yi kullanacak.  
**Tasarım dili:** Görseldeki gibi açık renkli, modern, kart tabanlı, yumuşak gölgeli, mavi/turkuaz/turuncu aksanlı dashboard.  
**Kritik karar:** Duolingo yaklaşımını birebir kopyalamıyoruz. Dil öğrenimindeki kısa tekrar döngüsünü AI eğitimine uyarlıyoruz. AI uzmanlığı için sadece “3 dakikalık ders” yeterli değildir; bu yüzden sistem **mikro alışkanlık + derin proje** üzerine kurulmalıdır.

---

## 1. Stratejik Yön Değişikliği

Önceki plan iyi bir eğitim platformu iskeletiydi. Bu V2 planın farkı şudur:

> Platformun ana işi “ders göstermek” değil, kullanıcıyı her gün küçük bir öğrenme eylemine sokmak, her hafta ölçülebilir beceri üretmesini sağlamak ve uzun vadede portföy kanıtı oluşturmaktır.

Buna göre ürün beş büyüme ve öğrenme ilkesine yaslanır:

1. **Oyunlaştırma:** XP, seri, lig, rozet, günlük görev, haftalık meydan okuma.
2. **Küçük adımlar:** Her gün 3-5 dakikalık “AI Byte” dersi; haftada 1-2 derin uygulama.
3. **Veri ve deney kültürü:** Her ürün kararı event tracking ve A/B test mantığıyla ölçülür.
4. **Maskot ve karakter sistemi:** Platformun sıcak, hatırlatıcı ve sosyal medyada paylaşılabilir bir kişiliği olur.
5. **Freemium model:** Temel eğitim ücretsiz kalır; premium değer AI mentor, proje inceleme, gelişmiş analiz ve sertifika tarafında oluşur.

---

## 2. Ürün Konumlandırması

### 2.1 Tek cümlelik konumlandırma

**AI Uzmanlık Platformu, yapay zeka öğrenimini günlük alışkanlığa dönüştüren; mikro ders, pratik, proje, portföy ve AI mentor sistemini birleştiren oyunlaştırılmış eğitim platformudur.**

### 2.2 Klasik LMS’ten farkı

| Klasik LMS | Bu Platform |
|---|---|
| Uzun video/ders listesi | 3-5 dakikalık mikro ders akışı |
| Kullanıcı motivasyonuna bırakır | Seri, XP, görev ve lig ile günlük tetikleme yapar |
| Tamamlama yüzdesi odaklı | Beceri, proje ve portföy kanıtı odaklı |
| Az ölçüm | Event tracking, funnel, retention ve A/B test odaklı |
| Statik içerik | AI mentor + adaptif tekrar + kişisel öneri |

### 2.3 AI eğitimine özel denge

AI uzmanlığı ciddi beceri gerektirir. Bu yüzden platform üç öğrenme moduna ayrılır:

| Mod | Süre | Amaç | Örnek |
|---|---:|---|---|
| **Learn** | 3-5 dk | Kavramı küçük parçada öğretmek | “Token nedir?” |
| **Practice** | 5-12 dk | Tek beceriyi uygulatmak | “Prompt’u düzelt” |
| **Build** | 30-180 dk | Portföy kanıtı üretmek | “Basit RAG asistanı kur” |

Günlük alışkanlık **Learn/Practice** ile, gerçek uzmanlık **Build** ile oluşur.

---

## 3. Ana Ürün Döngüsü

### 3.1 Günlük alışkanlık döngüsü

1. Kullanıcı web/push/e-posta bildirimi alır.
2. Dashboard’da “Bugünkü AI Byte” kartını görür.
3. 3-5 dakikalık mikro dersi tamamlar.
4. Mini alıştırma yapar.
5. XP kazanır.
6. Streak uzar.
7. Günlük görev tamamlanır.
8. Bir sonraki küçük adım önerilir.

### 3.2 Haftalık beceri döngüsü

1. Kullanıcı hafta başında mini hedef seçer.
2. 5-7 mikro ders tamamlar.
3. Bir kontrol testi geçer.
4. Bir proje adımı teslim eder.
5. AI mentor veya admin rubriğe göre geri bildirim verir.
6. Portföy kanıtı güncellenir.

### 3.3 Sosyal rekabet döngüsü

1. Kullanıcı haftalık lige yerleşir.
2. Kalite kapısından geçen XP sıralamaya eklenir.
3. İlk 10 üst lige çıkar, son 5 alt lige iner.
4. Başlangıç kullanıcılarında ilk 2 hafta düşme kapalıdır.
5. Lig ekranı kullanıcıyı tekrar pratik yapmaya çağırır.

---

## 4. Bilgi Mimarisi

Sol menü görseldeki yapıyı korur ama oyunlaştırma modülleri eklenir:

1. Ana Panel
2. Bugünkü AI Byte
3. Yol Haritası
4. Seviyeler
5. Pratik Alanı
6. Projeler
7. Ligler
8. Görevler
9. 12 Aylık Plan
10. Uzmanlaşma
11. Portföy
12. Kaynaklar
13. AI Asistan
14. Bildirimler
15. Admin Panel

Mobil uygulama çıktığında bu yapı tab bar + stack navigation’a indirgenir:

- Ana
- Öğren
- Pratik
- Lig
- Profil

---

## 5. Dashboard Tasarımı

Dashboard görseldeki kart düzenini korumalıdır. Yeni kartlar:

### 5.1 Üst hero alanı

- Başlık: “Yapay Zeka Uzmanı Öğrenme Yol Haritası”
- Alt metin: “Bugün 3 dakikalık bir adımla serini koru.”
- Primary CTA: “Bugünkü AI Byte”
- Secondary CTA: “Yol Haritasını İncele”
- Sağ tarafta soyut 3D/AI görseli veya maskot görseli.

### 5.2 Birinci kart sırası

1. Başlangıç seviyesi kartı
2. Orta seviye kartı
3. İleri seviye kartı
4. Haftalık çalışma dağılımı

### 5.3 İkinci kart sırası

1. Bugünkü görevler
2. Seri ve XP durumu
3. Lig sıralaması
4. Portföy kanıtları

### 5.4 Üçüncü kart sırası

1. 12 aylık yol haritası timeline
2. Uzmanlaşma alanları
3. AI mentor önerisi
4. Deneme/tekrar listesi

---

## 6. Oyunlaştırma Sistemi

### 6.1 XP sistemi

XP sadece gerçek öğrenme eylemlerinde verilmelidir. Boş tıklama, sayfa gezme veya içeriği açıp kapatma XP vermemelidir.

| Eylem | XP | Kalite şartı |
|---|---:|---|
| Mikro ders tamamlama | 10 | Mini alıştırma zorunlu |
| Pratik görevi | 15 | Cevap/kod/prompt girilmiş olmalı |
| Kontrol testi geçme | 25 | En az %70 |
| Proje adımı | 40 | Kanıt dosyası/linki gerekir |
| Proje teslimi | 120 | Rubrik incelemesi gerekir |
| Resmi kaynak özeti | 20 | AI/admin değerlendirmesi gerekir |
| Yardımcı peer feedback | 10 | Alıcı “faydalı” işaretlemeli |

**Anti-cheat kuralı:** Günlük XP limiti, kalite kapısı, tekrar eden aynı içerik için azalan XP ve şüpheli aktivite bayrağı olmalıdır.

### 6.2 Streak sistemi

- Günlük hedef varsayılanı: 20 XP.
- Kullanıcı hedefi 10/20/30/40/50 XP olarak seçebilir.
- Seri şu durumda uzar: günlük XP hedefi tamamlandıysa veya en az 1 mikro ders tamamlandıysa.
- Ayda 2 ücretsiz streak freeze verilir.
- Premium kullanıcıda ayda 5 freeze olabilir.
- “Streak kurtarma”: Kullanıcı kaçırdığı günden sonraki 24 saat içinde 2 mikro ders tamamlarsa ayda 1 kez serisini kurtarabilir.

### 6.3 Lig sistemi

| Lig | Amaç |
|---|---|
| Bronze | Başlangıç motivasyonu |
| Silver | İlk rekabet hissi |
| Gold | Düzenli kullanıcı ayrımı |
| Sapphire | Orta aktif kullanıcı |
| Ruby | Güçlü alışkanlık |
| Emerald | Proje odaklı kullanıcı |
| Diamond | En aktif ve kaliteli üreticiler |

Kurallar:

- Lig sezonu 7 gün sürer.
- Grup büyüklüğü 30 kullanıcıdır.
- İlk 10 bir üst lige çıkar.
- Son 5 bir alt lige düşer.
- İlk 2 hafta yeni kullanıcılarda düşme kapalıdır.
- Kullanıcılar seviye ve son 14 günlük aktiviteye göre eşleştirilir.
- Sadece kalite kapısından geçmiş XP lige yazılır.

### 6.4 Rozet sistemi

Rozetler davranışları şekillendirmelidir:

| Rozet | Koşul |
|---|---|
| İlk AI Byte | İlk mikro dersi tamamlama |
| 7 Günlük Seri | 7 gün üst üste günlük hedef |
| Python Filizi | Python modülünü bitirme |
| İlk Model | İlk scikit-learn modelini çalıştırma |
| RAG Kurucusu | Basit RAG prototipi gönderme |
| Güvenlik Bilinci | Prompt injection görevini geçme |
| Portföy Üreticisi | 3 portföy kanıtı ekleme |

### 6.5 Görev sistemi

Günlük görevler kısa olmalı:

- Bir AI Byte tamamla.
- Bir pratik sorusu çöz.
- Dünkü hatanı tekrar et.

Haftalık görevler beceriye bağlanmalı:

- Bir proje adımı teslim et.
- Bir resmi kaynağı özetle.
- Bir kontrol testini geç.

---

## 7. Küçük Adımlar İlkesi ve Ders Tasarımı

### 7.1 Mikro ders standardı

Her mikro ders şu kurala uymalıdır:

1. Tek öğrenme hedefi.
2. Tek kavram.
3. Tek küçük örnek.
4. Tek uygulama.
5. Anında geri bildirim.
6. Bir sonraki küçük adım.

Ders süresi:

- Başlangıç: 3-5 dakika
- Orta: 4-7 dakika
- İleri: 5-8 dakika

Fakat ileri seviyede bile “ders” kısa kalır; derin çalışma “Build” modunda yapılır.

### 7.2 Mikro ders şablonu

```md
# Ders Başlığı

Süre: 3-5 dk
XP: 10
Seviye: Başlangıç / Orta / İleri
Beceri: Tek beceri adı

## 1. Bugünkü küçük hedef
Bu dersin sonunda şunu yapabileceksin: ...

## 2. Mini açıklama
En fazla 120-180 kelime.

## 3. Mini örnek
Kod, prompt, tablo veya kısa senaryo.

## 4. Şimdi sen dene
Tek soru veya tek kod/prompt tamamlama görevi.

## 5. Anında geri bildirim
Doğruysa neden doğru, yanlışsa hangi kavram eksik?

## 6. Sonraki adım
Bir sonraki ders veya pratik önerisi.
```

### 7.3 AI eğitiminde kullanılacak alıştırma tipleri

| Tip | Açıklama | Otomatik ölçüm |
|---|---|---|
| Çoktan seçmeli | Kavram kontrolü | Evet |
| Kod boşluk doldurma | Python/ML kod tamamlama | Evet |
| Hata ayıklama | Hatalı kodu bulma | Kısmen |
| Prompt düzeltme | Kötü prompt’u iyileştirme | AI rubrik gerekir |
| Kısa cevap | Kavramı kendi cümlesiyle açıklama | AI rubrik gerekir |
| Mini notebook | Küçük veri/model görevi | Kısmen |
| Eval kararı | Model çıktısını puanlama | Evet/Kısmen |

---

## 8. Öğrenme Yol Haritaları ve İçerik Yapısı

Bu V2 seed toplamda şu başlangıç içeriğini verir:

| Seviye | Modül sayısı | Seed ders/kontrol sayısı | Önerilen süre |
|---|---:|---:|---|
| Başlangıç | 7 | 42 | 8-12 hafta |
| Orta | 8 | 48 | 3-9 ay |
| İleri | 8 | 48 | 9-18+ ay |

### 8.1 Modül matrisi

| Seviye | Modül | Tahmini süre | İlk mikro dersler | Kapanış |
|---|---|---:|---|---|
| Başlangıç AI Uzmanı | AI Okuryazarlığı | 5 gün | Yapay zeka nedir, ne değildir?, Kural tabanlı sistem, ML ve LLM farkı, AI projesinde veri-model-çıktı zinciri... | Kontrol testi + mini görev |
| Başlangıç AI Uzmanı | Python Temelleri | 5 gün | Python ortamını kurma, Değişkenler ve veri tipleri, Koşullar ve döngüler... | Kontrol testi + mini görev |
| Başlangıç AI Uzmanı | Veri Analizi | 5 gün | Tablo verisini okuma, Satır ve sütun seçme, Eksik veriyi anlama... | Kontrol testi + mini görev |
| Başlangıç AI Uzmanı | Matematik Sezgisi | 5 gün | Vektör fikri, Ortalama, varyans ve dağılım, Olasılık sezgisi... | Kontrol testi + mini görev |
| Başlangıç AI Uzmanı | Klasik Makine Öğrenmesi | 5 gün | Regresyon problemi, Sınıflandırma problemi, Train-test ayrımı... | Kontrol testi + mini görev |
| Başlangıç AI Uzmanı | LLM Temelleri | 5 gün | Token nedir?, Prompt yazma prensipleri, Few-shot örnekleme... | Kontrol testi + mini görev |
| Başlangıç AI Uzmanı | Başlangıç Projesi | 5 gün | Problem seçme, Veriyi hazırlama, Basit model kurma... | Kontrol testi + mini görev |
| Orta Seviye AI Uzmanı | Derin Öğrenme | 5 gün | Nöron ve katman sezgisi, Tensor kavramı, İleri yayılım... | Kontrol testi + mini görev |
| Orta Seviye AI Uzmanı | Model Eğitimi ve Değerlendirme | 5 gün | Overfitting nedir?, Regularization, Hiperparametre denemesi... | Kontrol testi + mini görev |
| Orta Seviye AI Uzmanı | Embedding ve RAG | 5 gün | Embedding nedir?, Vektör arama, Chunking stratejisi... | Kontrol testi + mini görev |
| Orta Seviye AI Uzmanı | LLM Uygulamaları | 5 gün | API ile LLM çağırma, Structured output, Tool/function calling mantığı... | Kontrol testi + mini görev |
| Orta Seviye AI Uzmanı | Deployment | 5 gün | API endpoint tasarımı, Docker temel akışı, Ortam değişkenleri... | Kontrol testi + mini görev |
| Orta Seviye AI Uzmanı | MLOps | 5 gün | Experiment tracking, Model versioning, Dataset versioning... | Kontrol testi + mini görev |
| Orta Seviye AI Uzmanı | Ürün Analitiği | 5 gün | Event tracking nedir?, Funnel okuma, Retention metriği... | Kontrol testi + mini görev |
| Orta Seviye AI Uzmanı | Orta Seviye Proje | 5 gün | RAG problem seçimi, Bilgi tabanı hazırlığı, Retrieval değerlendirme... | Kontrol testi + mini görev |
| İleri Seviye AI Uzmanı | Gelişmiş LLM Mimarisi | 5 gün | Transformer sezgisi, Attention mekanizması, Context window yönetimi... | Kontrol testi + mini görev |
| İleri Seviye AI Uzmanı | AI Ajanları | 5 gün | Ajan nedir?, Planlama ve araç kullanımı, Memory tasarımı... | Kontrol testi + mini görev |
| İleri Seviye AI Uzmanı | LLM Değerlendirme | 5 gün | Eval set hazırlama, LLM-as-judge sınırları, RAGAS benzeri metrikler... | Kontrol testi + mini görev |
| İleri Seviye AI Uzmanı | AI Güvenliği | 5 gün | Prompt injection, Data leakage, Jailbreak denemeleri... | Kontrol testi + mini görev |
| İleri Seviye AI Uzmanı | Fine-tuning ve Adaptasyon | 5 gün | Fine-tuning ne zaman gerekir?, Veri formatı, LoRA sezgisi... | Kontrol testi + mini görev |
| İleri Seviye AI Uzmanı | Üretim Sistemleri | 5 gün | Caching, Rate limit, Queue ve worker... | Kontrol testi + mini görev |
| İleri Seviye AI Uzmanı | Araştırma Okuryazarlığı | 5 gün | Makale okuma yöntemi, Ablation study, Reproduction planı... | Kontrol testi + mini görev |
| İleri Seviye AI Uzmanı | İleri Seviye Bitirme Projesi | 5 gün | Ürün fikri ve kullanıcı problemi, Mimari tasarım, Güvenlik ve eval planı... | Kontrol testi + mini görev |

### 8.2 Başlangıç seviyesi hedefi

Başlangıç kullanıcı şunları yapabilir hale gelmelidir:

- Python ile temel kod yazmak.
- Basit veri analizi yapmak.
- Train/test ayrımını anlamak.
- Basit regresyon/sınıflandırma modeli çalıştırmak.
- LLM çıktısını körü körüne kabul etmeden değerlendirmek.
- GitHub/README ile ilk portföy kanıtını sunmak.

### 8.3 Orta seviye hedefi

Orta kullanıcı şunları yapabilir hale gelmelidir:

- PyTorch ile basit derin öğrenme modeli kurmak.
- Embedding ve vektör arama mantığını kullanmak.
- RAG tabanlı küçük bir uygulama geliştirmek.
- API ve deployment temel akışını kurmak.
- MLflow benzeri araçlarla deney takip etmek.
- Ürün analitiği ve A/B test mantığını anlamak.

### 8.4 İleri seviye hedefi

İleri kullanıcı şunları yapabilir hale gelmelidir:

- Üretim odaklı LLM/RAG/ajan sistemi tasarlamak.
- Prompt injection, veri sızıntısı ve erişim kontrolü risklerini yönetmek.
- Eval set, red-team test ve kalite dashboardu hazırlamak.
- Fine-tuning gerekip gerekmediğine teknik gerekçeyle karar vermek.
- Gözlemlenebilirlik, rate limit, cache ve incident response planı oluşturmak.
- Teknik vaka çalışması olarak portföy sunumu yazmak.

---

## 9. Proje ve Portföy Sistemi

Mikro dersler kullanıcıyı içeri sokar; projeler gerçek değeri üretir.

### 9.1 Proje aşamaları

Her proje 5 adımdan oluşmalıdır:

1. Problem tanımı
2. Veri/kaynak hazırlığı
3. Çözüm prototipi
4. Değerlendirme
5. Portföy sunumu

### 9.2 Teslim kanıtları

- GitHub repo
- Notebook
- README
- Demo linki veya kısa video
- Değerlendirme raporu
- Güvenlik/risk notu

### 9.3 Rubrik

Her proje 100 puan üzerinden değerlendirilmelidir:

| Kriter | Puan |
|---|---:|
| Teknik doğruluk | 25 |
| Uygulanabilirlik | 20 |
| Değerlendirme kalitesi | 20 |
| Dokümantasyon | 20 |
| Portföy sunumu | 15 |

---

## 10. Maskot ve Karakter Sistemi

### 10.1 Maskot önerisi

Önerilen ana karakter: **Aiva**

Aiva’nın rolü:

- Günlük hedefi hatırlatır.
- Dersi tamamlayınca küçük tepki verir.
- Kullanıcı zorlandığında moral verir.
- Lig ve görevlerde hafif rekabet duygusu yaratır.
- Sosyal medya içeriklerinde markanın yüzü olur.

### 10.2 Yardımcı karakterler

| Karakter | Rol |
|---|---|
| Debug Usta | Kod hatalarını açıklar |
| Veri Dedektifi | Veri analizi görevlerinde görünür |
| Güvenlik Nöbetçisi | LLM risklerinde uyarır |
| Ürün Mentoru | Proje ve portföy kararlarında yönlendirir |

### 10.3 Ton kuralları

Yapılmalı:

- Hafif mizah
- Net hatırlatma
- Küçük başarıyı görünür kılma
- Kullanıcının emeğine saygı

Yapılmamalı:

- Aşırı suçluluk hissettirme
- Küçümseyici dil
- Öğrenme kaygısını artırma
- Çocuklaştırıcı ton

### 10.4 Sosyal medya içerik formatları

- “Bugünün 3 dakikalık AI hatası”
- “Aiva prompt’unu yargılıyor”
- “1 dakikada RAG yanılgısı”
- “Debug Usta kod kokusu buldu”
- “7 günlük AI streak meydan okuması”

---

## 11. Veri, Analitik ve A/B Test Kültürü

### 11.1 North Star Metric

**WAL — Weekly Active Learners**

Bir kullanıcı şu koşulu sağlıyorsa haftalık anlamlı öğrenen sayılır:

- Haftada en az 3 mikro ders tamamladı
- Ve en az 1 quiz/pratik/proje adımı tamamladı

Sadece giriş yapmak veya sayfa gezmek yeterli değildir.

### 11.2 Aktivasyon metrikleri

- Kayıt → yol seçimi
- Yol seçimi → ilk ders başlatma
- İlk ders başlatma → ilk ders tamamlama
- İlk ders → ilk quiz/pratik
- İlk hafta → 3 gün aktif kullanım

### 11.3 Retention metrikleri

- D1 retention
- D7 retention
- D30 retention
- Haftalık aktif öğrenen oranı
- Streak kırıldıktan sonra geri dönüş oranı

### 11.4 Öğrenme kalite metrikleri

- Quiz başarı oranı
- Yanlış tekrar tamamlama oranı
- Proje teslim oranı
- Rubrik ortalaması
- AI mentorun çözüm önerisinden sonra başarı artışı

### 11.5 Event listesi

Seed dosyasında şu event’ler tanımlıdır:

- UserSignedUp
- PathSelected
- LessonStarted
- LessonCompleted
- ExerciseSubmitted
- XPGranted
- StreakExtended
- StreakBroken
- QuestCompleted
- QuizSubmitted
- ProjectStepSubmitted
- ProjectSubmitted
- AIMessageSent
- PaywallViewed
- SubscriptionStarted
- NotificationSent
- NotificationOpened
- ExperimentAssigned

### 11.6 A/B test adayları

| Test | Hipotez | Varyantlar | Ana metrik | Koruma metriği |
|---|---|---|---|---|
| `onboarding_goal_options` | Günlük hedefi kullanıcıya seçtirmek D7 retention artırır. | default_20xp, user_select_goal, recommended_by_level | D7 retention | lesson completion rate |
| `lesson_cta_copy` | '3 dakikalık AI Byte' CTA'sı başlangıç ders tamamlama oranını artırır. | Yola Başla, 3 Dakikalık AI Byte, Bugünkü Mini Dersi Bitir | first_lesson_completed | bounce_rate |
| `streak_loss_copy` | Suçluluk yerine destekleyici dil uzun vadeli geri dönüşü artırır. | neutral, playful, supportive | next_day_return | unsubscribe_notification |
| `xp_reward_scale` | Başlangıçta daha sık küçük ödül motivasyonu artırır. | standard, front_loaded, quality_weighted | lessons_per_active_user | quiz_score |
| `project_unlock_timing` | Proje çok geç açılırsa kullanıcı gerçek değer hissetmez. | after_unit_3, after_unit_5, always_visible_locked | project_started | beginner_dropoff |

### 11.7 Deney altyapısı

MVP’de çok karmaşık bir deney platformu gerekmez ama veri modeli en baştan hazırlanmalıdır.

Gerekli tablolar:

- Experiment
- ExperimentVariant
- ExperimentAssignment
- AnalyticsEvent

Atama yöntemi:

- `hash(userId + experimentKey)` ile deterministik varyant seçimi.
- Kullanıcı aynı testte her zaman aynı varyantı görür.
- Admin panelde test aktif/pasif yapılabilir.
- Her testte ana metrik, guardrail metrik ve durdurma kuralı zorunlu alan olmalıdır.

---

## 12. Freemium İş Modeli

### 12.1 Free plan

Ücretsiz kalmalı:

- Başlangıç/orta/ileri yol haritasını görme
- Temel mikro dersler
- Temel quizler
- Temel projeler
- Sınırlı AI mentor sorusu
- Lig ve streak sistemi
- Kaynak kütüphanesi

Neden? Çünkü büyük kullanıcı tabanı, sosyal rekabet ve organik büyüme için çekirdek eğitim erişilebilir olmalıdır.

### 12.2 Plus plan

Plus özellikleri:

- Daha yüksek AI mentor limiti
- AI kod/prompt inceleme
- Ek streak freeze
- Gelişmiş tekrar listesi
- Kişisel çalışma planı
- Reklamsız deneyim, reklam modeli seçilirse
- Gelişmiş proje şablonları

### 12.3 Pro plan

Pro özellikleri:

- Proje portföy incelemesi
- Sertifika/rozet doğrulama
- İleri seviye lab’ler
- Kariyer/portföy raporu
- Takım/kurumsal modül hazırlığı

### 12.4 MVP monetizasyon kararı

MVP’de ödeme sistemi şart değildir. Öncelik şudur:

1. Retention çalışıyor mu?
2. Kullanıcı günlük mikro ders tamamlıyor mu?
3. Proje teslimi oluşuyor mu?
4. AI mentor kullanıcıya gerçek değer katıyor mu?

Bu cevaplar netleşmeden agresif paywall yapılmamalıdır.

---

## 13. Teknik Mimari

### 13.1 Backend modülleri

ASP.NET Core API şu bounded context’lerle kurulmalıdır:

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
12. Billing
13. Admin

### 13.2 Veritabanı entity listesi

| Entity | Amaç |
|---|---|
| `ApplicationUser` | Kullanıcı profili, hedef XP, abonelik durumu, timezone |
| `LearningPath` | Başlangıç/orta/ileri yol haritaları |
| `Unit` | Yol haritası içindeki modül |
| `Lesson` | 3-5 dakikalık mikro ders, pratik veya kontrol testi |
| `Exercise` | Ders içindeki ölçülebilir mini görev |
| `Resource` | Resmi doküman, kurs, video, notebook vb. |
| `UserLessonProgress` | Başladı/tamamladı/skor/süre |
| `QuizAttempt` | Quiz sonucu, yanlışlar, tekrar listesi |
| `Project` | Portföy projesi tanımı |
| `ProjectSubmission` | Kullanıcı teslimi, GitHub/demo/README |
| `XpTransaction` | XP kazanımı; sebep, kalite kapısı, tarih |
| `UserStreak` | Günlük seri, freeze, kurtarma hakkı |
| `Badge` | Rozet tanımı |
| `UserBadge` | Kazanılmış rozetler |
| `Quest` | Günlük/haftalık görev |
| `UserQuest` | Görev ilerlemesi |
| `LeagueSeason` | Haftalık lig dönemi |
| `LeagueParticipant` | Ligdeki kullanıcı, XP, sıralama |
| `NotificationTemplate` | Hatırlatıcı metinleri |
| `NotificationLog` | Gönderim, açılma, tıklanma |
| `Experiment` | A/B test tanımı |
| `ExperimentVariant` | Varyant ağırlıkları |
| `ExperimentAssignment` | Kullanıcının varyantı |
| `AnalyticsEvent` | Ürün analitiği olayları |
| `AiConversation` | AI mentor konuşmaları |
| `AiMessage` | AI mentor mesajları ve güvenlik metadata |
| `SubscriptionPlan` | Free/Plus/Pro plan tanımları |
| `UserSubscription` | Abonelik durumu |

### 13.3 API endpoint grupları

| Grup | Endpoint taslağı |
|---|---|
| Auth | `POST /auth/register`, `POST /auth/login`, `POST /auth/refresh`, `POST /auth/logout` |
| Dashboard | `GET /dashboard/me` |
| Learning | `GET /learning-paths`, `GET /learning-paths/{slug}`, `GET /lessons/{slug}`, `POST /lessons/{slug}/start`, `POST /lessons/{slug}/complete` |
| Exercise/Quiz | `POST /exercises/{id}/submit`, `POST /quizzes/{lessonSlug}/attempts`, `GET /review/mistakes` |
| Projects | `GET /projects`, `GET /projects/{slug}`, `POST /projects/{slug}/submissions`, `GET /portfolio/me` |
| Gamification | `GET /gamification/me`, `GET /quests/daily`, `POST /quests/{id}/claim`, `GET /badges/me`, `GET /leagues/current` |
| Notifications | `GET /notifications`, `POST /notifications/{id}/read`, `POST /notification-preferences` |
| AI Mentor | `POST /ai/chat`, `POST /ai/explain-lesson`, `POST /ai/review-submission`, `POST /ai/generate-practice` |
| Analytics | `POST /events/track`, `GET /admin/analytics/funnel`, `GET /admin/analytics/retention` |
| Experimentation | `GET /experiments/assignments`, `POST /admin/experiments`, `PATCH /admin/experiments/{id}` |
| Admin Content | `POST /admin/paths`, `POST /admin/units`, `POST /admin/lessons`, `POST /admin/resources`, `PATCH /admin/lessons/{id}` |

### 13.4 XP transaction prensibi

XP doğrudan kullanıcı tablosuna “artır” şeklinde yazılmamalıdır. Her XP kazanımı ayrı transaction olarak saklanmalıdır.

Sebep:

- Hangi eylem XP verdi görülebilir.
- Hatalı XP geri alınabilir.
- Lig hesabı kalite kapısına göre yapılabilir.
- Fraud/abuse analizi yapılabilir.

Örnek alanlar:

```csharp
public class XpTransaction
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string EventType { get; set; } = default!;
    public int Amount { get; set; }
    public string? ReferenceType { get; set; }
    public Guid? ReferenceId { get; set; }
    public bool PassedQualityGate { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
```

### 13.5 Streak hesaplama prensibi

Streak hesapları kullanıcının timezone bilgisine göre yapılmalıdır. Aksi halde gece yarısı sorunları retention’ı bozar.

Örnek alanlar:

```csharp
public class UserStreak
{
    public Guid UserId { get; set; }
    public int CurrentStreakDays { get; set; }
    public int LongestStreakDays { get; set; }
    public DateOnly? LastCompletedLocalDate { get; set; }
    public int FreezeCount { get; set; }
    public int MonthlyRecoveryCount { get; set; }
}
```

### 13.6 Lig hesaplama prensibi

Lig sıralaması her event’te ağır sorgu yapmamalıdır.

Öneri:

- XP transaction yazılır.
- Background job sezon içindeki kullanıcı XP toplamını günceller.
- Leaderboard cache’lenir.
- Sezon bitiminde promotion/demotion job çalışır.

### 13.7 Notification altyapısı

MVP’de web içi bildirim + e-posta yeterlidir. Mobil çıkınca push eklenir.

Notification tercihleri:

- Sabah hatırlatması
- Akşam streak hatırlatması
- Proje hatırlatması
- Lig son saat hatırlatması
- E-posta sıklığı

Her notification için event tutulmalıdır:

- Sent
- Opened
- Clicked
- Unsubscribed

---

## 14. React Frontend Planı

### 14.1 Klasör yapısı

```txt
src/
  app/
    router/
    providers/
    queryClient.ts
  features/
    auth/
    dashboard/
    learning/
    lessonPlayer/
    practice/
    projects/
    gamification/
    leagues/
    quests/
    portfolio/
    resources/
    aiMentor/
    notifications/
    admin/
    analytics/
  shared/
    api/
    components/
    hooks/
    icons/
    layouts/
    utils/
    styles/
```

### 14.2 Ana bileşenler

- `AppShell`
- `Sidebar`
- `TopSearchBar`
- `HeroCard`
- `LevelCard`
- `ProgressTimeline`
- `WeeklyDistributionCard`
- `DailyQuestCard`
- `StreakCard`
- `XpProgressBar`
- `LeagueCard`
- `BadgeGrid`
- `LessonPlayer`
- `ExerciseRenderer`
- `ProjectSubmissionForm`
- `AiMentorPanel`
- `ExperimentVariantBoundary`

### 14.3 LessonPlayer davranışı

LessonPlayer tek sayfada uzun içerik göstermemelidir. Adım adım çalışmalıdır:

1. Hedef
2. Mini açıklama
3. Örnek
4. Alıştırma
5. Geri bildirim
6. Ödül ekranı
7. Sonraki ders CTA

Bu akış mobil uygulamaya da uygundur.

### 14.4 Tasarım tokenları

```css
:root {
  --ai-primary: #5E7CFB;
  --ai-primary-600: #4367F7;
  --ai-primary-soft: #EEF3FF;
  --ai-secondary: #6FD3C3;
  --ai-secondary-600: #22B8AA;
  --ai-secondary-soft: #EAFBF8;
  --ai-accent: #F8A23D;
  --ai-accent-600: #F28A17;
  --ai-accent-soft: #FFF3E2;
  --ai-bg: #F7F9FD;
  --ai-surface: #FFFFFF;
  --ai-border: #E6EAF2;
  --ai-text: #111827;
  --ai-muted: #6B7280;
  --ai-radius-lg: 24px;
  --ai-radius-md: 16px;
  --ai-shadow-soft: 0 16px 40px rgba(17, 24, 39, 0.08);
}
```

---

## 15. AI Mentor Sistemi

### 15.1 MVP yetenekleri

AI mentor ilk sürümde şunları yapmalıdır:

- Ders kavramını daha basit anlatmak.
- Kullanıcının kısa cevabını rubriğe göre değerlendirmek.
- Prompt iyileştirme önerisi vermek.
- Proje teslimi için ön geri bildirim sunmak.
- Kullanıcının yanlışlarına göre tekrar dersi önermek.

### 15.2 AI mentor güvenlik prensipleri

- API anahtarı frontend veya mobil istemcide tutulmaz.
- Tüm AI çağrıları backend üzerinden yapılır.
- Kullanıcı girdisi loglanırken kişisel veri maskeleme düşünülür.
- Sistem prompt’u kullanıcıya gösterilmez.
- Prompt injection testleri yapılır.
- AI cevabı kritik konularda kaynak ve sınır belirtmelidir.
- AI mentor “kesin not veren öğretmen” değil, yardımcı değerlendirici olmalıdır.

### 15.3 AI mentor context paketi

AI çağrısına şu bilgiler verilebilir:

- Kullanıcı seviyesi
- Aktif yol haritası
- Aktif ders
- Son yanlışlar
- Son proje adımı
- Kullanıcının hedefi
- İlgili kaynak özetleri

Fakat gereksiz tüm kullanıcı geçmişi gönderilmemelidir.

---

## 16. Admin Panel

Admin panel klasik içerik yönetiminden fazlasını içermelidir.

### 16.1 İçerik yönetimi

- Yol haritası oluştur/düzenle
- Modül oluştur/düzenle
- Mikro ders oluştur/düzenle
- Alıştırma ekle
- Quiz sorusu ekle
- Proje rubriği oluştur
- Kaynak bağlantısı ekle

### 16.2 Oyunlaştırma yönetimi

- XP kuralları
- Rozetler
- Görevler
- Lig sezonları
- Streak freeze ayarları
- Notification şablonları

### 16.3 Deney yönetimi

- A/B test oluştur
- Varyant tanımla
- Kullanıcı yüzdesi ayarla
- Testi başlat/durdur
- Metrikleri gör

### 16.4 İçerik kalite paneli

- Ders tamamlama oranı
- Ders sonrası quiz başarı oranı
- En çok hata yapılan sorular
- En çok AI yardım istenen dersler
- Drop-off noktaları

---

## 17. Sprint Planı

### Sprint 0 — Hazırlık

- Repo yapısı
- Backend/frontend proje iskeleti
- UI tokenları
- Seed JSON import planı
- Auth kararı

### Sprint 1 — Core web MVP

- Auth
- App shell
- Dashboard
- Yol haritası listesi
- Seviye/modül/ders görüntüleme

### Sprint 2 — Mikro ders oynatıcı

- LessonPlayer
- ExerciseRenderer
- Ders başlat/tamamla
- Progress kayıtları
- Temel quiz

### Sprint 3 — XP, streak ve görevler

- XP transaction sistemi
- Streak hesaplama
- Günlük görevler
- Ödül ekranı
- Gamification dashboard kartları

### Sprint 4 — Proje ve portföy

- Proje listesi
- Proje detay
- Teslim formu
- Rubrik
- Portföy kanıtları

### Sprint 5 — Ligler ve bildirimler

- Weekly league season
- Leaderboard
- Promotion/demotion job
- Web notification
- E-posta hatırlatma

### Sprint 6 — AI mentor

- AI chat endpoint
- Ders açıklama
- Kısa cevap değerlendirme
- Prompt değerlendirme
- Proje ön inceleme

### Sprint 7 — Analitik ve A/B test

- Event tracking
- Admin funnel paneli
- Experiment assignment
- İlk 3 A/B test

### Sprint 8 — Freemium hazırlığı

- Plan modelleri
- Kullanım limitleri
- Paywall ekranları
- Plus/Pro özellik bayrakları

### Sprint 9 — Mobil hazırlık

- API kontrat temizliği
- React Native navigation planı
- Push notification hazırlığı
- Offline mini ders cache tasarımı

---

## 18. MVP Kabul Kriterleri

MVP tamam denebilmesi için şu akışlar çalışmalıdır:

1. Kullanıcı kayıt olur.
2. Günlük hedefini seçer.
3. Yol haritası seçer.
4. Dashboard’da bugünkü AI Byte’ı görür.
5. Mikro dersi adım adım tamamlar.
6. Mini alıştırma yapar.
7. XP kazanır.
8. Streak uzar.
9. Günlük görev tamamlanır.
10. Bir kontrol testi çözer.
11. Bir proje adımı teslim eder.
12. Portföy kanıtı oluşur.
13. Lig sıralamasında görünür.
14. AI mentor dersle ilgili açıklama verir.
15. Admin yeni mikro ders ekleyebilir.
16. Event tracking kayıtları oluşur.
17. En az bir A/B test varyantı kullanıcıya atanır.

---

## 19. Riskler ve Önlemler

| Risk | Açıklama | Önlem |
|---|---|---|
| Yüzeysel öğrenme | Her şeyi 3 dakikaya sıkıştırmak beceri üretmez | Mikro ders + haftalık proje dengesini koru |
| Manipülatif oyunlaştırma | Kullanıcıda suçluluk/bağımlılık hissi oluşabilir | Destekleyici ton, bildirim tercihleri, sağlıklı limitler |
| XP farming | Kullanıcı gerçek öğrenmeden XP kasabilir | Kalite kapısı, günlük limit, rubrik, anti-cheat |
| A/B test yanılgısı | Az veriyle yanlış karar | MVP’de az ve net test, guardrail metrik |
| AI mentor hatası | Yanlış açıklama/yanlış değerlendirme | Kaynak belirtme, sınır koyma, admin review, kullanıcı itirazı |
| İçerik şişmesi | Yol haritası ağırlaşır | Her ders tek hedef, modüller kilitli/aşamalı |
| Mobilde karmaşıklık | Web ekranı mobile taşınamaz | LessonPlayer ve kartlar baştan mobile-first tasarlanmalı |

---

## 20. Yazılımcı / AI Kodlama Ajanı İçin Net Talimat

Aşağıdaki yaklaşımı uygula:

1. Platformu LMS olarak değil, oyunlaştırılmış mikro öğrenme ürünü olarak geliştir.
2. Ders entity’sini uzun metin sayfası gibi değil, adım adım akan LessonPlayer içeriği olarak modelle.
3. XP, streak, quest ve league sistemlerini ilk MVP’de basit ama gerçek çalışır halde kur.
4. Tüm kullanıcı davranışlarını event olarak kaydet.
5. A/B test altyapısını baştan koy; ama MVP’de az test çalıştır.
6. AI mentor backend üzerinden çalışsın; istemciye API anahtarı sızmasın.
7. Free/Plus/Pro planlarını veri modelinde hazırla; ödeme entegrasyonu sonraya bırakılabilir.
8. React bileşenlerini mobilde tekrar kullanılacak mantıkla küçük ve modüler tasarla.
9. Admin panelde içerik kadar oyunlaştırma ve deney ayarları da yönetilebilir olsun.
10. Seed JSON’daki `learningPaths`, `gamificationConfig`, `notificationTemplates`, `experimentation`, `projects` alanlarını veritabanına aktar.

---

## 21. Ek Dosyalar

Bu planla birlikte üretilen seed dosyası:

- `ai_egitim_platformu_gamified_seed_v2.json`

Bu dosyada şunlar yer alır:

- Oyunlaştırma kuralları
- XP kuralları
- Streak sistemi
- Ligler
- Rozetler
- Günlük/haftalık görevler
- Maskot/karakter sistemi
- Bildirim şablonları
- A/B test adayları
- Kaynaklar
- Başlangıç/orta/ileri öğrenme yolları
- Mikro dersler
- Projeler
- Rubrikler

---

## 22. Referans Notları

Bu plan, Duolingo’nun kamuya açık strateji ve ürün yaklaşımından esinlenir. Birebir marka, karakter veya içerik kopyalanmaz. Özellikle şu ilkeler AI eğitimine uyarlanmıştır:

- Yüksek hacimli deney ve A/B test kültürü
- Kısa, hızlı tamamlanan dersler
- Puan, seviye ve ilerleme hissi
- Freemium erişim mantığı
- Maskot/karakter üzerinden marka kişiliği
