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
        private const string TableNameWithSchema = "[NuFridge].[Version]";
        private const string ScriptNameColumn = "ScriptName";
        private const string IdColumn = "Id";

        public string ProvideScript(Func<IDbCommand> commandFactory)
        {
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

            return "";
        }
    }
}