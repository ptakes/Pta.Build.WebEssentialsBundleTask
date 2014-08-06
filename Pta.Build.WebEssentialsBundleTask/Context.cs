using System;
using System.Collections.Generic;

namespace Pta.Build.WebEssentialsBundleTask
{
	internal class Context
	{
		public IEnumerable<string> AllBundles { get; set; }
		public IEnumerable<string> AllHtmlFiles { get; set; }

		public string Configuration { get; set; }
		public string ProjectDirectory { get; set; }

		public BundleMap StylesMap { get; set; }
		public BundleMap ScriptsMap { get; set; }

		public Action<string> LogErrorWriter { get; set; }
		public Action<string> LogInformationWriter { get; set; }
		public Action<string> LogWarningWriter { get; set; }

		public void LogError(string message, params object[] args)
		{
			if (LogErrorWriter != null)
			{
				LogErrorWriter(String.Format(message, args));
			}
		}

		public void LogInformation(string message, params object[] args)
		{
			if (LogInformationWriter != null)
			{
				LogInformationWriter(String.Format(message, args));
			}
		}

		public void LogWarning(string message, params object[] args)
		{
			if (LogWarningWriter != null)
			{
				LogWarningWriter(String.Format(message, args));
			}
		}
	}
}
