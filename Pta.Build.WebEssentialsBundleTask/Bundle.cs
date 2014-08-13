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

		public Bundle(Context context, string path)
		{
			var fullPath = PathHelper.GetFullPath(context.ProjectDirectory, path);
			var xml = XDocument.Load(fullPath);

			var resourcePath = fullPath.Substring(0, fullPath.Length - ".bundle".Length);

			AddVersionQuery = GetAddVersionQuery(xml, context.AddVersionQuery);
			BundleFile = new File(context, this, resourcePath);
			Files = GetFiles(xml, context);
			Key = GetKey(context.WebRootDirectory, resourcePath);
			Minify = GetMinifyFlag(xml);
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

		private string GetKey(string webRootDirectoy, string resourcePath)
		{
			resourcePath = Path.Combine(Path.GetDirectoryName(resourcePath), Path.GetFileNameWithoutExtension(resourcePath));
			return PathHelper.GetAbsoluteUrl(webRootDirectoy, resourcePath);
		}

		private bool GetMinifyFlag(XDocument xml)
		{
			var value = xml.Descendants("minify").FirstOrDefault();
			return (value != null) ? (bool)value : true;
		}
	}
}
