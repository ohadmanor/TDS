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
using System.ComponentModel;
using TDSClient.SAGInterface;

namespace TDSClient.Forms
{
    /// <summary>
    /// Interaction logic for frmActivities.xaml
    /// </summary>
    public partial class frmActivities : Window
    {

        structTransportCommonProperty AtomCommonProperty = null;
        public frmActivities(structTransportCommonProperty CommonProperty)
        {
            InitializeComponent();
            this.Background = new LinearGradientBrush(Colors.AliceBlue, Colors.LightGray, 90);
            AtomCommonProperty = CommonProperty;
            Title =AtomCommonProperty.AtomName+" "+ Title;

            SetGrid();
            LoadData();
        }
        private void SetGrid()
        {
            dtGridActivities.Style = null;
            dtGridActivities.Columns.Clear();
            dtGridActivities.MaxWidth = 400;
            dtGridActivities.HorizontalAlignment = HorizontalAlignment.Left;
            dtGridActivities.AutoGenerateColumns = false;
            dtGridActivities.SelectionUnit = DataGridSelectionUnit.FullRow;
            dtGridActivities.CanUserAddRows = false;
            dtGridActivities.CanUserDeleteRows = false;
            dtGridActivities.ColumnHeaderHeight = 20;
            dtGridActivities.HeadersVisibility = DataGridHeadersVisibility.Column;
            dtGridActivities.RowDetailsVisibilityMode = DataGridRowDetailsVisibilityMode.VisibleWhenSelected;
            dtGridActivities.RowHeaderWidth = 0;


           

            DataGridTextColumn col1 = new DataGridTextColumn();
            col1.Header = "#";//Properties.Resources.strName;// (string)this.Resources["strHeaderName"];// "Name";
           // col1.MinWidth = 200;
            col1.CanUserSort = true;
            col1.IsReadOnly = true;
            col1.Binding = new System.Windows.Data.Binding("ActivityId");
            dtGridActivities.Columns.Add(col1);


            DataGridTextColumn col2 = new DataGridTextColumn();
            col2.Header = Properties.Resources.strStartActivity;
           // col1.MinWidth = 200;
            col2.CanUserSort = true;
            col2.IsReadOnly = true;
            col2.Binding = new System.Windows.Data.Binding("StartActivityOffset");
            dtGridActivities.Columns.Add(col2);

            DataGridTextColumn col3 = new DataGridTextColumn();
            col3.Header = Properties.Resources.strRouteName;
            // col1.MinWidth = 200;
            col3.CanUserSort = true;
            col3.IsReadOnly = true;
            col3.Binding = new System.Windows.Data.Binding("RouteName");
            dtGridActivities.Columns.Add(col3);

            DataGridTextColumn col4 = new DataGridTextColumn();
            col4.Header = Properties.Resources.strGridHeaderSpeedKmh;
            // col1.MinWidth = 200;
            col4.CanUserSort = true;
            col4.IsReadOnly = true;
            col4.Binding = new System.Windows.Data.Binding("Speed");
            dtGridActivities.Columns.Add(col4);


        }
        private async void LoadData()
        {
            List<ActivityDTONotify> DataList = new List<ActivityDTONotify>();
           
            try
            {
                IEnumerable<GeneralActivityDTO> activities = await SAGSignalR.GetActivitesByAtomName(VMMainViewModel.Instance.SimulationHubProxy, AtomCommonProperty.AtomName);
                if (activities!=null)
                {
                    foreach (GeneralActivityDTO activity in activities)
                    {
                        ActivityDTONotify Rdata = new ActivityDTONotify();
                        Rdata.ActivityDTO = activity;
                        DataList.Add(Rdata);
                    }
                }
              

                dtGridActivities.ItemsSource = null;
                dtGridActivities.ItemsSource = DataList;

                if (dtGridActivities.ItemsSource == null)
                {
                    dtGridActivities.ItemsSource = new List<ActivityDTONotify>();
                }
            }
            catch (Exception ex)
            {

            }

        }
        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            frmActivityEdit frm = new frmActivityEdit(AtomCommonProperty,null);

