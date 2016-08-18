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

            var gameElements = nstDatabase.Descendants().Where(e => e.Name.LocalName.ToLower() == "game");

            foreach (var gameElement in gameElements)
            {
                var cartridgeElement = gameElement.Elements("cartridge").FirstOrDefault();
                if (cartridgeElement == null)
                {
                    cartridgeElement = gameElement.Elements("arcade").FirstOrDefault();
                }
                if (cartridgeElement == null)
                    continue;

                string crc = cartridgeElement.Attribute("crc").Value.ToUpper();
                var boardElement = cartridgeElement.Elements("board").FirstOrDefault();

                var mapperIdAttribute = boardElement.Attribute("mapper");
                if (mapperIdAttribute == null)
                    continue;

                byte mapperId = 0;
                if (!byte.TryParse(mapperIdAttribute.Value, out mapperId))
                    continue;

                List<Peripheral> peripherals = new List<Peripheral>();
                foreach (var peripheralsElement in gameElement.Elements("peripherals"))
                {
                    var deviceElement = peripheralsElement.Elements("device").FirstOrDefault();
                    if (deviceElement == null)
                        continue;

                    string deviceType = deviceElement.Attribute("type").Value.ToLower();

                    Peripheral peripheral = Peripheral.Joypad;
                    Enum.TryParse<Peripheral>(deviceType, true, out peripheral);

                    peripherals.Add(peripheral);
                }

                NstDatabaseEntry entry = new NstDatabaseEntry(crc, mapperId, peripherals);

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


