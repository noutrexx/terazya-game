# Denge: Bir Ülkenin Hikâyesi — Game Design Document (GDD)

> Sürüm: 0.1 (Tasarım) · Hedef platform: Android & iOS (dikey) · Motor: Unity 2022.3 LTS + Unity UI (uGUI) · Dil: Türkçe (i18n-hazır)

Bu belge oyunun **tasarım sözleşmesidir**. Kod ve içerik bu belgeye dayanır; ihtilaf halinde "Geliştirme Anayasası" (teknik kurallar) önceliklidir.

---

## 1. Oyunun Kısa Tanımı

**Denge**, oyuncunun hayali bir ülkenin yöneticisi olduğu, kart kaydırma temelli bir mobil yönetim ve karar oyunudur. Her tur, ülkeyle ilgili bir **olay kartı** gösterilir. Oyuncu kartı **sağa veya sola kaydırarak** iki karardan birini seçer. Her karar, ülkenin altı temel değerini, aktif krizleri, karakter/kurum ilişkilerini ve gelecekte karşılaşılacak olayları etkiler.

Oyun, "doğru cevabın olmadığı" gri ahlaki seçimler üzerine kuruludur: amaç en uzun süre iktidarda kalmak değil, **farklı yönetim stratejileri deneyerek ülkenin geleceğini şekillendirmek** ve farklı sonları keşfetmektir. Tür olarak "swipe-decision" yönetim oyunlarına yakındır; ancak tüm içerik, metin ve mekanik **özgündür** — gerçek ülke, parti veya politikacı içermez.

---

## 2. Oyunun Hedef Kitlesi

- **Birincil:** 16+ yaş, kısa oturumlu (3–10 dk) mobil oyun seven, hikâye/karar odaklı oyuncular.
- **İkincil:** Strateji ve "what-if" senaryolarını seven, tekrar oynanabilirlik (replayability) arayan oyuncular.
- **Portföy bağlamı:** Bu proje aynı zamanda **mimari ve mühendislik kalitesini** gösteren bir portföy projesidir; ikincil "kitle" teknik değerlendiriciler (işveren/geliştirici) olarak da düşünülmüştür.
- **Erişilebilirlik:** Renk körlüğü, düşük görme, motor zorluk ve düşük donanımlı cihaz kullanıcıları baştan kapsam içindedir.

---

## 3. Oyuncunun Temel Amacı

Oyuncunun amacı **tek bir "kazanma"** değildir. Bunun yerine:

1. Altı değeri kritik sınırlardan (0 / 100) koruyarak yönetimi sürdürmek.
2. Krizleri yönetmek, politikalar uygulamak, karakterlerle ilişki kurmak.
3. Farklı stratejilerle **farklı sonları keşfetmek** (koleksiyon hissi).
4. Yüksek **kalite skoru** elde etmek (sadece hayatta kalma süresi değil; denge, kriz çözümü, tamamlanan zincirler ve keşif).

Oyun, oyuncuyu "tek doğru yol" yerine **kişisel yönetim tarzı** geliştirmeye iter.

---

## 4. Temel Oyun Döngüsü (Core Loop)

```
[Kart Sunulur]
   → Oyuncu kartı okur (karakter + başlık + açıklama)
   → Sağa/sola kaydırır VEYA karar butonuna basar
   → Karar uygulanır (değer değişimleri, bayraklar, politikalar, krizler, zamanlanmış olaylar)
   → Değerler animasyonla güncellenir, gizli/açık geri bildirim verilir
   → Tur ilerler (yıl/dönem güncellenir)
   → Bir son tetiklendi mi? → Evet: Sonuç ekranı | Hayır: Sonraki kart seçilir
```

Bir oturum: ortalama **40–120 tur** hedeflenir (bot simülasyonuyla kalibre edilir). Her tur **5–15 saniye** sürer. Toplam oturum **3–10 dk**.

---

## 5. Uzun Vadeli İlerleme Sistemi

Oyunun tek bir oyunun ötesindeki ilerlemesi **meta ilerleme** ile sağlanır (Bölüm 22):

