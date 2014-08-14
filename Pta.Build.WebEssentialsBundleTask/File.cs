using System;
using System.Text.RegularExpressions;

namespace Pta.Build.WebEssentialsBundleTask
{
	public class File
	{
		private static readonly Regex ExtensionRegex = new Regex(@"\.(?<type>css|js)",
				RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.Compiled);

		public string Path { get; private set; }
		public string Url { get { return _url.Value; } }

		private readonly Lazy<string> _url;

		public File(Context context, Bundle bundle, string path)
		{
			var fullPath = PathHelper.GetFullPath(context.ProjectDirectory, path);
			if (!context.DebugBuild && bundle.Minify)
			{
				fullPath = File.ExtensionRegex.Replace(fullPath, ".min.${type}");
			}

			Path = fullPath;

			_url = new Lazy<string>(() =>
			{
				var url = PathHelper.GetAbsoluteUrl(context.WebRootDirectory, fullPath);

				if (bundle.AddVersionQuery)
				{
					var hash = PathHelper.GetHash(fullPath);
					url += "?_v=" + hash;
				}

				return url;
			});
		}
	}
}
