using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.UI;

[System.Serializable]
public class HastalikVerisi {
    public string hastalik;
    public string bolge;
    public string renk;
    public string aciklama;
}

[System.Serializable]
public class ParcaGorseli {
    public string parcaAdi;
    public Sprite gorsel;
}

public class HeartController : MonoBehaviour {
    private string apiUrl = "http://127.0.0.1:8000/unity_verisi";
    private string sonHastalik = "";
    private Coroutine aktifAnimasyon;
    private Renderer sonBoyananParca; 

    [Header("Arayüz (UI) Ayarları")]
    public GameObject bilgiPaneli;
    public TMP_Text baslikYazisi;
    public TMP_Text aciklamaYazisi;
    public Image gercekGorselKutusu; 
    public ParcaGorseli[] parcaGorselleri;

    [Header("Yapay Zeka Giriş Paneli")]
    public TMP_InputField sikayetKutusu; 

    [Header("Yükleme (Loading) Ekranı")]
    public GameObject loadingPaneli; 
    public Transform donenIkon;      
    private Coroutine loadingAnimasyonu;

    [Header("Kamera & Zoom Ayarları")]
    public Transform anaKamera;
    public float zoomMiktari = 3.5f;
    public float zoomHizi = 2f;

    private Vector3 ilkKameraPozisyonu;
    private Quaternion ilkKameraRotasyonu;

    private bool manuelXRayAcik = false;

    void Start() {
        if (anaKamera == null) anaKamera = Camera.main.transform;
        
        ilkKameraPozisyonu = anaKamera.position;
        ilkKameraRotasyonu = anaKamera.rotation;

        if(bilgiPaneli != null) bilgiPaneli.SetActive(false);
        if(loadingPaneli != null) loadingPaneli.SetActive(false); 
        
        // OYUN BAŞLARKEN KALBİ KESİNLİKLE KATI (OPAK) ET YAP
        manuelXRayAcik = false;
        XRayUygula(false); 

        StartCoroutine(VeriDinle());
    }

    public void TransparanlikGecis() {
        manuelXRayAcik = !manuelXRayAcik; 
        XRayUygula(manuelXRayAcik);
    }

    public void YapayZekaAnaliziniBaslat() {
        if (sikayetKutusu != null && !string.IsNullOrEmpty(sikayetKutusu.text)) {
            if (loadingPaneli != null) loadingPaneli.SetActive(true);
            if (donenIkon != null) {
                if (loadingAnimasyonu != null) StopCoroutine(loadingAnimasyonu);
                loadingAnimasyonu = StartCoroutine(YuvarlakDondur());
            }
            StartCoroutine(YapayZekayaSor(sikayetKutusu.text));
        }
    }

    IEnumerator YuvarlakDondur() {
        while(true) {
            donenIkon.Rotate(0, 0, -350f * Time.deltaTime);
            yield return null;
        }
    }

    IEnumerator YapayZekayaSor(string metin) {
        string url = "http://127.0.0.1:8000/analiz/" + UnityWebRequest.EscapeURL(metin);
        using (UnityWebRequest istek = UnityWebRequest.Get(url)) {
            yield return istek.SendWebRequest();
        }
    }

    public void SimulasyonuTemizle() {
        StartCoroutine(SunucuyuSifirla());

        if (bilgiPaneli != null) bilgiPaneli.SetActive(false);
        if (sikayetKutusu != null) sikayetKutusu.text = ""; 
        sonHastalik = "Sağlıklı"; 
        
        if (loadingPaneli != null) loadingPaneli.SetActive(false);
        if (loadingAnimasyonu != null) StopCoroutine(loadingAnimasyonu);

        StopCoroutine("KameraOdaklan");
        if (anaKamera != null) {
            anaKamera.position = ilkKameraPozisyonu;
            anaKamera.rotation = ilkKameraRotasyonu;
        }

        if (aktifAnimasyon != null) StopCoroutine(aktifAnimasyon);
        if (sonBoyananParca != null) {
            sonBoyananParca.material.color = new Color(0.8f, 0.1f, 0.1f);
            sonBoyananParca.material.SetColor("_BaseColor", new Color(0.8f, 0.1f, 0.1f));
            sonBoyananParca = null;
        }

        // SIFIRLAYINCA KALBİ TEKRAR KATI (OPAK) ET YAP
        manuelXRayAcik = false;
        XRayUygula(false); 
    }

    IEnumerator SunucuyuSifirla() {
        string url = "http://127.0.0.1:8000/sifirla";
        using (UnityWebRequest istek = UnityWebRequest.Get(url)) {
            yield return istek.SendWebRequest();
        }
    }

