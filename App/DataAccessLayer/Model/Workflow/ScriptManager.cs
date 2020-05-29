using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using CSScriptLibrary;
using Intersoft.CISSA.DataAccessLayer.Cache;
using Intersoft.CISSA.DataAccessLayer.Utils;
using Raven.Abstractions.Extensions;

namespace Intersoft.CISSA.DataAccessLayer.Model.Workflow
{
    public class ScriptManager
    {
        private const string DefaultUsings =
            "using System;\n" +
            "using Microsoft.CSharp;\n" +
            "using System.Collections.Generic;\n" +
            "using System.Linq;\n" +
            "using System.IO;\n" +
            "using System.Dynamic;\n" +
            "using System.Drawing;\n" +
            "using System.Text.RegularExpressions;\n" +
            "using Intersoft.Cissa.Report.Xls;\n" +
            "using Intersoft.Cissa.Report.WordDoc;\n" +
            "using Intersoft.Cissa.Report.Styles;\n" +
            "using Intersoft.CISSA.DataAccessLayer.Model;\n" +
            "using Intersoft.CISSA.DataAccessLayer.Model.Workflow;\n" +
            "using Intersoft.CISSA.DataAccessLayer.Model.Documents;\n" +
            "using Intersoft.CISSA.DataAccessLayer.Model.Enums;\n" +
            "using Intersoft.CISSA.DataAccessLayer.Repository;\n" +
            "using Intersoft.CISSA.DataAccessLayer.Report;\n" +
            "using Intersoft.CISSA.DataAccessLayer.Model.Query.Interfaces;\n" +
            "using Intersoft.CISSA.DataAccessLayer.Model.Query.Builders;\n" +
            "using Intersoft.CISSA.DataAccessLayer.Model.Query;\n" +
            "using Intersoft.CISSA.DataAccessLayer.Model.Query.Sql;\n" +
            "using Intersoft.CISSA.DataAccessLayer.Model.Organizations;\n" +
            "using Intersoft.CISSA.DataAccessLayer.Utils;\n" +
            "using Intersoft.Cissa.Report.Common;\n";

        public string PreparedScript { get; private set; }
        
        public ScriptType ScriptType
        {
            get {
                int indexUsing = PreparedScript.IndexOf("using", StringComparison.Ordinal);
                if (indexUsing == -1) indexUsing = PreparedScript.IndexOf("class", StringComparison.Ordinal);
                if (indexUsing == -1) indexUsing = int.MaxValue;

                int indexExpr = PreparedScript.IndexOf("(", StringComparison.Ordinal);
                if (indexExpr == -1) indexExpr = int.MaxValue;
                
                int indexFunc = PreparedScript.IndexOf("{", StringComparison.Ordinal);
                if (indexFunc == -1) indexFunc = int.MaxValue;

                if (indexExpr >= 0 && indexExpr < indexUsing && indexExpr < indexFunc)  return ScriptType.Expression;
                if (indexUsing >= 0 && indexUsing < indexExpr && indexUsing < indexFunc) return ScriptType.Class;
                if (indexFunc >= 0 && indexFunc < indexExpr && indexFunc < indexUsing) return ScriptType.Function;

                return ScriptType.Unknown;
            }
        }

        private readonly CsParser _parser;

        public ScriptManager (string script)
        {
            if (string.IsNullOrEmpty(script))
            {
                PreparedScript = ""; 
                return;
            }

            _parser = new CsParser(script);

            PreparedScript = script; // PrepareScript(script);
        }

        public void Execute(WorkflowContext context)
        {
            ExecuteBase(context, "void");
        }

        public object ExecuteWithResult(WorkflowContext context)
        {
            return ExecuteBase(context, "object");
        }
        
        public static readonly ObjectKeyCache<string, Assembly> ScriptCache = new ObjectKeyCache<string, Assembly>();
        // public static readonly object ScriptLoadLock = new object();
        // public static readonly ConcurrentDictionary<string, Lazy<Assembly>> ScriptCache = new ConcurrentDictionary<string, Lazy<Assembly>>();
        private static readonly ReaderWriterLock ScriptCacheLock = new ReaderWriterLock();
        private const int LockTimeout = 50000;

        public static void ClearCaches()
        {
            ScriptCacheLock.AcquireWriterLock(LockTimeout);
            try
            {
                ScriptCache.Clear();
            }
            finally
            {
                ScriptCacheLock.ReleaseWriterLock();
            }
        }

