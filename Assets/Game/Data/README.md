# Data

Veri odaklı ScriptableObject varlıkları bu klasör altında tutulur:

- `Cards/` — Olay kartları (Faz 5 şeması, Faz 11 içeriği)
- `Characters/` — Karakter ve kurumlar (Faz 7)
- `Endings/` — Yönetim sonları (Faz 9)
- `Achievements/` — Başarımlar (Faz 9)
- `Policies/` — Politikalar (Faz 8)

Kural: ScriptableObject verileri runtime sırasında doğrudan değiştirilmez; runtime durumu `GameState` modelinde tutulur.