- Keşfedilen sonlar bir "**Sonlar Galerisi**"nde birikir.
- Açılan başarımlar kalıcıdır.
- Bazı **başlangıç seçenekleri** (farklı ülke başlangıç değerleri / yönetim profilleri) belirli sonlar veya başarımlarla açılır.
- En yüksek skor, toplam yönetilen tur, oynanan oyun sayısı kaydedilir.

İlerleme **kozmetik/keşif odaklıdır**; "pay-to-win" veya kalıcı güç artışı yoktur — adil dengeyi korumak için (Bölüm 25).

---

## 6. Kaybetme ve Kazanma Anlayışı

- **Klasik "kaybetme" yoktur.** Yönetim sonlanır; her sonlanma bir **"son" (ending)** olarak değerlendirilir — kimisi felaket, kimisi başarı, kimisi nötr/ironik.
- **"Kazanma"**, belirli pozitif/nadir sonlara ulaşmak ve yüksek kalite skoru elde etmektir.
- Bir oyun bitince oyuncuya **ne öğrendiği** (hangi değer dengesizleşti, hangi kriz çözülemedi) net gösterilir → bir sonraki denemeyi besler.
- Felsefe: **"Kaybetmek de bir hikâyedir."** Her son, oyuncunun yönetim tarzının doğal sonucudur.

---

## 7. Altı Temel Ülke Değerinin Görevleri

Her değer 0–100 arası tutulur. Başlangıç varsayılanı her biri için **50** (yapılandırılabilir, başlangıç profiline göre değişir).

| Değer | Temsil ettiği | Düşükken risk | Yüksekken risk |
|-------|---------------|----------------|-----------------|
| **Ekonomi** | Hazine, üretim, refah | Yoksulluk, iflas | Aşırı sermaye gücü |
| **Halk Desteği** | Yönetimin meşruiyeti | Ayaklanma, darbe zemini | Popülizm tuzağı, sürdürülemez vaatler |
| **Güvenlik** | İç/dış güvenlik, asayiş | Kaos, kontrol kaybı | Baskı, polis devleti |
| **Özgürlük** | Bireysel haklar, basın | Otoriterleşme | Otorite boşluğu, kaos |
| **Çevre** | Doğal kaynaklar, iklim | Ekolojik çöküş | Aşırı kısıtla ekonomik durgunluk |
| **Diplomasi** | Dış ilişkiler, ittifaklar | İzolasyon, yaptırım | Dışa bağımlılık, egemenlik kaybı |

Değerler **birbiriyle gerilim** içindedir (örn. Güvenlik↑ genelde Özgürlük↓). Bu gerilim oyunun çekirdek tansiyonudur.

---

## 8. Değerlerin 0 ve 100 Sınırlarına Ulaşmasının Sonuçları

Her değerin **hem alt (0) hem üst (100)** sınırı ayrı bir yönetim sonu tetikler:

| Değer | 0 sonu | 100 sonu |
|-------|--------|----------|
| Ekonomi | **Devlet İflası** | **Şirketokrasinin Yükselişi** (şirketler yönetimi ele geçirir) |
| Halk Desteği | **Halk Ayaklanması** | **Popülizm Çöküşü** (sürdürülemez vaatler ülkeyi tüketir) |
| Güvenlik | **Kontrolün Kaybı** (anarşi) | **Baskıcı Güvenlik Rejimi** |
| Özgürlük | **Otoriter Rejim** | **Otorite Boşluğu** (devlet işlevsizleşir) |
| Çevre | **Ekolojik Çöküş** | **Yeşil Durgunluk** (aşırı kısıt ekonomiyi felç eder) |
| Diplomasi | **Uluslararası İzolasyon** | **Egemenlik Kaybı** (dış güçlere bağımlılık) |

= **12 değer-tabanlı son** (MVP hedefinin tamamı buradan karşılanır; ek sonlar zincir/karakter/kriz kaynaklıdır).

---

## 9. Olay Kartlarının Çalışma Biçimi

Bir **olay kartı** = bir karar anı. Veri odaklıdır (ScriptableObject). Alanları (tam şema Faz 5):

