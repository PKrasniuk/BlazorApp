namespace BlazorApp.Common.Models
{
    public class PaginationDetails<T>
    {
        public int? PageIndex { get; set; }

        public int? PageSize { get; set; }

        public T SyncPointReference { get; set; }

        public int? CollectionSize { get; set; }
    }
}