using System;
using System.IO;

namespace GATUtils.Utilities.FileUtils
{
	public enum Dir {Root, Io, ApplicationSettings, Fix, FixSessionData, FixLogs, Temp, ApplicationLogs, Data, ThirdPartyData, ExternalLibaries, ExternalData };

	public static class GatFile
	{
		public static string AppDirectory { get { return s_applicationDir; } }

		static GatFile()
		{
			DirectoryInfo currentDirectory = new DirectoryInfo(Environment.CurrentDirectory); //this is the assembly path
			
			s_applicationDir = currentDirectory.FullName; 
			_CreateDefaultDirs();
		}

		public static string FormatFolderPath(string folder)
		{
			if (folder.EndsWith("\\")) return folder;
			return string.Format("{0}\\", folder);
		}

		public static bool TryCreateDirIfNeeded(string path)
		{
			Exception e;
			if (!_TryCreateDirIfNeeded(path, out e))
			{
				//GeneralLogger.Instance.AddLine("BullseyeFile: Failed to create directory {0} , exception: {1}",
				//                       path ?? "Null", e);
				return false;
			}

			return true;
		}

		public static string Path(Dir dir, string subPath, string filename)
		{
			return Path(s_mySubDirectoryPaths[(int)dir], subPath, filename);
		}

		public static string Path(Dir dir, string filename)
		{
			return Path(dir, null, filename);
		}

		public static string GetDirectoryPath(Dir dir)
		{
			return Path(dir, null, null);
		}

		public static string Path(string path, string subPath, string filename)
		{
			// If this is relative path, add product directory)
			if (path.Length == 0 || path.StartsWith("\\"))
			{
				path = s_applicationDir + path.TrimEnd('\\');
			}

			path = FormatFolderPath(path);

			if (!string.IsNullOrEmpty(subPath))
			{
				path += subPath.Trim('\\') + @"\";
			}
				
			TryCreateDirIfNeeded(path);

			if (!string.IsNullOrEmpty(filename))
			{
				path += filename;
			}

			return path;
		}

	    public static void DeleteFileIfExists(string path)
	    {
	        if (File.Exists(path))
	        {
	            File.Delete(path);
	        }
	    }

		private static bool _TryCreateDirIfNeeded(string fullPath, out Exception exception)
		{
			
			try
			{
				exception = null;
				string folderName = System.IO.Path.GetDirectoryName(fullPath);

				if (!string.IsNullOrEmpty(folderName))
				{
					Directory.CreateDirectory(folderName);
				}
				return true;
			}
			catch (IOException e)
			{
				exception = e;
				return false;
			}
			catch (UnauthorizedAccessException e)
			{
				exception = e;
				return false;
			}
			catch (ArgumentException e)
			{
				exception = e;
				return false;
			}
			catch (NotSupportedException e)
			{
				exception = e;
				return false;
			}
		}

		private static void _CreateDefaultDirs()
		{
			foreach (Dir dir in s_defaultDirs)
			{
				TryCreateDirIfNeeded(Path(s_mySubDirectoryPaths[(int)dir], null, ""));
			}
		}

		private static void _AddSubDir(string path)
		{
			TryCreateDirIfNeeded(path);
		}

		private static readonly string[] s_mySubDirectoryPaths =
		{
			"\\",														/* Root */
			"\\IO\\",													/* IO */
			"\\IO\\AppSettings\\",                                      /* Application Settings */
			"\\IO\\Fix\\",                                              /* FIX */
			"\\IO\\Fix\\Session\\",                                     /* FIX Session persistence */
			"\\IO\\Fix\\Logs\\",                                        /* Fix Session Log */
			"\\IO\\Temp\\",						                        /* Temp */
			"\\IO\\Log\\",			        				    		/* Logs */
			"\\IO\\Data\\",                                             /* Needed Data files */
			"\\ThirdParty\\",                                           /* Holds external Libraries And Data files */
			"\\ThirdParty\\Lib\\",                                      /* ThirdParty DLLs */
			"\\ThirdParty\\Data\\",                                     /* ThirdParty data source */
		};

		// Private Properties
		// -----------------
		private static readonly string s_applicationDir;
		private static readonly Dir[] s_defaultDirs = { Dir.Io, Dir.ApplicationSettings, Dir.Temp, Dir.ApplicationLogs, Dir.Data, Dir.ExternalLibaries, Dir.ExternalData };
	}
}
