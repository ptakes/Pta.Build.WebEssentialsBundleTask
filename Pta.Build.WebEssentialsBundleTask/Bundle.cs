using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Pta.Build.WebEssentialsBundleTask
{
	internal class Bundle
	{
		private static readonly Regex BundleFileExtensionRegex = new Regex(@"\.(?<type>css|js)+\.bundle",
			RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.Compiled);

		public IEnumerable<string> Files { get; private set; }
		public string Html { get; set; }
		public string Key { get; private set; }
		public bool Minify { get; private set; }
		public string SettingsPath { get; private set; }
		public string Type { get; private set; }
		public string Url { get; private set; }

		private Bundle()
		{
		}

		public static Bundle Load(string rootPath, string settingsPath)
		{
			settingsPath = FileHelpers.GetAbsolutePath(rootPath, settingsPath);
			var xml = XDocument.Load(settingsPath);

			var bundle = new Bundle
			{
				Files = GetFiles(xml),
				Key = GetKey(rootPath, settingsPath),
				Minify = GetMinifyFlag(xml),
				SettingsPath = settingsPath,
				Type = GetType(settingsPath)
			};

			bundle.Url = GetUrl(xml, rootPath, bundle);

			return bundle;
		}

		private static IEnumerable<string> GetFiles(XDocument xml)
		{
			return xml.Descendants("file").Select(f => (string)f).ToArray();
		}

		private static string GetKey(string rootPath, string settingsPath)
		{
			var key = FileHelpers.GetAbsoluteUrl(rootPath, settingsPath);
			return BundleFileExtensionRegex.Replace(key, String.Empty);
		}

		private static bool GetMinifyFlag(XDocument xml)
		{
			var value = xml.Descendants("minify").FirstOrDefault();
			return (value != null) ? (bool)value : true;
		}

		private static string GetType(string settingsPath)
		{
			return BundleFileExtensionRegex.Match(settingsPath).Groups["type"].Value;
		}

		private static string GetUrl(XDocument xml, string rootPath, Bundle bundle)
		{
			var value = xml.Descendants("outputDirectory").FirstOrDefault();
			var outputPath = (value != null) ? ((string)value).Trim() : null;

			if (String.IsNullOrEmpty(outputPath))
			{
				outputPath = bundle.SettingsPath;
			}
			else
			{
				if (!Path.IsPathRooted(outputPath) || outputPath[0] == Path.DirectorySeparatorChar)
				{
					outputPath = outputPath.Replace('/', Path.DirectorySeparatorChar);
					if (outputPath[0] == Path.DirectorySeparatorChar)
					{
						outputPath = Path.Combine(rootPath, outputPath.Substring(1));
					}
					else
					{
						outputPath = Path.Combine(Path.GetDirectoryName(bundle.SettingsPath), outputPath);
					}
				}

				outputPath = Path.Combine(outputPath, Path.GetFileName(bundle.SettingsPath));
			}

			var replacement = bundle.Minify ? ".min" : String.Empty;
			replacement += ".${type}";
			outputPath = BundleFileExtensionRegex.Replace(outputPath, replacement);

			return FileHelpers.GetAbsoluteUrl(rootPath, outputPath);
		}
	}
}
