# 🫀 Intelligent Heart Simulation - MetaHeart

Geliştirdiğim bu bireysel proje, doğal dildeki şikayetleri yapay zeka ile analiz ederek olası kalp rahatsızlıklarını 3D anatomik model üzerinde görselleştiren interaktif bir simülasyondur.

## ✨ Özellikler
* **Doğal Dil İşleme (NLP):** Serbest metin formatındaki hasta şikayetlerini (örn: "Göğsüm sıkışıyor ve sol kolum uyuşuyor") anlar ve işler.
* **Dinamik 3D Görselleştirme:** Hedeflenen riskli dokuyu Unity üzerinde vurgular. İç organ analizlerinde X-Ray (şeffaflık) modunu otomatik devreye sokar.
* **İnteraktif Kontrol:** Fare tekerleği ile dinamik zoom ve X/Y eksenlerinde 3D modeli serbest döndürme imkanı.

## 🛠️ Kullanılan Teknolojiler
* **Ön Uç (Frontend):** Unity 3D, C#, UnityWebRequest
* **Arka Uç (Backend):** Python, FastAPI, Uvicorn
* **Yapay Zeka:** Google Gemini 2.5 Flash API
* **Güvenlik:** python-dotenv (Çevresel değişken izolasyonu)

## 🚀 Kurulum ve Kullanım
Projeyi kendi ortamınızda test etmek için şu adımları izleyin:

1. Bu repoyu bilgisayarınıza klonlayın.
2. Python sunucusunun olduğu kök klasörde `.env` adında uzantısız bir dosya oluşturun.
3. Google AI Studio'dan aldığınız API anahtarını dosyanın içine şu şekilde ekleyin: `GEMINI_API_KEY=sizin_anahtariniz`
4. Gerekli kütüphaneleri kurun: `pip install fastapi uvicorn google-generativeai python-dotenv`
5. Terminalden `python main.py` komutuyla sunucuyu başlatın.
6. `Unity_Simulasyonu` klasörünü Unity Hub üzerinden açıp projeyi çalıştırın.
