using NesCore.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SharpNes.Database
{
    class NstDatabase
    {
        public NstDatabase()
        {
            entries = new Dictionary<string, NstDatabaseEntry>();

            XDocument nstDatabase = XDocument.Parse(Properties.Resources.NstDatabase);

            var cartridgeElements = nstDatabase.Descendants().Where(e => e.Name.LocalName.ToLower() == "cartridge");

            foreach (var cartridgeelement in cartridgeElements)
            {
                string crc = cartridgeelement.Attribute("crc").Value.ToUpper();
                var boardElement = cartridgeelement.Descendants().FirstOrDefault(e => e.Name.LocalName.ToLower() == "board");

                var mapperIdAttribute = boardElement.Attribute("mapper");
                if (mapperIdAttribute == null)
                    continue;

                byte mapperId = 0;
                if (!byte.TryParse(mapperIdAttribute.Value, out mapperId))
                    continue;

                NstDatabaseEntry entry = new NstDatabaseEntry(crc, mapperId);

                entries[crc] = entry;
            }
        }

        public NstDatabaseEntry this[uint crc]
        {
            get
            {
                string crcKey = Hex.Format(crc).Replace("$", "");
                if (entries.ContainsKey(crcKey))
                    return entries[crcKey];
                else
                    return null;
            }
        }
        private Dictionary<string, NstDatabaseEntry> entries;
    }
}


