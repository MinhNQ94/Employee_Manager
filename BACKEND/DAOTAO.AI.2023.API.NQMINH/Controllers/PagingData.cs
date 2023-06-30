using DAOTAO.AI._2023.API.NQMINH.Entities;

namespace DAOTAO.AI._2023.API.NQMINH.Controllers
{
    internal class PagingData<T>
    {
        public PagingData()
        {
        }

        public List<Employee> Data { get; set; }
        public long TotalCount { get; set; }
    }
}