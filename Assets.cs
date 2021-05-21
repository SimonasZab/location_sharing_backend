using location_sharing_backend.Models.Settings;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace location_sharing_backend
{
	public class Assets
	{
		private const string ASSETS_DIR = "Assets";
		private const string HTML_PAGES_DIR = "HtmlPages";
		private const string SECRET_DIR = "Secrets";

		public static readonly string UserValidationSuccessPage = LoadTextFile(ASSETS_DIR, HTML_PAGES_DIR, "UserValidationSuccessPage.html");
		public static readonly string UserValidationErrorPage = LoadTextFile(ASSETS_DIR, HTML_PAGES_DIR, "UserValidationErrorPage.html");
		public static readonly DbInfo DbInfo = LoadJson<DbInfo>(ASSETS_DIR, "dbInfo.json");
		public static readonly Secrets Secrets = LoadJson<Secrets>(ASSETS_DIR, SECRET_DIR, "secrets.json");
		public static readonly OtherSettings OtherSettings = LoadJson<OtherSettings>(ASSETS_DIR, "otherSettings.json");
		public static readonly string RegistartionEmailTemplate = LoadTextFile(ASSETS_DIR, HTML_PAGES_DIR, "RegistrationEmailTemplate.html");

		static Assets() { }

		private static Dictionary<string, string> LoadJsonDict(params string[] filePath)
		{
			return LoadJson<Dictionary<string, string>>(filePath);
		}

		private static T LoadJson<T>(params string[] filePath)
		{
			string textFromFile = LoadTextFile(filePath);
			T deserializedObj = JsonConvert.DeserializeObject<T>(textFromFile);
			return deserializedObj;
		}

		private static string LoadTextFile(params string[] filePath)
		{
			string completeFilePath = GetFullPathToLocalFile(filePath);
			return File.ReadAllText(completeFilePath);
		}

		private static string GetFullPathToLocalFile(params string[] paths) {
			string[] currentDirArr = { Directory.GetCurrentDirectory() };

			string completeFilePath = Path.Combine(currentDirArr.Concat(paths).ToArray());
			if (!File.Exists(completeFilePath))
			{
				throw new Exception($"{completeFilePath} does not exist");
			}
			return completeFilePath;
		}
	}
}
