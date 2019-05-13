namespace LearnOpenTK.GLControl
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.glControl = new OpenTK.GLControl();
            this.trackBarX = new System.Windows.Forms.TrackBar();
            this.lblText01 = new System.Windows.Forms.Label();
            this.lblText00 = new System.Windows.Forms.Label();
            this.trackBarY = new System.Windows.Forms.TrackBar();
            this.lblText02 = new System.Windows.Forms.Label();
            this.colorDialog = new System.Windows.Forms.ColorDialog();
            this.buttonChangeColor = new System.Windows.Forms.Button();
            this.buttonRandomize = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarX)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarY)).BeginInit();
            this.SuspendLayout();
            // 
            // glControl
            // 
            this.glControl.Location = new System.Drawing.Point(-2, 0);
            this.glControl.Name = "glControl";
            this.glControl.Size = new System.Drawing.Size(630, 452);
            this.glControl.TabIndex = 0;
            this.glControl.VSync = false;
            this.glControl.Load += new System.EventHandler(this.GLControl_Load);
            this.glControl.Paint += new System.Windows.Forms.PaintEventHandler(this.GLControl_Paint);
            this.glControl.Resize += new System.EventHandler(this.GLControl_Resize);
            // 
            // trackBarX
            // 
            this.trackBarX.Location = new System.Drawing.Point(634, 120);
            this.trackBarX.Maximum = 360;
            this.trackBarX.Name = "trackBarX";
            this.trackBarX.Size = new System.Drawing.Size(154, 45);
            this.trackBarX.TabIndex = 1;
            this.trackBarX.TickFrequency = 45;
            this.trackBarX.Scroll += new System.EventHandler(this.TrackBarX_Scroll);
            // 
            // lblText01
            // 
            this.lblText01.Location = new System.Drawing.Point(634, 97);
            this.lblText01.Name = "lblText01";
            this.lblText01.Size = new System.Drawing.Size(154, 21);
            this.lblText01.TabIndex = 2;
            this.lblText01.Text = "X Axis";
            this.lblText01.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblText00
            // 
            this.lblText00.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.lblText00.Location = new System.Drawing.Point(634, 61);
            this.lblText00.Name = "lblText00";
            this.lblText00.Size = new System.Drawing.Size(154, 21);
            this.lblText00.TabIndex = 2;
            this.lblText00.Text = "Rotation";
            this.lblText00.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // trackBarY
            // 
            this.trackBarY.Location = new System.Drawing.Point(634, 220);
            this.trackBarY.Maximum = 360;
            this.trackBarY.Name = "trackBarY";
            this.trackBarY.Size = new System.Drawing.Size(154, 45);
            this.trackBarY.TabIndex = 1;
            this.trackBarY.TickFrequency = 45;
            this.trackBarY.Scroll += new System.EventHandler(this.TrackBarY_Scroll);
            // 
            // lblText02
            // 
            this.lblText02.Location = new System.Drawing.Point(634, 196);
            this.lblText02.Name = "lblText02";
            this.lblText02.Size = new System.Drawing.Size(154, 21);
            this.lblText02.TabIndex = 2;
            this.lblText02.Text = "Y Axis";
            this.lblText02.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // colorDialog
            // 
            this.colorDialog.Color = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.colorDialog.SolidColorOnly = true;
            // 
            // buttonChangeColor
            // 
            this.buttonChangeColor.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.buttonChangeColor.Location = new System.Drawing.Point(637, 386);
            this.buttonChangeColor.Name = "buttonChangeColor";
            this.buttonChangeColor.Size = new System.Drawing.Size(151, 52);
            this.buttonChangeColor.TabIndex = 3;
            this.buttonChangeColor.Text = "Change Background";
            this.buttonChangeColor.UseVisualStyleBackColor = true;
            this.buttonChangeColor.Click += new System.EventHandler(this.ButtonChangeColor_Click);
            // 
            // buttonRandomize
            // 
            this.buttonRandomize.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.buttonRandomize.Location = new System.Drawing.Point(637, 328);
            this.buttonRandomize.Name = "buttonRandomize";
            this.buttonRandomize.Size = new System.Drawing.Size(151, 52);
            this.buttonRandomize.TabIndex = 3;
            this.buttonRandomize.Text = "Randomize Colors";
            this.buttonRandomize.UseVisualStyleBackColor = true;
            this.buttonRandomize.Click += new System.EventHandler(this.ButtonRandomize_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.buttonRandomize);
            this.Controls.Add(this.buttonChangeColor);
            this.Controls.Add(this.lblText00);
            this.Controls.Add(this.lblText02);
            this.Controls.Add(this.lblText01);
            this.Controls.Add(this.trackBarY);
            this.Controls.Add(this.trackBarX);
            this.Controls.Add(this.glControl);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.ShowIcon = false;
            this.Text = "LearnOpenTK - GLControl";
            ((System.ComponentModel.ISupportInitialize)(this.trackBarX)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarY)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private OpenTK.GLControl glControl;
        private System.Windows.Forms.TrackBar trackBarX;
        private System.Windows.Forms.Label lblText01;
        private System.Windows.Forms.Label lblText00;
        private System.Windows.Forms.TrackBar trackBarY;
        private System.Windows.Forms.Label lblText02;
        private System.Windows.Forms.ColorDialog colorDialog;
        private System.Windows.Forms.Button buttonChangeColor;
        private System.Windows.Forms.Button buttonRandomize;
    }
}