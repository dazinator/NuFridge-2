using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuFridge.Shared.Database.Model;
using NuFridge.Shared.Database.Repository;

namespace NuFridge.Shared.Database.Services
{
    public class FrameworkService : IFrameworkService
    {
        private readonly IFrameworkRepository _frameworkRepository;

        public FrameworkService(IFrameworkRepository frameworkRepository)
        {
            _frameworkRepository = frameworkRepository;
        }

        public IEnumerable<Framework> GetAll()
        {
            return _frameworkRepository.GetAll();
        }

        public void Insert(Framework framework)
        {
            _frameworkRepository.Insert(framework);
        }
    }

    public interface IFrameworkService
    {
        IEnumerable<Framework> GetAll();
        void Insert(Framework framework);
    }
}