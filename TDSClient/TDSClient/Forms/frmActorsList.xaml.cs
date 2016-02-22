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
    /// Interaction logic for frmActorsList.xaml
    /// </summary>
    public partial class frmActorsList : Window
    {
        Cursor _allOpsCursor = null;
        private Point _startPoint;
        private bool _isDragging;

        public bool IsDragging
        {
            get { return _isDragging; }
            set {
                 _isDragging = value; 
                }
        }

        public frmActorsList()
        {
            InitializeComponent();
            this.Background = new LinearGradientBrush(Colors.AliceBlue, Colors.LightGray, 90);
            SetGrid();
            LoadData();
        }
        private void SetGrid()
        {
            dtGridActors.Style = null;
            dtGridActors.Columns.Clear();
            dtGridActors.MaxWidth = 400;
            dtGridActors.HorizontalAlignment = HorizontalAlignment.Left;
            dtGridActors.AutoGenerateColumns = false;
            dtGridActors.SelectionUnit = DataGridSelectionUnit.FullRow;
            dtGridActors.CanUserAddRows = false;
            dtGridActors.CanUserDeleteRows = false;
            dtGridActors.ColumnHeaderHeight = 20;
            dtGridActors.HeadersVisibility = DataGridHeadersVisibility.Column;
            dtGridActors.RowDetailsVisibilityMode = DataGridRowDetailsVisibilityMode.VisibleWhenSelected;
            dtGridActors.RowHeaderWidth = 0;

            DataGridTextColumn col1 = new DataGridTextColumn();
            col1.Header = Properties.Resources.strName;// (string)this.Resources["strHeaderName"];// "Name";
            col1.MinWidth = 200;
            col1.CanUserSort = true;
            col1.IsReadOnly = true;
            col1.Binding = new System.Windows.Data.Binding("Name");
            dtGridActors.Columns.Add(col1);


            DataGridCheckBoxColumn col2 = new DataGridCheckBoxColumn();
            col2.Header = "Deployed";
            col2.MinWidth = 240;
            col2.CanUserSort = true;
            col2.IsReadOnly = true;
            col2.Binding = new System.Windows.Data.Binding("isDeployed");
            dtGridActors.Columns.Add(col2);



            DataGridCheckBoxColumn col3 = new DataGridCheckBoxColumn();
            col3.Header = Properties.Resources.strActivites;
            col3.MinWidth = 240;
            col3.CanUserSort = true;
            col3.IsReadOnly = true;
            col3.Binding = new System.Windows.Data.Binding("isActivityes");
            dtGridActors.Columns.Add(col3);


        }
        private async void LoadData()
        {
            List<AtomDTOData> DataList = new List<AtomDTOData>();
            try
            {
                IEnumerable<FormationTree> Atoms = await SAGSignalR.GetAllAtomsFromTree(VMMainViewModel.Instance.SimulationHubProxy);

                if (Atoms!=null)
                {
                    foreach (FormationTree atom in Atoms)
                    {
                        AtomDTOData Rdata = new AtomDTOData();
                        Rdata.atom = atom;
                        DataList.Add(Rdata);
                    }

                }


                dtGridActors.ItemsSource = null;
                dtGridActors.ItemsSource = DataList;

                if(dtGridActors.ItemsSource==null)
                {
                    dtGridActors.ItemsSource = new List<AtomDTOData>();
                }

            }
            catch (Exception ex)
            {

            }

        }
        private void cmdNew_Click(object sender, RoutedEventArgs e)
        {
            frmActorEdit frm = null;
            foreach (Window win in Application.Current.Windows)
            {
                if (win is frmActorEdit)
                {
                    win.Activate();
                    return;
                }
            }
            frm = new frmActorEdit(null);     

            frm.EndAtomObjectsEditEvent += frm_EndAtomObjectsEditEvent;
            frm.Owner = this;    
            frm.Show();
        }

        public async void  frm_EndAtomObjectsEditEvent(object sender, AtomObjectsEditEventArgs args)
        {

           FormationTree atomDTO=  await SAGSignalR.SaveTreeObject(VMMainViewModel.Instance.SimulationHubProxy, args.atomDTO);
           if (args.isNew)
           {
               AtomDTOData Rdata = new AtomDTOData();
               Rdata.atom = atomDTO;

               ((List<AtomDTOData>)(dtGridActors.ItemsSource)).Add(Rdata);

               dtGridActors.Items.Refresh();
               dtGridActors.SelectedItem = Rdata;
               dtGridActors.CurrentItem = Rdata;
               DataGridWPFUtility.DataGridGotoLast(dtGridActors);
           }
        }

        private async void cmdDelete_Click(object sender, RoutedEventArgs e)
        {
            int i = dtGridActors.SelectedIndex;
            if (i < 0) return;
            MessageBoxResult res = MessageBox.Show(Properties.Resources.strAreyousureyouwanttodeleteit, String.Empty, MessageBoxButton.YesNo);
            if (res == MessageBoxResult.No) return;

            AtomDTOData currAtomData = (AtomDTOData)dtGridActors.Items[i];

            if (currAtomData.atom.isActivityes)
            {
                MessageBoxResult res2 = MessageBox.Show("All Activities will be deleted!", String.Empty, MessageBoxButton.YesNo);
                if (res2 == MessageBoxResult.No) return;
            }


           // GameManagerClientObl.AddUpdateRoute(currRouteData.Route, String.Empty);

            await TDSClient.SAGInterface.SAGSignalR.DeleteAtomFromTreeByGuid(VMMainViewModel.Instance.SimulationHubProxy, currAtomData.atom.GUID);

            ((List<AtomDTOData>)(dtGridActors.ItemsSource)).Remove(currAtomData);
            dtGridActors.Items.Refresh();
            dtGridActors.Items.MoveCurrentToNext();

        }

        private void cmdExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void dtGridActors_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _startPoint = e.GetPosition(null);

         
        }

        private void dtGridActors_GiveFeedback(object sender, GiveFeedbackEventArgs e)
        {
           // TreeViewItem nd = (TreeViewItem)((TreeView)sender).SelectedItem;
           // StackPanel st = (StackPanel)nd.Header;
            Image im = null;
            if (_allOpsCursor == null)
            {

              //  TextBlock lbl = st.Children[1] as TextBlock;
               // if (lbl != null)  //  Unit font
                {

                    Label lblDrag = new Label();
                    lblDrag.Content = new string((char)150, 1);// lbl.Text;//  .Content;
                  //  lblDrag.Content = new string((char)86, 1);// lbl.Text;//  .Content;
                    lblDrag.Foreground = Brushes.Red; //lbl.Foreground;
                    lblDrag.FontFamily = new System.Windows.Media.FontFamily("Wingdings 2");//"Wingdings 2";// UserSession.KingsGameFontFamily;// new FontFamily("Simulation Font Environmental");
                    lblDrag.FontSize =  40;// lbl.FontSize;
                    double fs = 20;// lbl.FontSize;
                    lblDrag.Height = 40;// lbl.Height;
                    lblDrag.Width = 40;// lbl.Width;

                    lblDrag.FontWeight = System.Windows.FontWeights.Bold;

                    //new
                   // lblDrag.Foreground = System.Windows.Media.Brushes.Black;

                    try
                    {
                        _allOpsCursor = CursorHelper.CreateCursor(lblDrag, (int)8, (int)8, false);
                        Mouse.SetCursor(_allOpsCursor);
                    }
                    catch (Exception ex)
                    {
                    }


                }




            }

            e.UseDefaultCursors = false;
            e.Handled = true;
        }

        private void dtGridActors_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)// && !IsDragging)
            {

                 int i = dtGridActors.SelectedIndex;
                 if (i < 0) return;
                 AtomDTOData currAtomData = (AtomDTOData)dtGridActors.Items[i];
                 Point position = e.GetPosition(null);

                 if (Math.Abs(position.X - _startPoint.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(position.Y - _startPoint.Y) > SystemParameters.MinimumVerticalDragDistance)
                 {
                     _allOpsCursor = null;
                     GiveFeedbackEventHandler handler = new GiveFeedbackEventHandler(dtGridActors_GiveFeedback);
                     this.dtGridActors.GiveFeedback += handler;

                     IsDragging = true;
                     try
                     {
                         DataObject data = new DataObject(typeof(FormationTree), currAtomData.atom);
                         data.SetText("Actor");
                         DragDropEffects de = DragDrop.DoDragDrop(dtGridActors, data, DragDropEffects.Move);
                     }
                     catch (Exception ex)
                     {
                         //this.treeViewForce.GiveFeedback -= handler;
                         //this.treeViewForce.GiveFeedback += handler;
                     }
                     this.dtGridActors.GiveFeedback -= handler;
                     IsDragging = false;
                 }
            }
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            VMMainViewModel.Instance.MyMainMap.AtomDeployedEvent -= MyMainMap_AtomDeployedEvent;
        }

        void MyMainMap_AtomDeployedEvent(object sender, AtomDeployedEventArgs args)
        {
            List<AtomDTOData> atoms = (List<AtomDTOData>)(dtGridActors.ItemsSource);
            foreach(AtomDTOData atom in atoms)
            {
                if(atom.atom.GUID== args.atom.UnitGuid)
                {
                    atom.atom.isDeployed = true;

                }
            }
            dtGridActors.Items.Refresh();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            VMMainViewModel.Instance.MyMainMap.AtomDeployedEvent += MyMainMap_AtomDeployedEvent;
        }

        private void cmdShowMe_Click(object sender, RoutedEventArgs e)
        {
            int i = dtGridActors.SelectedIndex;
            if (i < 0) return;
          

            AtomDTOData currAtomData = (AtomDTOData)dtGridActors.Items[i];

            frmSearchAtom frm = new frmSearchAtom(currAtomData.atom.GUID);

         
            frm.Owner = this;
            frm.Show();
        }

        private void cmdActivities_Click(object sender, RoutedEventArgs e)
        {
            int i = dtGridActors.SelectedIndex;
            if (i < 0) return;
            AtomDTOData currAtomData = (AtomDTOData)dtGridActors.Items[i];

            structTransportCommonProperty Tr = null;
            VMMainViewModel.Instance.colGroundAtoms.TryGetValue(currAtomData.atom.Identification, out Tr);
            if (Tr != null)
            {
                TDSClient.Forms.frmActivities frm = new Forms.frmActivities(Tr);
                frm.Owner = this;
                frm.Show();
            }
        }
    }

    class AtomDTOData : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public FormationTree atom;
     //   private bool deployed;


        public string Name
        {
            get { return atom.Identification; }
            set { }
        }
        public bool isDeployed
        {
            get { return atom.isDeployed; }
            set {
                   atom.isDeployed = value;
                   NotifyPropertyChanged("isDeployed");
                }
        }

        public bool isActivityes
        {
            get { return atom.isActivityes; }
            set
            {
                atom.isActivityes = value;
                NotifyPropertyChanged("isActivityes");
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
