# Randevu Planlayıcı

Günlük randevuları tek ekranda gösteren bir C# ve WinForms uygulaması. Seçilen saat doluysa çakışan randevuyu işaretler ve günün kalanında yeni randevunun sığacağı ilk aralığı önerir.

## Çalıştırma

Windows ve .NET 8 SDK gerekir. Proje klasöründe:

```sh
dotnet run
```

## Denemek için

1. Uygulamanın açıldığı günü seçili bırak.
2. Bir randevu adı gir, başlangıcı **10:30**, süreyi **45 dakika** olarak ayarla ve kaydet.
3. Ajandada ekip görüşmesi işaretlenir. **11:30 saatini seç** düğmesine bas. Önerilen saat forma gelir.
4. Randevuyu tekrar kaydet. Yeni kayıt ajandada **11:30–12:15** olarak görünür.

Çalışma saatleri 09:00–18:00. Bir randevunun bittiği dakikada yenisi başlayabilir. Kayıtlar oturum boyunca tutulur. Uygulama yeniden açıldığında örnek kayıtlar yüklenir.

Saat karşılaştırma ve boş aralık hesapları `Core/Planner.cs` dosyasında, ekran ve kullanıcı etkileşimleri `AgendaForm.cs` dosyasında.

## Testler

Planlama testlerini proje klasöründen çalıştır:

```sh
dotnet test Tests/RandevuPlanlayici.Tests.csproj
```

## Lisans

Uygulama [MIT lisansı](LICENSE) kapsamında, Figtree yazı tipi [SIL Open Font License](Fonts/OFL.txt) kapsamındadır.
