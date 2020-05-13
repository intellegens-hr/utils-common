namespace Intellegens.Commons.Services
{
    public interface IDtoBase<TKey>
    {
        public TKey GetIdValue();

        public string GetIdPropertyName();
    }
}