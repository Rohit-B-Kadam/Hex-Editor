using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Hex_Editor
{
    /// <summary>
    /// Only Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private HexEditor hexEditObj;

        public MainWindow()
        {
            InitializeComponent();
            hexEditObj = new HexEditor();
            this.Loaded += (s, e) => DataContext = hexEditObj;
        }

        private void LoadExe(object sender, RoutedEventArgs e1)
        {
            try
            {
                //Load Exe
                if (hexEditObj.hcode == null)
                {
                    hexEditObj.LoadExeFile(name: txtExeName.Text);
                    hexEditObj.EnableExeNameTxt = false;
                }

                 if(!hexEditObj.isNumber(txtDataSize.Text))
                {
                    MessageBox.Show("Give only number");
                    return;
                }

                this.Dispatcher.Invoke(() =>
                {
                    hexEditObj.LoadData();
                }, DispatcherPriority.DataBind);

                hexEditObj.EnableResetBtn = true;

            }
            catch (Exception)
            {
                MessageBox.Show("SomeWrong happen plz try again");
                hexEditObj.SetDefaultLayout();
            }
        }
        
        private void SearchString(object sender, RoutedEventArgs e)
        {
            if (hexEditObj.highLightCellIndexes.Count != 0)
            {
                hexEditObj.HighLightCells(DGHexCode, Brushes.White);
                hexEditObj.highLightCellIndexes.Clear();
            }

            long result = hexEditObj.SearchTheString();

            if( result >= hexEditObj.Data.Count*16 )
            {
                hexEditObj.FindPosition = result + "but can't highlight display data is less";
                hexEditObj.highLightCellIndexes.Clear();
            }
            else if(result != -1 )
            {
                hexEditObj.HighLightCells(DGHexCode, Brushes.LightCoral);
            }

        }

        private void txtExeName_KeyUp(object sender, KeyEventArgs e)
        {
            // Enable the load buttom when user give .exe file
            if (txtExeName.Text.Contains(".exe"))
            {
                    btnLoadExe.IsEnabled = true;
                
            }
            else
            {
                btnLoadExe.IsEnabled = false;
            }
         }

        private void txtSearchStr_KeyUp(object sender, KeyEventArgs e)
        {
            // Enable search button when txtSearchStr is value is click
            if (txtSearchString.Text.Length != 0)
            {
                hexEditObj.EnableFindBtn = true;
            }
            else
                hexEditObj.EnableFindBtn = false;
        
        }

        private void Conversion_Click(object sender, RoutedEventArgs e)
        {
            string cn = (sender as RadioButton).Name;
            switch (cn)
            {
                case "rbAscii":
                    hexEditObj.IsAsciiType = true;
                    break;
                case "rbHex":
                    hexEditObj.IsAsciiType = false;
                    break;
            }
        }

        private void ResetApp(object sender, RoutedEventArgs e)
        {
            if (hexEditObj.OffsetDisplay == hexEditObj.hcode.offset)
                hexEditObj.SetDefaultLayout();
            else
                MessageBox.Show(hexEditObj.OffsetDisplay+"Loading process is running. We can't reset"+ hexEditObj.hcode.offset, "Error");
        }
    }


    // Main logic 
    public class HexEditor : INotifyPropertyChanged
    {
        //Important
        private void Notify(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
        public event PropertyChangedEventHandler PropertyChanged;

        // Demostrating use of get and set proverty
        private ObservableCollection<DataItem> _data;
        public ObservableCollection<DataItem> Data
        {
            get { return _data; }
            set
            {
                _data = value;
                Notify("Data");
            }
        }

        private string _exeName;
        public string ExeName
        {
            get { return _exeName; }
            set
            {
                _exeName = value;
                Notify("ExeName");
            }
        }

        private int _dataSize;
        public int DataSize
        {
            get
            {
                return _dataSize;
            }
            set
            {
                if (isNumber(value.ToString()))
                {
                    _dataSize = value;
                }
                Notify("DataSize");
            }
        }

        private string _searchString;
        public string SearchString
        {
            get { return _searchString; }
            set
            {
                _searchString = value;
                Notify("SearchString");
            }
        }

        private string _findPosition;
        public string FindPosition
        {
            get { return _findPosition; }
            set
            {
                _findPosition = value;
                Notify("FindPosition");
            }
        }
        
        private bool _enableLoadBtn;
        public bool EnableLoadBtn
        {
            get { return _enableLoadBtn; }
            set
            {
                _enableLoadBtn = value;
                Notify("EnableLoadBtn");
            }
        }

        private bool _enableFindBtn;
        public bool EnableFindBtn
        {
            get { return _enableFindBtn; }
            set
            {
                _enableFindBtn = value;
                Notify("EnableFindBtn");
            }
        }

        private bool _enableSearch;
        public bool EnableSearch
        {
            get { return _enableSearch; }
            set
            {
                _enableSearch = value;
                Notify("EnableSearch");
            }
        }

        private bool _enableResetBtn;
        public bool EnableResetBtn
        {
            get { return _enableResetBtn; }
            set
            {
                _enableResetBtn = value;
                Notify("EnableResetBtn");
            }
        }
        
        public long offsetDisplay;
        public long OffsetDisplay
        {
            get { return offsetDisplay; }
            set
            {
                offsetDisplay = value;
                Notify("OffsetDisplay");
            }
        }

        private bool _enableExeNameTxt;
        public bool EnableExeNameTxt
        {
            get { return _enableExeNameTxt; }
            set
            {
                _enableExeNameTxt = value;
                Notify("EnableExeNameTxt");
            }
        }

        private bool _isAsciiType;
        public bool IsAsciiType
        {
            get { return _isAsciiType; }
            set {
                _isAsciiType = value;
                Notify("IsAsciiType"); }
        }

        public ExeReader hcode;

        public List<long> highLightCellIndexes;

        public Dispatcher thread;


        // method
        public HexEditor()
        {
            SetDefaultLayout();
        }

        // Set All default Value
        public void SetDefaultLayout()
        {
            hcode = null;
            ExeName = "";
            SearchString = "";
            FindPosition = "result";
            DataSize = 100;
            OffsetDisplay = 0;
            EnableExeNameTxt = true;
            EnableSearch = false;
            EnableLoadBtn = false;
            EnableFindBtn = false;
            EnableResetBtn = false;
            IsAsciiType = true;
            thread = Dispatcher.CurrentDispatcher;
            
            Data = new ObservableCollection<DataItem>();

            highLightCellIndexes = new List<long>();
        }

        public void LoadExeFile(string name)
        {
             hcode = new ExeReader(name); 
        }

        // Load data in datagrid
        public void LoadData()
        {
            // here I am loading( or adding) to dataGrid
            long pagesize = 1;
            
            Action act = () =>
            {
                foreach (var item in hcode.GetData(pagesize))
                {
                    _data.Add(item);
                }
            };

            long start = OffsetDisplay / 16;

            OffsetDisplay += DataSize * 16;
            if (OffsetDisplay > hcode.byteArr.Length)
                OffsetDisplay = hcode.byteArr.Length;

            long end = OffsetDisplay / 16;

            for (long i = start; i < end; i++)
            {
                thread.BeginInvoke(act, DispatcherPriority.ApplicationIdle);
            }
            EnableSearch = true;
            
        }

        public long SearchTheString()
        {
            long result= -1;
            byte[] bytes;
            if(SearchString.Equals(""))
            {
                MessageBox.Show("Plz give String to search");
                return -1;
            }

            // Converting the given string into byte[]
            if(IsAsciiType)
            {
                bytes = Encoding.ASCII.GetBytes(SearchString);
                result = hcode.FindString(bytes);
                
            }
            else
            {
                string hex = SearchString;
                // removing space and dash between the hexcode if use give
                hex = hex.Replace(" ", "");
                hex = hex.Replace("-", "");
                bytes = new byte[hex.Length / 2];
                for (int i = 0; i < bytes.Length; i++)
                {
                    bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
                }
                result = hcode.FindString(bytes);
            }

            if (result != -1)
            {
                for(long i = 0; i < bytes.Length; i++)
                {
                    highLightCellIndexes.Add(result + i);
                }
                FindPosition = result.ToString("X2");
            }
            else
            {
                FindPosition = "not found";
            }
            return result;
        }

        public void HighLightCells(DataGrid dataGrid , Brush color)
        {
            int i, j;
            foreach (var item in highLightCellIndexes)
            {
                i = Convert.ToInt32(item / 16);
                j = Convert.ToInt32(item % 16) + 1;   // because first row is offset

                DataGridRow row = dataGrid.ItemContainerGenerator.ContainerFromItem(dataGrid.Items[i]) as DataGridRow;
                DataGridCell cell = dataGrid.Columns[j].GetCellContent(row).Parent as DataGridCell;
                cell.Background = color;
            }
        }

        public bool isNumber(string numString)
        {
            int number = 0;
            bool isNum = int.TryParse(numString, out number);

            if (isNum)
                return true;
            else
                return false;
        }
    }
}
