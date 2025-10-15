using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.SDK.Repository
{
    public class Repository<T> : BaseRepository<T, ApplicationDbContext> where T : class
    {
        public Repository(ApplicationDbContext applicationDbContext)
            : base(applicationDbContext)
        {

        }
    }
}
