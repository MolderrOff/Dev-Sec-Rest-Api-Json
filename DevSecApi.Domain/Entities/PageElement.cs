using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevSecApi.Domain.Entities;

public class PageElement
{
    public int Id { get; set; }
    public string AttributeValue { get; set; } = string.Empty;
    public string FullHtml { get; set; } = string.Empty;
}
