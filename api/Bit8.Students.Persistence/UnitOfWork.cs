using System.Data;
using Bit8.Students.Common;
using Bit8.Students.Persistence.Repositories;
using MySql.Data.MySqlClient;

namespace Bit8.Students.Persistence
{
    public class UnitOfWork : IUnitOfWork
    {
        private IDbConnection _connection;
        private IDbTransaction _transaction;

        private IDisciplineRepository _disciplineRepository;
        private ISemesterRepository _semesterRepository;

        public UnitOfWork(IBConfiguration configuration)
        {
            _connection = new MySqlConnection(configuration.ConnectionString);
            _connection.Open();
            _transaction = _connection.BeginTransaction();
        }
        
        public void Dispose()
        {
            _transaction?.Dispose();
            _connection?.Dispose();
        }

        public IDisciplineRepository DisciplineRepository => _disciplineRepository ?? (_disciplineRepository = new DisciplineRepository(_transaction));
        public ISemesterRepository SemesterRepository => _semesterRepository ?? (_semesterRepository = new SemesterRepository(_transaction));

        public void Commit()
        {
            try
            {
                _transaction.Commit();
            }
            catch
            {
                _transaction.Rollback();
                throw;
            }
            finally
            {
                _transaction.Dispose();
                _transaction = _connection.BeginTransaction();
            }
        }
    }
}