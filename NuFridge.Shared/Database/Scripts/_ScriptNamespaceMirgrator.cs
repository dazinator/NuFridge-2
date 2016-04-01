using System;
using System.Data;
using System.Transactions;
using DbUp.Engine;
using IsolationLevel = System.Data.IsolationLevel;

namespace NuFridge.Shared.Database.Scripts
{
    // ReSharper disable once InconsistentNaming
    public class _ScriptNamespaceMirgrator : IScript
    {
        private const string OldNamespace = "NuFridge.Shared.Server.Storage.Scripts.NuFridge.Shared.Server.Storage.Scripts.";
        private const string IdParamName = "id";
        private const string ScriptNameParamName = "scriptName";
        private const string SchemaName = "NuFridge";
        private static readonly string TableNameWithSchema = $"[{SchemaName}].[Version]";
        private const string ScriptNameColumn = "ScriptName";
        private const string IdColumn = "Id";

        public string ProvideScript(Func<IDbCommand> commandFactory)
        {
            try
            {
                // Ensure the schema exists in the database.
                EnsureSchema(commandFactory, SchemaName);

                using (IDbCommand readCommand = commandFactory())
                {
                    readCommand.CommandText = $"SELECT [{IdColumn}], [{ScriptNameColumn}] FROM {TableNameWithSchema}";
                    using (IDataReader reader = readCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int id = int.Parse(reader[IdColumn].ToString());
                            string scriptName = reader[ScriptNameColumn].ToString();

                            if (scriptName.StartsWith(OldNamespace))
                            {
                                using (IDbCommand writeCommand = commandFactory())
                                {
                                    string scriptIdentifier = scriptName.Substring(OldNamespace.Length);
                                    string updatedScriptName = $"{GetType().Namespace}.{scriptIdentifier}";

                                    writeCommand.CommandText =
                                        $"UPDATE {TableNameWithSchema} SET [{ScriptNameColumn}] = @{ScriptNameParamName} WHERE [{IdColumn}] = @{IdParamName}";

                                    IDbDataParameter idParam = writeCommand.CreateParameter();
                                    idParam.ParameterName = IdParamName;
                                    idParam.Value = id;
                                    idParam.DbType = DbType.Int32;
                                    writeCommand.Parameters.Add(idParam);

                                    IDbDataParameter scriptNameParam = writeCommand.CreateParameter();
                                    scriptNameParam.ParameterName = ScriptNameParamName;
                                    scriptNameParam.Value = updatedScriptName;
                                    scriptNameParam.DbType = DbType.String;
                                    writeCommand.Parameters.Add(scriptNameParam);

                                    writeCommand.ExecuteNonQuery();
                                }
                            }
                        }
                    }
                }
            }
            catch (System.Data.SqlClient.SqlException e)
            {
                // table probablydoesn't exist.
                return "";
            }
            return "";
        }

        private void EnsureSchema(Func<IDbCommand> commandFactory, string schemaName)
        {
            using (IDbCommand ensureSchemaCommand = commandFactory())
            {
                var sql =
                $"IF NOT EXISTS(SELECT schema_name FROM information_schema.schemata WHERE schema_name = '{schemaName}') " +
                $"BEGIN " +
                $"EXEC sp_executesql N'CREATE SCHEMA {schemaName}' " +
                $"END";

                ensureSchemaCommand.CommandText = sql;
                ensureSchemaCommand.ExecuteNonQuery();
            }



        }
    }
}