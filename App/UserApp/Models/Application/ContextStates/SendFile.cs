namespace Intersoft.CISSA.UserApp.Models.Application.ContextStates
{
    public class SendFile : BaseContextState
    {
        public string FileName { get; private set; }
        public byte[] FileData { get; private set; }

        public SendFile(IContext context, string fileName) : base(context)
        {
            FileName = fileName;
        }

        public SendFile(IContext context, ContextState previous, string fileName) : base(context, previous)
        {
            FileName = fileName;
        }

        public SendFile(IContext context, string fileName, byte[] data)
            : base(context)
        {
            FileName = fileName;
            FileData = data;
        }

        public SendFile(IContext context, ContextState previous, string fileName, byte[] data)
            : base(context, previous)
        {
            FileName = fileName;
            FileData = data;
        }

        public override ContextAction GetAction(IContext context)
        {
            return new ContextAction("Report", "File");
        }
    }
}