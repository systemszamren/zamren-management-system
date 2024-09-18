using Microsoft.AspNetCore.Mvc;
using zamren_management_system.Areas.Common.Responses;

namespace zamren_management_system.Areas.Common.Interfaces;

public interface IDatatableService
{
    /// <summary>
    /// Retrieves entities for a datatable.
    /// </summary>
    /// <param name="form">The form collection containing the datatable parameters.</param>
    /// <param name="filterFunc">The function to filter the entities.</param>
    /// <param name="selectFunc">The function to select the entities.</param>
    /// <param name="list">The list of entities.</param>
    /// <param name="customResponse"> 'Optional' The output custom response.</param>
    /// <returns>A JsonResult containing the datatable data.</returns>
    JsonResult GetEntitiesForDatatable<T>(IFormCollection form, Func<T, bool>? filterFunc,
        Func<T, T>? selectFunc, List<T> list, CustomIdentityResult? customResponse = null) where T : class;
}