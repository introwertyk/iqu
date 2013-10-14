using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace iQu.iEngineConfiguration
{
	/// <summary>
	/// Component configuration class.
	/// Initiated based on complete component DB configuration.
	/// </summary>
    public class Configuration
	{
		#region Properties
		public Dictionary<int, Dictionary<String, String>> Generic
		{
			get;
			private set;
		}
		public Dictionary<string, Attribute> Attributes
		{
			get;
			private set;
		}
		#endregion

		#region Constructors
		public Configuration()
		{	
			this.Generic = new Dictionary<int, Dictionary<string, string>>();
			this.Attributes = new Dictionary<string, Attribute>(StringComparer.OrdinalIgnoreCase);
		}
		#endregion
	}
}
