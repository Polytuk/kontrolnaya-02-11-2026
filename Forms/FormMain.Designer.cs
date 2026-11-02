namespace BodagovKR1_.Forms
{
    partial class FormMain
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.tabControlMain = new System.Windows.Forms.TabControl();
            this.tabOOP = new System.Windows.Forms.TabPage();
            this.tabAnalytics = new System.Windows.Forms.TabPage();
            this.tabAggregates = new System.Windows.Forms.TabPage();
            this.tabPaginationEvents = new System.Windows.Forms.TabPage();
            this.tabControlMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControlMain
            // 
            this.tabControlMain.Controls.Add(this.tabOOP);
            this.tabControlMain.Controls.Add(this.tabAnalytics);
            this.tabControlMain.Controls.Add(this.tabAggregates);
            this.tabControlMain.Controls.Add(this.tabPaginationEvents);
            this.tabControlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControlMain.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.tabControlMain.Location = new System.Drawing.Point(0, 0);
            this.tabControlMain.Name = "tabControlMain";
            this.tabControlMain.SelectedIndex = 0;
            this.tabControlMain.Size = new System.Drawing.Size(1100, 720);
            this.tabControlMain.TabIndex = 0;
            // 
            // tabOOP
            // 
            this.tabOOP.Location = new System.Drawing.Point(4, 30);
            this.tabOOP.Name = "tabOOP";
            this.tabOOP.Padding = new System.Windows.Forms.Padding(10);
            this.tabOOP.Size = new System.Drawing.Size(1092, 686);
            this.tabOOP.TabIndex = 0;
            this.tabOOP.Text = "1. Реестр ТС и ООП-Модель";
            this.tabOOP.UseVisualStyleBackColor = true;
            // 
            // tabAnalytics
            // 
            this.tabAnalytics.Location = new System.Drawing.Point(4, 30);
            this.tabAnalytics.Name = "tabAnalytics";
            this.tabAnalytics.Padding = new System.Windows.Forms.Padding(10);
            this.tabAnalytics.Size = new System.Drawing.Size(1092, 686);
            this.tabAnalytics.TabIndex = 1;
            this.tabAnalytics.Text = "2. LINQ-Аналитика и DTO";
            this.tabAnalytics.UseVisualStyleBackColor = true;
            // 
            // tabAggregates
            // 
            this.tabAggregates.Location = new System.Drawing.Point(4, 30);
            this.tabAggregates.Name = "tabAggregates";
            this.tabAggregates.Padding = new System.Windows.Forms.Padding(10);
            this.tabAggregates.Size = new System.Drawing.Size(1092, 686);
            this.tabAggregates.TabIndex = 2;
            this.tabAggregates.Text = "3. Группировка, Агрегаты и Множества";
            this.tabAggregates.UseVisualStyleBackColor = true;
            // 
            // tabPaginationEvents
            // 
            this.tabPaginationEvents.Location = new System.Drawing.Point(4, 30);
            this.tabPaginationEvents.Name = "tabPaginationEvents";
            this.tabPaginationEvents.Padding = new System.Windows.Forms.Padding(10);
            this.tabPaginationEvents.Size = new System.Drawing.Size(1092, 686);
            this.tabPaginationEvents.TabIndex = 3;
            this.tabPaginationEvents.Text = "4. Постраничный просмотр и События";
            this.tabPaginationEvents.UseVisualStyleBackColor = true;
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1100, 720);
            this.Controls.Add(this.tabControlMain);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.MinimumSize = new System.Drawing.Size(1000, 680);
            this.Name = "FormMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "CorpFleet Analytics — Корпоративная система управления автопарком";
            this.tabControlMain.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        public System.Windows.Forms.TabControl tabControlMain;
        public System.Windows.Forms.TabPage tabOOP;
        public System.Windows.Forms.TabPage tabAnalytics;
        public System.Windows.Forms.TabPage tabAggregates;
        public System.Windows.Forms.TabPage tabPaginationEvents;
    }
}

