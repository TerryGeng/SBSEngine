using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SBSEnvironment.Parsing
{
    class SourcePosition
    {
        public int LineNum { get; private set; }
        public int InLinePosition {
            get
            {
                return Position - lineBegin;
            }
        }
        int Position { get; set; }

        int lineBegin;

        public SourcePosition()
        {
            LineNum = 0;
            lineBegin = 0;
        }

        public void AddLine()
        {
            ++LineNum;
            lineBegin = Position;
        }

        public void SetPosition(int position)
        {
            Position = position;
        }
    }
}
