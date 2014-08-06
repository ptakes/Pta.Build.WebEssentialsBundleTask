using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Pta.Build.WebEssentialsBundleTask
{
	internal class HtmlParser
	{
		private const string BeginBundleMarker = "<!--begin-{0}: {1}-->";
		private const string BeginBundleMarkerMatcher = @"(?<bundle><!--\s*begin-(?<type>styles|scripts):\s+(?<key>.*)\s*)-->";
		private const string EndBundleMarker = "<!--end-{0}: {1}-->";
		private const string EndBundleMarkerMatcher = @"(?<bundle><!--\s*end-(?<type>{0}):\s+(?<key>{1})\s*)-->";

		private static readonly Regex BeginBundleMarkerRegex = new Regex(BeginBundleMarkerMatcher,
			RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.Compiled);
		private static readonly Regex ScriptsRegex = new Regex(@"(?<bundle>!!\s?scripts\s?:\s?(?<key>.*)\s?!!)",
			RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.Compiled);
		private static readonly Regex StylesRegex = new Regex(@"(?<bundle>!!\s?styles\s?:\s?(?<key>.*)\s?!!)",
			RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.Compiled);

		private readonly Context _context;

		private HtmlParser(Context context)
		{
			_context = context;
		}

		public static bool Build(Context context)
		{
			var parser = new HtmlParser(context);
			return parser.UpdateHtmlFiles();
		}

		public void ResetHtmlFile(ref string html)
		{
			var builder = new StringBuilder();
			var start = 0;
			foreach (var match in BeginBundleMarkerRegex.Matches(html).Cast<Match>())
			{
				var beginMatch = match;
				var bundleType = beginMatch.Groups["type"].Value;
				var key = beginMatch.Groups["key"].Value;

				var endRegex = new Regex(String.Format(EndBundleMarkerMatcher, Regex.Escape(bundleType), Regex.Escape(key)),
					RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);

				var endMatch = endRegex.Match(html, beginMatch.Index + beginMatch.Length);
				if (!endMatch.Success)
				{
					_context.LogWarning("Bundle not end marker found for: {0}", key);
					start = beginMatch.Index + beginMatch.Length;
					continue;
				}

				if (endMatch.Index < beginMatch.Index)
				{
					var tempMatch = beginMatch;
					beginMatch = endMatch;
					endMatch = tempMatch;
				}

				builder.Append(html.Substring(start, beginMatch.Index - start));
				builder.AppendFormat("!!{0}:{1}!!", bundleType, key);

				start = endMatch.Index + endMatch.Length;
			}
			if (start < html.Length)
			{
				builder.Append(html.Substring(start));
			}

			html = builder.ToString();
		}

		private bool UpdateHtmlFile(ref string html, string bundleType, BundleMap map, IEnumerable<Match> matches)
		{
			var result = true;
			var builder = new StringBuilder();
			var start = 0;
			foreach (var match in matches)
			{
				var key = match.Groups["key"].Value;
				if (!map.ContainsKey(key))
				{
					_context.LogError("{0} bundle '{1}' not found.", CultureInfo.InvariantCulture.TextInfo.ToTitleCase(bundleType), match.Groups["key"].Value);
					result = false;
					continue;
				}

				builder.Append(html.Substring(start, match.Index - start));
				builder.AppendFormat(BeginBundleMarker, bundleType, key);
				builder.AppendLine();
				builder.Append(map[key].Html);
				builder.AppendFormat(EndBundleMarker, bundleType, key);

				start = match.Index + match.Length;
			}
			if (start < html.Length)
			{
				builder.Append(html.Substring(start));
			}

			html = builder.ToString();
			return result;
		}

		private bool UpdateHtmlFiles()
		{
			var success = true;

			foreach (var htmlFile in _context.AllHtmlFiles)
			{
				var htmlFilePath = Path.Combine(_context.ProjectDirectory, htmlFile);
				var html = default(string);

				if (!FileHelpers.TryReadTextFile(htmlFilePath, out html))
				{
					_context.LogError("Failed to read html file '{0}'.", htmlFilePath);
					success = false;
					continue;
				}

				var htmlHashCode = GetHashCode(html);

				ResetHtmlFile(ref html);
				var success1 = UpdateHtmlFile(ref html, "styles", _context.StylesMap, StylesRegex.Matches(html).Cast<Match>());
				var success2 = UpdateHtmlFile(ref html, "scripts", _context.ScriptsMap, ScriptsRegex.Matches(html).Cast<Match>());
				if (success1 && success2)
				{
					if (htmlHashCode != GetHashCode(html))
					{
						if (!FileHelpers.TryWriteTextFile(htmlFilePath, html))
						{
							_context.LogError("Failed to write to html file '{0}'.", htmlFilePath);
							success = false;
						}
					}
				}
				else
				{
					success = false;
				}
			}

			return success;
		}

		private int GetHashCode(string content)
		{
			return Regex.Replace(content, @"\s+", String.Empty).GetHashCode();
		}
	}
}
