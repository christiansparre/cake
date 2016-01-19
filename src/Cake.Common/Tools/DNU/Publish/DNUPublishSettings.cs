using Cake.Core.IO;
using Cake.Core.Tooling;

namespace Cake.Common.Tools.DNU.Publish
{
    public class DNUPublishSettings : ToolSettings
    {
        public DirectoryPath OutputDirectory { get; set; }
        public bool NoSource { get; set; }
        public string Runtime { get; set; }
    }
}