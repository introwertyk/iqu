using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using iMDirectory.iEngineConfiguration;

namespace iQu.iEngineConfiguration
{
	/// <summary>
	/// Component configuration class.
	/// Initiated based on complete component DB configuration.
	/// </summary>
    public class Configuration
	{
		#region Properties
		public Dictionary<int, Dictionary<int, Dictionary<String, String>>> Generic
		{
			get;
			private set;
		}
		public Dictionary<string, Attribute> Attributes
		{
			get;
			private set;
		}
		public Dictionary<int, Connector> Connectors
		{
			get;
			private set;
		}
		public Dictionary<int, Class> Classes
		{
			get;
			private set;
		}
		#endregion

		#region Constructors
		public Configuration()
		{	
			this.Generic = new Dictionary<int,Dictionary<int,Dictionary<string,string>>>();
			this.Attributes = new Dictionary<string, Attribute>(StringComparer.OrdinalIgnoreCase);
			this.Connectors = new Dictionary<int, Connector>();
			this.Classes = new Dictionary<int, Class>();
		}
		#endregion
	}
}
