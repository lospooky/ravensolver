using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TreeStructures
{
    public interface IHasCommonAttributes
    {
        CommonAttributesNode CommonAttributes { get; }
        bool HasCommonAttribute(Tree.AttributeNames attribute);
    }
}
