using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Utility
{
    public class AssemblerException: Exception
    {
        public AssemblerException(ushort sourceLineNumber, string sourceLine, string message)
            : base(message)
        {
            SourceLineNumber = sourceLineNumber;
            SourceLine = sourceLine;
        }

        public AssemblerException(ushort sorceLineNumber, string sourceLine, string message, Exception innerException)
            : base(message, innerException)
        {
            SourceLineNumber = sorceLineNumber;
            SourceLine = sourceLine;
        }

        public ushort SourceLineNumber { get; private set; }
        public string SourceLine { get; private set; }
    }
}
