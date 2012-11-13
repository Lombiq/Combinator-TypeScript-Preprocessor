using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Noesis.Javascript;
using Orchard.FileSystems.VirtualPath;
using Piedone.Combinator.EventHandlers;

namespace Piedone.Combinator.TypeScript
{
    public class TypeScriptPreprocessor : ICombinatorResourceEventHandler
    {
        private readonly IVirtualPathProvider _virtualPathProvider;


        public TypeScriptPreprocessor(IVirtualPathProvider virtualPathProvider)
        {
            _virtualPathProvider = virtualPathProvider;
        }


        public void OnContentLoaded(Models.CombinatorResource resource)
        {
            if (Path.GetExtension(resource.AbsoluteUrl.ToString()).ToLowerInvariant() != ".ts") return;

            // Code taken from https://github.com/giggio/TypeScriptCompiler
            using (var context = new JavascriptContext())
            {
                context.SetParameter("jsCode", resource.Content);

                using (var stream = _virtualPathProvider.OpenFile("~/Modules/Piedone.Combinator.TypeScript/typescript.js"))
                {
                    context.Run(new StreamReader(stream).ReadToEnd());
                }

                using (var stream = _virtualPathProvider.OpenFile("~/Modules/Piedone.Combinator.TypeScript/compiler.js"))
                {
                    context.Run(new StreamReader(stream).ReadToEnd());
                }

                var tsCode = (string)context.GetParameter("tsCode");
                var error = (string)context.GetParameter("error");

                if (!String.IsNullOrWhiteSpace(error))
                {
                    throw new ApplicationException("Preprocessing of the TypeScript resource " + resource.AbsoluteUrl + " failed. Error: " + error);
                }

                resource.Content = tsCode.Trim();
            }
        }

        public void OnContentProcessed(Models.CombinatorResource resource)
        {
        }
    }
}