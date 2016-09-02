using EnvDTE;
using Microsoft.VisualStudio.TemplateWizard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CatFlapTemplateLogic
{
    public class CatFlapWizard : IWizard
    {
        public void BeforeOpeningFile(EnvDTE.ProjectItem projectItem)
        {

        }

        public void ProjectFinishedGenerating(EnvDTE.Project project)
        {

        }

        private ProjectItem GetFiles(ProjectItem item, List<CodeElement> contexts)
        {
            //base case
            if (item.ProjectItems == null)
                return null;

            var items = item.ProjectItems.GetEnumerator();
            while (items.MoveNext())
            {
                var currentItem = (ProjectItem)items.Current;
                var fcm = currentItem.FileCodeModel;
                if (fcm != null)
                {
                    var rez = FindDbContexts(fcm.CodeElements, contexts);
                }
                GetFiles(currentItem, contexts);
            }

            return item;
        }

        private List<CodeElement> FindDbContexts(CodeElements obj, List<CodeElement> results = null)
        {
            if (results == null)
            {
                results = new List<CodeElement>();
            }
            var fce = obj;
            if (fce != null)
            {
				foreach(var v in fce)
				{
					var s = (CodeElement)v;
					if (s.Kind == vsCMElement.vsCMElementClass)
					{
						var o = (CodeClass)s;
						try
						{
							var f = o.FullName;
						}
						catch (Exception ex)
						{

						}
						if (o.Bases.Count == 1)
						{
							var w = o.Bases.Item(1).FullName;
							if (w.Contains("System.Data.Entity.DbContext"))
							{
								results.Add(s);
							}
						}
						FindDbContexts(s.Children, results);
					}
					else if (s.Kind == vsCMElement.vsCMElementNamespace)
					{
						FindDbContexts(s.Children, results);
					}
					else
					{
						FindDbContexts(s.Children, results);
						//MessageBox.Show(s.Kind.ToString());
					}
				}

     //           for (short x = 1; x <= fce.Count; x++)
     //           {
					//try
					//{
					//	var s = fce.Item(x);
					//	if (s.Kind == vsCMElement.vsCMElementClass)
					//	{
					//		var o = (CodeClass)s;
					//		if (o.Bases.Count == 1)
					//		{
					//			var w = o.Bases.Item(1).FullName;
					//			if (w.Contains("System.Data.Entity.DbContext"))
					//			{
					//				results.Add(s);
					//			}
					//		}
					//	}
					//	else if (s.Kind == vsCMElement.vsCMElementNamespace)
					//	{
					//		FindDbContexts(s.Children, results);
					//	}
					//	else
					//	{
					//		FindDbContexts(s.Children, results);
					//		//MessageBox.Show(s.Kind.ToString());
					//	}
					//}
					//catch (Exception ex) { }
     //           }
            }
            return results;
        }

        public void ProjectItemFinishedGenerating(EnvDTE.ProjectItem projectItem)
        {

        }

        public void RunFinished()
        {

        }

        public void RunStarted(object automationObject, Dictionary<string, string> replacementsDictionary, WizardRunKind runKind, object[] customParams)
        {
            var className = replacementsDictionary["$safeitemname$"];
            Dictionary<string, List<Tuple<string, string>>> contextDefinitions = GetContexts(automationObject);
            var input = new Dictionary<string,List<string>>();
            contextDefinitions.Keys.ToList().ForEach(x => input.Add(x, contextDefinitions[x].Select(y => y.Item2).ToList()));

            var wiz = new WizardOptions(input);
            wiz.ShowDialog();

            replacementsDictionary.Add("$contexttype$", wiz.SelectedContext);

            StringBuilder accessorInsts = new StringBuilder();
            StringBuilder accessorDefs = new StringBuilder();
            StringBuilder proxyDefs = new StringBuilder();
            bool first = true;
            string spacer = "";

            foreach (var item in contextDefinitions[wiz.SelectedContext].Where(x => wiz.SelectedSets.Contains(x.Item2)))
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    spacer = "            ";
                }

                accessorInsts.AppendLine(item.Item2 + " = CreateAccessor<" + className + ", global::" + item.Item1 + ">();");
                accessorDefs.AppendLine(spacer + "public ISetAccessor<" + className + ", global::" + wiz.SelectedContext + ", global::" + item.Item1 + "> " + item.Item2 + " { get; private set; }");
                accessorDefs.AppendLine();
            }

            replacementsDictionary.Add("$accessorinstantiation$", accessorInsts.ToString());
            replacementsDictionary.Add("$accessordefinition$", accessorDefs.ToString());
        }

        private string GetProxyName(string item)
        {
            if (!item.Contains(".") && !item.Contains("+"))
            {
                return item + "Query";
            }
            else
            {
                return item.Substring(item.LastIndexOfAny(new char[] { '.', '+' }) + 1) + "Query";
            }
        }

        private Dictionary<string, List<Tuple<string, string>>> GetContexts(object automationObject)
        {
            //Cntxt            //p.type//p.name
            Dictionary<string, List<Tuple<string, string>>> contextDefinitions = new Dictionary<string, List<Tuple<string, string>>>();
            List<CodeElement> contexts = new List<CodeElement>();

            var dte = (DTE)automationObject;
            var projects = dte.Solution.Projects.GetEnumerator();

            while (projects.MoveNext())
            {
                foreach (var item in ((Project)projects.Current).ProjectItems)
                {
                    GetFiles((ProjectItem)item, contexts);
                }
            }

            foreach (var c in contexts)
            {

                var codeClass = (CodeClass)c;
                contextDefinitions.Add(codeClass.FullName, new List<Tuple<string, string>>());
                var currentList = contextDefinitions[codeClass.FullName];
                for (int x = 1; x <= codeClass.Members.Count; x++)
                {
                    var item = codeClass.Members.Item(x);
                    if (item.Kind == vsCMElement.vsCMElementProperty)
                    {
                        var codeProperty = (CodeProperty)item;
                        var genericParameter = codeProperty.Type.AsFullName.Split('<')[1].Replace(">", "");
                        currentList.Add(new Tuple<string, string>(genericParameter, codeProperty.Name));
                    }
                }
            }
            return contextDefinitions;
        }

        public bool ShouldAddProjectItem(string filePath)
        {
            return true;
        }
    }
}
