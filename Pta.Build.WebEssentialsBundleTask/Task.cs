using System;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Pta.Build.WebEssentialsBundleTask
{
	public class WebEssentialsBundleTask : Task
	{
		public bool AddVersionQuery { get; set; }
		public ITaskItem[] Bundles { get; set; }
		public string Configuration { get; set; }
		public string DebugConfiguration { get; set; }
		public ITaskItem[] HtmlFiles { get; set; }
		public string ProjectDir { get; set; }
		public string WebRootDir { get; set; }

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

			var configuration = String.IsNullOrWhiteSpace(Configuration) ? "Debug" : Configuration;
			var debugConfiguration = String.IsNullOrWhiteSpace(DebugConfiguration) ? "Debug" : DebugConfiguration;

			var context = new Context
			{
				AddVersionQuery = AddVersionQuery,
				DebugBuild = configuration.Equals(debugConfiguration, StringComparison.OrdinalIgnoreCase),
				Log = Log,
				ProjectDirectory = PathHelper.GetFullPath(ProjectDir),
				WebRootDirectory = String.IsNullOrWhiteSpace(WebRootDir) ? ProjectDir : PathHelper.GetFullPath(ProjectDir, WebRootDir),
			};

			Log.LogMessage("AddVersionQuery: " + context.AddVersionQuery);
			Log.LogMessage("DebugBuild: " + context.DebugBuild);
			Log.LogMessage("ProjectDirectory: " + context.ProjectDirectory);
			Log.LogMessage("WebRootDirectory: " + context.WebRootDirectory);

			context.Bundles = (Bundles ?? new ITaskItem[0]).Select(x => x.ItemSpec);
			Log.LogMessage("Bundles: #" + context.Bundles.Count());

			context.HtmlFiles = (HtmlFiles ?? new ITaskItem[0]).Select(x => x.ItemSpec);
			Log.LogMessage("HtmlFiles: #" + context.HtmlFiles.Count());

			context.StylesMap = LoadBundles(context, "css", "<link rel='stylesheet' href='{0}'>\r\n");
			context.ScriptsMap = LoadBundles(context, "js", "<script src='{0}'></script>\r\n");

			return HtmlParser.Build(context);
		}

		private BundleMap LoadBundles(Context context, string type, string template)
		{
			var bundleFiles = context.Bundles
				.Where(b => b.EndsWith("." + type + ".bundle", StringComparison.OrdinalIgnoreCase))
				.ToList();

			var map = new BundleMap(bundleFiles.Count);
			foreach (var bundleFile in bundleFiles)
			{
				try
				{
					var bundle = new Bundle(context, bundleFile);
					Log.LogMessage("Bundle: " + bundleFile);
					Log.LogMessage("\tType: " + type);
					Log.LogMessage("\tKey: " + bundle.Key);

					if (context.DebugBuild)
					{
						var html = new StringBuilder();
						foreach (var file in bundle.Files)
						{
							html.AppendFormat(template, file.Url);
							Log.LogMessage("\tFile: " + file.Path);
							Log.LogMessage("\t\tUrl: " + file.Url);
						}
						bundle.Html = html.ToString();
					}
					else
					{
						bundle.Html = String.Format(template, bundle.BundleFile.Url);
						Log.LogMessage("\tUrl: " + bundle.BundleFile.Url);
					}

					map.Add(bundle.Key, bundle);
					Log.LogMessage("Found bundle '{0}':\r\n{1}", bundle.Key, bundle.Html);
				}
				catch (Exception ex)
				{
					Log.LogError("Failed to load or parse bundle file '{0}'\r\n{1}", bundleFile, ex);
				}
			}

			return map;
		}
	}
}