- Kimlik: `Id`, `Title`, sunan `Character/Institution`, `Description`
- Kararlar: `LeftText`, `RightText`, `LeftEffects[]`, `RightEffects[]`
- Sınıflandırma: `Category`, `Priority`, `Weight`
- Zamanlama: `MinTurn`, `MaxTurn`, `OneShot`, `CooldownTurns`
- Koşullar: `RequiredFlags`, `ForbiddenFlags`, `RequiredPolicies`, `ForbiddenPolicies`, değer min/max aralıkları, karakter ilişki eşikleri
- İlişkiler: `ChainId`, `PreviousEventId`, `ScheduledEventsAfterChoice[]`, `IsEmergency`

Kart **runtime'da değiştirilmez**; oyun durumu ayrı çalışma modellerinde tutulur (Anayasa kuralı).

---

## 10. Sağ ve Sol Kararların Gösterimi

- Kart üstte karakter/kurum, ortada başlık + açıklama gösterir.
- **Sola kaydırma** → `LeftText` kararı; **sağa kaydırma** → `RightText` kararı.
- Sürükleme sırasında ilgili kenarda **karar metni** belirir, kart **kaydırma miktarına göre döner**.
- **Eşik geçilince** görsel onay (renk/parlama) verilir; bırakılınca karar uygulanır. Eşik geçilmezse kart yerine döner.
- **Erişilebilirlik:** Kaydırma yerine ekranda **"Sol Karar" / "Sağ Karar" butonları** (ayarlardan açılır) aynı işlevi görür.
- Karar **öncesi kesin sayısal sonuç gösterilmez**; bunun yerine her seçenekte **hangi değerlerin ↑/↓ etkilenebileceği** küçük yön ikonlarıyla ima edilir (belirsizlik korunur).

---

## 11. Kararların Kısa ve Uzun Vadeli Etkileri

- **Kısa vade:** Anında değer değişimleri (doğrudan veya rastgele aralık), bayrak değişimi, ilişki değişimi.
- **Uzun vade:**
  - **Süreli etkiler:** N tur boyunca her tur uygulanan değişimler (örn. "3 tur boyunca Ekonomi -2").
  - **Zamanlanmış olaylar:** Karar, M tur sonra yeni bir kartı tetikler.
  - **Politika başlatma/bitirme**, **kriz başlatma/bitirme**.
  - **Gizli bayraklar:** İleride farklı olayların/sonların kapısını açar.
- Bir kararın etkisi **anında görünmeyebilir**; sonuçlar tur ilerledikçe ortaya çıkar → "her seçimin bedeli vardır" hissi.

---

## 12. Zincirleme Olay Sistemi

Kararlar gelecekte yeni olayları **zamanlayarak** çok adımlı hikâyeler kurar.

**Örnek zincir — "Enerji Santrali":**
```
1) Yeni enerji santrali önerisi
   → Kabul → 2a) Çevre protestoları başlar
              → Bastır → 3a) Kısa vade Güvenlik↑ ama Özgürlük↓, uzun vade Çevre↓
              → Dinle  → 3b) Ekonomi yavaşlar ama Çevre korunur
   → Ret → 2b) Sanayi tepkisi: Ekonomi↓, ileride yatırım fırsatı kapanır
```

- Zincirler `ChainId` + `PreviousEventId` ile bağlanır.
- Bir adımın seçimi, sonraki adımı **zamanlanmış olay** olarak kuyruğa ekler.
- Her zincir **min. 3 olay** içerir (Faz 8 hedefi: en az 6 zincir).

---

## 13. Koşullu Olay Sistemi

Kartlar yalnızca **koşulları sağlanınca** havuza girer:

- Değer aralığı (örn. `Ekonomi < 30`).
- Bayrak gereksinimi/yasağı (`RequiredFlags`, `ForbiddenFlags`).
- Aktif/yasak politika.
- Karakter ilişki eşiği (örn. "Genelkurmay ilişkisi < -50 ise darbe kartı").
- Tur aralığı (`MinTurn`–`MaxTurn`), dönem.

Bu, oyunun **duruma tepki vermesini** sağlar: kötü giden bir ülkede kriz kartları, iyi ilişkilerde fırsat kartları belirir.

---

## 14. Tekrarlanabilir Olaylar

- `OneShot = false` olan kartlar tekrar gösterilebilir.
- **`CooldownTurns`** ile aynı kartın yakın zamanda tekrarı engellenir.
- Tekrarlanabilir kartlar genelde **jenerik/atmosferik** olaylardır (havuzu dolu tutar, çeşitlilik sağlar).
- Seçim algoritması (Bölüm 28) son gösterilenleri ve kategori tekrarını cezalandırarak tekrar hissini azaltır.

