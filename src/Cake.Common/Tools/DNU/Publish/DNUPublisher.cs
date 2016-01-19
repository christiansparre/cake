using Cake.Core;
using Cake.Core.IO;

namespace Cake.Common.Tools.DNU.Publish
{
    public class DNUPublisher : DNUTool<DNUPublishSettings>
    {
        private readonly ICakeEnvironment _environment;

        public DNUPublisher(IFileSystem fileSystem, ICakeEnvironment environment, IProcessRunner processRunner, IGlobber globber) : base(fileSystem, environment, processRunner, globber)
        {
            _environment = environment;
        }

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