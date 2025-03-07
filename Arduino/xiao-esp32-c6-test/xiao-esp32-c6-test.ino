#include <Arduino.h>
#include <SPIFFS.h>
#include <WiFi.h>
#include <WiFiUdp.h>
#include <NTPClient.h>
#include <time.h>

const char* ssid = "POCOX6Pro";
const char* password = "dayan1212";

WiFiUDP ntpUDP;
NTPClient timeClient(ntpUDP, "pool.ntp.org", -21600, 60000);

const String PASSWORD = "1234";
unsigned long lastMillis;
unsigned long currentEpochTime;
int indice = 1;

String mapaTeclas[] = {"F1", "F2", "F3", "F4", "1", "2", "3", "A", "4", "5", "6", "B", "7", "8", "9", "C", "*", "0", "#", "E"};
int totalTeclas = sizeof(mapaTeclas) / sizeof(mapaTeclas[0]);

void setup() {
    Serial.begin(115200);
    delay(2000);

    Serial.println("ESP32-C6 Iniciado...");
    
    WiFi.begin(ssid, password);
    Serial.print("Conectando a WiFi");
    while (WiFi.status() != WL_CONNECTED) {
        delay(500);
        Serial.print(".");
    }
    Serial.println("\nWiFi conectado.");

    timeClient.begin();
    timeClient.update();
    currentEpochTime = timeClient.getEpochTime();

    WiFi.disconnect(true);
    WiFi.mode(WIFI_OFF);
    Serial.println("WiFi desconectado. Usando tiempo interno.");

    lastMillis = millis();

    if (!SPIFFS.begin(true)) {
        Serial.println("Error al montar SPIFFS");
        return;
    }
    Serial.println("Sistema de archivos montado");
}

void loop() {
    actualizarTiempo();
    emularTecla();
    manejarComandosSerial();
    delay(2000); // Simula la presión de una tecla cada 2 segundos
}

void actualizarTiempo() {
    unsigned long elapsedMillis = millis() - lastMillis;
    if (elapsedMillis >= 1000) {
        currentEpochTime += elapsedMillis / 1000;
        lastMillis = millis();
    }
}

void emularTecla() {
    String tecla = mapaTeclas[random(totalTeclas)];
    Serial.println("Tecla emulada: " + tecla);
    guardarTecla(tecla);
}

void guardarTecla(String tecla) {
    String fechaHora = obtenerFechaHora();
    File archivo = SPIFFS.open("/log_teclado.txt", "a");
    if (!archivo) {
        Serial.println("Error al abrir el archivo.");
        return;
    }
    archivo.print(indice);
    archivo.print(" ");
    archivo.print(tecla);
    archivo.print(" ");
    archivo.println(fechaHora);
    archivo.close();
    indice++;
}

String obtenerFechaHora() {
    time_t rawtime = currentEpochTime;
    struct tm * timeinfo = localtime(&rawtime);
    char buffer[30];
    int centesimas = (millis() % 1000) / 10;
    sprintf(buffer, "%02d/%02d/%04d %02d:%02d:%02d:%02d", 
            timeinfo->tm_mday, timeinfo->tm_mon + 1, timeinfo->tm_year + 1900, 
            timeinfo->tm_hour, timeinfo->tm_min, timeinfo->tm_sec, centesimas);
    return String(buffer);
}

void manejarComandosSerial() {
    if (Serial.available()) {
        String comando = Serial.readStringUntil('\n');
        comando.trim();

        if (comando == "GET_DATOS") {
            Serial.println("PASSWORD_REQUEST");
            while (Serial.available() == 0);
            String ingreso = Serial.readStringUntil('\n');
            ingreso.trim();
            if (ingreso == PASSWORD) {
                Serial.println("Acceso concedido");
                File archivo = SPIFFS.open("/log_teclado.txt", "r");
                if (!archivo) {
                    Serial.println("ERROR: No se pudo abrir el archivo.");
                    return;
                }
                while (archivo.available()) {
                    Serial.write(archivo.read());
                }
                Serial.println("\nEND_OF_DATA");
                archivo.close();
            } else {
                Serial.println("Acceso denegado.");
            }
        } 
        else if (comando == "DELETE_DATOS") {
            Serial.println("PASSWORD_REQUEST");
            while (Serial.available() == 0);
            String ingreso = Serial.readStringUntil('\n');
            ingreso.trim();
            if (ingreso == PASSWORD) {
                Serial.println("Eliminando datos...");
                if (SPIFFS.exists("/log_teclado.txt")) {
                    SPIFFS.remove("/log_teclado.txt");
                    Serial.println("ARCHIVO ELIMINADO");
                } else {
                    Serial.println("NO HAY DATOS PARA BORRAR");
                }
            } else {
                Serial.println("Acceso denegado.");
            }
        }
    }
}