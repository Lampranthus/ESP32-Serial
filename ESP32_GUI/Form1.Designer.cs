namespace ESP32_GUI
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.ComboBox cmbPorts;
        private System.Windows.Forms.Button btnActualizarPuertos;
        private System.Windows.Forms.Button btnObtenerDatos;
        private System.Windows.Forms.Button btnBorrarDatos;
        private System.Windows.Forms.Button btnSalir;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.cmbPorts = new System.Windows.Forms.ComboBox();
            this.btnActualizarPuertos = new System.Windows.Forms.Button();
            this.btnObtenerDatos = new System.Windows.Forms.Button();
            this.btnBorrarDatos = new System.Windows.Forms.Button();
            this.btnSalir = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // cmbPorts
            // 
            this.cmbPorts.FormattingEnabled = true;
            this.cmbPorts.Location = new System.Drawing.Point(12, 12);
            this.cmbPorts.Name = "cmbPorts";
            this.cmbPorts.Size = new System.Drawing.Size(120, 21);
            this.cmbPorts.TabIndex = 0;
            // 
            // btnActualizarPuertos
            // 
            this.btnActualizarPuertos.Location = new System.Drawing.Point(138, 10);
            this.btnActualizarPuertos.Name = "btnActualizarPuertos";
            this.btnActualizarPuertos.Size = new System.Drawing.Size(75, 23);
            this.btnActualizarPuertos.TabIndex = 1;
            this.btnActualizarPuertos.Text = "Actualizar";
            this.btnActualizarPuertos.UseVisualStyleBackColor = true;
            this.btnActualizarPuertos.Click += new System.EventHandler(this.btnActualizarPuertos_Click);
            // 
            // btnObtenerDatos
            // 
            this.btnObtenerDatos.Location = new System.Drawing.Point(12, 39);
            this.btnObtenerDatos.Name = "btnObtenerDatos";
            this.btnObtenerDatos.Size = new System.Drawing.Size(120, 23);
            this.btnObtenerDatos.TabIndex = 2;
            this.btnObtenerDatos.Text = "Obtener Datos";
            this.btnObtenerDatos.UseVisualStyleBackColor = true;
            this.btnObtenerDatos.Click += new System.EventHandler(this.btnObtenerDatos_Click);
            // 
            // btnBorrarDatos
            // 
            this.btnBorrarDatos.Location = new System.Drawing.Point(138, 39);
            this.btnBorrarDatos.Name = "btnBorrarDatos";
            this.btnBorrarDatos.Size = new System.Drawing.Size(75, 23);
            this.btnBorrarDatos.TabIndex = 3;
            this.btnBorrarDatos.Text = "Borrar Datos";
            this.btnBorrarDatos.UseVisualStyleBackColor = true;
            this.btnBorrarDatos.Click += new System.EventHandler(this.btnBorrarDatos_Click);
            // 
            // btnSalir
            // 
            this.btnSalir.Location = new System.Drawing.Point(12, 68);
            this.btnSalir.Name = "btnSalir";
            this.btnSalir.Size = new System.Drawing.Size(120, 23);
            this.btnSalir.TabIndex = 4;
            this.btnSalir.Text = "Salir";
            this.btnSalir.UseVisualStyleBackColor = true;
            this.btnSalir.Click += new System.EventHandler(this.btnSalir_Click);
            // 
            // Form1
            // 
            this.ClientSize = new System.Drawing.Size(224, 101);
            this.Controls.Add(this.btnSalir);
            this.Controls.Add(this.btnBorrarDatos);
            this.Controls.Add(this.btnObtenerDatos);
            this.Controls.Add(this.btnActualizarPuertos);
            this.Controls.Add(this.cmbPorts);
            this.Name = "Form1";
            this.Text = "ESP32 Control";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
        }
    }
}
