using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevSecApi.Domain.Entities;

namespace DevSecApi.Domain.Repositories;

public  interface IPayloadRepository : IRepository<Payload>
{
    Task<Payload?> GetByIdAsync(Guid id);
    Task SaveHtmlElementAsync(long id, string attributeValue, string fullHtml);
    Task AddRangeAsync(IEnumerable<DevSecApi.Domain.Entities.PageElement> entities);
}
