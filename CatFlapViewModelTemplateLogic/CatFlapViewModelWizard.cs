using EnvDTE;
using Microsoft.VisualStudio.TemplateWizard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CatFlapViewModelTemplateLogic
{
    public class CatFlapViewModelWizard : IWizard
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

        private CodeClass GetClass(ProjectItem item, string className)
        {
            CodeClass result = null;

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
                    result = FindClassByName(fcm.CodeElements, className);
                }
                if (result == null)
                {
                    result = GetClass(currentItem, className);
                }
                else
                {
                    break;
                }
            }

            return result;
        }

        private CodeClass FindClassByName(CodeElements obj, string name)
        {
            CodeClass result = null;
            var fce = obj;
            if (fce != null)
            {
                for (short x = 1; x <= fce.Count; x++)
                {
                    var s = fce.Item(x);
                    if (s.Kind == vsCMElement.vsCMElementClass)
                    {
                        var o = (CodeClass)s;
                        var w = o.FullName;
                        if (w == name)
                        {
                            result = o;
                        }
                    }
                    else if (s.Kind == vsCMElement.vsCMElementNamespace)
                    {
                        result = FindClassByName(s.Children, name);
                    }
                    else
                    {
                        result = FindClassByName(s.Children, name);
                        //MessageBox.Show(s.Kind.ToString());
                    }
                    if (result != null)
                    {
                        break;
                    }
                }
            }
            return result;
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
                for (short x = 1; x <= fce.Count; x++)
                {
                    var s = fce.Item(x);
                    if (s.Kind == vsCMElement.vsCMElementClass)
                    {
                        var o = (CodeClass)s;
                        if (o.Bases.Count == 1)
                        {
                            var w = o.Bases.Item(1).FullName;
                            if (w.Contains("System.Data.Entity.DbContext"))
                            {
                                results.Add(s);
                            }
                        }
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
            Dictionary<string, List<Tuple<string, List<Tuple<string, string>>>>> contextDefinitions = GetContexts(automationObject);
            var dte = (DTE)automationObject;
            var wiz = new WizardOptions(contextDefinitions, replacementsDictionary["$safeitemname$"]);
            wiz.ShowDialog();
            replacementsDictionary.Add("$body$", wiz.Code);
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

        private Dictionary<string, List<Tuple<string, List<Tuple<string, string>>>>> GetContexts(object automationObject)
        {
            //Cntxt            //p.type//p.name
            Dictionary<string, List<Tuple<string, List<Tuple<string,string>>>>> contextDefinitions = new Dictionary<string, List<Tuple<string, List<Tuple<string,string>>>>>();
            List<CodeElement> contexts = new List<CodeElement>();

            var dte = (DTE)automationObject;
            var projects = dte.Solution.Projects.GetEnumerator();

            while (projects.MoveNext())
            {
                var items = ((Project)projects.Current).ProjectItems.GetEnumerator();
                while (items.MoveNext())
                {
                    var item = (ProjectItem)items.Current;
                    GetFiles(item, contexts);
                }
            }

            foreach (var c in contexts)
            {

                var codeClass = (CodeClass)c;
                contextDefinitions.Add(codeClass.FullName, new List<Tuple<string, List<Tuple<string,string>>>>());
                var currentList = contextDefinitions[codeClass.FullName];
                for (int x = 1; x <= codeClass.Members.Count; x++)
                {
                    var item = codeClass.Members.Item(x);
                    if (item.Kind == vsCMElement.vsCMElementProperty)
                    {
                        var codeProperty = (CodeProperty)item;
                        var genericParameter = codeProperty.Type.AsFullName.Split('<')[1].Replace(">", "");
                        var classItem = GetClass(automationObject, genericParameter);
                        if(classItem != null)
                        {
                            var members = new List<Tuple<string,string>>();
                            for (int y = 1; y <= classItem.Members.Count; y++)
                            {
                                var member = classItem.Members.Item(y);
                                if (member.Kind == vsCMElement.vsCMElementProperty)
                                {
                                    var property = (CodeProperty)member;
                                    members.Add(new Tuple<string, string>(property.Name, property.Type.AsFullName));
                                }
                            }

                            currentList.Add(new Tuple<string, List<Tuple<string,string>>>(genericParameter, members));
                        }
                    }
                }
            }
            return contextDefinitions;
        }

        private CodeClass GetClass(object automationObject, string typeName)
        {
            var dte = (DTE)automationObject;
            var projects = dte.Solution.Projects.GetEnumerator();
            CodeClass result = null;
            while (projects.MoveNext() && result == null)
            {
                var items = ((Project)projects.Current).ProjectItems.GetEnumerator();
                while (items.MoveNext() && result == null)
                {
                    var item = (ProjectItem)items.Current;
                    result = GetClass(item, typeName);
                }
            }
            return result;
        }

        public bool ShouldAddProjectItem(string filePath)
        {
            return true;
        }
    }
}
