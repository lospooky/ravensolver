using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TreeStructures
{
    class ChildrenComparer<T> : IComparer<T> where T : Node
    {
        public int Compare(T n1, T n2)
        {
            List<Node> ch1 = n1.Children;
            List<Node> ch2 = n2.Children;

            if (ch1 == null)
            {
                if (ch2 == null)
                    return 0;
                else
                    return -1;
            }
            else
            {
                if (ch2 == null)
                    return 1;
                else
                {
                    int count1 = ch1.Count;
                    int count2 = ch2.Count;

                    return count1.CompareTo(count2);
                }
            }
        }
    }
}
