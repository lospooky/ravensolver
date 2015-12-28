using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace TreeStructures
{
    public interface IVisualNode
    {
        void DrawIn(Canvas canvas, double xTranslation, double yTranslation);
        void DrawIn(Canvas canvas);
    }
}
