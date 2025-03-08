import os
import serial
import time
import datetime
import pyzipper

# Configuración del puerto serie
ESP_PORT = "COM5"  # Ajusta según tu configuración
BAUD_RATE = 115200
TIMEOUT = 1

try:
    esp = serial.Serial(ESP_PORT, BAUD_RATE, timeout=TIMEOUT)
    time.sleep(2)  # Pausa para estabilizar la conexión
    print(f"Conectado al ESP en {ESP_PORT} a {BAUD_RATE} baudios.")
except serial.SerialException:
    print(f"Error: No se pudo abrir el puerto {ESP_PORT}. Verifica la conexión.")
    exit()

def enviar_y_recibir(mensaje):
    """Envía un mensaje y devuelve la respuesta del ESP en una lista de líneas."""
    esp.write((mensaje + "\n").encode("utf-8"))
    time.sleep(0.5)  # Espera para recibir respuesta
    respuesta = esp.readlines()
    return [line.decode("utf-8").strip() for line in respuesta]

def obtener_datos():
    """Solicita los datos del ESP con autenticación."""
    print("Enviando solicitud de datos...")
    respuesta = enviar_y_recibir("GET_DATOS")

    if any("PASSWORD_REQUEST" in linea for linea in respuesta):
        password = input("Ingrese la contraseña: ")  # Entrada oculta de la contraseña
        respuesta = enviar_y_recibir(password)

        if any("Acceso denegado" in linea for linea in respuesta):
            print("Acceso denegado. Verifica la contraseña.")
        elif any("ACCESO_CONCEDIDO" in linea for linea in respuesta):
            print("Acceso concedido. Descargando datos...")

            # Obtener la fecha y hora actual para nombrar el archivo
            fecha_hora = datetime.datetime.now().strftime("%Y-%m-%d_%H-%M-%S")
            nombre_archivo = f"datos_{fecha_hora}.txt"

            # Guardar datos en un archivo de texto, excluyendo "Acceso concedido"
            with open(nombre_archivo, "w", encoding="utf-8") as archivo:
                archivo.write("\u00cdndice | Tecla | Fecha y Hora\n")  # Encabezado
                for linea in respuesta:
                    if "END_OF_DATA" in linea:
                        break
                    if "ACCESO_CONCEDIDO" not in linea:
                        archivo.write(linea + "\n")

            print(f"Datos guardados en: {nombre_archivo}")

            # Comprimir el archivo con contraseña
            nombre_zip = f"datos_{fecha_hora}.zip"
            password_zip = b"clave_segura"  # Contraseña del ZIP

            with pyzipper.AESZipFile(nombre_zip, 'w', compression=pyzipper.ZIP_DEFLATED, encryption=pyzipper.WZ_AES) as zipf:
                zipf.setpassword(password_zip)
                zipf.write(nombre_archivo)

            print(f"Archivo comprimido creado: {nombre_zip}")

            # Eliminar el archivo de texto original
            os.remove(nombre_archivo)
        else:
            print("Respuesta inesperada del ESP.")
    else:
        print("No se recibió la solicitud de contraseña.")

def borrar_datos():
    """Solicita borrar los datos al ESP con contraseña."""
    print("Enviando solicitud de borrado de datos...")
    respuesta = enviar_y_recibir("DELETE_DATOS")

    if "PASSWORD_REQUEST" in respuesta:
        ingreso = input("Ingrese la contraseña: ")
        esp.write((ingreso + "\n").encode("utf-8"))
        time.sleep(1)

        respuesta = esp.readlines()
        respuesta = [line.decode("utf-8").strip() for line in respuesta]

        if "DATOS_ELIMINADO" in respuesta:
            print("Datos eliminados correctamente.")
        elif "NO_DATOS" in respuesta:
            print("No había datos que borrar.")
        else:
            print("Acceso denegado. Verifica la contraseña.")
    else:
        print("No se recibió la solicitud de contraseña.")

def comunicarse_con_esp():
    """Menú principal del programa."""
    while True:
        print("\nOpciones:")
        print("1. Obtener datos y comprimir")
        print("2. Borrar datos")
        print("3. Salir")
        opcion = input("Selecciona una opción: ").strip()

        if opcion == "1":
            obtener_datos()
        elif opcion == "2":
            borrar_datos()
        elif opcion == "3":
            print("Saliendo...")
            esp.close()
            break
        else:
            print("Opción inválida. Intenta de nuevo.")

# Ejecutar el programa
if __name__ == "__main__":
    comunicarse_con_esp()
