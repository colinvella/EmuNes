using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Utility
{
    public class AssemblerException: Exception
    {
        public AssemblerException(UInt16 sourceLineNumber, string sourceLine, string message)
            : base(message)
        {
            SourceLineNumber = sourceLineNumber;
            SourceLine = sourceLine;
        }

        public AssemblerException(UInt16 sorceLineNumber, string sourceLine, string message, Exception innerException)
            : base(message, innerException)
        {
            SourceLineNumber = sorceLineNumber;
            SourceLine = sourceLine;
        }

        public UInt16 SourceLineNumber { get; private set; }
        public string SourceLine { get; private set; }
    }
}
