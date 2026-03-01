from fastapi import FastAPI
import uvicorn
import google.generativeai as genai
import json
import re
import os # YENİ EKLEDİK
from dotenv import load_dotenv # YENİ EKLEDİK

# .env dosyasındaki gizli şifreleri sisteme yükle
load_dotenv() 

app = FastAPI()

# Şifreyi kodun içine yazmak yerine bilgisayardaki gizli dosyadan çekiyoruz!
gizli_api_anahtari = os.getenv("GEMINI_API_KEY")
genai.configure(api_key=gizli_api_anahtari)
model = genai.GenerativeModel('gemini-2.5-flash')

# ... (KODUN GERİ KALANI TAMAMEN AYNI KALACAK) ...

aktif_sinyal = {"hastalik": "Sağlıklı", "bolge": "Yok", "renk": "white", "aciklama": "Sistem hazır. Lütfen bir şikayet girin."}

@app.get("/unity_verisi")
def unity_icin_veri():
    return aktif_sinyal

@app.get("/analiz/{kullanici_metni}")
def yapay_zeka_analiz(kullanici_metni: str):
    global aktif_sinyal
    
    sistem_talimati = f"""Sen Teknofest Sağlıkta Yapay Zeka projesi için geliştirilmiş uzman bir medikal yapay zeka ve simülasyon motorusun. 
    Kullanıcının durumu/şikayeti şu: '{kullanici_metni}'
    
    Görevlerin:
    1. Bu durumun gelecekte kalbin hangi kısmına zarar vereceğini analiz et.
    2. SADECE şu listeden en uygun parçayı seçmek ZORUNDASIN: [Aorta, Arteries2, Avvalves, Hart_basis, Heartear, Ligament, Pulmonary_trunk, Valves, Veins]
    3. Hasar şiddetine göre bir renk belirle (black, red, orange, purple).
    4. Açıklaman destan gibi UZUN OLMAYACAK. En fazla 2 veya 3 cümlelik, net, vurucu ve hastanın anlayacağı dilde bir ÖZET yaz.
    5. Hastalık adını doğrudan verme. "Uzun vadede [Sorun/Hastalık] riski" formatında yaz. (Örnek: "Uzun vadede kalp krizi riski").
    
    Bana SADECE şu formatta geçerli bir JSON çıktısı ver:
    {{"hastalik": "Uzun vadede ... riski", "bolge": "Secilen_Parca", "renk": "renk_kodu", "aciklama": "2-3 cümlelik kısa özet..."}}"""

    try:
        cevap = model.generate_content(sistem_talimati)
        
        # KESİN ÇÖZÜM: Yapay zeka ne metin gönderirse göndersin, içindeki JSON'ı cımbızla çek
        match = re.search(r'\{.*\}', cevap.text, re.DOTALL)
        if match:
            cevap_metni = match.group(0)
        else:
            cevap_metni = cevap.text
            
        uretilen_veri = json.loads(cevap_metni)
        
        aktif_sinyal = {
            "hastalik": uretilen_veri["hastalik"],
            "bolge": uretilen_veri["bolge"],
            "renk": uretilen_veri["renk"],
            "aciklama": uretilen_veri["aciklama"]
        }
        return {"mesaj": "Analiz başarılı!"}
        
    except Exception as e:
        print("HATA OLUŞTU:", e)
        # ACİL DURUM PLANI: Hata olursa Unity donmasın diye Aorta'ya uyarı sinyali gönder!
        aktif_sinyal = {
            "hastalik": "SİSTEM BAĞLANTI HATASI",
            "bolge": "Aorta", 
            "renk": "white",
            "aciklama": f"Yapay zeka veriyi işlerken bir sorun yaşadı. Lütfen 'Sıfırla' butonuna basıp tekrar deneyin.\nDetay: {str(e)}"
        }
        return {"hata": "Analiz başarısız.", "detay": str(e)}

@app.get("/sifirla")
def sistemi_sifirla():
    global aktif_sinyal
    aktif_sinyal = {"hastalik": "Sağlıklı", "bolge": "Yok", "renk": "white", "aciklama": "Sistem hazır. Lütfen bir şikayet girin."}
    return {"mesaj": "Sistem başarıyla sıfırlandı!"}

if __name__ == "__main__":
    uvicorn.run("main:app", host="127.0.0.1", port=8000, reload=True)