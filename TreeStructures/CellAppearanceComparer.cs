using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TreeStructures
{
        class CellAppearanceComparer : IComparer<ElementInstanceNode>
        {
            public int Compare(ElementInstanceNode n1, ElementInstanceNode n2)
            {
                throw new NotImplementedException();
                //List<CellNode> ch1 = n1.Cells;
                //List<CellNode> ch2 = n2.Cells;

                //if (ch1 == null)
                //{
                //    if (ch2 == null)
                //        return 0;
                //    else
                //        return -1;
                //}
                //else
                //{
                //    if (ch2 == null)
                //        return 1;
                //    else
                //    {
                //        int count1 = ch1.Count;
                //        int count2 = ch2.Count;

                //        return count1.CompareTo(count2);
                //    }
                //}
            }
        }
}
