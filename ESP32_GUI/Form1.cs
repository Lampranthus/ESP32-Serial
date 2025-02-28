using System;
using System.IO.Ports;
using System.Windows.Forms;

namespace ESP32_GUI
{
    public partial class Form1 : Form
    {
        private SerialPort serialPort;

        public Form1()
        {
            InitializeComponent();
            serialPort = new SerialPort();
        }

        private void btnActualizarPuertos_Click(object sender, EventArgs e)
        {
            // Actualiza la lista de puertos COM
            cmbPorts.Items.Clear();
            cmbPorts.Items.AddRange(SerialPort.GetPortNames());
        }

        private void btnObtenerDatos_Click(object sender, EventArgs e)
        {
            // Lógica para obtener datos del ESP32 a través del puerto serial
            if (cmbPorts.SelectedItem == null)
            {
                MessageBox.Show("Por favor, selecciona un puerto COM.");
                return;
            }

            try
            {
                if (!serialPort.IsOpen)
                {
                    serialPort.PortName = cmbPorts.SelectedItem.ToString();
                    serialPort.BaudRate = 115200;  // Ajusta la tasa de baudios si es necesario
                    serialPort.Open();
                    MessageBox.Show("Puerto serial abierto correctamente.");

                    // Enviar un comando para solicitar los datos
                    serialPort.WriteLine("GET_DATOS");
                    // Espera para que el ESP32 responda
                    System.Threading.Thread.Sleep(500);

                    // Leer la respuesta
                    string response = serialPort.ReadLine();
                    if (response.Contains("Contraseña requerida"))
                    {
                        // Pedir contraseña al usuario
                        string password = Prompt.ShowDialog("Ingrese la contraseña:", "Contraseña");

                        // Enviar la contraseña al ESP32
                        serialPort.WriteLine(password);
                        // Espera para que el ESP32 procese la contraseña
                        System.Threading.Thread.Sleep(500);

                        // Leer la respuesta de autenticación
                        response = serialPort.ReadLine();
                        if (response.Contains("Contraseña correcta"))
                        {
                            MessageBox.Show("Contraseña correcta. Datos obtenidos.");
                            // Aquí puedes agregar más lógica para procesar los datos
                        }
                        else
                        {
                            MessageBox.Show("Contraseña incorrecta.");
                        }
                    }
                    else
                    {
                        MessageBox.Show($"Respuesta del ESP32: {response}");
                    }
                }
                else
                {
                    MessageBox.Show("El puerto serial ya está abierto.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al abrir el puerto: {ex.Message}");
            }
        }

        private void btnBorrarDatos_Click(object sender, EventArgs e)
        {
            // Lógica para borrar datos
            if (serialPort.IsOpen)
            {
                serialPort.WriteLine("DELETE_DATOS");
                // Espera para que el ESP32 procese la solicitud
                System.Threading.Thread.Sleep(500);

                string response = serialPort.ReadLine();
                MessageBox.Show($"Respuesta del ESP32: {response}");
            }
            else
            {
                MessageBox.Show("El puerto serial no está abierto.");
            }
        }

        private void btnSalir_Click(object sender, EventArgs e)
        {
            // Cerrar la aplicación
            this.Close();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Cargar los puertos COM al iniciar el formulario
            cmbPorts.Items.AddRange(SerialPort.GetPortNames());
        }
    }

    // Clase para mostrar un cuadro de entrada (InputBox) de forma personalizada
    public static class Prompt
    {
        public static string ShowDialog(string text, string caption)
        {
            Form prompt = new Form()
            {
                Width = 300,
                Height = 150,
                Text = caption,
                StartPosition = FormStartPosition.CenterScreen
            };

            Label textLabel = new Label() { Left = 20, Top = 20, Text = text };
            TextBox textBox = new TextBox() { Left = 20, Top = 50, Width = 240 };
            Button confirmation = new Button() { Text = "OK", Left = 160, Width = 100, Top = 70, DialogResult = DialogResult.OK };
            confirmation.Click += (sender, e) => { prompt.Close(); };
            prompt.Controls.Add(textLabel);
            prompt.Controls.Add(textBox);
            prompt.Controls.Add(confirmation);
            prompt.AcceptButton = confirmation;

            return prompt.ShowDialog() == DialogResult.OK ? textBox.Text : "";
        }
    }
}
