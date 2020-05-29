using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Intersoft.Cissa.Report.Xls;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Data;
using Intersoft.CISSA.DataAccessLayer.Model.Workflow;

namespace Intersoft.Cis.Utils.CisToolApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var xls = new XlsDef();
        }

        private readonly IList<ScriptDef> _scriptDefs = new List<ScriptDef>();
        public IList<ScriptDef> Scripts { get { return _scriptDefs; } }

        private int _scriptNo;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            using (var dc = new DataContext())
            {
                _scriptNo = 0;

                foreach (
                    var script in
                        dc.Entities.Object_Defs.OfType<Script_Activity>()
                            .Where(o => o.Deleted == null || o.Deleted == false))
                {
                    var sm = new ScriptManager(script.Script);
                    try
                    {
                        _scriptNo++;
                        /*var wcd = new WorkflowContextData();
                        var wc = new WorkflowContext(wcd, dc);*/

                        sm.Compile("void");

                        AddSuccess(script);
                    }
                    catch (Exception ex)
                    {
                        AddFail(script, ex);
                    }
                }
            }
        }

        private void AddFail(Script_Activity script, Exception exception)
        {
            Scripts.Add(new ScriptDef(script, false, exception));

            var item = new ListViewItem
            {
                Content = String.Format("{2}: Fail: [{0}] {1}", script.Id, script.Full_Name ?? script.Name, _scriptNo)
            };

            ScriptList.Items.Add(item);
            Debug.Print("{3}. Fail: [{0}] \"{1}\" Ex: {2}", script.Id, script.Full_Name ?? script.Name, exception.Message, _scriptNo);
        }

        private void AddSuccess(Script_Activity script)
        {
            Scripts.Add(new ScriptDef(script, true, null));

            var item = new ListViewItem
            {
                Content = String.Format("{2}. Success: [{0}] \"{1}\"", script.Id, script.Full_Name ?? script.Name, _scriptNo)
            };

            ScriptList.Items.Add(item);
            Debug.Print("{2}. Success: [{0}] {1}", script.Id, script.Full_Name ?? script.Name, _scriptNo);
        }
    }

    public class ScriptDef
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public string Caption { get; private set; }
        public string Script { get; private set; }

        public bool Success { get; private set; }
        public string Error { get; private set; }
        public string StackTrace { get; private set; }

        public ScriptDef(Script_Activity activity, bool success, Exception ex)
        {
            Id = activity.Id;
            Name = activity.Name;
            Caption = activity.Full_Name;
            Script = activity.Script;
            Success = success;
            if (!success && ex != null)
            {
                Error = ex.Message;
                StackTrace = ex.StackTrace;
            }
        }
    }
}
