using System;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using IO = System.IO;

namespace Pta.Build.WebEssentialsBundleTask
{
	public static class PathHelper
	{
		public static string GetAbsoluteUrl(string webRootDirectory, string path = "/")
		{
			var relativePath = default(string);
			if (!TryGetRelativePath(webRootDirectory, path, out relativePath))
			{
				return path;
			}

			var url = relativePath.Replace('\\', '/');
			if (url.StartsWith("./"))
			{
				return url.Substring(1);
			}

			if (!url.StartsWith("/"))
			{
				return "/" + url;
			}

			return url;
		}

		public static string GetFullPath(string currentDirectory, string path = null)
		{
			currentDirectory = currentDirectory.Replace('/', '\\');
			currentDirectory = IO.Path.GetFullPath(currentDirectory);

			if (String.IsNullOrWhiteSpace(path))
			{
				return currentDirectory;
			}

			path = NormalizePath(path);
			if (!IO.Path.IsPathRooted(path))
			{
				path = IO.Path.Combine(currentDirectory, path);
			}

			return IO.Path.GetFullPath(path);
		}

		public static string GetHash(string fileName)
		{
			using (var stream = IO.File.OpenRead(fileName))
			using (var bufferedStream = new IO.BufferedStream(stream, 1024 * 32))
			{
				var sha256 = new SHA256Managed();
				var hash = sha256.ComputeHash(bufferedStream);
				return BitConverter.ToString(hash).Replace("-", String.Empty).ToLowerInvariant();
			}
		}

		public static bool TryGetRelativePath(string currentDirectory, string path, out string relativePath)
		{
			const int MaxPathLength = 260;

			var buffer = new StringBuilder(MaxPathLength);
			if (PathRelativePathTo(buffer, currentDirectory, IO.FileAttributes.Directory, path, IO.FileAttributes.Normal))
			{
				relativePath = buffer.ToString();
				return true;
			}
			else
			{
				relativePath = null;
				return false;
			}
		}

		public static bool TryReadTextFile(string filePath, out string content)
		{
			try
			{
				content = IO.File.ReadAllText(filePath);
				return true;
			}
			catch
			{
				content = null;
				return false;
			}
		}

		public static bool TryWriteTextFile(string filePath, string content, bool withBOM = true)
		{
			try
			{
				IO.File.WriteAllText(filePath, content, new UTF8Encoding(withBOM));
				return true;
			}
			catch
			{
				return false;
			}
		}

		private static string NormalizePath(string path)
		{
			if (String.IsNullOrWhiteSpace(path))
			{
				return String.Empty;
			}

			path = path.Replace('/', '\\');

			if (path.StartsWith(@"\\"))
			{
				return path;
			}

			if (path.IndexOf(@":\") == 1)
			{
				return path;
			}

			if (path.IndexOf(@":") == 1)
			{
				return path.Substring(2);
			}

			if (path[0] == '\\')
			{
				return "." + path;
			}

			return path;
		}

		[DllImport("shlwapi.dll", CharSet = CharSet.Auto)]
		private static extern bool PathRelativePathTo([Out] StringBuilder relativePath, [In] string fromPath, [In] IO.FileAttributes fromPathType, [In] string toPath, [In] IO.FileAttributes toPathType);
	}
}
