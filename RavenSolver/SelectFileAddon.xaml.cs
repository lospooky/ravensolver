using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Interop;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Threading;
using WpfCustomFileDialog;
using Encoder;
using Utilities;

namespace RavenSolver
{

    public partial class SelectFileAddon : WpfCustomFileDialog.WindowAddOnBase
    {
        internal IntPtr _hFileDialogWrapperHandle = IntPtr.Zero;

        public SelectFileAddon()
        {
            InitializeComponent();
        }


        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
        }




        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ParentDlg.EventFileNameChanged += new PathChangedEventHandler(ParentDlg_EventFileNameChanged);
            ParentDlg.EventFolderNameChanged += new PathChangedEventHandler(ParentDlg_EventFolderNameChanged);
            ParentDlg.EventFilterChanged += new FilterChangedEventHandler(ParentDlg_EventFilterChanged);
        }



        void ParentDlg_EventFilterChanged(IFileDlgExt sender, int index)
        {

        }

        void ParentDlg_EventFolderNameChanged(IFileDlgExt sender, string filePath)
        {
        }
        string _filePath;
        void ParentDlg_EventFileNameChanged(IFileDlgExt sender, string filePath)
        {
            if (!string.IsNullOrEmpty(System.IO.Path.GetFileName(filePath)))
            {
                sender.FileDlgEnableOkBtn = true;
                using (System.IO.FileStream file = System.IO.File.OpenRead(filePath))
                {
                    //_fsize.Content = string.Format("{0:#,#} bytes", file.Length);
                    if (file.Length > 0)
                        _filePath = filePath;

                    RavenEncoder parser = new RavenEncoder();

                    parser.LoadFile(filePath, true);

                    //To be sent/retrieved from GUI or sth like that
                    Canvas p = parser.ProblemGrid;
                    if (p == null)
                        Logging.logError("Unable to preview file");
                    else
                    {
                        GuiCanvas.Content = (Utils.DeepCopy(p));
                        Logging.logInfo("File previewed");
                    }
                }
            }
            else
            {
            }

        }
    }
}
