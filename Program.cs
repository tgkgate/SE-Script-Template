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

		private readonly Dictionary<string, Action> userCommands;
		private readonly MyIni ini;

		private bool configLoaded;

		public Program()
		{
			userCommands = new Dictionary<string, Action>(StringComparer.OrdinalIgnoreCase) {
				{ "load", Load },
				{ "save", Save }
			};

			ini = new MyIni();

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

					if (userCommands.TryGetValue(cmd, out Action commandAction)) {
						commandAction();
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
			}

			saveVersion = ini.Get(sectionKey, "version").ToSingle(1.0f);

			// Sanity Check
			// Do not continue loading if save version is newer than script version
			// script will need updating.
			if (saveVersion > scriptVersion) {
				Echo(string.Format(GetText("ERROR_SAVE_VERSION_MISMATCH"), saveVersion, scriptVersion));

				Runtime.UpdateFrequency = UpdateFrequency.None;

				return;
			}

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

		private enum Language
		{
			en
		}

		private readonly Language lang = Language.en;
		
		private readonly Dictionary<Language, Dictionary<string, string>> langDict = new Dictionary<Language, Dictionary<string, string>>() {
			{
				Language.en, new Dictionary<string,string>() {
					#pragma warning disable format
					//
					// Language independant idstring									Localized string the idstring will be replace with
					//
					{ "ERROR_SAVE_VERSION_MISMATCH",									"Error: Save Version is newer than Script Version : '{0:D2}' > '{1:D2}'\nAborting..." }
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
