﻿namespace CRM.Model
{
    /// <summary>
    /// Represents a data provider manager
    /// </summary>
    public partial interface IDataProviderManager
    {
        #region Properties

        /// <summary>
        /// Gets data provider
        /// </summary>
        IFAADDataProvider DataProvider { get; }

        #endregion
    }
}
