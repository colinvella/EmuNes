using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NesCore;
using NesCore.Processor;

namespace NesCoreTest
{
    [TestClass]
    public class InstructionSetTest
    {
        [TestMethod]
        public void TestInstructionDefinitions()
        {
            Mos6502 processor = new Mos6502();
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
