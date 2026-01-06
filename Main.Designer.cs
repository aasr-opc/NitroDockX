namespace NitroDock
{
    partial class NitroDockMain
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NitroDockMain));
            NitroDockMain_OpacityPanel = new OpacityPanel();
            SuspendLayout();
            // 
            // NitroDockMain_OpacityPanel
            // 
            NitroDockMain_OpacityPanel.BackColor = Color.Transparent;
            NitroDockMain_OpacityPanel.Dock = DockStyle.Fill;
            NitroDockMain_OpacityPanel.Location = new Point(0, 0);
            NitroDockMain_OpacityPanel.Name = "NitroDockMain_OpacityPanel";
            NitroDockMain_OpacityPanel.Opacity = 0.5F;
            NitroDockMain_OpacityPanel.Size = new Size(110, 527);
            NitroDockMain_OpacityPanel.TabIndex = 0;
            // 
            // NitroDockMain
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(110, 527);
            Controls.Add(NitroDockMain_OpacityPanel);
            FormBorderStyle = FormBorderStyle.None;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "NitroDockMain";
            Opacity = 0.5D;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "NitroDockX";
            TopMost = true;
            ResumeLayout(false);
        }

        #endregion

        public OpacityPanel NitroDockMain_OpacityPanel;
    }
}
