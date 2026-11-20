# Domain Modeli

## 1. Genel bakış

Proje modular monolith olarak ilerler. Her modül kendi verisinin sahibidir, diğer modüllerle sadece Id ve küçük public arabirimler/event'ler üzerinden konuşur. Modüller arasında navigation property yoktur.

| Modül | Sahibi olduğu şey | Dışarıdan ihtiyaç duyduğu |
| --- | --- | --- |
| Catalog | Kategori ağacı, ürün, varyant, özellikler, fotoğraflar | Yok |
| Inventory | Varyant bazlı stok ve rezervasyonlar | Yok (VariantId'yi Id olarak bilir) |
| Basket | Kullanıcının sepeti | Catalog'dan varyant bilgisi (ad, fiyat, görsel) |
| Ordering | Sipariş, sipariş kalemleri, sipariş durumu, teslimat adresi (kopya) | Basket (sepet içeriği), Catalog (fiyat ve ad), Inventory (rezervasyon), Payment (ödeme başlatma) |
| Payment | Ödeme denemeleri, ödeme sağlayıcı soyutlaması | Ordering'den sadece OrderId ve tutar |
| Identity | Kullanıcı, giriş (en sona bırakıldı) | Yok |

Notum:

## 2. Ortak tipler

- **Id'ler:** `Guid`. Modüller birbirine sadece bu Id'lerle referans verir (`ProductId`, `VariantId`, `OrderId`, `UserId`).
- **Money (value object):** `Amount: decimal` (veritabanında `decimal(19,4)`), `Currency: string` (MVP'de sabit "TRY"). Fiyat hesaplarında yuvarlama kuralı tek yerde, Money içinde tanımlanır.
- **Shared projesi** sadece bu tür evrensel tipleri taşır.

Notum:

## 3. Catalog modülü

**Aggregate root: Product.** Varyantlar, görseller ve özellikler Product olmadan anlamsız olduğu için Product'ın içindedir ve sadece Product üzerinden değiştirilir.

### Category
- Id: Guid
- ParentId: Guid? (null ise kök kategori: Kadın, Erkek, Çocuk, Bebek)
- Name: string
- Slug: string (benzersiz, Türkçe karakterden arındırılmış: "Çocuk Gömlek" → `cocuk-gomlek`)
- SortOrder: int
- IsActive: bool

Kendi kendine referans veren ağaç (adjacency list). Kadın > Giyim > Tişört gibi seviyeler ParentId zinciriyle kurulur. "Kadın" listelenirken altındaki tüm kategorilerin ürünleri gösterilir; bu, ağaçta aşağı inen bir sorguyla yapılır.

### Product (aggregate root)
- Id: Guid
- CategoryId: Guid (ürünün bağlı olduğu en alt seviye kategori)
- Name: string
- Slug: string (benzersiz)
- Description: string
- Status: enum (Draft, Active, Archived)
- CreatedAt, UpdatedAt: DateTimeOffset

### ProductImage
- Id: Guid
- ProductId: Guid
- ColorName: string? (boşsa fotoğraf tüm renkler için ortaktır; doluysa Variant.ColorName ile eşleşir)
- Url: string
- SortOrder: int
- IsMain: bool

### Variant
- Id: Guid
- ProductId: Guid
- ColorName: string ("Lacivert")
- HexCode: string? (renk kutucuğu için)
- Size: string ("M", "42", "3-4 Yaş")
- Sku: string (benzersiz, stok takip kodu)
- Price: Money
- IsActive: bool

Varyant, renk ve bedenin birleşimidir. Fiyat varyantta tutulur; liste sayfasında gösterilen fiyat, ürünün aktif varyantlarındaki en düşük fiyattan hesaplanır.

### ProductAttribute
- Id: Guid
- ProductId: Guid
- Key: string ("Materyal", "Yıkama Talimatı", "Kalıp")
- Value: string ("%100 Pamuk", "30 derecede yıkayınız")
- SortOrder: int

Sabit sütun yerine ad–değer listesi seçildi, çünkü ürün tipine göre özellikler değişir (ayakkabıda taban malzemesi, tişörtte kumaş). Eksisi: değerler serbest metin olduğu için filtrelemesi zordur. MVP'de filtre gerekmediği için bu kabul edilebilir.

İlişkiler:
- Category 1 — N Category (ParentId)
- Category 1 — N Product
- Product 1 — N Variant
- Product 1 — N ProductImage
- Product 1 — N ProductAttribute

Notum:

## 4. Inventory modülü

Stok, Catalog'dan ayrıdır: katalog nadiren değişir ve çok okunur, stok sürekli değişir ve eşzamanlılık sorunu yaşatır.

### StockItem
- Id: Guid
- VariantId: Guid (Catalog'daki varyanta sadece Id ile referans)
- QuantityOnHand: int (depodaki toplam)
- ReservedQuantity: int (aktif rezervasyonların toplamı)
- RowVersion: byte[] (optimistic concurrency)
- Satın alınabilir stok = QuantityOnHand − ReservedQuantity (hesaplanan özellik)

### StockReservation
- Id: Guid
- VariantId: Guid
- OrderId: Guid
- Quantity: int
- Status: enum (Active, Committed, Released, Expired)
- ExpiresAt: DateTimeOffset
- CreatedAt: DateTimeOffset

Neden rezervasyon? Kullanıcı ödeme ekranındayken son ürün başkasına satılmasın. Ödeme gelmezse rezervasyon süresi dolar ve stok serbest kalır.

**Rezervasyon süresi: 15 dakika, `appsettings.json`'dan okunur.** Çok kısa olursa kullanıcı 3D Secure ekranında bankadan gelen SMS'i beklerken rezervasyonu kaybeder, çok uzun olursa terk edilmiş sepetler son ürünleri tutar. Süresi dolan rezervasyonları arka planda çalışan bir servis (`BackgroundService`) periyodik olarak serbest bırakır; bu servis sonraki bir aşamada yazılacak.

Notum:

## 5. Basket modülü

### Basket
- Id: Guid
- UserId: Guid
- CreatedAt, UpdatedAt: DateTimeOffset

### BasketItem
- Id: Guid
- BasketId: Guid
- VariantId: Guid
- Quantity: int
- AddedAt: DateTimeOffset

Sepet fiyat saklamaz. Fiyat, ad ve görsel sepet gösterilirken Catalog'dan okunur; böylece sepetteki fiyat her zaman güncel olur. Fiyat sabitlenmesi sipariş verildiği anda yapılır.

Notum:

## 6. Ordering modülü

**Aggregate root: Order.** Kalemleri ve teslimat adresi içindedir.

### Order (aggregate root)
- Id: Guid
- OrderNumber: string (insan tarafından okunabilir, benzersiz)
- UserId: Guid
- Status: enum (bkz. Bölüm 9)
- SubtotalAmount: Money
- ShippingFee: Money
- TotalAmount: Money
- ShippingAddress: ShippingAddress (value object, siparişin içine kopyalanır)
- CreatedAt, PaidAt?, CancelledAt?: DateTimeOffset
- RowVersion: byte[]

### OrderItem
- Id: Guid
- OrderId: Guid
- VariantId: Guid (referans için, ürünle bağ koparılmaz ama veri kopyadır)
- ProductName, ColorName, Size, Sku: string (satın alma anındaki değerler)
- ImageUrl: string
- UnitPrice: Money (satın alma anındaki fiyat)
- Quantity: int
- LineTotal: Money

### ShippingAddress (value object)
- FullName, Phone, City, District, AddressLine, PostalCode: string

İlişkiler:
- Order 1 — N OrderItem
- Order 1 — 1 ShippingAddress (ayrı tablo değil, Order'a gömülü / owned type)

Notum:

## 7. Payment modülü

### Payment
- Id: Guid
- OrderId: Guid (Ordering'e sadece Id ile referans)
- Amount: Money
- Status: enum (Initiated, PendingThreeDSecure, Succeeded, Failed)
- Provider: string (MVP'de kendi yazdığımız sahte sağlayıcı)
- ProviderReference: string?
- FailureReason: string?
- IdempotencyKey: string (benzersiz)
- CreatedAt, CompletedAt?: DateTimeOffset

İlişki: Order 1 — N Payment. Ödeme ayrı bir entity'dir, çünkü bir siparişin birden fazla ödeme denemesi olabilir (ilk kart reddedildi, ikinci kartla tekrar denendi). Ordering ödemenin ayrıntılarını bilmez, sadece sonucu öğrenir.

`IPaymentProvider` arabirimi ödeme sağlayıcısını soyutlar; MVP'de sahte bir implementasyon kullanılır, böylece ileride gerçek bir sağlayıcıya geçmek tek yerde değişiklik olur.

Notum:

## 8. Identity modülü (sonra)

Şimdilik sadece `UserId: Guid` kavramı vardır. Sepet ve sipariş bu Id'yi taşır.

**Geçici çözüm:** Kullanıcı `ICurrentUser` arabiriminin arkasına saklanır (`Guid UserId { get; }`). Geliştirme sırasında sahte bir implementasyon `X-User-Id` header'ından Guid okur veya sabit bir test Guid'i döndürür. Servisler ve endpoint'ler `UserId`'yi asla route veya body'den almaz, hep `ICurrentUser` üzerinden alır. JWT geldiğinde sadece implementasyon değişir. Bu yöntem sadece geliştirme içindir, bu hâliyle canlıya çıkılmaz. Giriş ve kayıt, rol yönetimi ve kayıtlı adres defteri Identity kurulduğunda eklenir. Kullanıcı bilgisinin (ad, e-posta) siparişe kopyalanıp kopyalanmayacağı o zaman kararlaştırılır.

Notum:

## 9. Sipariş durumları ve geçişler

| Durum | Anlamı |
| --- | --- |
| PendingPayment | Sipariş oluştu, stok rezerve edildi, ödeme bekleniyor |
| Paid | Ödeme alındı, rezervasyon kalıcılaştı |
| Shipped | Kargoya verildi |
| Delivered | Teslim edildi |
| Cancelled | İptal edildi |

İzin verilen geçişler:

| Nereden | Nereye | Ne zaman |
| --- | --- | --- |
| PendingPayment | Paid | Ödeme başarılı |
| PendingPayment | Cancelled | Rezervasyon süresi doldu veya kullanıcı vazgeçti |
| Paid | Shipped | Depodan çıktı |
| Paid | Cancelled | Kargoya verilmeden iptal (iade gerekir, sonraki aşamada) |
| Shipped | Delivered | Teslim edildi |

Yasak olan geçişler örnekleri: Shipped → PendingPayment, Delivered → Cancelled, Cancelled → herhangi bir durum. Geçiş kuralları Order sınıfının metotlarında (`MarkAsPaid()`, `Cancel()`) zorlanır, dışarıdan `Status` doğrudan değiştirilemez (`private set`).

**Karar: enum + metot, state machine kütüphanesi yok.** 5 durum ve 5 geçiş için kurallar tek yerde (Order sınıfı) toplanıyor, bu yeterli. Durum sayısı ve koşullar ciddi şekilde büyürse state machine'e geçmek ileride kolay bir refactor olur.

İade ve değişim (ReturnRequested, Returned) MVP dışıdır.

Notum:

## 10. Checkout akışı (modüller nasıl konuşuyor)

1. Kullanıcı teslimat adresini göndererek checkout başlatır.
2. Ordering, sepet içeriğini Basket'ten, güncel fiyat ve adları Catalog'dan okur ve `PendingPayment` durumunda bir Order oluşturur (fiyat, ad ve adres siparişe kopyalanır).
3. Ordering, Inventory'den tüm kalemler için rezervasyon ister (örneğin 15 dakika süreli). Stok yetersizse sipariş iptal edilir.
4. Payment, bir ödeme kaydı açar, sağlayıcıyı çağırır ve 3D Secure adımını yürütür.
5. Ödeme başarılıysa `PaymentSucceeded` event'i yayınlanır: Ordering siparişi `Paid` yapar, Inventory rezervasyonu kalıcılaştırır (stok düşer), Basket temizlenir.
6. Ödeme denemesi başarısız olursa: Payment kaydı `Failed` olur, sipariş `PendingPayment`'ta kalır ve kullanıcı rezervasyon süresi dolana kadar başka bir kartla tekrar deneyebilir (yeni bir Payment kaydı açılır).
7. Rezervasyon süresi dolarsa veya kullanıcı vazgeçerse: Ordering siparişi `Cancelled` yapar, Inventory rezervasyonu serbest bırakır.

Stok düşme zamanı: sepete eklemede değil, rezervasyon anında "ayrılır", ödeme başarılı olunca kalıcı olarak düşer.

Notum:

## 11. İlk 7 sorunun cevapları

1. **Fiyat ve stok nerede?** Varyantta. Fiyat Variant'ta (XL için ek ücret olabilir), stok ise Inventory modülünde VariantId ile tutulur. Neden: aynı ürünün farklı beden ve renkleri farklı fiyatta ve farklı stokta olabilir.
2. **Kategori yapısı?** Hiyerarşik (ParentId). MVP'de bir ürün tek bir en alt kategoriye bağlıdır. Birden fazla yerde görünmesi gereken ürünler (Yeni Gelenler gibi) için ileride ayrı bir "koleksiyon" kavramı eklenir. Neden: tek kategori basit ve yeterli, çoka-çok ilişki sonradan eklenebilir.
3. **Sepet?** Veritabanında, UserId'ye bağlı. MVP'de sepet için giriş gerekir, misafir sepeti (giriş yapmamış kullanıcı) ve girişte birleştirme backlog'dadır. Neden: Identity sona bırakıldığı için önce basit tutuyoruz.
4. **Fiyat sipariş sonrası değişirse?** Eski sipariş değişmez. OrderItem satın alma anındaki ad, bedeni, fiyatı ve adedi kendi alanlarında saklar (snapshot). Neden: sipariş geçmişi, fatura ve iade hesapları o günkü değerle yapılmalı.
5. **Sipariş durumları?** Bölüm 9'da. Geçişler Order sınıfı içinde zorlanır.
6. **Ödeme?** Ayrı entity (Payment), Order'a OrderId ile bağlı, 1–N. Neden: birden fazla deneme olabilir ve ödeme modülü sınırı korumalı.
7. **Adres?** Teslimat adresi siparişe kopyalanır (ShippingAddress value object). Kullanıcı sonradan profil adresini değiştirse eski sipariş etkilenmez.

Notum:

## 12. MVP kapsamı

| MVP'de | Sonra (backlog) |
| --- | --- |
| Kategori ağacı, Product, Variant, ProductImage | Favoriler |
| ProductAttribute (basit hâli) | Kampanya ve kupon |
| Stok ve rezervasyon | Misafir sepeti |
| Sepet (girişli kullanıcı) | Gönderi takibi (Shipment) |
| Sipariş, kalemler, durumlar | İade ve değişim |
| Ödeme (sahte sağlayıcı, 3D Secure akışı) | Yorum ve puanlama |
| Teslimat adresi (siparişin içinde) | Arama, kayıtlı adres defteri |

## 13. Kararlar ve açık sorular

**Kararlar**
- [x] **ProductColor katmanı yok.** Renk, Variant'ın alanıdır (`ColorName`, `HexCode`), fotoğraf Product'a bağlıdır ve isteğe bağlı `ColorName` ile renge eşlenir. Bedeli: renk eşleşmesi metne dayanır. İleride gerekirse migration ile katman eklenir.
- [x] **Rezervasyon süresi 15 dakika**, ayarlanabilir; temizlemeyi arka plan servisi yapar (Bölüm 4).
- [x] **Sipariş durumları enum + metotla** yönetilir (Bölüm 9).
- [x] **Identity gelene kadar `ICurrentUser`** ile sahte kullanıcı kullanılır (Bölüm 8).

**Açık soru**
- [ ] Ödeme başarılı oldu ama Ordering siparişi `Paid` yapamadan uygulama çöktü: ne olur? Bu riski fark ettim ama çözümünü henüz bilmiyorum. Mesajlaşma aşamasında (outbox pattern) öğrenilip bu bölüme geri dönülecek.


