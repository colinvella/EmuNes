using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NesCore;
using NesCore.Processing;

namespace NesCoreTest
{
    [TestClass]
    public class InstructionSetTest
    {
        [TestMethod]
        public void CompleteDefinition()
        {
            Processor processor = new Processor(null);
            byte opCode = 0;
            foreach (Instruction instruction in processor.InstructionSet)
            {
                Assert.IsNotNull(instruction, "Opcode instruction " + ToHex(opCode) + " not defined");
                System.Console.WriteLine(ToHex(opCode) + ": " + instruction);
                opCode++;
            }
        }

        private string ToHex(byte value)
        {
            return "0x" + value.ToString("X");
        }
    }
}
