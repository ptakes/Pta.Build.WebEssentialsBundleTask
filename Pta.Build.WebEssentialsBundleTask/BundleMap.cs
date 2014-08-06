using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pta.Build.WebEssentialsBundleTask
{
	internal class BundleMap : Dictionary<string, Bundle>
	{
		private readonly Context _context;

		private BundleMap(Context context)
			: base(StringComparer.InvariantCultureIgnoreCase)
		{
			_context = context;
		}

		public static BundleMap Load(Context context, string bundleType, string htmlTemplate)
		{
			var map = new BundleMap(context);
			map.LoadBundles(bundleType, htmlTemplate);
			return map;
		}

		private IEnumerable<string> GetBundlePaths(string bundleType)
		{
			return _context.AllBundles
						.Where(b => b.EndsWith("." + bundleType + ".bundle", StringComparison.InvariantCultureIgnoreCase));
		}

		private void LoadBundles(string bundleType, string htmlTemplate)
		{
			var debugMode = "debug".Equals(_context.Configuration, StringComparison.OrdinalIgnoreCase);

			foreach (var bundlePath in GetBundlePaths(bundleType))
			{
				try
				{
					var bundle = Bundle.Load(_context.ProjectDirectory, bundlePath);

					if (debugMode)
					{
						var builder = new StringBuilder();
						foreach (var file in bundle.Files)
						{
							builder.AppendFormat(htmlTemplate, GetUrlWithHashVersion((string)file));
						}

						bundle.Html = builder.ToString();
					}
					else
					{
						bundle.Html = String.Format(htmlTemplate, GetUrlWithHashVersion(bundle.Url));
					}

					Add(bundle.Key, bundle);
					_context.LogInformation("Found bundle '{0}':\r\n{1}", bundle.Key, bundle.Html);
				}
				catch (Exception ex)
				{
					_context.LogError("Failed to load or parse bundle file '{0}'\r\n{1}", bundlePath, ex);
				}
			}
		}

		private string GetUrlWithHashVersion(string file)
		{
			var hash = FileHelpers.GetHash(_context.ProjectDirectory, file);
			return file + "?_v=" + hash;
		}
	}
}