        public Assembly Compile(string resultType)
        {
            //TODO: дописать различные виды вызова скрипта в зависимости от типа
            //            try
            //            {
            string classScript = "";
            /*
            switch (ScriptType)
            {
                case ScriptType.Class:
                    classScript = DefaultUsings + PreparedScript;
                    break;

                case ScriptType.Function:
                    classScript = DefaultUsings +
                                    " class Expression { \n " +
                                    "   public static " + resultType + " Execute(WorkflowContext context) " +
                                    PreparedScript +
                                    " } ";
                    break;

                case ScriptType.Expression:
                    classScript = DefaultUsings +
                                    " class Expression { \n " +
                                    "   public static " + resultType + " Execute(WorkflowContext context) { " +
                                    "     return " + PreparedScript + " ; " +
                                    "   } " +
                                    " } ";
                    break;
            }*/
            if (_parser.NextToken() != TokenType.Eof)
            {
                if (_parser.Token == TokenType.Comment ||
                    _parser.Token == TokenType.LineComment) _parser.SkipComments();

                if (_parser.Token == TokenType.Symbol && _parser.TokenSymbol == "{")
                {
                    classScript = DefaultUsings +
                                  " class Expression { \n " +
                                  "   public static " + resultType + " Execute(WorkflowContext context) " +
                                  PreparedScript +
                                  " } ";
                }
                else if (_parser.Token == TokenType.Symbol ||
                         _parser.TokenSymbol == "(")
                {
                    classScript = DefaultUsings +
                                  " class Expression { \n " +
                                  "   public static " + resultType + " Execute(WorkflowContext context) { " +
                                  "     return " + PreparedScript + " ; " +
                                  "   } " +
                                  " } ";
                }
                else
                    classScript = DefaultUsings + PreparedScript;
            }
            else
            {
                classScript = DefaultUsings +
                              " class Expression { \n " +
                              "   public static " + resultType + " Execute(WorkflowContext context) {" +
                              PreparedScript +
                              " }} ";
                
            }

            /*ObjectCacheItem<string, Assembly> cached;
            // lock (ScriptLoadLock)
                cached = ScriptCache.Find(classScript);

            if (cached != null) return cached.CachedObject;*/ // 09-02-17
            
            //lock (ScriptLoadLock) // 09-02-17
            /*return ScriptCache.GetOrAdd(classScript, s =>
                new Lazy<Assembly>(() =>
                {
                    CSScript.AssemblyResolvingEnabled = true;
                    //Evaluator.ReferenceAssembliesFromCode(code); 
                    return CSScript.LoadCode(s);
                    //ScriptCache.Add(assembly, classScript);

                    //return assembly;
                })).Value;*/ // 10-02-17
            ScriptCacheLock.AcquireReaderLock(LockTimeout);
            try
            {
                var cached = ScriptCache.Find(classScript);

                if (cached != null) return cached.CachedObject;

                var lc = ScriptCacheLock.UpgradeToWriterLock(LockTimeout);
                try
                {
                    cached = ScriptCache.Find(classScript);
                    if (cached != null) return cached.CachedObject;

                    CSScript.AssemblyResolvingEnabled = true;
                    //Evaluator.ReferenceAssembliesFromCode(code); 
                    var assembly = CSScript.LoadCode(classScript);
                    ScriptCache.Add(assembly, classScript);

                    return assembly;
                }
                finally
                {
                    ScriptCacheLock.DowngradeFromWriterLock(ref lc);
                }
            }
            finally
            {
                ScriptCacheLock.ReleaseReaderLock();
            }
        }

        private object ExecuteBase(WorkflowContext context, string resultType)
        {
            Assembly assembly = Compile(resultType);

            MethodDelegate method = assembly.GetStaticMethod("*.Execute", context /*, DataRepository repository*/);

            if (resultType == "void")
            {
                method(context);
                return null;
            }

            return method(context);
        }

        public static void LoadScript(string script)
        {
            if (String.IsNullOrEmpty(script)) return;

            var parser = new CsParser(script);

            while (parser.NextToken() != TokenType.Eof)
            {
                if (parser.Token == TokenType.Comment ||
                    parser.Token == TokenType.LineComment) parser.SkipComments();
                else if (parser.Token != TokenType.Eof)
                {
                    var classScript = DefaultUsings + "\n" + script;

                    /* ObjectCacheItem<string, Assembly> cached;
                    // lock (ScriptLoadLock)
                        cached = ScriptCache.Find(classScript);

                    if (cached == null)
                    {
                        //lock (ScriptLoadLock)
                        {
                            var assembly = CSScript.LoadCode(classScript);
                            ScriptCache.Add(assembly, classScript);
                        }
                    }*/ // 09-02-17
                    /*ScriptCache.GetOrAdd(classScript, s =>
                        new Lazy<Assembly>(() => CSScript.LoadCode(s)));*/ // 10-02-17
                    ScriptCacheLock.AcquireReaderLock(LockTimeout);
                    try
                    {
                        var cached = ScriptCache.Find(classScript);
                        if (cached == null)
                        {
                            var lc = ScriptCacheLock.UpgradeToWriterLock(LockTimeout);
                            try
                            {
                                cached = ScriptCache.Find(classScript);
                                if (cached != null) return;

                                var assembly = CSScript.LoadCode(classScript);
                                ScriptCache.Add(assembly, classScript);
                            }
                            finally
                            {
                                ScriptCacheLock.DowngradeFromWriterLock(ref lc);
                            }
                        }
                        return;
                    }
                    finally
                    {
                        ScriptCacheLock.ReleaseReaderLock();
                    }
                }
            }
        }

        //private static string PrepareScript(string script)
        //{
        //    //1. remove comments
        //    script = RemoveMultiLineComments(script);
        //    script = RemoveOneLineComments(script);

        //    //2. удалям пробелы по краям
        //    script = script.Trim();

        //    return script;
        //}

        //private static string RemoveMultiLineComments(string script)
        //{
        //    return RemoveComments(script, "/*", "*/");
        //}

        //private static string RemoveOneLineComments(string script)
        //{
        //    return RemoveComments(script, "//", "\n");
        //}

        //private static string RemoveComments(string script, string beginCommentSymbol, string endCommentSymbol)
        //{
        //    string resultScript = script;
        //    int frst = script.IndexOf(beginCommentSymbol);
        //    if (frst > -1)
        //    {
        //        int lst = script.Length;
        //        if (script.IndexOf(endCommentSymbol) > -1)
        //        {
        //            lst = script.IndexOf(endCommentSymbol) + 1;
        //        }
        //        resultScript = script.Remove(frst, lst - frst);
        //    }

        //    if (resultScript == script) return script;

        //    return RemoveComments(resultScript, beginCommentSymbol, endCommentSymbol);
        //}
    }
}
