using BECOSOFT.Data.Models;
using BECOSOFT.Data.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace BECOSOFT.Data.Query {
    internal interface IBulkCopyHelper : IBaseService {
        void Handle(List<TempTable<object>> tempTables, SqlConnection connection, Func<DatabaseCommand, int> commandExecuter);
    }
}