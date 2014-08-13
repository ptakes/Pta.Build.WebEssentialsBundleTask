using System.Collections.Generic;
using Microsoft.Build.Utilities;

namespace Pta.Build.WebEssentialsBundleTask
{
	public class Context
	{
		public bool AddVersionQuery { get; set; }
		public bool DebugBuild { get; set; }
		public string ProjectDirectory { get; set; }
		public string WebRootDirectory { get; set; }

		public IEnumerable<string> Bundles { get; set; }
		public IEnumerable<string> HtmlFiles { get; set; }

		public BundleMap StylesMap { get; set; }
		public BundleMap ScriptsMap { get; set; }

		public TaskLoggingHelper Log { get; set; }
	}
}
