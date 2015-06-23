using System;
using System.Collections.Generic;
using System.Data;

namespace NuFridge.Shared.Server.Storage
{
    public interface ITransaction : IDisposable
    {
        TResult ExecuteScalar<TResult>(string query, CommandParameters args);

        void ExecuteReader(string query, Action<IDataReader> readerCallback);

        void ExecuteReader(string query, CommandParameters args, Action<IDataReader> readerCallback);

        IEnumerable<TDocument> ExecuteReader<TDocument>(string query, CommandParameters args);

        IEnumerable<TDocument> ExecuteReaderWithProjection<TDocument>(string query, CommandParameters args, Func<IProjectionMapper, TDocument> projectionMapper);

        IQueryBuilder<TDocument> Query<TDocument>() where TDocument : class;

        TDocument Load<TDocument>(int id) where TDocument : class;

        TDocument[] Load<TDocument>(IEnumerable<int> ids) where TDocument : class;

        TDocument LoadRequired<TDocument>(int id) where TDocument : class;

        TDocument[] LoadRequired<TDocument>(IEnumerable<int> ids) where TDocument : class;

        void Insert<TDocument>(TDocument instance) where TDocument : class;

        void Insert<TDocument>(string tableName, TDocument instance) where TDocument : class;


        void Update<TDocument>(TDocument instance) where TDocument : class;

        void Delete<TDocument>(TDocument instance) where TDocument : class;

        void Commit();

        void Rollback();
    }
}
