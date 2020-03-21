

using System;

namespace CRM.Model
{
    /// <summary>
    /// Represents the data provider manager
    /// </summary>
    public partial class DataProviderManager : IDataProviderManager
    {
        #region Methods

        /// <summary>
        /// Gets data provider by specific type
        /// </summary>
        /// <param name="dataProviderType">Data provider type</param>
        /// <returns></returns>
        public static IFAADDataProvider GetDataProvider(DataProviderType dataProviderType)
        {
            switch (dataProviderType)
            {
                case DataProviderType.SqlServer:
                    return new MsSqlFAADDataProvider();
                default:
                    throw new Exception($"Not supported data provider name: '{dataProviderType}'");
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets data provider
        /// </summary>
        public IFAADDataProvider DataProvider
        {
            get
            {

                return GetDataProvider(DataProviderType.SqlServer);
            }
        }

        #endregion
    }
}
