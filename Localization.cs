#region Usings
using Sandbox.ModAPI.Ingame;
using System.Collections.Generic;
#endregion

namespace IngameScript
{
	internal partial class Program : MyGridProgram
	{
		private readonly string langDefault = "en";
		private string lang = "en";

		private readonly Dictionary<string, Dictionary<string, string>> langDict = new Dictionary<string, Dictionary<string, string>>
		{
			{
				"en", new Dictionary<string,string>
				{
					{ "CONFIGURATION_LOADED",                               "Configuration Loaded." },
					{ "CONFIGURATION_SAVED",                                "Configuration Saved." },

					{ "ERROR_SAVE_VERSION_MISMATCH",                        "Error: Save Version is newer than Script Version : '{0:D2}' > '{1:D2}'\nAborting..." }
				}
			},
			{
				"de", new Dictionary<string, string>
				{
				}
			},
			{
				"es", new Dictionary<string, string>
				{
				}
			}
		};

		/// <summary>
		/// Returns the localized version of <paramref name="key">key</paramref> or an empty string if not found
		/// </summary>
		/// <param name="key">idstring</param>
		/// <returns>string Localized String</returns>
		private string GetText(string key)
		{
			if (string.IsNullOrEmpty(key)) {
				return string.Empty;
			}

			if (langDict.ContainsKey(lang) && langDict[lang].ContainsKey(key)) {
				return langDict[lang][key];
			}
			else if (langDict["en"].ContainsKey(key)) {
				return string.Format("Untranslated Key: [{0}] for language [{1}]\n{2}", key, lang, langDict["en"][key]);
			}

			return string.Empty;
		}

		//
		// Helpers
		//

		private readonly string[] _activityStrings = new string[] { "    ", ".   ", " .  ", "  . ", "   ." };
		private int _activityCounter;

		private string GetActivityIndicator()
		{
			if (_activityCounter >= _activityStrings.Length) {
				_activityCounter = 0;
			}

			return _activityStrings[_activityCounter++];
		}
	}
}
