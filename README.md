## 📌 Genel Bakış
**Pusula Student Automation**, ABP Framework ve Blazor ile geliştirilen, admin/öğretmen/öğrenci rolleri için uçtan uca ders, not ve devamsızlık yönetimi sunan çok katmanlı bir otomasyon sistemidir. Kullanıcılar rol bazlı yönlendirilir; giriş yapmayanlar doğrudan login sayfasına, giriş yapanlar ise rollerine göre özelleşmiş dashboard’lara yönlendirilir. ABP’nin self-registration akışı devre dışı bırakılmıştır; tüm öğretmen ve öğrenciler admin panelinden oluşturulur.

---

## 🧱 Mimarî & Teknoloji Yığını
| Katman | Teknolojiler | Açıklama |
| ------ | ------------ | -------- |
| Sunum  | Blazor Server + WASM (component/page düzeni) | Rol bazlı dashboard, filtrelenebilir listeler, inline edit formları |
| Uygulama | ABP Application Services, DTO/AutoMapper | SOLID + DDD uyumlu servisler, global exception middleware’i |
| Veri | EF Core + PostgreSQL | Ders, kullanıcı, rapor ve günlük kayıtları |
| Altyapı | Docker, Redis, ElasticSearch, Serilog | Dağıtık cache, loglama ve containerized çalışma |

- **Redis Cache:** `TeacherAppService.GetListAsync` öğretmen listelerini Redis’te 3 dk saklar; CRUD işlemleri cache versiyonunu yeniler. Bu sayede admin panelindeki yoğun listelemeler DB yerine cache’den servis edilir.
- **ElasticSearch Logları:** Özellikle öğrenci oturum açma/günlük rapor girişleri gibi kritik iş akışları Serilog → Elastic’e yazılır; güvenlik ve denetlenebilirlik kolaylaşır.
- **Global Exception Handling:** ABP’nin `AutomationBlazorModule` ayarlarında tanımlı; kullanıcıya temiz hata mesajı, log kanalına detaylı stack yazılır.

---

## 🎯 Rol Tabanlı Özellikler

### 👨‍💼 Admin Portalı
- Öğretmen ve öğrenci kayıtlarını form üzerinden oluşturur (ABP default register kapalı).
- Liste altındaki filtreler (isim, cinsiyet vb.) ile kayıtları arar; tabloyu Excel/PDF olarak indirebilir.
- Öğretmen/öğrenci kartlarının üzerindeki **Detay** ve **Düzenle** butonlarıyla:
  - Ders oluşturma/güncelleme/silme.
  - Derse öğrenci ekleme/çıkarma (combobox sadece derste olmayan öğrencileri listeler, duplicate engellenir).
  - Kullanıcı bilgilerini inline form üzerinde güncelleme.

### 👩‍🏫 Teacher Portalı
- Soldaki formdan yeni öğrenci oluşturabilir (admin onaylı sistemle entegre).
- Orta panelde tüm dersleri görebilir; text veya ders durumu filtresi + “Yalnızca Derslerim” seçeneğiyle sadece kendi derslerine inebilir.
- Ders **Detay** panelinde:
  - Öğrencilere vize-final-not, devamsızlık, yorum ekleme/güncelleme/silme.
  - Derse yeni öğrenci ekleme veya çıkarma (bağımlı rapor tabloları atomik şekilde güncellenir).
  - Günlük rapor formunda tarih bazlı “geldi/gelmedi”, günlük not ve yorum kayıtları tutma; kayıtlar ID üzerinden güncellenir veya silinir.

### 👨‍🎓 Student Portalı
- Kayıtlı olduğu dersleri, ders öğretmenlerini, yıllık notlarını ve günlük raporlarını görüntüler.
- Öğretmen yorumları, devamsızlık ve sözlü notlarını filtreleyebilir; sadece kendisine ait verilere erişir.

---

## ⚙️ Kurulum (Kısa)
1. **Ön koşullar:** .NET 9 SDK, Node 20+, ABP CLI, Docker (PostgreSQL 16 & Redis 7 konteynerları).  
2. **Klon & Restore:**  
   ```bash
   git clone https://github.com/semihgrc/Pusula.Student.Automation.2025.git
   cd Pusula.Student.Automation
   dotnet restore
   abp install-libs
   ```
3. **Docker altyapısı (örnek):**
   ```bash
   docker run --name pusula-postgres -e POSTGRES_PASSWORD=myPassw0rd -e POSTGRES_DB=test_db -p 5436:5432 -d postgres:16
   docker run --name pusula-redis -p 6379:6379 -d redis:7
   ```
   Gerekiyorsa `src/Pusula.Student.Automation.Blazor/appsettings.json` dosyasında `ConnectionStrings.Default/Redis` değerlerini güncelle.
4. **Migration & Seed:**  
   ```bash
   dotnet run --project src/Pusula.Student.Automation.DbMigrator
   ```
5. **Uygulamayı çalıştır:**  
   ```bash
   dotnet run --project src/Pusula.Student.Automation.Blazor
   ```
   → https://localhost:44333 üzerinden **ABP default admin** (`admin / 1q2w3E*`) ile giriş yap. Giriş yapılmadan hiçbir sayfaya erişilemez, login olmayan kullanıcılar otomatik olarak `/Account/Login`’a yönlendirilir.

---

## 🔑 Kullanım Akışı & Test Notları
- **İlk giriş mutlaka admin ile** yapılır; çünkü öğrenci/öğretmen kayıtları admin formlarından eklenir.
- Öğretmen/öğrenci listelerinde filtre → tabloyu güncelle → Excel/PDF çıktılarını al.
- Öğretmen/öğrenci kartlarındaki **Düzenle** butonları, kartın üzerinde inline form açar; buradan bilgiler güncellenebilir.
- Ders detayında öğrenciler arayüzden eklenip çıkarıldığında rapor tabloları otomatik güncellenir.
- Redis doğrulaması için `docker exec -it <redis-container> redis-cli monitor | findstr TeacherList` komutuyla admin panelinde listeyi yenilerken `GET/SET TeacherList:*` anahtarlarını gözlemleyebilirsin.
- Elastik loglarını kontrol etmek için ElasticSearch + Kibana stack’inde `Application:Pusula.Student.Automation` filtresiyle logları incele (özellikle öğrenci girişleri).

---

## ✅ Tamamlanan Bonuslar
- Modern Blazor UI: rol bazlı dashboard, modüler form ve kimlik yönetimi tasarlandı.
- Swagger/OpenAPI ile tüm endpoint’ler belgeli.
- JWT + role-based authorization, ABP claim pipeline’ına entegre.
- Redis cache + ElasticSearch loglama devreye alındı.
- Öğrenci, öğretmen, ders ve günlük rapor gereksinimleri eksiksiz tamamlandı.

---

## ⚠️ Dikkat Edilecekler
- Admin oluşturmadan sisteme başka kullanıcı eklenemez.
- Öğrenciye not/yorum/devamsızlık eklenmeden önce öğrenci ilgili derse kaydedilmiş olmalı.
- Teacher portalında görülen diğer öğretmen dersleri “read-only”dır; yalnızca kendi derslerinde işlem yapabilir.
- Günlük raporlar tarih/id bazlı tutulur; mevcut rapor seçilip kaydedildiğinde aynı kaydın üstüne yazılır, veri tutarlılığı korunur.

---

Projeyi kurup çalıştırdıktan sonra `TeacherManagement` sayfasına giderek hem Redis cache’i hem de elastic loglarını doğrulayabilir, role-based yönlendirmeleri test edebilirsin.````
