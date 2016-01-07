using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using Microsoft.Windows.Controls;
using System.Windows.Controls;

using System.ComponentModel;


namespace TDSClient
{
    class DataGridWPFUtility
    {
        public static void DataGridGotoLast(DataGrid dtGrid)
        {
            try
            {
                int index = dtGrid.Items.Count - 1;
                if (index >= 0)
                {
                    dtGrid.SelectedItem = dtGrid.Items[index];
                    dtGrid.UpdateLayout();
                    dtGrid.ScrollIntoView(dtGrid.SelectedItem);
                }
            }
            catch
            {
            }
        }

        public static void DataGridGotoByIndex(DataGrid dtGrid, int index)
        {
            try
            {
                if (index < 0 || index > (dtGrid.Items.Count - 1)) return;               

                dtGrid.SelectedItem = dtGrid.Items[index];
                dtGrid.UpdateLayout();
                dtGrid.ScrollIntoView(dtGrid.SelectedItem);
            }
            catch
            {
            }
        }

        public static bool isDataGridHasCellValidationError(DataGrid dtGrid)
        {
            bool IsError = false;
            try
            {
                System.Reflection.PropertyInfo P = null;
                P = dtGrid.GetType().GetProperty("HasCellValidationError",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (P != null)
                {
                    IsError = (bool)P.GetValue(dtGrid, null);
                }              
            }
            catch
            {
            }
            return IsError;
        }
    }
}
