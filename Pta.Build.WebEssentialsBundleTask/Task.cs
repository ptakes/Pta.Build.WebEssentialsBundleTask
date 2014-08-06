using System;
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Pta.Build.WebEssentialsBundleTask
{
	public class WebEssentialsBundleTask : Task
	{
		public ITaskItem[] Bundles { get; set; }
		public string Configuration { get; set; }
		public ITaskItem[] HtmlFiles { get; set; }
		public string ProjectDir { get; set; }

		public override bool Execute()
		{
			if (String.IsNullOrWhiteSpace(ProjectDir))
			{
				Log.LogError("ProjectDir not set.");
				return false;
			}
			if (!Directory.Exists(ProjectDir))
			{
				Log.LogError("ProjectDir '{0}' doesn't exist.", ProjectDir);
				return false;
			}

			var context = new Context
			{
				AllBundles = (Bundles ?? new ITaskItem[0]).Select(x => FileHelpers.GetAbsolutePath(ProjectDir, x.ItemSpec)).ToArray(),
				AllHtmlFiles = (HtmlFiles ?? new ITaskItem[0]).Select(x => FileHelpers.GetAbsolutePath(ProjectDir, x.ItemSpec)).ToArray(),
				Configuration = Configuration ?? String.Empty,
				ProjectDirectory = ProjectDir,

				LogErrorWriter = message => Log.LogError(message),
				LogInformationWriter = message => Log.LogMessage(message),
				LogWarningWriter = message => Log.LogWarning(message),
			};

			context.StylesMap = BundleMap.Load(context, "css", "<link rel='stylesheet' href='{0}'>\r\n");
			context.ScriptsMap = BundleMap.Load(context, "js", "<script src='{0}'></script>\r\n");

			return HtmlParser.Build(context);
		}
	}
}
