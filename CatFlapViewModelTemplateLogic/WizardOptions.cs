using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CatFlapViewModelTemplateLogic
{
    public partial class WizardOptions : Form
    {
        private Dictionary<string, List<Tuple<string, List<Tuple<string, string>>>>> contextDefinitions;
        private Dictionary<NameStringNode, string> propertyType = new Dictionary<NameStringNode, string>();
        private List<TreeNode> contexts = new List<TreeNode>();

        public WizardOptions()
        {
            InitializeComponent();
        }

        public void ShowTheFeckingThing()
        {
            this.ShowDialog();
        }

        public WizardOptions(Dictionary<string, List<Tuple<string, List<Tuple<string, string>>>>> contextDefinitions, string fileName)
        {
            InitializeComponent();
            this.contextDefinitions = contextDefinitions;
            List<TreeNode> allMiddleNodes = new List<TreeNode>();
            TreeNode toCheck = null;
            TreeNode parentOfToCheck = null;
            string suffix = "";
            string prefix = "";

            foreach (var x in contextDefinitions)
            {
                NameStringNode xNode = new NameStringNode(x.Key);
                xNode.Name = x.Key;
                foreach (var y in x.Value)
                {
                    NameStringNode yNode = new NameStringNode(y.Item1.Substring(y.Item1.LastIndexOf('.') + 1));
                    yNode.Name = y.Item1;
                    yNode.ToolTipText = "Entity type: " + y.Item1;
                    allMiddleNodes.Add(yNode);
                    if (fileName.Contains(yNode.Text))
                    {
                        var index = fileName.IndexOf(yNode.Text);
                        var len = index + yNode.Text.Length;
                        suffix = fileName.Substring(len, fileName.Length - len);
                        prefix = fileName.Substring(0, index);
                        toCheck = yNode;
                        parentOfToCheck = xNode;
                    }
                    foreach (var z in y.Item2)
                    {
                        NameStringNode zNode = new NameStringNode(z.Item1 + " (" + z.Item2 + ")");
                        zNode.Name = z.Item1;
                        zNode.ToolTipText = "Entity property: " + z.Item1 + " (" + z.Item2 + ")";
                        zNode.BackColor = Color.DarkGray;
                        zNode.ForeColor = Color.White;
                        propertyType.Add(zNode, z.Item2);
                        yNode.Nodes.Add(zNode);
                    }
                    xNode.Nodes.Add(yNode);
                }
                contexts.Add(xNode);
            }

            allMiddleNodes.ForEach(x => x.Text = prefix + x.Text + suffix);

            if (contexts.Count > 0)
            {
                comboBox1.Items.AddRange(contexts.Cast<object>().ToArray());

                if (parentOfToCheck != null)
                {
                    treeView1.Nodes.Add(parentOfToCheck);
                    comboBox1.SelectedItem = parentOfToCheck;
                    parentOfToCheck.Expand();
                    toCheck.Checked = true;
                }
                else
                {
                    treeView1.Nodes.Add(contexts.First());
                    comboBox1.SelectedItem = contexts.First();
                    contexts.First().Expand();
                }
            }
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            
        }

        private void treeView1_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (propertyType.ContainsKey(e.Node as NameStringNode)) //end node (entity property) selected
            {
                var toSearch = e.Node.Parent.Parent;
                NameStringNode match = null;
                try
                {
                    var type = propertyType[e.Node as NameStringNode];
                    if (type.Contains("<") && type.Contains(">"))
                    {
                        type = type.Substring(type.IndexOf('<') + 1, type.LastIndexOf('>') - type.IndexOf('<') - 1);
                    }
                    match = toSearch.Nodes[type] as NameStringNode;
                }
                catch { }
                if (match != null && !match.Checked)
                {
                    match.Checked = true;
                }
            }
            else if (e.Node.Parent != null) //middle node (entity) selected
            {
                if (e.Node.Checked)
                {
                    (e.Node as NameStringNode).Expand();
                }
                else
                {
                    (e.Node as NameStringNode).Collapse();
                }

                foreach (var node in e.Node.Nodes)
                {
                    if (e.Node.Checked)
                    {
                        (node as NameStringNode).BackColor = Color.Transparent;
                        (node as NameStringNode).ForeColor = Color.Black;
                    }
                    else
                    {
                        (node as NameStringNode).BackColor = Color.DarkGray;
                        (node as NameStringNode).ForeColor = Color.White;
                    }
                }
            }
            else //top node (context) selected
            {

            }
        }

        private void treeView1_BeforeCheck(object sender, TreeViewCancelEventArgs e)
        {
            if (e.Node.BackColor == Color.DarkGray)
            {
                e.Cancel = true;
            }
            else if (e.Node.Parent == null)
            {
                e.Cancel = true;
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            treeView1.Nodes.Clear();
            treeView1.Nodes.Add(comboBox1.SelectedItem as NameStringNode);
            (comboBox1.SelectedItem as NameStringNode).Expand();
        }

        private void treeView1_BeforeCollapse(object sender, TreeViewCancelEventArgs e)
        {
            if (e.Node.Parent == null)
            {
                e.Cancel = true;
            }
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            var node = treeView1.GetNodeAt(treeView1.PointToClient(Cursor.Position));
            
            rightClickNode = node;
            var key = propertyType.Keys.FirstOrDefault(x => x.Name == node.Name);
            if (node.Parent != null && node.Nodes.Count > 0) //middle (entity) node
            {
                contextMenuStrip1.Items["toolStripMenuItem1"].Text = "ViewModel name for entity " + node.Name;
                contextMenuStrip1.Items["toolStripTextBox1"].Text = node.Text;
            }
            else if (key != null) //end (entity property) node
            {
                contextMenuStrip1.Items["toolStripMenuItem1"].Text = "ViewModel property name for entity property " + node.Parent.Name + "." + node.Name;
                contextMenuStrip1.Items["toolStripTextBox1"].Text = node.Text.Substring(0, rightClickNode.Text.IndexOf(" "));
            }
            else
            {
                e.Cancel = true;
            }
        }

        private void treeView1_BeforeSelect(object sender, TreeViewCancelEventArgs e)
        {
            e.Cancel = true;
        }

        private void toolStripTextBox1_TextChanged(object sender, EventArgs e)
        {
            if (rightClickNode != null)
            {
                if (rightClickNode.Nodes.Count == 0) //end node
                {
                    rightClickNode.Text = toolStripTextBox1.Text + rightClickNode.Text.Substring(rightClickNode.Text.IndexOf(" "));
                }
                else
                {
                    rightClickNode.Text = toolStripTextBox1.Text;
                }
            }
        }

        TreeNode rightClickNode = null;

        public string Code { get; private set; }

        private void contextMenuStrip1_Closing(object sender, ToolStripDropDownClosingEventArgs e)
        {
            rightClickNode = null;
        }

        private void toolStripTextBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                contextMenuStrip1.Close();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var assembly = Assembly.GetExecutingAssembly();
            List<StringBuilder> classes = new List<StringBuilder>(); 
            var viewModel = "CatFlapViewModelTemplateLogic.ViewModelTemplate.txt";
            var property = "CatFlapViewModelTemplateLogic.PropertyDeclarationTemplate.txt";
            
            using (Stream stream = assembly.GetManifestResourceStream(viewModel))
            using (StreamReader reader = new StreamReader(stream))
            {
                viewModel = reader.ReadToEnd();
            }

            using (Stream stream = assembly.GetManifestResourceStream(property))
            using (StreamReader reader = new StreamReader(stream))
            {
                property = reader.ReadToEnd();
            }
            var modelNodes = new List<TreeNode>();
            var classBuilder = new StringBuilder();
            foreach (TreeNode tn in treeView1.Nodes[0].Nodes)
            {
                if (tn.Checked)
                {
                    var bodyBuilder = new StringBuilder(); //tee hee
                    var classText = viewModel.Replace("{{MODELNAME}}", tn.Text);
                    modelNodes.Add(tn);
                    foreach (TreeNode innerTN in tn.Nodes)
                    {
                        if (innerTN.Checked)
                        {
                            var start = innerTN.Text.IndexOf("(");
                            string typeName = innerTN.Text.Substring(start + 1, innerTN.Text.Length - start - 2);
                            string propertyName = innerTN.Text.Substring(0, innerTN.Text.IndexOf(" "));
                            if (propertyName != innerTN.Name)
                            {
                                bodyBuilder.AppendLine(string.Format("    [MapTo(typeof({0}), \"{1}\")]", tn.Name, innerTN.Name));
                            }
                            bodyBuilder.AppendLine(property.Replace("{{TYPE}} {{NAME}}", typeName + " " + propertyName));
                            bodyBuilder.AppendLine();
                        }
                    }
                    var body = bodyBuilder.ToString();
                    if (body.EndsWith(Environment.NewLine))
                    {
                        body = body.Substring(0, body.Length - 1);
                    }
                    classText = classText.Replace("{{CLASSBODY}}", body);
                    classBuilder.AppendLine(classText);
                    classBuilder.AppendLine();
                }
            }
            string code = classBuilder.ToString();
            modelNodes.ForEach(x =>
                {
                    code = code.Replace(x.Name + " ", x.Text + " ");
                    code = code.Replace(x.Name + ">", x.Text + ">");
                });
            Code = code;
            this.Close();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                Thread.Sleep(50);
                var rootNote = (TreeNode)treeView1.Nodes[0].Clone();
                foreach (var node in rootNote.Nodes.Cast<TreeNode>().ToArray())
                {
                    if (node != null && !node.Checked)
                    {
                        rootNote.Nodes.Remove(node);
                    }
                }
                treeView1.Nodes.Clear();
                treeView1.Nodes.Add(rootNote);
                rootNote.ExpandAll();
                checkBox2.Enabled = true;
                if (checkBox2.Checked)
                {
                    checkBox2_CheckedChanged(null, null);
                }
            }
            else
            {
                treeView1.Nodes.Clear();
                treeView1.Nodes.Add(comboBox1.SelectedItem as TreeNode);
                checkBox2.Enabled = false;
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
            {
                Thread.Sleep(50);
                var rootNote = (TreeNode)treeView1.Nodes[0].Clone();
                foreach (var node in rootNote.Nodes.Cast<TreeNode>().ToArray())
                {
                    foreach (var innerNode in node.Nodes.Cast<TreeNode>().ToArray())
                    {
                        if (innerNode != null && !innerNode.Checked)
                        {
                            node.Nodes.Remove(innerNode);
                        }
                    }
                    
                }
                treeView1.Nodes.Clear();
                treeView1.Nodes.Add(rootNote);
                rootNote.ExpandAll();
            }
            else
            {
                treeView1.Nodes.Clear();
                treeView1.Nodes.Add(comboBox1.SelectedItem as TreeNode);
                checkBox1_CheckedChanged(null, null);
            }
        }
    }

    public class NoClickTree : TreeView
    {
        protected override void WndProc(ref Message m)
        {
            // Suppress WM_LBUTTONDBLCLK
            if (m.Msg == 0x203) { m.Result = IntPtr.Zero; }
            else base.WndProc(ref m);
        }
    }

    public class NameStringNode : TreeNode
    {
        public NameStringNode() { }
        public NameStringNode(string text) : base(text) { }

        public override string ToString()
        {
            return this.Name;
        }
    }
}
