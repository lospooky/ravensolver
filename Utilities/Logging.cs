using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Utilities {
    public delegate void GuiLogMessageHandler(object sender, String message);

    public class Logging
    {
        public event GuiLogMessageHandler GuiLogMessage;
        public static Logging Instance = new Logging();


        protected virtual void OnGuiLogMessage(String message)
        {
            if (GuiLogMessage != null)
                GuiLogMessage(this, message);
        }


        public static void logException(Exception ex, int level)
        {
            Exception inner;
            String exString = "L" + level + ": " + ex.Message + "\n\n" + ex.StackTrace;
            while((inner = ex.InnerException)!=null) {
                exString += "\n\nInner exception: " + inner.Message + "\n\n" + inner.StackTrace;
            }

            Instance.OnGuiLogMessage("Exception " + exString);
            System.Diagnostics.Debug.WriteLine("\nLogging exception:\n" + exString);
        }

        public static void logInfo(String information) {
            Instance.OnGuiLogMessage(information);
            System.Diagnostics.Debug.WriteLine("\nLogging information entry:\n" + information);
        }
        public static void logError(String error) {
            Instance.OnGuiLogMessage("Error: " + error);
            System.Diagnostics.Debug.WriteLine("\nLogging error entry:\n" + error);
        }
    }
}
