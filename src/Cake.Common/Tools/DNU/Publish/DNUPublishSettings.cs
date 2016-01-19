using Cake.Core.IO;
using Cake.Core.Tooling;

namespace Cake.Common.Tools.DNU.Publish
{
    /// <summary>
    /// Contains setting used by the <see cref="DNUPublisher"/>
    /// </summary>
    public class DNUPublishSettings : ToolSettings
    {
        /// <summary>
        /// Gets or sets the OutputDirectory
        /// </summary>
        public DirectoryPath OutputDirectory { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether sources should be included
        /// </summary>
        public bool NoSource { get; set; }

        /// <summary>
        /// Gets or sets the Runtime
        /// </summary>
        public string Runtime { get; set; }
    }
}