---

## 15. Tek Kullanımlık Olaylar

- `OneShot = true` kartlar **bir kez** gösterilir, sonra havuzdan kalıcı çıkar (gösterilen ID listesi kayıtta tutulur).
- Genelde **hikâye/zincir/karakter** odaklı, dönüm noktası niteliğindeki olaylardır.
- Bir oyunda asla tekrar gelmezler → her kararın "anlık ve geri dönüşsüz" olduğu hissini güçlendirir.

---

## 16. Acil Kriz Olayları

- `IsEmergency = true` ve yüksek `Priority` taşır.
- Bir **kriz aktifken** seçim algoritması önce krize özel kartları değerlendirir (Bölüm 28, adım 2).
- Acil olaylar normal akışı **kesip öne geçer**; oyuncuya "şu an yangın var" hissi verir.
- Krizler **aşamalı** kötüleşir (Bölüm: Krizler / Faz 8): her aşama daha sert etkiler ve kendi acil kartlarını getirir; çözülemezse yönetim sonuna dönüşebilir.

---

## 17. Karakter ve Kurum İlişkileri

- 8 başlangıç karakteri/kurumu (Faz 7): Maliye Bakanı, Genelkurmay Başkanı, Muhalefet Lideri, Çevre Bilimci, Sanayi Birliği Başkanı, Gazeteciler Birliği Temsilcisi, Dışişleri Danışmanı, Halk Meclisi Sözcüsü.
- İlişki **-100…+100**; oyuncuya **kesin sayı değil durum etiketi** olarak gösterilir (örn. "Düşman / Mesafeli / Tarafsız / Müttefik / Sadık").
- İlişkiler: kararlarla değişir, yeni olaylar açar/engeller, özel sonları tetikler (örn. ordu ilişkisi çok düşükse **darbe** sonu).
- Görsel: özel asset yok → **baş harf avatarı + tema rengi + sade şekil ikon + rol başlığı**.

---

## 18. Gizli Değişkenler

- **Gizli bayraklar (flags):** Oyuncuya gösterilmeyen durum işaretleri (örn. `yolsuzluk_ortaya_cikti`, `gizli_anlasma`).
- Kararların geçmişini "hatırlar"; ileride olayları/sonları koşullar.
- Bazı değerler de **kısmen gizlidir**: oyuncu kesin sayıyı görmez, sadece yön/durum görür (belirsizlik tasarımı).
- Gizli değişkenler **kayda yazılır** ama UI'da gösterilmez → keşif ve sürpriz için.

---

## 19. Yasalar ve Aktif Politikalar

- **Politikalar** belirli kararlarla başlatılır; süre boyunca **her tur etki** uygular (Faz 8).
- Alanlar: başlangıç etkileri, her-tur etkileri, süre, bitiş etkileri, uyumluluk koşulları, açtığı/engellediği olaylar, erken iptal edilebilirlik.
- Örnekler: Evrensel Temel Gelir, Sıkı Sınır Kontrolü, Yeşil Dönüşüm Programı, Basın Düzenleme Yasası, Sanayi Teşvik Paketi, Diplomatik Açılım.
- Politikalar oyuncuya **uzun vadeli stratejik kaldıraç** sunar; ama bedelleri vardır (örn. UBI → Halk↑ ama Ekonomi her tur↓).

---

## 20. Ülkenin Dönemlere Ayrılması

- Oyun **yıl + dönem** kavramı taşır. Her ~birkaç tur = 1 yıl; belirli yıl eşikleri **dönemleri** ayırır (örn. *Kuruluş → Büyüme → Olgunluk → Miras*).
- Dönemler: bazı kartların `MinTurn/MaxTurn` ile hangi dönemde geleceğini belirler; tema ve zorluk dönemle değişir.
- Tam sürümde **çoklu yönetim dönemi** (farklı liderlik çağları) hedeflenir.

---

## 21. Oyun Sonları

