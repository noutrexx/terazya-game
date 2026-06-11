# Manuel Mobil Test Kontrol Listesi (Faz 6 — Kart Karar Arayüzü)

Bu liste, otomatik (EditMode/PlayMode) testlerle doğrulanamayan **görsel ve cihaz** yönlerini kapsar.
Android/iOS cihazda veya Unity Device Simulator'da test edilmelidir.

## Kurulum
- [ ] `Denge/İçerik/Test Kartları Üret` çalıştırıldı (Resources'ta EventCardDatabase var).
- [ ] `Denge/TMP Temel Kaynaklarını İçe Aktar` çalıştırıldı (yazılar görünüyor).
- [ ] Ana menüden "Yeni Yönetim" oyun sahnesini açıyor.

## Yerleşim
- [ ] Üstte 6 değer (ikon + kısa isim + sayı + dolum çubuğu) yan yana sığıyor.
- [ ] Ortada kart; karakter, başlık ve açıklama okunaklı.
- [ ] Altta tur/yıl/dönem bilgisi + Ayarlar + Geçmiş butonları.
- [ ] Dikey (portrait) yönde açılıyor; yatay zorlanmıyor.

## Kart Etkileşimi
- [ ] Kart parmağı takip ederek sağa/sola sürükleniyor.
- [ ] Sürüklerken ilgili karar metni (sol/sağ) beliriyor.
- [ ] Sürükleme miktarına göre kart dönüyor.
- [ ] Eşik geçilince kartın rengi/geri bildirimi değişiyor.
- [ ] Eşik geçilip bırakılınca karar uygulanıyor; geçilmezse kart yerine dönüyor.
- [ ] Animasyon sırasında ikinci bir karar alınmıyor (aynı karar iki kez uygulanmıyor).
- [ ] Sol/Sağ erişilebilirlik butonları kaydırmayla aynı sonucu veriyor.

## Geri Bildirim / Animasyon
- [ ] Yeni kart geçişi akıcı.
- [ ] Değer değiştiğinde dolum çubuğu yumuşak güncelleniyor.
- [ ] Kritik seviyeye yaklaşan değer renk + "!" sembolüyle uyarıyor (sadece renk değil).
- [ ] Oyun sonunda sonuç sahnesine geçiş çalışıyor.

## Cihaz / Erişilebilirlik
- [ ] **Safe Area:** çentikli/yuvarlak köşeli cihazda UI kesilmiyor.
- [ ] Farklı ekran oranları (16:9, 19.5:9, tablet 4:3) düzgün.
- [ ] Uzun Türkçe metinler taşmıyor (otomatik küçülme/sarma çalışıyor).
- [ ] Dokunmatik hedefler yeterince büyük (≈ parmak boyutu).
- [ ] Renk kontrastı yeterli; renk körlüğünde sembol/sayı yine anlaşılıyor.
- [ ] Ayarlardan "Animasyonları azalt" açıkken geçişler anlık (Faz 13 ile bağlanınca).
- [ ] Düşük donanımlı cihazda akıcılık kabul edilebilir (kare düşüşü yok).

## Notlar
- Otomatik doğrulanan kısımlar: kaydırma karar matematiği (SwipeEvaluator), karar
  uygulama/çift-uygulama engeli, animasyon sırasında input bloklama, stat güncelleme,
  oyun döngüsü akışı — bkz. EditMode/PlayMode testleri.
