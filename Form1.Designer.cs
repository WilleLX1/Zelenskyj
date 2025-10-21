namespace Zelenskyj
{
    partial class Form1
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            ZelenskyjBox = new PictureBox();
            ((System.ComponentModel.ISupportInitialize)ZelenskyjBox).BeginInit();
            SuspendLayout();
            // 
            // ZelenskyjBox
            // 
            ZelenskyjBox.BackColor = Color.Transparent;
            ZelenskyjBox.Image = (Image)resources.GetObject("ZelenskyjBox.Image");
            ZelenskyjBox.InitialImage = null;
            ZelenskyjBox.Location = new Point(73, 33);
            ZelenskyjBox.Name = "ZelenskyjBox";
            ZelenskyjBox.Size = new Size(284, 332);
            ZelenskyjBox.TabIndex = 0;
            ZelenskyjBox.TabStop = false;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoSize = true;
            ClientSize = new Size(427, 412);
            Controls.Add(ZelenskyjBox);
            Name = "Form1";
            StartPosition = FormStartPosition.Manual;
            Text = "Zelenskyj";
            Load += Form1_Load;
            ((System.ComponentModel.ISupportInitialize)ZelenskyjBox).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private PictureBox ZelenskyjBox;
    }
}
