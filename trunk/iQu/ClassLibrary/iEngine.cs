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
	class iEngine
	{
		#region Properties
		private iQu.iEngineConfiguration.Configuration Configuration
		{
			get;
			set;
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

			}
			catch (Exception eX)
			{
				throw new Exception(string.Format("{0}::{1}", new StackFrame(0, true).GetMethod().Name, eX.Message));
			}
		}
		#endregion
	}
}
