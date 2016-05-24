namespace NesCore.Memory
{
    /// <summary>
    /// Represents an addressable byte store with an address range of $0000-$FFFF
    /// </summary>
    public interface MemoryMap
    {
        /// <summary>
        /// The size of the memory map
        /// </summary>
        uint Size { get; }

        /// <summary>
        /// Initialises all the memory locations to zeroes
        /// </summary>
        void Wipe();

        /// <summary>
        /// Reads or writes a byte value at the given address 
        /// </summary>
        /// <param name="address">Address from where to read or where to write byte value</param>
        /// <returns></returns>
        byte this[ushort address] { get; set; }
    }
}