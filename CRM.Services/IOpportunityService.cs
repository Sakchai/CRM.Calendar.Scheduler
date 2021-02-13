using System.Data;

namespace CRM.Services
{
    public interface IOpportunityService
    {
        DataTable OpportunityReport();
        DataTable OpportunityActivityReport();
        DataTable OpportunityStageProgress();
    }
}
