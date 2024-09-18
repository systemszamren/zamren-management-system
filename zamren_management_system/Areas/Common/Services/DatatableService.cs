using zamren_management_system.Areas.Common.LinqExtensions;
using Microsoft.AspNetCore.Mvc;
using zamren_management_system.Areas.Common.Enums;
using zamren_management_system.Areas.Common.Interfaces;
using zamren_management_system.Areas.Common.Responses;

namespace zamren_management_system.Areas.Common.Services;

public class DatatableService : IDatatableService
{
    public JsonResult GetEntitiesForDatatable<T>(IFormCollection form, Func<T, bool>? filterFunc,
        Func<T, T>? selectFunc, List<T> list, CustomIdentityResult? response = null) where T : class
    {
        var start = Convert.ToInt32(form["start"].FirstOrDefault() ?? "0");
        var length = Convert.ToInt32(form["length"].FirstOrDefault() ?? "10");
        var draw = form["draw"].FirstOrDefault() ?? "";
        var sortColumnName = form["columns[" + form["order[0][column]"] + "][name]"].FirstOrDefault() ?? "";
        var sortDirection = form["order[0][dir]"].FirstOrDefault() ?? "asc";


        // Use the provided list instead of fetching from the DbContext
        var entities = list.AsQueryable();

        if (filterFunc != null)
        {
            entities = entities.AsEnumerable().Where(filterFunc).AsQueryable();
        }

        var totalRows = entities.Count();

        if (selectFunc != null)
        {
            entities = entities.AsEnumerable().Select(selectFunc).AsQueryable();
        }

        var entityList = entities.ToList();

        var totalRowsAfterFiltering = entityList.Count;

        var data = entityList.Skip(start).Take(length).ToList();

        if (!string.IsNullOrEmpty(sortColumnName))
        {
            data = (data.AsQueryable()
                        .OrderByDynamic(sortColumnName, sortDirection == "asc" ? Order.Asc : Order.Desc) ??
                    throw new InvalidOperationException()).ToList();
        }

        return new JsonResult(new
        {
            draw,
            recordsFiltered = totalRowsAfterFiltering,
            recordsTotal = totalRows,
            data,
            response
        });
    }
}