using Intellegens.Commons.Services;

namespace Intellegens.Commons.DemoApi.Dto
{
    public class BaseDto : IDtoBase<int>
    {
        public int Id { get; set; }

        public string GetIdPropertyName()
        {
            return nameof(Id);
        }

        public int GetIdValue()
        {
            return Id;
        }
    }
}