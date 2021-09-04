#region Usings
using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Text;
using VRage.Game.ModAPI.Ingame.Utilities;
#endregion

namespace IngameScript
{
	internal partial class Program : MyGridProgram
	{
		private readonly float scriptVersion = 1.0f;
		private readonly Dictionary<string, Action> userActions;
		private readonly Dictionary<string, Action<string>> userCommands;
		private readonly StringBuilder outputTerminal;
		private readonly StringBuilder outputLastMessage;
		private readonly MyIni ini;

		private readonly bool autoStart = true;
		private bool configLoaded;

		public Program()
		{
			// simple function call without paramaters
			userActions = new Dictionary<string, Action>(StringComparer.OrdinalIgnoreCase) {
				{ "load", Load },	// Load Configuration
				{ "save", Save },	// Save Configuration
				{ "run", Start },	// Start Execution (Continous)
				{ "stop", Stop }	// Halt Execution
			};

			// function call with single parameter
			userCommands = new Dictionary<string, Action<string>>(StringComparer.OrdinalIgnoreCase) {
				{ "run", Start },	// Start Execution (Continous)
			};

			ini = new MyIni();

			configLoaded = false;

			Runtime.UpdateFrequency = UpdateFrequency.Once;
		}

		public void SetMessage(string message)
		{
			outputLastMessage.Clear();
			outputLastMessage.Append(message);
		}
		public void ClearMessage()
		{
			outputLastMessage.Clear();
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
						}
					}
				}
			}
			else if ((UpdateSource & UpdateType.Update1) != 0) {
				// Run every tick
			}
			else if ((UpdateSource & UpdateType.Update10) != 0) {
				// Run every 10 ticks
			}
			else if ((UpdateSource & UpdateType.Update100) != 0) {
				// Run every 100 ticks

				outputTerminal.Clear();
				outputTerminal.AppendStringBuilder(outputLastMessage);
				outputTerminal.AppendLine();
				outputTerminal.AppendFormat("{0}", GetActivityIndicator());
			}
			else if ((UpdateSource & UpdateType.Once) != 0) {
				// Run just one time.

				if (!configLoaded) {
					Load();

					if (configLoaded && autoStart) {
						Start();
					}
				}
			}
		}

		public void Start()
		{
			Runtime.UpdateFrequency = UpdateFrequency.Update10 | UpdateFrequency.Update100;
		}

		public void Start(string param)
		{
			switch (param) {
				default:
					break;
			}

			Start();
		}

		public void Stop()
		{
			Runtime.UpdateFrequency = UpdateFrequency.None;
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

			SetMessage(GetText("CONFIGURATION_LOADED"));
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
			SetMessage(GetText("CONFIGURATION_SAVED"));
		}
	}
}
