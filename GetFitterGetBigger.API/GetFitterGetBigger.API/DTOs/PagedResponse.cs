using System;
using System.Collections.Generic;

namespace GetFitterGetBigger.API.DTOs;

/// <summary>
/// Generic DTO for paginated responses
/// </summary>
/// <typeparam name="T">The type of items in the response</typeparam>
public class PagedResponse<T>
{
    /// <summary>
    /// The items for the current page
    /// </summary>
    public IEnumerable<T> Items { get; set; } = new List<T>();
    
    /// <summary>
    /// The current page number (1-based)
    /// </summary>
    public int CurrentPage { get; set; }
    
    /// <summary>
    /// The number of items per page
    /// </summary>
    public int PageSize { get; set; }
    
    /// <summary>
    /// The total number of items across all pages
    /// </summary>
    public int TotalCount { get; set; }
    
    /// <summary>
    /// The total number of pages
    /// </summary>
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    
    /// <summary>
    /// Indicates if there is a previous page
    /// </summary>
    public bool HasPreviousPage => CurrentPage > 1;
    
    /// <summary>
    /// Indicates if there is a next page
    /// </summary>
    public bool HasNextPage => CurrentPage < TotalPages;
    
    /// <summary>
    /// Creates a new paged response
    /// </summary>
    /// <param name="items">The items for the current page</param>
    /// <param name="currentPage">The current page number</param>
    /// <param name="pageSize">The page size</param>
    /// <param name="totalCount">The total count of items</param>
    public PagedResponse(IEnumerable<T> items, int currentPage, int pageSize, int totalCount)
    {
        Items = items;
        CurrentPage = currentPage;
        PageSize = pageSize;
        TotalCount = totalCount;
    }
    
    /// <summary>
    /// Default constructor for serialization
    /// </summary>
    public PagedResponse()
    {
    }
    
    /// <summary>
    /// Empty paged response instance following the Empty pattern
    /// </summary>
    public static PagedResponse<T> Empty => new()
    {
        Items = new List<T>(),
        CurrentPage = 0,
        PageSize = 0,
        TotalCount = 0
    };
}