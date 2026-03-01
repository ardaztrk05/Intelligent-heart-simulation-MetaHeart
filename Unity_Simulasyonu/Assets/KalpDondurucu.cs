using UnityEngine;

public class KalpDondurucu : MonoBehaviour {
    [Header("Döndürme Ayarları")]
    public float donmeHizi = 1.5f; // YENİ: Eskisinden çok daha yavaş, ağır ve kontrollü

    [Header("Zoom Ayarları (Mouse Tekerleği)")]
    public float manuelZoomHizi = 150f; // YENİ: Tekerleği çevirdiğinde anında tepki verecek kadar hızlı
    
    void Update() {
        // SADECE X ve Y EKSENİNDE DÖNDÜR (Sol tık basılıyken)
        if (Input.GetMouseButton(0)) {
            float rotX = Input.GetAxis("Mouse X") * donmeHizi;
            float rotY = Input.GetAxis("Mouse Y") * donmeHizi;

            // Z ekseninde takla atmasını engellemek için "Space.World" kullanıyoruz
            transform.Rotate(Vector3.up, -rotX, Space.World);
            transform.Rotate(Vector3.right, rotY, Space.World);
        }

        // FARE TEKERLEĞİ İLE SERBEST ZOOM
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0.0f) {
            // Kamerayı baktığı yöne doğru ileri/geri hareket ettir
            Camera.main.transform.Translate(Vector3.forward * scroll * manuelZoomHizi * Time.deltaTime, Space.Self);
        }
    }
}