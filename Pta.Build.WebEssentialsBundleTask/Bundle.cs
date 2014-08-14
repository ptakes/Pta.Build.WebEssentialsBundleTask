using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Pta.Build.WebEssentialsBundleTask
{
	public class Bundle
	{
		public bool AddVersionQuery { get; private set; }
		public File BundleFile { get; private set; }
		public IEnumerable<File> Files { get; private set; }
		public string Html { get; set; }
		public string Key { get; private set; }
		public bool Minify { get; private set; }
		public string OutputDirectory { get; private set; }

		public Bundle(Context context, string path)
		{
			var fullPath = PathHelper.GetFullPath(context.ProjectDirectory, path);

			var bundleDirectory = Path.GetDirectoryName(fullPath);
			var bundleFile = Path.GetFileName(fullPath);
			var resourceFile = bundleFile.Substring(0, bundleFile.Length - ".bundle".Length);
			Key = GetKey(context.WebRootDirectory, bundleDirectory, resourceFile);

			var xml = XDocument.Load(fullPath);
			AddVersionQuery = GetAddVersionQuery(xml, context.AddVersionQuery);
			Minify = GetMinifyFlag(xml);
			OutputDirectory = GetOutputDirectory(xml, context.ProjectDirectory, bundleDirectory);

			var resourcePath = PathHelper.GetFullPath(OutputDirectory, resourceFile);
			BundleFile = new File(context, this, resourcePath);

			Files = GetFiles(xml, context);
		}

		private bool GetAddVersionQuery(XDocument xml, bool defaultValue)
		{
			var value = xml.Descendants("addVersionQuery").FirstOrDefault();
			return (value != null) ? (bool)value : defaultValue;
		}

		private IEnumerable<File> GetFiles(XDocument xml, Context context)
		{
			return xml.Descendants("file")
				.Select(f => new File(context, this, (string)f))
				.ToArray();
		}

		private string GetKey(string webRootDirectoy, string bundleDirectory, string resourceFile)
		{
			var keyPath = Path.Combine(bundleDirectory, Path.GetFileNameWithoutExtension(resourceFile));
			return PathHelper.GetAbsoluteUrl(webRootDirectoy, keyPath);
		}

		private bool GetMinifyFlag(XDocument xml)
		{
			var value = xml.Descendants("minify").FirstOrDefault();
			return (value != null) ? (bool)value : true;
		}

		private string GetOutputDirectory(XDocument xml, string projectDirectoy, string bundleDirectory)
		{
			var temp = xml.Descendants("outputDirectory").FirstOrDefault();
			var value = (temp != null) ? (string)temp : null;
			if (String.IsNullOrWhiteSpace(value))
			{
				return bundleDirectory;
			}
			else if (value[0] == '/')
			{
				return PathHelper.GetFullPath(projectDirectoy, value.Substring(1));
			}
			else
			{
				return PathHelper.GetFullPath(bundleDirectory, value);
			}
		}
	}
}
