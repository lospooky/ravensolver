using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Controls;
using TreeStructures;
using System.Collections.Specialized;
using Utilities;

namespace Encoder
{
    public class RavenEncoder
    {
        Canvas c;

        #region Public Methods
        public bool LoadFile(string filepath, bool isPreview) {
            StreamReader fr = new StreamReader(filepath);
            try {
                string input = fr.ReadToEnd();
                fr.Close();
                fr.Dispose();
                c = (Canvas)XamlReader.Parse(input);
                Logging.logInfo("File " + Path.GetFileName(filepath)+" loaded correctly");
                return true;
            }
            catch (IOException) {
                Logging.logError("Input File Read Error");
                fr.Close();
                fr.Dispose();
                return false;
            }
        }

        public Canvas ProblemGrid {
            get {
                if (c != null) {
                    return (Canvas)c.FindName(Keywords.Problem);
                }
                else {
                    return null;
                }
            }
        }

        public Canvas CandidateGrid {
            get {
                if (c != null) {
                    return (Canvas)c.FindName(Keywords.Candidates);
                }
                else {
                    return null;
                }
            }
        }
        #endregion
    }
}
