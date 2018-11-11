namespace EvoMod2
{
	partial class SettingsForm
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
			this.startButton = new System.Windows.Forms.Button();
			this.natResourcesDataGrid = new System.Windows.Forms.DataGridView();
			this.Volume = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Color = new System.Windows.Forms.DataGridViewComboBoxColumn();
			this.NodeCount = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.IsFood = new System.Windows.Forms.DataGridViewCheckBoxColumn();
			((System.ComponentModel.ISupportInitialize)(this.natResourcesDataGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// startButton
			// 
			this.startButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.startButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.startButton.Location = new System.Drawing.Point(12, 415);
			this.startButton.Name = "startButton";
			this.startButton.Size = new System.Drawing.Size(75, 23);
			this.startButton.TabIndex = 0;
			this.startButton.Text = "Start";
			this.startButton.UseVisualStyleBackColor = true;
			// 
			// natResourcesDataGrid
			// 
			this.natResourcesDataGrid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
			this.natResourcesDataGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.natResourcesDataGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Volume,
            this.Color,
            this.NodeCount,
            this.IsFood});
			this.natResourcesDataGrid.Location = new System.Drawing.Point(13, 13);
			this.natResourcesDataGrid.Name = "natResourcesDataGrid";
			this.natResourcesDataGrid.RowHeadersVisible = false;
			this.natResourcesDataGrid.Size = new System.Drawing.Size(352, 150);
			this.natResourcesDataGrid.TabIndex = 1;
			// 
			// Volume
			// 
			this.Volume.HeaderText = "Volume";
			this.Volume.Name = "Volume";
			// 
			// Color
			// 
			this.Color.HeaderText = "Color";
			this.Color.Name = "Color";
			this.Color.Resizable = System.Windows.Forms.DataGridViewTriState.True;
			this.Color.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
			// 
			// NodeCount
			// 
			this.NodeCount.HeaderText = "Node Count";
			this.NodeCount.Name = "NodeCount";
			// 
			// IsFood
			// 
			this.IsFood.FalseValue = "false";
			this.IsFood.HeaderText = "Is Food";
			this.IsFood.IndeterminateValue = "false";
			this.IsFood.Name = "IsFood";
			this.IsFood.Resizable = System.Windows.Forms.DataGridViewTriState.True;
			this.IsFood.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
			this.IsFood.TrueValue = "true";
			// 
			// SettingsForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(800, 450);
			this.Controls.Add(this.natResourcesDataGrid);
			this.Controls.Add(this.startButton);
			this.Name = "SettingsForm";
			this.Text = "SettingsForm";
			((System.ComponentModel.ISupportInitialize)(this.natResourcesDataGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button startButton;
		private System.Windows.Forms.DataGridView natResourcesDataGrid;
		private System.Windows.Forms.DataGridViewTextBoxColumn Volume;
		private System.Windows.Forms.DataGridViewComboBoxColumn Color;
		private System.Windows.Forms.DataGridViewTextBoxColumn NodeCount;
		private System.Windows.Forms.DataGridViewCheckBoxColumn IsFood;
	}
}