- MVP: **en az 12 özgün son** (Bölüm 8'deki 12 değer-sonu çekirdektir; ek olarak karakter/kriz/zincir sonları).
- Tam sürüm: **30 son**.
- Her son: `Id`, başlık, açıklama, tetikleyici koşullar, kategori (felaket/nötr/başarı/ironik), **nadirlik**, sonuç ekranı tema rengi, meta ödüller.
- Tetikleyiciler: kritik değer sınırı, özel karar, çözülemeyen kriz, karakter/kurum darbesi, belirli zincir sonucu, **maksimum görev süresi** (huzurlu emeklilik gibi pozitif son).

---

## 22. Başarımlar

- MVP: **en az 10**; tam sürüm: **40**.
- Örnek temalar: "İlk yönetim", "5 farklı son keşfet", "Hiç krize girmeden 50 tur", "Tüm değerleri 40–60 bandında 20 tur tut", "Bir krizi çöz", "Bir zinciri tamamla", "Bir değeri kritik sınırdan döndür".
- Kalıcıdır; meta ilerlemeye yazılır; bazı başlangıç seçeneklerini açabilir.

---

## 23. Meta İlerleme Sistemi

Tek oyunlar arası kalıcı veri (Faz 10 kayıt sistemiyle):
- Keşfedilen sonlar, açılan başarımlar.
- Oynanan toplam oyun, en yüksek skor, toplam yönetilen tur.
- Açılmış başlangıç seçenekleri, ayarlar.
- **Kozmetik/keşif odaklı** — kalıcı güç artışı yok (adalet, Bölüm 25).

---

## 24. Günlük Görev / Reklam Sistemlerinin Neden MVP Dışında Tutulması

- **Odak:** MVP'nin değeri **çekirdek karar döngüsü + içerik kalitesi + mimari**dir. Reklam/IAP/daily-quest bunları test etmez, geliştirme süresini çalar.
- **Bütünlük:** Reklam ve günlük görev, oyunun **"adil ve hikâye odaklı"** felsefesiyle gerilim yaratır (engagement-baiting).
- **Portföy:** Reklam SDK'ları ek bağımlılık, gizlilik ve platform onayı yükü getirir; portföy projesinde temiz mimari daha kıymetlidir.
- **Teknik borç:** Sonradan eklenebilecek şekilde **mimari hazır** bırakılır (servis arayüzleri), ama MVP'de uygulanmaz.

---

## 25. Oyuncuyu Zorlayan Ancak Adil Kalan Dengeleme Yaklaşımı

İlkeler:
1. **Tek kararla ölüm nadir olmalı** — sonlar genelde **birikmiş** dengesizlikten gelir (oyuncu uyarılmıştır).
2. **Kritik yaklaşım uyarısı:** Bir değer 0/100'e yaklaşınca görsel uyarı (renk + sembol + metin).
3. **Geri dönüş mümkün olmalı:** Çoğu durumdan stratejiyle çıkış yolu bulunur → "kaçınılmaz ölüm döngüsü" simülasyonla avlanır (Faz 12).
4. **Belirsizlik adildir, hile değildir:** Sonuçlar tam kesin gösterilmez ama **yönü** ima edilir; rastgelelik **sınırlı aralıkta**.
5. **Hiçbir strateji tek başına baskın olmamalı** — bot simülasyonu (Faz 12) baskın stratejileri tespit eder ve içerik gerekçeli düzeltilir.
6. **Zorluk dönemle artar**, ama her zaman okunabilir kalır.

---

## 26. İlk Oynanabilir Sürümün (MVP) Kapsamı

| Öğe | MVP hedefi |
|-----|-----------|
| Olay kartı | **≥ 60** (kategori dağılımı Faz 11) |
| Karakter/kurum | **8** |
| Yönetim sonu | **≥ 12** |
| Başarım | **≥ 10** |
| Olay zinciri | **6** (her biri ≥3 olay) |
| Kriz türü | **4** (Ekonomik durgunluk, Büyük protestolar, Sınır gerilimi, Kuraklık) |
| Politika | **6** |
| Dil | Türkçe (i18n altyapısı hazır) |

**MVP içerir:** Çekirdek döngü, kaydırma+buton UI, 6 değer, karar/etki sistemi, kart seçim algoritması, koşullu/zincir/tek-kullanımlık/acil olaylar, karakterler+ilişkiler, politikalar, krizler, sonlar+sonuç ekranı+skor, kayıt+meta ilerleme, ayarlar+erişilebilirlik, tutorial, simülasyon/dengeleme aracı.
**MVP içermez:** Reklam, IAP, günlük görev, online/bulut kayıt, ses/görsel prodüksiyon asset'leri, çoklu dil içeriği (EN), çoklu yönetim dönemi içeriği.

---

## 27. Gelecekte Eklenebilecek Özellikler

- İngilizce ve diğer diller (içerik), tam sürüm 250 kart / 20 karakter / 30 son / 40 başarım.
- Çoklu yönetim dönemi (nesiller arası miras mekaniği).
- Ses tasarımı, müzik, basit animasyonlu görsel tema seçenekleri.
- Bulut kayıt / hesap senkronu, sonlar galerisi paylaşımı.
- Senaryo/kampanya modu, haftalık seed challenge (paylaşılan seed).
- Topluluk içerik aracı (kart editörü dışa açılır).
- (Mimari hazır ama opsiyonel) reklam/IAP destek modülü.

---

## 28. Olay Kartı Seçim Algoritması (Ayrıntılı)

**Hedef:** Tekrar hissini en aza indiren, duruma tepki veren, **aynı seed + aynı GameState ile %100 deterministik** seçim.

**Girdiler:** `GameState` (değerler, bayraklar, politikalar, krizler, ilişkiler, gösterilen kartlar, cooldown'lar, tur), tüm kart havuzu, `IRandomService` (seed'li).

**Adımlar:**

1. **Zamanlanmış zorunlu olaylar:** Kuyrukta bu tura ait zamanlanmış/zincir olayı varsa → onu doğrudan seç (deterministik kuyruk sırası). *(En yüksek öncelik.)*
2. **Aktif kriz olayları:** Aktif kriz varsa, krizin mevcut aşamasına ait acil kartlar değerlendirmeye alınır (yüksek `Priority`). Uygun acil kart varsa havuz bunlarla daraltılır.
3. **Koşul filtresi:** Değer aralığı, gerekli/yasaklı bayrak, gerekli/istenmeyen politika, ilişki eşiği, `MinTurn/MaxTurn`/dönem koşulunu **sağlamayan** kartlar elenir.
4. **Yakın gösterim filtresi:** `CooldownTurns` içinde gösterilmiş kartlar elenir (son N kart penceresi).
5. **Tek-kullanımlık filtresi:** `OneShot` olup daha önce gösterilmiş kartlar elenir.
6. **Ağırlık hesabı:** Kalan her kart için temel `Weight`'ten **etkili ağırlık** hesaplanır.
7. **Kategori tekrar cezası:** Son K kararın kategorisiyle aynı kategorideki kartların ağırlığı çarpan (`< 1.0`) ile düşürülür (ardışık aynı tür olayı seyreltir). Ayrıca son gösterilen kart ID'lerine "yeni-likten uzaklık" bonusu uygulanabilir.
8. **Seed'li ağırlıklı seçim:** Etkili ağırlıkların kümülatif dağılımı üzerinden `IRandomService.NextDouble()` ile **deterministik** seçim.
9. **Güvenli fallback:** Hiç uygun kart yoksa → her zaman koşulsuz uygun, etkisiz/atmosferik **fallback kartı** seçilir (oyun asla kilitlenmez). Fallback kullanımı loglanır (denge uyarısı).

**Determinizm garantisi:** Tüm rastgelelik tek `IRandomService` üzerinden; seed `GameState`'te saklanır ve her çağrıda ilerletilir. Filtre/sıralama adımları **stabil** (ID'ye göre tie-break) → aynı seed + aynı state → aynı kart. Bu, Faz 12 simülasyonunun yeniden üretilebilirliğinin temelidir.

