using System;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using ICSharpCode.SharpZipLib.Zip; // Necesario para compresión con contraseña

public class MainForm : Form
{
    private ComboBox portSelector;
    private Button getDataButton, deleteDataButton;
    private SerialPort serialPort = new SerialPort();

    public MainForm()
    {
        Text = "ESP32 Data Manager";
        Width = 400;
        Height = 200;

        portSelector = new ComboBox { Left = 20, Top = 20, Width = 150 };
        getDataButton = new Button { Text = "Obtener Datos", Left = 200, Top = 20, Width = 150 };
        deleteDataButton = new Button { Text = "Borrar Datos", Left = 200, Top = 60, Width = 150 };

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
        serialPort = new SerialPort(portSelector.SelectedItem.ToString(), 115200);
        serialPort.Open();
        serialPort.WriteLine("GET_DATOS");
        Thread.Sleep(500);

        string response = serialPort.ReadExisting();
        if (response.Contains("PASSWORD_REQUEST"))
        {
            string password = Prompt.ShowDialog("Ingrese la contraseña:", "Contraseña");
            serialPort.WriteLine(password);
            Thread.Sleep(500);
            response = serialPort.ReadExisting();
            if (response.Contains("Acceso concedido"))
            {
                SaveData(response);
            }
        }
        serialPort.Close();
    }

    private void SaveData(string data)
    {
        SaveFileDialog saveFileDialog = new SaveFileDialog { Filter = "ZIP files (*.zip)|*.zip" };
        if (saveFileDialog.ShowDialog() == DialogResult.OK)
        {
            string zipPath = saveFileDialog.FileName;
            string tempFile = Path.GetTempFileName();
            File.WriteAllText(tempFile, data);

            string zipPassword = Prompt.ShowDialog("Ingrese la contraseña del ZIP:", "Contraseña ZIP");

            using (FileStream fsOut = File.Create(zipPath))
            using (ZipOutputStream zipStream = new ZipOutputStream(fsOut))
            {
                zipStream.SetLevel(9); // Nivel de compresión
                zipStream.Password = zipPassword;

                byte[] fileBytes = File.ReadAllBytes(tempFile);
                ZipEntry newEntry = new ZipEntry(Path.GetFileName(tempFile))
                {
                    DateTime = DateTime.Now,
                    Size = fileBytes.Length
                };

                zipStream.PutNextEntry(newEntry);
                zipStream.Write(fileBytes, 0, fileBytes.Length);
                zipStream.CloseEntry();
            }

            File.Delete(tempFile);
            MessageBox.Show("Archivo guardado correctamente.");
        }
    }

    private void DeleteData()
    {
        if (portSelector.SelectedItem == null) return;
        serialPort = new SerialPort(portSelector.SelectedItem.ToString(), 115200);
        serialPort.Open();
        serialPort.WriteLine("DELETE_DATOS");
        Thread.Sleep(500);

        string response = serialPort.ReadExisting();
        if (response.Contains("PASSWORD_REQUEST"))
        {
            string password = Prompt.ShowDialog("Ingrese la contraseña:", "Contraseña");
            serialPort.WriteLine(password);
            Thread.Sleep(500);
            response = serialPort.ReadExisting();
            MessageBox.Show(response.Contains("ARCHIVO ELIMINADO") ? "Datos eliminados" : "Acceso denegado");
        }
        serialPort.Close();
    }

    [STAThread]
    public static void Main()  // Mantén solo esta definición
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new MainForm());
    }
}

public static class Prompt
{
    public static string ShowDialog(string text, string caption)
    {
        Form prompt = new Form { Width = 300, Height = 150, Text = caption };
        TextBox inputBox = new TextBox { Left = 50, Top = 20, Width = 200 };
        Button confirmation = new Button { Text = "OK", Left = 110, Top = 50, Width = 80 };
        confirmation.Click += (sender, e) => { prompt.Close(); };
        prompt.Controls.Add(inputBox);
        prompt.Controls.Add(confirmation);
        prompt.ShowDialog();
        return inputBox.Text;
    }
}
