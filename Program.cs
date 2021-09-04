#region Usings
using System;
using System.Collections.Generic;
using System.Text;

using Sandbox.ModAPI.Ingame;

using VRage;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.Game.ModAPI.Ingame.Utilities;

using VRageMath;
using static VRage.Game.MyDefinitionErrors;
#endregion

//

namespace IngameScript
{
	internal partial class Program : MyGridProgram
	{
		private readonly float scriptVersion = 1.0f;

		private readonly Dictionary<string, Action> userCommands;
		private readonly MyIni ini;

		public Program()
		{
			userCommands = new Dictionary<string, Action>(StringComparer.OrdinalIgnoreCase) {
				{ "load", Load },
				{ "save", Save }
			};

			ini = new MyIni();

			Load();

			Runtime.UpdateFrequency = UpdateFrequency.Once;
		}

		public void Main(string Arguments, UpdateType UpdateSource)
		{
			if ((UpdateSource & UpdateType.Terminal) != 0) {
				MyCommandLine CommandLine = new MyCommandLine();

				if (CommandLine.TryParse(Arguments)) {
					string cmd = CommandLine.Argument(0).ToLower();

					if (userCommands.TryGetValue(cmd, out Action commandAction)) {
						commandAction();
					}
				}

				return;
			}
			else if ((UpdateSource & UpdateType.Update1) != 0) {
			}
			else if ((UpdateSource & UpdateType.Update10) != 0) {
			}
			else if ((UpdateSource & UpdateType.Update100) != 0) {
			}
		}

		public void Load()
		{
			string sectionKey = "Settings";

			if (string.IsNullOrWhiteSpace(Me.CustomData) || !ini.TryParse(Me.CustomData)) {
				ini.Clear();

				ini.AddSection(sectionKey);
				ini.Set(sectionKey, "version", scriptVersion);
			}

			if (MyIni.HasSection(Me.CustomData, sectionKey) && ini.TryParse(Me.CustomData)) {
				float saveVersion;

				saveVersion = ini.Get(sectionKey, "version").ToSingle(1.0f);

				if (saveVersion > scriptVersion) {
					Echo(string.Format(GetText("ERROR_SAVE_VERSION_MISMATCH"), saveVersion, scriptVersion));
				}
			}
		}

		public void Save()
		{
			Me.CustomData = ini.ToString();
		}

		//
		// - Language / Localization
		//

		private enum Language
		{
			en
		}

		private readonly Language lang = Language.en;
		
		private readonly Dictionary<Language, Dictionary<string, string>> langDict = new Dictionary<Language, Dictionary<string, string>>() {
			{
				Language.en, new Dictionary<string,string>() {
					#pragma warning disable format
					{"ERROR_SAVE_VERSION_MISMATCH",					"Error: Save Version is newer than Script Version : '{0:D2}' > '{1:D2}'" }
					#pragma warning restore format
				}
			}
		};

		private string GetText(string key)
		{
			if (langDict.ContainsKey(lang) && langDict[lang].ContainsKey(key)) {
				return langDict[lang][key];
			}

			return string.Empty;
		}
	}

	/// <summary>
	/// Class containing extension methods for the following classes<br/>
	/// <br/>
	/// GridTerminalSystem
	/// </summary>
	public static class Extensions
	{
		private static readonly List<IMyTerminalBlock> blockCache = new List<IMyTerminalBlock>();

		public static T GetBlockOfType<T>(this IMyGridTerminalSystem gts, Func<T, bool> collect = null) where T : class
		{
			blockCache.Clear();
			gts.GetBlocksOfType<T>(blockCache as List<T>, collect);

			return blockCache.Count > 0 ? blockCache[0] as T : null;
		}

		public static T GetBlockOfTypeWithName<T>(this IMyGridTerminalSystem gts, string name) where T : class
		{
			return GetBlockOfType<IMyTerminalBlock>(gts, block => block is T && block.CustomName.StartsWith(name)) as T;
		}

		public static T GetBlockOfTypeWithName<T>(this IMyGridTerminalSystem gts, string name, IMyTerminalBlock anyBlock = null) where T : class
		{
			if (anyBlock == null) {
				return GetBlockOfType<IMyTerminalBlock>(gts, block => block is T && block.CustomName.StartsWith(name)) as T;
			}

			return GetBlockOfType<IMyTerminalBlock>(gts, block => block is T && block.IsSameConstructAs(anyBlock) && block.CustomName.StartsWith(name)) as T;
		}
	}
}
