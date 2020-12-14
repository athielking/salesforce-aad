using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Odata.Data
{
    public class AadConnectionInterceptor : DbConnectionInterceptor
    {
        private readonly AadSqlTokenProvider _tokenProvider;

        public AadConnectionInterceptor(AadSqlTokenProvider tokenProvider)
        {
            _tokenProvider = tokenProvider;
        }
        public override async Task<InterceptionResult> ConnectionOpeningAsync(DbConnection connection, ConnectionEventData eventData, InterceptionResult result, CancellationToken cancellationToken = default)
        {
            var sqlConnection = (SqlConnection)connection;
            if(NeedsAccessToken(sqlConnection))
            {
                var token = await _tokenProvider.GetAccessTokenAsync(cancellationToken);
                sqlConnection.AccessToken = token;
            }

            return await base.ConnectionOpeningAsync(connection, eventData, result, cancellationToken);
        }


        private bool NeedsAccessToken(SqlConnection connection)
        {
            var connectionStringBuilder = new SqlConnectionStringBuilder(connection.ConnectionString);

            return connectionStringBuilder.DataSource.Contains("database.windows.net", StringComparison.OrdinalIgnoreCase) && string.IsNullOrEmpty(connectionStringBuilder.UserID);
        }
    }
}
