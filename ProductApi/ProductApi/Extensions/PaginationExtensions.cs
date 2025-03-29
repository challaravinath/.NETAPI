using ProductApi.Models;

namespace ProductApi.Extensions
{
    public static class PaginationExtensions
    {
        public static void AppendPaginationHeaders<T>(this HttpResponse response, PagedResult<T> pagedResult)
        {
            response.Headers.Append("X-Pagination-TotalCount", pagedResult.TotalCount.ToString());
            response.Headers.Append("X-Pagination-PageSize", pagedResult.PageSize.ToString());
            response.Headers.Append("X-Pagination-CurrentPage", pagedResult.PageNumber.ToString());
            response.Headers.Append("X-Pagination-TotalPages", pagedResult.TotalPages.ToString());
            response.Headers.Append("X-Pagination-HasPrevPage", pagedResult.HasPreviousPage.ToString());
            response.Headers.Append("X-Pagination-HasNextPage", pagedResult.HasNextPage.ToString());
        }
    }
}
