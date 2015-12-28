using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace TreeStructures
{
    public abstract class InstanceNode : Node, IVisualNode
    {
        
        public abstract void DrawIn(Canvas canvas, double xTranslation, double yTranslation);
        public void DrawIn(Canvas canvas) { DrawIn(canvas, 0, 0); }

        #region Public methods

        #endregion

        #region Properties

        #endregion
    }
}