    IEnumerator VeriDinle() {
        while (true) {
            using (UnityWebRequest webRequest = UnityWebRequest.Get(apiUrl)) {
                yield return webRequest.SendWebRequest();
                if (webRequest.result == UnityWebRequest.Result.Success) {
                    HastalikVerisi gelen = JsonUtility.FromJson<HastalikVerisi>(webRequest.downloadHandler.text);
                    if (gelen.hastalik != sonHastalik && gelen.bolge != "Yok") {
                        sonHastalik = gelen.hastalik;
                        Guncelle(gelen);
                    }
                }
            }
            yield return new WaitForSeconds(1f);
        }
    }

    void Guncelle(HastalikVerisi veri) {
        if (loadingPaneli != null) loadingPaneli.SetActive(false);
        if (loadingAnimasyonu != null) StopCoroutine(loadingAnimasyonu);

        if (bilgiPaneli != null) {
            bilgiPaneli.SetActive(true);
            baslikYazisi.text = "<color=red>" + veri.hastalik.ToUpper() + "</color>";
            aciklamaYazisi.text = veri.aciklama;
            
            if (gercekGorselKutusu != null) {
                gercekGorselKutusu.sprite = null;
                gercekGorselKutusu.color = new Color(1, 1, 1, 0); 
                foreach (var pg in parcaGorselleri) {
                    if (pg.parcaAdi.Trim().ToLower() == veri.bolge.Trim().ToLower()) {
                        gercekGorselKutusu.sprite = pg.gorsel;
                        gercekGorselKutusu.color = Color.white;
                        break;
                    }
                }
            }
        }

        GameObject parca = GameObject.Find(veri.bolge);
        if (parca != null) {
            Renderer r = parca.GetComponent<Renderer>();
            if (sonBoyananParca != null && sonBoyananParca != r) {
                sonBoyananParca.material.color = new Color(0.8f, 0.1f, 0.1f);
                sonBoyananParca.material.SetColor("_BaseColor", new Color(0.8f, 0.1f, 0.1f));
            }
            sonBoyananParca = r;

            if (aktifAnimasyon != null) StopCoroutine(aktifAnimasyon);
            aktifAnimasyon = StartCoroutine(YanipSonme(r));

            bool icParcaMi = (veri.bolge == "Avvalves" || veri.bolge == "Valves");
            manuelXRayAcik = icParcaMi; 
            XRayUygula(manuelXRayAcik); 
            
            float guncelZoom = icParcaMi ? (zoomMiktari * 0.4f) : zoomMiktari;

            StopCoroutine("KameraOdaklan");
            StartCoroutine(KameraOdaklan(parca.transform.position, guncelZoom));
        }
    }

    // YENİ VE KUSURSUZ X-RAY SİSTEMİ: Materyali kodla OPAK ve ŞEFFAF modlar arası geçiş yaptırır!
    void XRayUygula(bool seffafMi) {
        GameObject disKas = GameObject.Find("Hart_basis"); 
        if (disKas != null) {
            Renderer r = disKas.GetComponent<Renderer>();
            Material mat = r.material;

            if (seffafMi) {
                // BUTONA BASILDI: Kalbi cam gibi ŞEFFAF (Transparent) yap
                mat.SetFloat("_Surface", 1); 
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.renderQueue = 3000;
                
                Color c = mat.color;
                c.a = 0.2f; // %20 Görünür
                mat.color = c;
                mat.SetColor("_BaseColor", c);
            } else {
                // NORMAL DURUM: Kalbi %100 KATI (Opaque) gerçek et yap
                mat.SetFloat("_Surface", 0); 
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                mat.SetInt("_ZWrite", 1);
                mat.renderQueue = 2000;
                
                Color c = mat.color;
                c.a = 1f; // %100 Katı
                mat.color = c;
                mat.SetColor("_BaseColor", c);
            }
        }
    }

    IEnumerator KameraOdaklan(Vector3 hedef, float anlikZoom) {
        if (anaKamera == null) yield break;
        Vector3 baslangic = anaKamera.position;
        Vector3 bitis = hedef + (anaKamera.position - hedef).normalized * anlikZoom;
        float t = 0;
        while (t < 1f) {
            t += Time.deltaTime * zoomHizi;
            anaKamera.position = Vector3.Lerp(baslangic, bitis, t);
            anaKamera.LookAt(hedef);
            yield return null;
        }
    }

    IEnumerator YanipSonme(Renderer r) {
        Material mat = r.material; 
        Color normalRenk = new Color(0.8f, 0.1f, 0.1f); 
        Color hasarRengi = Color.black; 
        float hiz = 4f; 
        while (true) {
            float ritim = Mathf.PingPong(Time.time * hiz, 1f);
            Color anlikRenk = Color.Lerp(normalRenk, hasarRengi, ritim);
            mat.color = anlikRenk; 
            mat.SetColor("_BaseColor", anlikRenk); 
            yield return null;
        }
    }
}