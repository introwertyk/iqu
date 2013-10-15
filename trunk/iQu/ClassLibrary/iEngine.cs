using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;

using iMDirectory.iEngineConfiguration;
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
						try
						{
							ProcessNonHashedJob(oConnRes);
							oSql.ExecuteNonSqlQuery(String.Format("EXEC spSetChangeSuccessUpdate @iQueueID = {0}", oConnRes["iQueueID"]));
						}
						catch (Exception eX)
						{
							//commit failure to queue
							oSql.ExecuteNonSqlQuery(String.Format("EXEC spSetChangeFailureUpdate @iQueueID = {0}", oConnRes["iQueueID"]) );
							this.oEventLog.WriteEntry(String.Format("{0}::{1}", new StackFrame(0, true).GetMethod().Name, eX.Message), EventLogEntryType.Error, 3001);
						}
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
					//load generic configuration
					foreach (Dictionary<string, object> oConnRes in oSql.RetrieveData("EXEC spGetConfiguration"))
					{
						int iConnectorID = Convert.ToInt32(oConnRes["iConnectorID"]);
						int iObjectClassID = Convert.ToInt32(oConnRes["iObjectClassID"]);
						string KeyName = oConnRes["KeyName"].ToString();
						string KeyValue = oConnRes["KeyValue"].ToString();

						Dictionary<int, Dictionary<string, string>> ObjecClassDictionary;

						if (!this.Configuration.Generic.TryGetValue(iConnectorID, out ObjecClassDictionary))
						{
							ObjecClassDictionary = new Dictionary<int, Dictionary<string, string>>();
							this.Configuration.Generic.Add(iConnectorID, ObjecClassDictionary);
						}

						if (!this.Configuration.Generic[iConnectorID].ContainsKey(iObjectClassID))
						{
							this.Configuration.Generic[iConnectorID].Add(iObjectClassID, new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));
						}

						this.Configuration.Generic[iConnectorID][iObjectClassID].Add(KeyName, KeyValue);
					}

					//load attributes definition
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

				using (Sql oSql = new Sql(ConfigurationManager.ConnectionStrings["iMDirectory"].ConnectionString))
				{
					//Collect connector definitions
					foreach (Dictionary<string, object> oConnRes in oSql.RetrieveData("EXEC spGetTarget"))
					{
						int iConnectorID = Convert.ToInt32(oConnRes["iConnectorID"]);

						Connector oConnector;

						if (!this.Configuration.Connectors.TryGetValue(iConnectorID, out oConnector))
						{
							oConnector = new Connector();
							this.Configuration.Connectors.Add(iConnectorID, oConnector);
						}

						oConnector.ConnectorID = iConnectorID;
						oConnector.DomainFQDN = oConnRes["DomainFQDN"] == DBNull.Value ? String.Empty : oConnRes["DomainFQDN"].ToString();
						oConnector.Type = oConnRes["Type"] == DBNull.Value ? String.Empty : oConnRes["Type"].ToString();
						oConnector.Category = oConnRes["Category"] == DBNull.Value ? String.Empty : oConnRes["Category"].ToString();
						oConnector.Version = oConnRes["Version"] == DBNull.Value ? String.Empty : oConnRes["Version"].ToString();
						if (oConnRes["Port"] != DBNull.Value) oConnector.Port = Convert.ToInt32(oConnRes["Port"]);
						if (oConnRes["ProtocolVersion"] != DBNull.Value) oConnector.ProtocolVersion = Convert.ToInt32(oConnRes["ProtocolVersion"]);
						if (oConnRes["PageSize"] != DBNull.Value) oConnector.PageSize = Convert.ToInt32(oConnRes["PageSize"]);

						Dictionary<int, Dictionary<string, object>> dicConfiguration = new Dictionary<int, Dictionary<string, object>>();

						//Collect connector k/v configuration
						foreach (Dictionary<string, object> oConfRes in oSql.RetrieveData(String.Format("EXEC spGetTargetConfiguration @iConnectorID={0}", iConnectorID)))
						{
							string sKey = oConfRes["KeyName"] == DBNull.Value ? String.Empty : oConfRes["KeyName"].ToString();
							object oVal = oConfRes["KeyValue"] == DBNull.Value ? null : oConfRes["KeyValue"];

							oConnector.Configuration.Add(sKey, oVal);
						}

						//Collect classes definition
						foreach (Dictionary<string, object> oClassRes in oSql.RetrieveData(String.Format("EXEC spGetTargetClasses @iConnectorID={0}", iConnectorID)))
						{
							int iObjectClassID = Convert.ToInt32(oClassRes["iObjectClassID"]);

							Class oObjectClass;

							if (!this.Configuration.Classes.TryGetValue(iObjectClassID, out oObjectClass))
							{
								oObjectClass = new Class();
								this.Configuration.Classes.Add(iObjectClassID, oObjectClass);
								oConnector.ObjectClasses.Add(oObjectClass);
							}

							oObjectClass.ObjectClassID = iObjectClassID;
							oObjectClass.ObjectClass = oClassRes["ObjectClass"].ToString();
							oObjectClass.TableContext = oClassRes["TableContext"].ToString();
							oObjectClass.Filter = oClassRes["Filter"].ToString();
							oObjectClass.OtherFilter = oClassRes["OtherFilter"].ToString();
							oObjectClass.SearchRoot = oClassRes["SearchRoot"] == DBNull.Value
								? null
								: oClassRes["SearchRoot"].ToString();
						}
					}

					//create parent/child connector relationships
					foreach (Dictionary<string, object> oConnRes in oSql.RetrieveData("EXEC spGetTarget"))
					{
						int iParentConnectorID = oConnRes["iParentConnectorID"] == DBNull.Value ? 0 : Convert.ToInt32(oConnRes["iParentConnectorID"]);
						int iChildConnectorID = oConnRes["iConnectorID"] == DBNull.Value ? 0 : Convert.ToInt32(oConnRes["iConnectorID"]);

						Connector oParentConnector;
						Connector oChildConnector;
						if (this.Configuration.Connectors.TryGetValue(iParentConnectorID, out oParentConnector))
						{
							if (this.Configuration.Connectors.TryGetValue(iChildConnectorID, out oChildConnector))
							{
								oParentConnector.ChildConnectors.Add(oChildConnector);
								oChildConnector.ParrentConnector = oParentConnector;
							}
						}
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
		private void ProcessNonHashedJob(Dictionary<string, object> DatabaseRecord)
		{
			try
			{
				int iObjectClassID = Convert.ToInt32(DatabaseRecord["iObjectClassID"]);
				Int64 ObjectID = Convert.ToInt64(DatabaseRecord["iObjectID"]);
				//Generic
				//If UseMetadata True; Retrieve distinguishedName from iMDirectory
				//If UseMetadata False; Retrieve objectGuid from iMDirectory then DN from AD
				using (Sql oSql = new Sql(ConfigurationManager.ConnectionStrings["iMDirectory"].ConnectionString))
				{
					string SqlQuery = String.Format(@"
SELECT objectGuid
FROM [{0}]
WHERE
	_iObjectClassID={1}
AND
	_iObjectID={2}",
this.Configuration.Classes[iObjectClassID].TableContext,
iObjectClassID,
ObjectID);

					oSql.RetrieveData(SqlQuery);
				}

			}
			catch (Exception eX)
			{
				throw new Exception(string.Format("{0}::{1}", new StackFrame(0, true).GetMethod().Name, eX.Message));
			}
		}

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
