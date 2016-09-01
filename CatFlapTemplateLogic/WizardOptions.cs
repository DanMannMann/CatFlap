using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CatFlapTemplateLogic
{
    public partial class WizardOptions : Form
    {
        private Dictionary<string, List<string>> _items;

        public WizardOptions()
        {
            InitializeComponent();
        }

        public WizardOptions(Dictionary<string,List<string>> items)
        {
            InitializeComponent();

            bool first = true;
            _items = items;
            foreach (var kvp in items)
            {
                listBox1.Items.Add(kvp.Key);
                if (first)
                {
                    listBox1.SelectedItem = kvp.Key;
                }
            }
        }

        public string SelectedContext 
        {
            get
            {
                return (string)listBox1.SelectedItem;
            }
        }

        public List<string> SelectedSets
        {
            get
            {
                return checkedListBox1.CheckedItems.Cast<string>().ToList();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void listBox1_SelectedValueChanged(object sender, EventArgs e)
        {
            checkedListBox1.Items.Clear();
            if (listBox1.SelectedItem != null)
            {
                foreach (var item in _items[(string)listBox1.SelectedItem])
                {
                    checkedListBox1.Items.Add(item, true);
                }
            }
        }
    }
}
