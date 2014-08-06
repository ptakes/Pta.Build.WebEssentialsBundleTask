using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace Pta.Build.WebEssentialsBundleTask
{
	internal static class FileHelpers
	{
		public static string GetAbsolutePath(string rootPath, string filePath)
		{
			rootPath = rootPath.Replace('/', Path.DirectorySeparatorChar);
			filePath = filePath.Replace('/', Path.DirectorySeparatorChar);

			if (filePath[0] != Path.DirectorySeparatorChar && Path.IsPathRooted(filePath))
			{
				return filePath;
			}

			return Path.GetFullPath(Path.Combine(rootPath, filePath.TrimStart(Path.DirectorySeparatorChar)));
		}

		public static string GetAbsoluteUrl(string rootPath, string filePath)
		{
			var relativePath = FileHelpers.GetRelativePath(rootPath, filePath);

			var key = relativePath.Replace(Path.DirectorySeparatorChar, '/');
			if (key[0] != '/')
			{
				key = "/" + key;
			}

			return key;
		}

		public static string GetHash(string rootPath, string filePath)
		{
			filePath = GetAbsolutePath(rootPath, filePath);
			using (var stream = File.OpenRead(filePath))
			using (var bufferedStream = new BufferedStream(stream, 1024 * 32))
			{
				var sha256 = new SHA256Managed();
				var hash = sha256.ComputeHash(bufferedStream);
				return BitConverter.ToString(hash).Replace("-", String.Empty).ToLowerInvariant();
			}
		}

		public static string GetRelativePath(string rootPath, string filePath)
		{
			var relativeFilePath = default(string);
			TryGetRelativePath(rootPath, filePath, out relativeFilePath);
			return relativeFilePath;
		}

		public static bool TryGetRelativePath(string rootPath, string filePath, out string relativeFilePath)
		{
			const int MaxPathLength = 260;

			filePath = GetAbsolutePath(rootPath, filePath);

			var buffer = new StringBuilder(MaxPathLength);
			if (PathRelativePathTo(buffer, rootPath, FileAttributes.Directory, filePath, FileAttributes.Normal))
			{
				relativeFilePath = buffer.ToString().TrimStart('.', Path.DirectorySeparatorChar);
				return true;
			}
			else
			{
				relativeFilePath = null;
				return false;
			}
		}

		public static bool TryReadTextFile(string filePath, out string content)
		{
			try
			{
				content = File.ReadAllText(filePath);
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
				File.WriteAllText(filePath, content, new UTF8Encoding(withBOM));
				return true;
			}
			catch
			{
				return false;
			}
		}

		[DllImport("shlwapi.dll", CharSet = CharSet.Auto)]
		private static extern bool PathRelativePathTo([Out] StringBuilder relativePath, [In] string fromPath, [In] FileAttributes fromPathType, [In] string toPath, [In] FileAttributes toPathType);
	}
}
