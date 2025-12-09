using System;
using System.Windows.Forms;
using System.Drawing;

namespace RevitAiEstimator
{
    public class EstimationOptionsForm : Form
    {
        public string ProjectContext { get; private set; }

        private TextBox txtContext;
        private Button btnOk;
        private Button btnCancel;

        public EstimationOptionsForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Smart Estimation Options";
            this.Size = new Size(400, 300);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            Label lblInstruction = new Label();
            lblInstruction.Text = "Enter Project Context (e.g., Location, specific cost data year, special requirements):";
            lblInstruction.Location = new Point(10, 10);
            lblInstruction.Size = new Size(360, 40);
            this.Controls.Add(lblInstruction);

            txtContext = new TextBox();
            txtContext.Multiline = true;
            txtContext.Location = new Point(10, 50);
            txtContext.Size = new Size(360, 150);
            txtContext.Text = "Project located in US, using RSMeans 2024 data."; // Default text
            this.Controls.Add(txtContext);

            btnOk = new Button();
            btnOk.Text = "Start Estimate";
            btnOk.DialogResult = DialogResult.OK;
            btnOk.Location = new Point(190, 210);
            btnOk.Size = new Size(100, 30);
            btnOk.Click += (s, e) => { ProjectContext = txtContext.Text; };
            this.Controls.Add(btnOk);

            btnCancel = new Button();
            btnCancel.Text = "Cancel";
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.Location = new Point(300, 210);
            btnCancel.Size = new Size(70, 30);
            this.Controls.Add(btnCancel);

            this.AcceptButton = btnOk;
            this.CancelButton = btnCancel;
        }
    }
}
