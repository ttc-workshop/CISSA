using System;
using CSScriptLibrary;
using Intersoft.CISSA.DataAccessLayer.Utils;

namespace Intersoft.CISSA.DataAccessLayer.Model.Documents.AutoAttr
{
    public class AutoAttributeScriptManager
    {
        private const string DefaultUsings =
            "using System;\n" +
            "using Intersoft.CISSA.DataAccessLayer.Model.Workflow;\n" +
            "using Intersoft.CISSA.DataAccessLayer.Model.Documents;\n" +
            "using Intersoft.CISSA.DataAccessLayer.Model.Enums;\n" +
            "using Intersoft.CISSA.DataAccessLayer.Repository;\n" +
            "using Intersoft.CISSA.DataAccessLayer.Model.Documents.AutoAttr;\n\n";

        private string PreparedScript { get; set; }

        private readonly CsParser _parser;

        public AutoAttributeScriptManager(string script)
        {
            if (string.IsNullOrEmpty(script))
            {
                PreparedScript = "";
                return;
            }

            _parser = new CsParser(script);

            PreparedScript = script;
        }

        public object Execute(AutoAttributeContext context)
        {
            string classScript = "";

            if (_parser.NextToken() != TokenType.Eof)
            {
                if (_parser.Token == TokenType.Comment ||
                    _parser.Token == TokenType.LineComment) _parser.SkipComments();

                if (_parser.Token == TokenType.Symbol && _parser.TokenSymbol == "{")
                {
                    classScript = DefaultUsings +
                                  " class Expression { \n " +
                                  "   public static object Execute(AutoAttributeContext context) " +
                                  PreparedScript +
                                  " } ";
                }
            }

            Console.WriteLine(classScript);
            var assembly = CSScript.LoadCode(classScript);

            MethodDelegate method = assembly.GetStaticMethod("*.Execute", context);

            return method(context);
        }
    }
}