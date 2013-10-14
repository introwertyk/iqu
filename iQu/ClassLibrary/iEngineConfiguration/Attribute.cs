using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace iQu.iEngineConfiguration
{
	/// <summary>
	/// Class for attributes definition;
	/// Defines action category based on configuration stored in underyling DB.
	/// </summary>
	public class Attribute
	{
		#region Properties
		public string Name
		{
			get;
			set;
		}
		public string Definition
		{
			get;
			set;
		}
		#endregion
	}
}
