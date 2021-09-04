#region Usings
using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
#endregion

namespace IngameScript
{
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
