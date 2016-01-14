using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

using TerrainService;
using TDSClient.SAGInterface;

namespace TDSClient
{
    public delegate void NotifyEndDrawPolygonEvent(object sender, DrawPolygonEventArgs args);
    public delegate void NotifyEndAtomObjectsEditEvent(object sender, AtomObjectsEditEventArgs args);

    public delegate void NotifyAtomDeployedEvent(object sender, AtomDeployedEventArgs args);

    public delegate void NotifyActivityDTOEditEvent(object sender, ActivityDTOEditEventArgs args);

    public class DrawPolygonEventArgs : System.EventArgs
    {
        public DPoint[] PolygonPnts;      
        public bool isCancel;
        public bool isNew;
        public string PolygonName;
        public enumPolygonType PolygonType;
        public enumLineStyle LineStyle;
        public System.Windows.Media.DashStyle LineWPFDashStyle;

        public DateTime ActivityTimeFrom;
        public DateTime ActivityTimeTo;      

        public string PolygonLayerName = string.Empty;    


        public string PatternColor;
        public string PatternName;

        public string LinePatternName;
        public bool isLinePatternClockwise;
        public bool isColoredLineShape;

    }
    public class AtomObjectsEditEventArgs : System.EventArgs
    {      
        public bool isCancel;
        public bool isNew;
        public FormationTree atomDTO;
    }
    public class ActivityDTOEditEventArgs : System.EventArgs
    {
        public bool isCancel;
        public bool isNew;
        public GeneralActivityDTO ActivityDTO;
    }
      



    public class AtomDeployedEventArgs : System.EventArgs
    {
        public AtomData atom;
    }

    public struct DPoint
    {
        public double X;     // X (or longitude) coordinate
        public double Y;     // Y (or latitude) coordinate
        public DPoint(double X_, double Y_)
        {
            X = X_;
            Y = Y_;
        }
    } 
    public class MapMouseEventArgsWPF : System.EventArgs
    {
        public object SelectObject;
        public double X;
        public double Y;
        public double MapXLongLatWGS84;
        public double MapYLongLatWGS84;
        public float MapHeight;
        public double XMainWindow;
        public double YMainWindow;

    }
    public class WMSProviderSelectedMaps : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private bool _isSelected;
        private int _UserMinZoom;
        private int _UserMaxZoom;

        private string _mapName;


        public int SeqNumber;




        public string MapName
        {
            get
            {

                return _mapName;
            }
            set
            {
                _mapName = value;
                NotifyPropertyChanged("MapName");
            }
        }


        public bool isSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                NotifyPropertyChanged("isSelected");
            }
        }


        public int UserMinZoom
        {
            get { return _UserMinZoom; }
            set
            {
                _UserMinZoom = value;

                if (_UserMinZoom < 0) _UserMinZoom = 0;
                if (_UserMinZoom > _UserMaxZoom) _UserMinZoom = _UserMaxZoom;

                NotifyPropertyChanged("UserMinZoom");
            }

        }

        public int UserMaxZoom
        {
            get { return _UserMaxZoom; }
            set
            {
                _UserMaxZoom = value;

                if (_UserMaxZoom > 24) _UserMaxZoom = 24;

                if (_UserMaxZoom < _UserMinZoom) _UserMaxZoom = _UserMinZoom;

                NotifyPropertyChanged("UserMaxZoom");
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
    class clsGeneralDataType
    {
    }
}
