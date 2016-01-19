using Cake.Core;
using Cake.Core.IO;

namespace Cake.Common.Tools.DNU.Publish
{
    /// <summary>
    /// DNU publisher
    /// </summary>
    public class DNUPublisher : DNUTool<DNUPublishSettings>
    {
        private readonly ICakeEnvironment _environment;

        /// <summary>
        /// Initializes a new instance of the <see cref="DNUPublisher"/> class
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="environment">The environment.</param>
        /// <param name="processRunner">The process runner.</param>
        /// <param name="globber">The globber.</param>
        public DNUPublisher(IFileSystem fileSystem, ICakeEnvironment environment, IProcessRunner processRunner, IGlobber globber) : base(fileSystem, environment, processRunner, globber)
        {
            _environment = environment;
        }

        /// <summary>
        /// Publishes the provided project
        /// </summary>
        /// <param name="path">The path to publish</param>
        /// <param name="settings">The settings</param>
        public void Publish(string path, DNUPublishSettings settings)
        {
            Run(settings, GetArguments(path, settings));
        }

        private ProcessArgumentBuilder GetArguments(string path, DNUPublishSettings settings)
        {
            var builder = new ProcessArgumentBuilder();

            builder.Append("publish");

            if (path != null)
            {
                builder.AppendQuoted(path);
            }

            if (settings.NoSource)
            {
                builder.Append("--no-source");
            }

            if (settings.Runtime != null)
            {
                builder.Append("--runtime");
                builder.AppendQuoted(settings.Runtime);
            }

            if (settings.OutputDirectory != null)
            {
                builder.Append("--out");
                builder.AppendQuoted(settings.OutputDirectory.MakeAbsolute(_environment).FullPath);
            }

            return builder;
        }
    }
}