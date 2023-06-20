using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.Interface
{
    public interface IAdminService
    {
        List<PropertyModel> GetApprovedListData();
        
        bool ApproveOwnerProperty(int PropertyId);
        FormResponse BlockProperty(FormModel data);
        public void UnBlockProperty(int PropertyId);
        List<PropertyModel> GetListedPropertyList();
        List<PropertyModel> GetBlockedPropertyList();
        string GetDocument(int UserId);
        string GetPropertyPaper(int PropertyId);
        List<string> GetFlaggedCommentList(int CustomerId);

    }
}