**Tekrar hissini azaltan ek önlemler:** kategori çeşitlilik penceresi, cooldown, son-gösterilenler penceresi, ağırlık tabanlı (deterministik ama dağılımı geniş) seçim, ve içerik tarafında yeterli havuz büyüklüğü (≥60 MVP).

---

# EK A — Geliştirme Fazları

| Faz | Başlık | Çıktı |
|-----|--------|-------|
| 1 | Anayasa/kurallar | Referans |
| 2 | **GDD (bu belge)** | Doküman |
| 3 | Proje mimarisi (katmanlar, asmdef, 3 sahne, interface'ler, bootstrap) | İskelet |
| 4 | Değerler + GameState + karar/etki servisi | Domain |
| 5 | Veri odaklı kart sistemi + seçim algoritması + Editor doğrulama araçları | Domain+Editor |
| 6 | Mobil kart UI (kaydırma+buton, animasyon, safe area, erişilebilirlik) | Presentation |
| 7 | Karakter/kurum sistemi + ilişki ekranı | Domain+UI |
| 8 | Politika + kriz + zincir sistemleri | Domain |
| 9 | Sonlar + sonuç ekranı + skor | Domain+UI |
| 10 | Kayıt + meta ilerleme (versiyon/migration/atomik yazma) | Infra |
| 11 | MVP içeriği (≥60 kart) + içerik doğrulama raporu | İçerik |
| 12 | Denge simülasyonu (8 bot, ≥10k oyun, JSON/CSV rapor) | Araç |
| 13 | Ayarlar + erişilebilirlik | Sistem |
| 14 | Tutorial (ilk 5 kart kontrollü) | Sistem |
| 15 | Production öncesi QA + düzeltme | Kalite |
| 16 | GitHub yayını (README, .gitignore, release notu) | Yayın |

---

# EK B — Teknik Riskler

| Risk | Etki | Azaltma |
|------|------|---------|
| **OneDrive senkronu (önlendi)** | Library bozulması | Proje OneDrive dışında (`C:\Dev\Denge`) |
| **Determinizm kırılması** | Simülasyon/QA güvenilmez | Tek `IRandomService`, seed state'te, stabil tie-break, EditMode determinizm testleri |
| **Effect/Condition sistem dağılması** | Politika/kriz/son ayrı kodlar | Tek `IEffect`/`ICondition` omurgası (Faz 4) |
| **İçerik dengesizliği** | OP/zayıf kartlar, ölüm döngüsü | Faz 12 bot simülasyonu + gerekçeli düzeltme |
| **Runtime SO mutasyonu** | Veri kirlenmesi, kayıt hatası | SO salt-okunur; runtime durum ayrı modellerde |
| **Kayıt bozulması/yarım yazma** | Veri kaybı | Atomik (temp→rename) yazma, versiyon+migration, fallback |
| **Mobil performans / GC** | Takılma, düşük donanım | Update kullanımını sınırla, alloc'ları azalt, profille |
| **TMP/uzun TR metin taşması** | UI bozulması | Auto-size sınırları, kaydırma, taşma testleri |
| **Safe Area / ekran oranı** | Kesik UI | Safe Area desteği, çoklu oran testleri |
| **Event aboneliği sızıntısı** | NRE, bellek | OnDisable'da unsubscribe; QA kontrolü (Faz 15) |

---

# EK C — Kabul Kriterleri (proje geneli)

- [ ] Domain katmanı Unity API'siz derlenir ve EditMode'da test edilir.
- [ ] Aynı seed + aynı GameState → aynı kart seçimi (deterministik test geçer).
- [ ] Tüm karar etki türleri tek servis üzerinden uygulanır ve test edilir.
- [ ] 6 değer 0–100 sınırında tutulur; 12 değer-sonu tetiklenebilir.
- [ ] ≥60 kart, 8 karakter, ≥12 son, ≥10 başarım, 6 zincir, 4 kriz, 6 politika (MVP).
- [ ] Kayıt: versiyonlu, migration'lı, atomik yazma, bozuk kayıtta güvenli fallback; autosave çalışır.
- [ ] Mobil UI: dikey, safe area, kaydırma + buton, animasyon azaltma, kontrast.
- [ ] Tutorial ilk oyunda otomatik, atlanabilir, ayarlardan tekrar açılır.
- [ ] Simülasyon aracı 8 bot × ≥10k oyun çalıştırır, JSON/CSV rapor üretir.
- [ ] Console hata/uyarı **sıfır**; EditMode + PlayMode testleri geçer; Android build alınır.
- [ ] İçerik özgün; gerçek ülke/parti/politikacı yok; propaganda yok.
- [ ] README + temiz .gitignore + release notu hazır.

---
*Bu GDD canlı bir belgedir; sonraki fazlarda mekanik kararları ortaya çıktıkça güncellenecektir.*
