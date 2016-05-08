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
        public void CheckDefinition()
        {
            Processor processor = new Processor();
            byte opCode = 0;
            foreach (Instruction instruction in processor.InstructionSet)
            {
                Assert.IsNotNull(instruction, "Opcode instruction " + ToHex(opCode) + " not defined");
                System.Console.WriteLine(ToHex(opCode) + ": " + instruction);

                Assert.IsTrue(opCode == instruction.Code,
                    "Instruction mapping mismatch: " + ToHex(opCode) + " / " + ToHex(instruction.Code));

                opCode++;
            }
        }

        private string ToHex(byte value)
        {
            return "0x" + value.ToString("X");
        }
    }
}
