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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Encoder;
using TreeStructures;
using Utilities;
using System.Windows.Threading;
using RavenTreeFunctions;
using WpfCustomFileDialog;
using System.IO;
using System.Diagnostics;

namespace RavenSolver
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    /// 

    public delegate void UpdateLabelDelegate(Label label, String message);


    public partial class Window1 : Window
    {
        private Canvas solutionCell;
        private String currentFileName = null;

        private String logResultsToFile = null;

        public Window1() {
            InitializeComponent();
            Logging.Instance.GuiLogMessage += new GuiLogMessageHandler(LogMessageHandler);
        }

        private void LogMessageHandler(object sender, String message) {
            TextboxLogger.Dispatcher.BeginInvoke(new UpdateLabelDelegate(UpdateLabel), DispatcherPriority.Normal, new object[]{TextboxLogger,message});
        }

        private void UpdateLabel(Label label, String labelContent) {
            label.Content += labelContent + "\n";
            ScrollViewerLogger.ScrollToBottom();
        }




        private void SetProblem(String fileName) {

            if (!File.Exists(fileName))
                return;
            ButtonRunAll.IsEnabled = true;
            currentFileName = fileName;

            TabMultiLogger.IsEnabled = false;

            lblActiveProblem.Content = "Active Problem: " + fileName.Substring(fileName.Length - 8, 3);

            RavenEncoder parser = new RavenEncoder();
            parser.LoadFile(fileName, false);


            Canvas p = parser.ProblemGrid;
            if (p == null)
                Logging.logError("Could not find problem grid");
            else
                Logging.logInfo("Problem grid found");

            Canvas c = parser.CandidateGrid;
            if (c == null)
                Logging.logError("Could not find candidates grid");
            else
                Logging.logInfo("Candidates grid found");


            Canvas guiProblemGrid = (Canvas)Utils.DeepCopy(p);

            GuiCanvas.Content = guiProblemGrid;
            Logging.logInfo("Problem Grid Sent To GUI");


            TabActiveProblem.IsEnabled = true;
            Tabs.SelectedItem = TabActiveProblem;

            zoomSlider.Visibility = Visibility.Visible;

            Dictionary<String, Dictionary<String, int>> functionsUsed = new Dictionary<string, Dictionary<string, int>>();

            Tree problemGrid = new Tree(p, Keywords.Problem);

            List<RavenFunction> functions = new List<RavenFunction>{
                new RFIdentity(problemGrid, RavenFunction.SearchDirection.HorizontalAndVertical, false),
                new RFDistributeionOfThree(problemGrid, RavenFunction.SearchDirection.HorizontalAndVertical, false),
                new RFNumericProgression(problemGrid, RavenFunction.SearchDirection.HorizontalAndVertical, false),
                new RFGlobalTranslation(problemGrid, RavenFunction.SearchDirection.HorizontalAndVertical, false),
//                new RFRowBasedTranslation(problemGrid, RavenFunction.SearchDirection.HorizontalAndVertical, false),
                new RFCellXOR(problemGrid, RavenFunction.SearchDirection.HorizontalAndVertical, false),
                new RFCellAND(problemGrid, RavenFunction.SearchDirection.HorizontalAndVertical, false),
//                new RFCellOR(problemGrid, RavenFunction.SearchDirection.HorizontalAndVertical, false),
            };

            List<Tree.ProcessableAttributes> processAttributes = new List<Tree.ProcessableAttributes> {
                Tree.ProcessableAttributes.ObjectsCount,
                Tree.ProcessableAttributes.CellObjects,
                Tree.ProcessableAttributes.AbsolutePositionInstances,
                Tree.ProcessableAttributes.ElementModels
            };
            RavenFunctionResult result = new RavenFunctionResult();
            Boolean processingDone = false;


            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            for (int i = 0; i < functions.Count; i++) {
                if (functions[i].processableAttributes.Contains(processAttributes[0]))
                    result = functions[i].RunAlgorithm(processAttributes[0]);
                if ((result.succeeded || result.elementsAdded != 0) || i==functions.Count-1) {
                    int startJ = i;
                    if (result.elementsAdded == 0) {
                        startJ = 0;
                        foreach (RavenFunction func in functions)
                            func.AddObjectsAtWill = true;
                    }
                    else {
                        functionsUsed.Add(processAttributes[0].ToString(), new Dictionary<string, int>());
                        functionsUsed[processAttributes[0].ToString()].Add(functions[i].GetType().Name, result.elementsAdded);
                    }
                    startJ = 0;
                    
                    for (int k = 1; k < processAttributes.Count; k++) {
                        for (int j = startJ; j < functions.Count; j++) {
                            result = new RavenFunctionResult();
                            if (functions[j].processableAttributes.Contains(processAttributes[k]))
                                result = functions[j].RunAlgorithm(processAttributes[k]);
                            if (result.succeeded || result.elementsAdded != 0) {
                                if (!functionsUsed.ContainsKey(processAttributes[k].ToString())) {
                                    functionsUsed.Add(processAttributes[k].ToString(), new Dictionary<string, int>());
                                }
                                if (!functionsUsed[processAttributes[k].ToString()].ContainsKey(functions[j].GetType().Name))
                                    functionsUsed[processAttributes[k].ToString()].Add(functions[j].GetType().Name, result.elementsAdded);
                                else
                                    functionsUsed[processAttributes[k].ToString()][functions[j].GetType().Name] += result.elementsAdded;
                            }
                            if ((result.succeeded || result.elementsAdded != 0) && processAttributes[k] == Tree.ProcessableAttributes.CellObjects) {
                                processingDone = true;
                                break;
                            }
                            else if (problemGrid.Cells[8].CellObjects.Count != 0) {
                                processingDone = true;
                                foreach (AbsoluteInstancePosition ap in problemGrid.Cells[8].CellObjects) {
                                    if (ap.Instance == null || (ap.Instance is ElementInstanceNode && ap.Instance.Attributes.Count == 0 && (((ElementInstanceNode)ap.Instance).Models == null || ((ElementInstanceNode)ap.Instance).Models.Count == 0))) {
                                        processingDone = false;
                                        break;
                                    }
                                }
                            }
                        }
                        if (processingDone == true)
                            break;
                    }
                    break;
                }
            }

            stopWatch.Stop();
            // Get the elapsed time as a TimeSpan value.
            TimeSpan ts = stopWatch.Elapsed;

            // Format and display the TimeSpan value.
            string elapsedTime = ts.TotalMilliseconds.ToString();


            if (logResultsToFile != null)
                File.AppendAllText(logResultsToFile + "t", elapsedTime + "\n", Encoding.ASCII);



            //for (int k = 1; k < processAttributes.Count; k++) {
            //    for (int j = 0; j < functions.Count; j++) {
            //        if (functions[j].processableAttributes.Contains(processAttributes[k]))
            //            result = functions[j].RunAlgorithm(processAttributes[k]);
            //    }
            //}


            Canvas fullVisualizationCanvas = new Canvas();
            VisualizeTree fullVisualization = new VisualizeTree(problemGrid, fullVisualizationCanvas);
            fullVisualization.Visualize(true, false);
            fullVisualizationCanvas.Name = "CanvasVisualization";
            VisualizationContent.Content = fullVisualizationCanvas;
            TabVisualization.IsEnabled = true;


            solutionCell = (Canvas)guiProblemGrid.FindName("SolutionCell");
            problemGrid.Cells[8].DrawIn(solutionCell);

            solutionCell.Visibility = (ChkSolutionVisible.IsChecked == true)? Visibility.Visible : Visibility.Hidden;


            if (logResultsToFile != null)
                File.AppendAllText(logResultsToFile, "\n\n\nProblem " + fileName.Substring(fileName.Length - 8, 3),Encoding.ASCII);

            SPNeededFunctions.Children.Clear();
            Logging.logInfo("------------------------------------------------------------\n\nOverview:");

            foreach (KeyValuePair<String, Dictionary<String, int>> kvp1 in functionsUsed) {
                if (logResultsToFile != null)
                    File.AppendAllText(logResultsToFile, "\n\n " + kvp1.Key + " solved using:", Encoding.ASCII);
                Logging.logInfo(kvp1.Key + " solved using:");
                Label l = new Label();
                l.Content = kvp1.Key + ":";
                l.Padding = new Thickness(0,10,0,0);
                SPNeededFunctions.Children.Add(l);
                    
                foreach (KeyValuePair<String, int> kvp2 in kvp1.Value) {
                    if (logResultsToFile != null)
                        File.AppendAllText(logResultsToFile, "\n   " + kvp2.Key + " : " + kvp2.Value + " connections", Encoding.ASCII);
                    Logging.logInfo(" " + kvp2.Key + " made " + kvp2.Value + " connections");
                    l = new Label();
                    l.Margin = new Thickness(0);
                    l.Padding = new Thickness(0);
                    l.Content = "   " + kvp2.Key + " : " + kvp2.Value;
                    SPNeededFunctions.Children.Add(l);
                }
            }

            Label l2 = new Label();
            l2.Content = "Processing completed, solution " + ((processingDone == false) ? "NOT " : "") + "generated in full.";
            l2.FontWeight = FontWeights.Bold;
            SPNeededFunctions.Children.Add(l2);
            Logging.logInfo("\nProcessing completed, solution " + ((processingDone == false) ? "NOT " : "") + "generated in full.");

            if (logResultsToFile != null)
                File.AppendAllText(logResultsToFile, "\n\nProcessing completed, solution " + ((processingDone == false) ? "NOT " : "") + "generated in full.", Encoding.ASCII);


            //Canvas visualizationSolution = VisualizeTree.Visualize(problemGrid, true, true);
            //visualizationSolution.Name = "CanvasVisualizationSolution";
            //ScrollViewerVisualizationSolution.Content = visualizationSolution;
            //TabVisualizationSolution.IsEnabled = true;

            //Canvas visualizationSolutionSimplified = VisualizeTree.Visualize(problemGrid, false, true);
            //visualizationSolutionSimplified.Name = "CanvasVisualizationSolutionSimplified";
            //ScrollViewerVisualizationSolutionSimplified.Content = visualizationSolutionSimplified;
            //TabVisualizationSolutionSimplified.IsEnabled = true;

            //solutionCell = (Canvas)guiProblemGrid.FindName("SolutionCell");
            //solutionCell.Visibility = Visibility.Hidden;
            //problemGrid.RenderSolutionCell(solutionCell);
            
           
        }


        private void ButtonNext_Click(object sender, RoutedEventArgs e) {
            if (currentFileName == null)
                return;
            String newName = currentFileName.Substring(0, currentFileName.Length - 7) +
                             (Int32.Parse(currentFileName.Substring(currentFileName.Length - 7, 2)) + 1).ToString("D2") +
                             currentFileName.Substring(currentFileName.Length - 5);
            if (!File.Exists(newName)) {
                String newSet = currentFileName.Substring(currentFileName.Length - 10, 1);
                if (newSet == "C") newSet = "D";
                else if (newSet == "D") newSet = "E"; 
                else return;

                newName = currentFileName.Substring(0, currentFileName.Length - 10) +
                                 newSet + @"\" + newSet + "01.xaml";
            }
            SetProblem(newName);
        }
        private void ButtonPrevious_Click(object sender, RoutedEventArgs e) {
            if (currentFileName == null)
                return;
            String newName = currentFileName.Substring(0, currentFileName.Length - 7) +
                             (Int32.Parse(currentFileName.Substring(currentFileName.Length - 7, 2)) - 1).ToString("D2") +
                             currentFileName.Substring(currentFileName.Length - 5);
            if (!File.Exists(newName)) {
                String newSet = currentFileName.Substring(currentFileName.Length - 10, 1);
                if (newSet == "E") newSet = "D";
                if (newSet == "D") newSet = "C";
                else return;

                newName = currentFileName.Substring(0, currentFileName.Length - 10) +
                                 newSet + @"\" + newSet + "12.xaml";
                if (!File.Exists(newName))
                    newName = currentFileName.Substring(0, currentFileName.Length - 10) +
                                     newSet + @"\" + newSet + "11.xaml";
            }
            SetProblem(newName);
        }

        private void ButtonParse_Click(object sender, RoutedEventArgs e) {
            SetProblem("Test.xaml");
        }


        private void ButtonChooseFile_Click(object sender, RoutedEventArgs e) {
            WpfCustomFileDialog.OpenFileDialog<SelectFileAddon> ofd = new WpfCustomFileDialog.OpenFileDialog<SelectFileAddon>();
            ofd.Filter = "XAML files (*.xaml)|*.xaml|All files (*.*)|*.*";
            ofd.Multiselect = false;
            ofd.Title = "Select the Raven matrix";
            ofd.FileDlgStartLocation = AddonWindowLocation.Right;
            ofd.FileDlgDefaultViewMode = NativeMethods.FolderViewMode.Tiles;
            ofd.FileDlgOkCaption = "&Select";
            bool? res = ofd.ShowDialog(this);

            if (res.Value == true) {
                txtInputFileName.Text = ofd.FileName;
                if (System.IO.File.Exists(txtInputFileName.Text)) {
                    SetProblem(txtInputFileName.Text);
                }
            }
        }

        private void ChkSolutionVisible_Checked(object sender, RoutedEventArgs e) {
            solutionCell.Visibility = Visibility.Visible;
        }

        private void ChkSolutionVisible_Unchecked(object sender, RoutedEventArgs e) {
            solutionCell.Visibility = Visibility.Hidden;
        }

        private void ButtonRunAll_Click(object sender, RoutedEventArgs e) {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "Log file"; // Default file name
            dlg.DefaultExt = ".txt"; // Default file extension
            dlg.Filter = "Text documents (.txt)|*.txt"; // Filter files by extension

            // Show save file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process save file dialog box results
            if (result == true) {
                // Save document
                this.logResultsToFile = dlg.FileName;

                File.WriteAllText(logResultsToFile, "Results for all SPM problems:\n\n",Encoding.ASCII);

                if (currentFileName == null)
                    return;


                String newName = currentFileName.Substring(0, currentFileName.Length - 10) + @"C\C01.xaml";
                SetProblem(newName);
                do {
                    newName = currentFileName;
                    ButtonNext_Click(null, null);
                } while (newName != currentFileName);


                TextboxMultiLogger.Content = File.ReadAllText(logResultsToFile, Encoding.ASCII);
                TabMultiLogger.IsEnabled = true;
                this.logResultsToFile = null;
            }

        }
    }
}
