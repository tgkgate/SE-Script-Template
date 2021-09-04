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
#endregion

//

namespace IngameScript
{
	internal partial class Program : MyGridProgram
	{
		private readonly float scriptVersion = 1.0f;

		private readonly Dictionary<string, Action> userActions;
		private readonly Dictionary<string, Action<string>> userCommands;
		private readonly MyIni ini;

		private bool configLoaded;

		public Program()
		{
			// simple function call without paramaters
			userActions = new Dictionary<string, Action>(StringComparer.OrdinalIgnoreCase) {
				{ "load", Load },
				{ "save", Save }
			};

			// function call with single parameter
			userCommands = new Dictionary<string, Action<string>>(StringComparer.OrdinalIgnoreCase) {
			};

			ini = new MyIni();

			configLoaded = false;

			Runtime.UpdateFrequency = UpdateFrequency.Once;
		}

		public void Main(string Arguments, UpdateType UpdateSource)
		{
			if ((UpdateSource & UpdateType.Terminal) != 0) {
				// Run when a terminal command or toolbar action triggers
				// the programmable block 'Run' action
				MyCommandLine CommandLine = new MyCommandLine();

				if (CommandLine.TryParse(Arguments)) {
					string cmd = CommandLine.Argument(0).ToLower();

					if (userActions.TryGetValue(cmd, out Action action)) {
						action();
					}
					else if (userCommands.TryGetValue(cmd, out Action<string> actionString)) {
						if (CommandLine.ArgumentCount == 2) {
							actionString(CommandLine.Argument(2));
							throw new ArgumentException();
						}
					}
				}

				return;
			}
			else if ((UpdateSource & UpdateType.Update1) != 0) {
				// Run every tick
			}
			else if ((UpdateSource & UpdateType.Update10) != 0) {
				// Run every 10 ticks
			}
			else if ((UpdateSource & UpdateType.Update100) != 0) {
				// Run every 100 ticks
			}
			else if ((UpdateSource & UpdateType.Once) != 0) {
				// Run just one time.

				// Useful for updating configuration data
				if (!configLoaded) {
					Load();
				}
			}
		}

		/// <summary>
		/// Load configuration data from customdata
		/// </summary>
		/// <remarks>
		/// Will not preserve pre-existing non-ini.data
		/// </remarks>
		public void Load()
		{
			string sectionKey = "Script Settings";
			float saveVersion;

			ini.Clear();

			if (string.IsNullOrWhiteSpace(Me.CustomData) || !ini.TryParse(Me.CustomData)) {
				ini.AddSection(sectionKey);
				ini.Set(sectionKey, "version", scriptVersion);
				ini.Set(sectionKey, "language", lang);
			}

			saveVersion = ini.Get(sectionKey, "version").ToSingle();

			// Sanity Check
			// Do not continue loading if save version is newer than script version
			// script will need updating.
			if (saveVersion > scriptVersion) {
				Echo(string.Format(GetText("ERROR_SAVE_VERSION_MISMATCH"), saveVersion, scriptVersion));

				Runtime.UpdateFrequency = UpdateFrequency.None;

				return;
			}

			lang = ini.Get(sectionKey, "language").ToString(langDefault);

			configLoaded = true;
		}

		/// <summary>
		/// Save configuration to custom data<br/>
		/// </summary>
		/// <remarks>
		/// Will not preserve pre-existing non-ini.data
		/// </remarks>
		public void Save()
		{
			Me.CustomData = ini.ToString();
		}

		//
		// - Language / Localization
		//

		private readonly string langDefault = "en";
		private string lang = "en";

		/// <summary>
		/// 
		/// </summary>
		private readonly Dictionary<string, Dictionary<string, string>> langDict = new Dictionary<string, Dictionary<string, string>>() {
			{
				"en", new Dictionary<string,string>() {
					#pragma warning disable format
					//
					// Language independant idstring									Localized string the idstring will be replace with
					//
					{ "ERROR_SAVE_VERSION_MISMATCH",									"Error: Save Version is newer than Script Version : '{0:D2}' > '{1:D2}'\nAborting..." }
					#pragma warning restore format
				}
			}
		};

		/// <summary>
		/// Returns the localized version of 'key' or an empty string if not found
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

			return string.Empty;
		}

		//
		// Output Helpers
		//

		/// <summary>
		/// This is an internal counter for the little . .. ... .... indicator
		/// </summary>
		private int _activityCounter;

		/// <summary>
		/// Returns an ever changing string to let the user know the script is "working"
		/// </summary>
		/// <returns>string Indicator</returns>
		private string GetActivityIndicator()
		{
			string[] strs = { ".   ", " .  ", "  . ", "   ." };

			if (_activityCounter >= strs.Length) {
				_activityCounter = 0;
			}

			return strs[_activityCounter++];
		}
	}

	//
	// - Extensions
	//

	/// <summary>
	/// Class containing extension methods for the following classes<br/>
	/// <br/>
	/// GridTerminalSystem
	/// </summary>
	public static class Extensions
	{
		private static readonly List<IMyTerminalBlock> blockCache = new List<IMyTerminalBlock>();

		/// <summary>
		/// Get a single block of type T, filtered by Func<br/>
		/// </summary>
		/// <remarks>
		/// Note: uses an internal block cache to prevent new allocations. Cache is regenerated on every call.
		/// </remarks>
		/// <typeparam name="T">type of the block to return</typeparam>
		/// <param name="gts">reference to GridTerminalSystem</param>
		/// <param name="collect">function to determin if a block should be added to collection</param>
		/// <returns>T block</returns>
		public static T GetBlockOfType<T>(this IMyGridTerminalSystem gts, Func<T, bool> collect = null) where T : class
		{
			blockCache.Clear();
			gts.GetBlocksOfType<T>(blockCache as List<T>, collect);

			return blockCache.Count > 0 ? blockCache[0] as T : null;
		}

		/// <summary>
		/// Get a single block to type T, whose name contains 'name'
		/// </summary>
		/// <typeparam name="T">type of the block to return</typeparam>
		/// <param name="gts">reference to GridTerminalSystem</param>
		/// <param name="name">name of the block to search for</param>
		/// <returns>T block</returns>
		public static T GetBlockOfTypeWithName<T>(this IMyGridTerminalSystem gts, string name) where T : class
		{
			return GetBlockOfType<IMyTerminalBlock>(gts, block => block is T && block.CustomName.Contains(name)) as T;
		}

		/// <summary>
		/// Get a single block to type T, whose name contains <see cref="param name">name</see>, and exists on the same grid (or sub-grid) as 'anyBlock'<br/>
		/// </summary>
		/// <remarks>
		/// sub-grids are those mechanically connected to a grid (via rotors, pistons, etc),<br/>
		/// but not including those connected by connectors.
		/// </remarks>
		/// <typeparam name="T">type of the block to return</typeparam>
		/// <param name="gts">reference to GridTerminalSystem</param>
		/// <param name="name">name of the block to search for</param>
		/// <param name="anyBlock">any block existing on the "grid" to filter with</param>
		/// <returns>T block</returns>
		public static T GetBlockOfTypeWithName<T>(this IMyGridTerminalSystem gts, string name, IMyTerminalBlock anyBlock = null) where T : class
		{
			if (anyBlock == null) {
				return GetBlockOfType<IMyTerminalBlock>(gts, block => block is T && block.CustomName.StartsWith(name)) as T;
			}

			return GetBlockOfType<IMyTerminalBlock>(gts, block => block is T && block.IsSameConstructAs(anyBlock) && block.CustomName.Contains(name)) as T;
		}
	}
}
