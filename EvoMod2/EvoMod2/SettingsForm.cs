using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EvoMod2
{
	public partial class SettingsForm : Form
	{
		public DataGridView NaturalResourcesDataGridView { get => natResourcesDataGrid; }

		public SettingsForm()
		{
			InitializeComponent();
			natResourcesDataGrid.Rows[0].Cells[1] = new DataGridViewColorDropDownCell();
			natResourcesDataGrid.UserAddedRow += NatResourcesDataGrid_UserAddedRow;
		}


		private void NatResourcesDataGrid_UserAddedRow(object sender, DataGridViewRowEventArgs e)
		{
			e.Row.Cells[1] = new DataGridViewColorDropDownCell();
			e.Row.Cells[3].Value = false;
		}

		public class DataGridViewColorDropDownCell : DataGridViewComboBoxCell
		{
			public DataGridViewColorDropDownCell() : base()
			{
				this.DataSource = Enum.GetValues(typeof(KnownColor))
					.Cast<KnownColor>()
					.Where(c => !System.Drawing.Color.FromKnownColor(c).IsSystemColor)
					.Select(kc => Enum.GetName(typeof(KnownColor), kc))
					.ToList();
			}
		}
	}
}
