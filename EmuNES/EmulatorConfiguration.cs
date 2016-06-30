using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SharpNes
{
    class EmulatorConfiguration
    {
        public static EmulatorConfiguration Instance { get { return instance; } }

        public string this[string key]
        {
            get
            {
                string value = ConfigurationManager.AppSettings[key];
                return value == null ? "" : value;
            }
            set
            {
                try
                {
                    var configurationFile
                        = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

                    var settings = configurationFile.AppSettings.Settings;

                    if (settings[key] == null)
                        settings.Add(key, value);
                    else
                        settings[key].Value = value;

                    configurationFile.Save(ConfigurationSaveMode.Modified);
                    ConfigurationManager.RefreshSection(configurationFile.AppSettings.SectionInformation.Name);
                }
                catch (ConfigurationErrorsException)
                {
                    MessageBox.Show("Unabled to update application configuration",
                        "Recently Used Files", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            }
        }

        private EmulatorConfiguration()
        {

        }

        private static EmulatorConfiguration instance = new EmulatorConfiguration();         
    }
}
