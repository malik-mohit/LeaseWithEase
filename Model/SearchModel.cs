using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class SearchModel
    {
        public int StateId { get; set; }
        public int CityId { get; set; }
        public List<PropertyModel> Property { get; set; } = new List<PropertyModel>();
    }
}