            frm.ActivityDTOEditEvent += frm_ActivityDTOEditEvent;

            frm.Owner = Application.Current.MainWindow;
            frm.Show();
        }
        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            int i = dtGridActivities.SelectedIndex;
            if (i < 0) return;
            ActivityDTONotify currActivityData = (ActivityDTONotify)dtGridActivities.Items[i];
            frmActivityEdit frm = new frmActivityEdit(AtomCommonProperty, currActivityData.ActivityDTO);
            frm.ActivityDTOEditEvent += frm_ActivityDTOEditEvent;

            frm.Owner = Application.Current.MainWindow;
            frm.Show();
        }
        private  async void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            int i = dtGridActivities.SelectedIndex;
            if (i < 0) return;
            MessageBoxResult res = MessageBox.Show(Properties.Resources.strAreyousureyouwanttodeleteit, String.Empty, MessageBoxButton.YesNo);
            if (res == MessageBoxResult.No) return;
            ActivityDTONotify currActivityData = (ActivityDTONotify)dtGridActivities.Items[i];

            await TDSClient.SAGInterface.SAGSignalR.DeleteActivityById(VMMainViewModel.Instance.SimulationHubProxy, currActivityData.ActivityId);

            ((List<ActivityDTONotify>)(dtGridActivities.ItemsSource)).Remove(currActivityData);
            dtGridActivities.Items.Refresh();
            dtGridActivities.Items.MoveCurrentToNext();

        }
        async  void  frm_ActivityDTOEditEvent(object sender, ActivityDTOEditEventArgs args)
        {

            

             if (args.isNew)
             {
                 int n = ((List<ActivityDTONotify>)(dtGridActivities.ItemsSource)).Count + 1;
                 args.ActivityDTO.Activity_SeqNumber = n;

                 GeneralActivityDTO activity = await SAGSignalR.SaveActivity(VMMainViewModel.Instance.SimulationHubProxy, args.ActivityDTO);

                 ActivityDTONotify Rdata = new ActivityDTONotify();
                 Rdata.ActivityDTO = activity;                
                 
                 ((List<ActivityDTONotify>)(dtGridActivities.ItemsSource)).Add(Rdata);

                 dtGridActivities.Items.Refresh();
                 dtGridActivities.SelectedItem = Rdata;
                 dtGridActivities.CurrentItem = Rdata;
                 DataGridWPFUtility.DataGridGotoLast(dtGridActivities);
             }
             else
             {
                 GeneralActivityDTO activity = await SAGSignalR.SaveActivity(VMMainViewModel.Instance.SimulationHubProxy, args.ActivityDTO);

                 if (dtGridActivities.ItemsSource == null) return;

                 List<ActivityDTONotify> listData = (List<ActivityDTONotify>)(dtGridActivities.ItemsSource);

               //  RouteData currRouteData = null;
                 for (int j = 0; j < listData.Count; j++)
                 {
                     if (listData[j].ActivityId == activity.ActivityId)
                     {
                        // currRouteData = listData[j];
                         listData[j].ActivityDTO = activity;
                         dtGridActivities.Items.Refresh();
                         break;
                     }
                 }

             }

            

        }

        

       
    }
    class ActivityDTONotify : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public GeneralActivityDTO ActivityDTO;       


        public int ActivityId
        {
            get { return ActivityDTO.ActivityId; }
            set { }
        }

        public TimeSpan StartActivityOffset
        {
            get { return ActivityDTO.StartActivityOffset; }
            set
            {
                ActivityDTO.StartActivityOffset= value;
                NotifyPropertyChanged("StartActivityOffset");
            }
        }

        public int Speed
        {
            get { return ActivityDTO.Speed; }
            set
            {
                ActivityDTO.Speed= value;
                NotifyPropertyChanged("Speed");
            }
        }


        public string RouteName
        {
            get { return ActivityDTO.RouteActivity.RouteName; }
            set
            {
               ActivityDTO.RouteActivity.RouteName= value;
                NotifyPropertyChanged("RouteName");
            }
        }


        public void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
    }
}
