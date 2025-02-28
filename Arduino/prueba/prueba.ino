#include <Arduino.h>

void setup() {
  Serial.begin(115200);  // Inicia la comunicación serial
  while (!Serial);  // Espera a que se abra el puerto serial
  Serial.println("ESP32 listo para recibir comandos...");
}

void loop() {
  if (Serial.available()) {
    String command = Serial.readStringUntil('\n');  // Lee el comando enviado desde el puerto serial
    if (command == "GET_DATOS") {
      Serial.println("Contraseña requerida");
      String password = Serial.readStringUntil('\n');
      if (password == "123456") {
        Serial.println("Contraseña correcta. Enviando datos...");
        // Simula enviar datos
        Serial.println("Datos obtenidos: 123,456,789");
      } else {
        Serial.println("Contraseña incorrecta");
      }
    } else if (command == "DELETE_DATOS") {
      Serial.println("Datos eliminados correctamente");
      // Aquí puedes agregar código para eliminar datos en memoria
    } else {
      Serial.println("Comando no reconocido");
    }
  }
}
