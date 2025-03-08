using System;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using ICSharpCode.SharpZipLib.Zip;

public class MainForm : Form
{
    private ComboBox portSelector;
    private Button getDataButton, deleteDataButton;
    private SerialPort serialPort = new SerialPort();
    private string RDY = "LISTO";

    public MainForm()
    {
        Text = "Data Manager";
        Width = 300;
        Height = 200;

        portSelector = new ComboBox { Text = "Puerto", Left = 100, Top = 30, Width = 100 };
        getDataButton = new Button { Text = "Descargar Datos", Left = 75, Top = 70, Width = 150 };
        deleteDataButton = new Button { Text = "Borrar Datos", Left = 75, Top = 110, Width = 150 };

        Controls.Add(portSelector);
        Controls.Add(getDataButton);
        Controls.Add(deleteDataButton);

        getDataButton.Click += (s, e) => GetData();
        deleteDataButton.Click += (s, e) => DeleteData();

        LoadPorts();
    }

    private void LoadPorts()
    {
        portSelector.Items.AddRange(SerialPort.GetPortNames());
    }

    private void GetData()
    {
        if (portSelector.SelectedItem == null) return;

        try
        {
            serialPort = new SerialPort(portSelector.SelectedItem.ToString(), 115200);
            serialPort.Open();
            serialPort.WriteLine("GET_DATOS");

            string response = ReadResponse();
            if (string.IsNullOrEmpty(response))
            {
                MessageBox.Show("Error: No se recibió respuesta del ESP32.");
                return;
            }

            if (response.Contains("PASSWORD_REQUEST"))
            {
                string password = PromptDialog.ShowDialog("Ingrese clave:", "Descargar Registros");
                serialPort.WriteLine(password);
                response = ReadResponse();
            }

            if (response.Contains("ACCESO_CONCEDIDO"))
            {
                serialPort.WriteLine(RDY);
                response = ReadResponse();
                if (response.Contains("END_OF_DATA"))
                {
                    //MessageBox.Show(response);
                    SaveData(response);
                }
                else
                {
                    MessageBox.Show("Error al descargar los datos.");
                }
            }
            else if (response.Contains("ACCESO_DENEGADO"))
            {
                MessageBox.Show("Contraseña incorrecta.");
            }
            serialPort.Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error en la comunicación: {ex.Message}");
        }
    }

    private void DeleteData()
    {
        if (portSelector.SelectedItem == null) return;

        try
        {
            serialPort = new SerialPort(portSelector.SelectedItem.ToString(), 115200);
            serialPort.Open();
            serialPort.WriteLine("DELETE_DATOS");
            
            string response = ReadResponse();
            if (response.Contains("PASSWORD_REQUEST"))
            {
                string password = PromptDialog.ShowDialog("Ingrese clave:", "Eliminar Registros");
                serialPort.WriteLine(password);
                response = ReadResponse();
            }

            if (response.Contains("ACCESO_CONCEDIDO"))
            {
                serialPort.WriteLine(RDY);
                response = ReadResponse();
                if (response.Contains("DELETE_SUCCESS"))
                {
                    MessageBox.Show("Datos eliminados correctamente.");
                }
                else if (response.Contains("NO_DATOS"))
                {
                    MessageBox.Show("No hay datos.");
                }
                else
                {
                    MessageBox.Show("Error al eliminar los datos.");
                }
            }
            else if (response.Contains("ACCESO_DENEGADO"))
            {
                MessageBox.Show("Contraseña incorrecta.");
            }
            serialPort.Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error en la comunicación: {ex.Message}");
        }
    }

    private async void SaveData(string data)
    {
        try
        {
            // Ejecutar la operación en un hilo de fondo
            await Task.Run(() =>
            {
                // Eliminar la línea "END_OF_DATA" si está presente
                var lines = data.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                                .Where(line => line != "END_OF_DATA")
                                .ToList();

                // Agregar las etiquetas como primera línea
                lines.Insert(0, "Fila\tTecla\tFecha\tHora");

                // Generar nombre único para el archivo ZIP basado en la fecha y hora actual
                string zipFileName = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + "_datos.zip";
                string zipFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, zipFileName); // Ruta donde se guarda el archivo

                // Pedir contraseña para proteger el archivo ZIP
                string password = PromptDialog.ShowDialog("Ingrese clave:", "Protección de archivo");

                // Comprimir directamente sin crear un archivo txt intermedio
                using (FileStream fsOut = File.Create(zipFilePath))
                using (ZipOutputStream zipStream = new ZipOutputStream(fsOut))
                {
                    zipStream.SetLevel(9); // Máxima compresión
                    zipStream.Password = password; // Asignar contraseña

                    // Convertir el contenido modificado (con etiquetas) en bytes
                    byte[] buffer = Encoding.UTF8.GetBytes(string.Join("\r\n", lines));
                    ZipEntry entry = new ZipEntry("datos.txt"); // Nombre del archivo dentro del ZIP
                    zipStream.PutNextEntry(entry);
                    zipStream.Write(buffer, 0, buffer.Length);
                    zipStream.CloseEntry();
                }

                // Mostrar mensaje en el hilo principal después de completar
                Invoke(new Action(() => MessageBox.Show($"Archivo comprimido guardado: {zipFilePath}")));
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al guardar el archivo: {ex.Message}");
        }
    }

    private string ReadResponse()
    {
        StringBuilder response = new StringBuilder();
        DateTime timeout = DateTime.Now.AddMilliseconds(2000);

        while (DateTime.Now < timeout)
        {
            try
            {
                response.Append(serialPort.ReadExisting());
                if (response.ToString().Contains("END_OF_DATA") || response.ToString().Contains("DELETE_SUCCESS"))
                    break;
            }
            catch { }
        }
        return response.ToString().Trim();
    }

    public static void Main()
    {
        Application.Run(new MainForm());
    }
}

public static class PromptDialog
{
    public static string ShowDialog(string text, string caption)
    {
        Form prompt = new Form
        {
            Width = 400,
            Height = 150,
            Text = caption
        };
        Label textLabel = new Label { Left = 20, Top = 20, Text = text };
        TextBox textBox = new TextBox { Left = 20, Top = 50, Width = 340 };
        Button confirmation = new Button { Text = "OK", Left = 280, Width = 80, Top = 80 };
        confirmation.Click += (sender, e) => { prompt.Close(); };
        prompt.Controls.Add(textLabel);
        prompt.Controls.Add(textBox);
        prompt.Controls.Add(confirmation);
        prompt.ShowDialog();
        return textBox.Text;
    }
}
