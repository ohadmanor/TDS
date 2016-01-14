using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using TDSClient.SAGInterface;

namespace TDSClient.Forms
{
    /// <summary>
    /// Interaction logic for frmActorEdit.xaml
    /// </summary>
    public partial class frmActorEdit : Window
    {
        public event NotifyEndAtomObjectsEditEvent EndAtomObjectsEditEvent;

        FormationTree refAtomDTO = null;
        public bool isNew = false;

        public frmActorEdit(FormationTree atomDTO)
        {
            InitializeComponent();
            this.Background = new LinearGradientBrush(Colors.AliceBlue, Colors.LightGray, 90);
            refAtomDTO = atomDTO;
            if (refAtomDTO == null) isNew = true;
        }
      
        public async Task<bool> CheckOk()
        {
            if (txtName.Text == String.Empty)
            {
                System.Windows.MessageBox.Show(TDSClient.Properties.Resources.strPlatformNameisEmpty, String.Empty, MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            txtName.Text = Utilites.TreatApostrophe(txtName.Text);
            if (isNew)
            {
                bool isExist = await SAGSignalR.isAtomNameFromTreeExist(VMMainViewModel.Instance.SimulationHubProxy, txtName.Text);
                if (isExist)
                {
                    System.Windows.MessageBox.Show(TDSClient.Properties.Resources.strPlatformWithTheSamenameAlreadyExists, String.Empty, MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }
            }

            return true;
        }
        private async void cmdExit_Click(object sender, RoutedEventArgs e)
        {
            bool isCheckOk = await CheckOk();
            if (isCheckOk == false) return;
            if (isNew)
            {
                refAtomDTO = new FormationTree();
                refAtomDTO.Identification = txtName.Text;
                refAtomDTO.PlatformCategoryId = enumPlatformId.GeneralHumans;
                refAtomDTO.PlatformType =string.Empty;

                AtomObjectsEditEventArgs args = new AtomObjectsEditEventArgs();
                args.isNew = isNew;


                args.atomDTO = refAtomDTO;
                if (EndAtomObjectsEditEvent != null)
                {
                    EndAtomObjectsEditEvent(this, args);
                }

            }

            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
