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

const String LISTO = "LISTO";
const String PASSWORD = "1234";
unsigned long lastMillis;
unsigned long currentEpochTime;
int indice = 1;

String mapaTeclas[] = {"F1", "F2", "F3", "F4", "1", "2", "3", "A", "4", "5", "6", "B", "7", "8", "9", "C", "*", "0", "#", "E"};
int totalTeclas = sizeof(mapaTeclas) / sizeof(mapaTeclas[0]);

void setup() {
    Serial.begin(115200);
    delay(2000);

    WiFi.begin(ssid, password);
    while (WiFi.status() != WL_CONNECTED) {
        delay(500);
    }

    timeClient.begin();
    timeClient.update();
    currentEpochTime = timeClient.getEpochTime();

    WiFi.disconnect(true);
    WiFi.mode(WIFI_OFF);

    lastMillis = millis();

    if (!SPIFFS.begin(true)) {
        return;
    }

    pinMode(LED_BUILTIN, OUTPUT);
}

void loop() {
    actualizarTiempo();
    emularTecla();
    manejarComandosSerial();
    delay(1000); 
    digitalWrite(LED_BUILTIN, HIGH);  
    delay(500);                      
    digitalWrite(LED_BUILTIN, LOW);   
    delay(500);
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
    guardarTecla(tecla);
}

void guardarTecla(String tecla) {
    String fechaHora = obtenerFechaHora();
    File archivo = SPIFFS.open("/log_teclado.txt", "a");
    if (!archivo) {
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
            delay(100); 
            Serial.println("PASSWORD_REQUEST");
            while (Serial.available() == 0);
            String ingreso = Serial.readStringUntil('\n');
            ingreso.trim();
            if (ingreso == PASSWORD) {
                delay(100); 
                Serial.println("ACCESO_CONCEDIDO");
                File archivo = SPIFFS.open("/log_teclado.txt", "r");
                if (!archivo) {
                    Serial.println("ERROR_ARCHIVO");
                    return;
                }
                while (Serial.available() == 0);
                  String RDY = Serial.readStringUntil('\n');
                RDY.trim();
                if (RDY == LISTO) {
                  delay(100);
                while (archivo.available()) {
                    Serial.write(archivo.read());
                }  
                }else{
                  Serial.println("ERROR_LISTO");
                }
                delay(100); 
                Serial.println("END_OF_DATA");
                archivo.close();
            } else {
                delay(100); 
                Serial.println("ACCESO_DENEGADO");
            }
        } 
        else if (comando == "DELETE_DATOS") {
            delay(100); 
            Serial.println("PASSWORD_REQUEST");
            while (Serial.available() == 0);
            String ingreso = Serial.readStringUntil('\n');
            ingreso.trim();
            if (ingreso == PASSWORD) {
                delay(100);
                Serial.println("ACCESO_CONCEDIDO");
                while (Serial.available() == 0);
                  String RDY = Serial.readStringUntil('\n');
                RDY.trim();
                if (RDY == LISTO) {
                  if (SPIFFS.exists("/log_teclado.txt")) {
                    SPIFFS.remove("/log_teclado.txt");
                    delay(100);
                    Serial.println("DELETE_SUCCESS");
                    indice = 1;
                  }else {
                    delay(100); 
                    Serial.println("NO_DATOS");
                  }
                }else{
                  Serial.println("ERROR_LISTO");
                }
            } else {
                delay(100); 
                Serial.println("ACCESO_DENEGADO");
            }
        } else {
            Serial.println("ERROR_PETICION");
        }
    }
}
