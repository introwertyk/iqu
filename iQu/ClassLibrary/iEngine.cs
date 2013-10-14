using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;

using iCOR3.iSqlDatabase;

namespace iQu
{
	/// <summary>
	/// Main iQu class. Initiates global update process.
	/// </summary>
	public class iEngine : IDisposable
	{
		#region Properties
		protected EventLog oEventLog;
		bool bDisposed;

		private iQu.iEngineConfiguration.Configuration Configuration
		{
			get;
			set;
		}
		#endregion

		#region Constructors
		public iEngine()
		{
			this.bDisposed = false;
			this.oEventLog = new System.Diagnostics.EventLog("Application", ".", System.Reflection.Assembly.GetExecutingAssembly().GetName().Name);
			this.SetConfiguration();
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Starts processing queue.
		/// </summary>
		public void Start()
		{
			try
			{
				using (Sql oSql = new Sql(ConfigurationManager.ConnectionStrings["iQu"].ConnectionString))
				{
					foreach (Dictionary<string, object> oConnRes in oSql.RetrieveData("EXEC spGetTransactionsToWorker"))
					{

					}
				}
 
			}
			catch (Exception eX)
			{
				this.oEventLog.WriteEntry(String.Format("{0}::{1}", new StackFrame(0, true).GetMethod().Name, eX.Message), EventLogEntryType.Error, 3060);
			}
		}
		#endregion

		#region Private Methods
		/// <summary>
		/// Loads configuration from underlying DB into iEngineConfiguration library classes instances.
		/// </summary>
		private void SetConfiguration()
		{
			try
			{
				this.Configuration = new iEngineConfiguration.Configuration();

				using (Sql oSql = new Sql(ConfigurationManager.ConnectionStrings["iQu"].ConnectionString))
				{
					foreach (Dictionary<string, object> oConnRes in oSql.RetrieveData("EXEC spGetConfiguration"))
					{
						int iObjectClassID = Convert.ToInt32(oConnRes["iObjectClassID"]);
						string KeyName = oConnRes["KeyName"].ToString();
						string KeyValue = oConnRes["KeyValue"].ToString();

						Dictionary<string,string> KeyValueCollection;

						if (!this.Configuration.Generic.TryGetValue(iObjectClassID, out KeyValueCollection))
						{
							KeyValueCollection = new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase);
							this.Configuration.Generic.Add(iObjectClassID, KeyValueCollection);
						}

						if (!this.Configuration.Generic[iObjectClassID].ContainsKey(KeyName) )
						{
							this.Configuration.Generic[iObjectClassID].Add(KeyName, KeyValue);
						}
					}

					foreach (Dictionary<string, object> oConnRes in oSql.RetrieveData("EXEC spGetAttributesDefinition"))
					{
						string AttributeName = oConnRes["Attribute"].ToString();
						string Definition = oConnRes["Definition"].ToString();

						iQu.iEngineConfiguration.Attribute AttributeObject;

						if (!this.Configuration.Attributes.TryGetValue(AttributeName, out AttributeObject))
						{
							AttributeObject = new iEngineConfiguration.Attribute();
							this.Configuration.Attributes.Add(AttributeName, AttributeObject);
						}

						AttributeObject.Name = AttributeName;
						AttributeObject.Definition = Definition;
					}
				}
			}
			catch (Exception eX)
			{
				throw new Exception(string.Format("{0}::{1}", new StackFrame(0, true).GetMethod().Name, eX.Message));
			}
		}

		/// <summary>
		/// Start generic worker for non-hashed transactions
		/// </summary>
		private 
		#endregion

		#region IDisposable Members
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool bDisposing)
		{
			if (!this.bDisposed)
			{
				if (bDisposing)
				{

				}

				this.bDisposed = true;
			}
		}
		#endregion
	}
}
