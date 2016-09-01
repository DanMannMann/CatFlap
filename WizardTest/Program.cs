using CatFlapViewModelTemplateLogic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WizardTest
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            DataContractSerializer dcs = new DataContractSerializer(typeof(Dictionary<string, List<Tuple<string, List<Tuple<string, string>>>>>));
            var conts = (Dictionary<string, List<Tuple<string, List<Tuple<string, string>>>>>)dcs.ReadObject(File.OpenRead("E:\\sdd.slz"));
            Application.Run(new WizardOptions(conts, "EditCustomerVM"));
        }
    }
}
