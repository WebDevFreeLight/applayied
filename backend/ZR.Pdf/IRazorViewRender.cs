using System.Collections.Generic;
using System.Threading.Tasks;

namespace ZR.Pdf.Mvc;

/// <summary>
///     ViewView operation
/// </summary>
public interface IRazorViewRender
{
    /// <summary>
    ///     View data bind and return
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    /// <param name="viewName"></param>
    /// <param name="model"></param>
    /// <returns></returns>
    Task<string> RenderViewToStringAsync<TModel>(string viewName, TModel model);

    /// <summary>
    /// RenderViewToStringAsync
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    /// <param name="viewName"></param>
    /// <param name="model"></param>
    /// <param name="viewLocations"></param>
    /// <returns></returns>
    Task<string> RenderViewToStringAsync<TModel>(string viewName, TModel model, List<string> viewLocations);

    /// <summary>
    /// RenderRazorViewToString
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    /// <param name="controller"></param>
    /// <param name="viewName"></param>
    /// <param name="model"></param>
    /// <returns></returns>

    Task<string> RenderRazorViewToString<TModel>(Microsoft.AspNetCore.Mvc.Controller controller, string viewName, TModel model);
    /// <summary>
    /// RenderRazorViewToString
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    /// <param name="controller"></param>
    /// <param name="viewName"></param>
    /// <param name="model"></param>
    /// <param name="viewLocations"></param>
    /// <returns></returns>
    Task<string> RenderRazorViewToString<TModel>(Microsoft.AspNetCore.Mvc.Controller controller, string viewName, TModel model, List<string> viewLocations);


    /// <summary>
    /// RenderViewToStringAsync
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    /// <param name="controllerName"></param>
    /// <param name="viewName"></param>
    /// <param name="model"></param>
    /// <returns></returns>
    Task<string> RenderViewToStringAsync<TModel>(string controllerName, string viewName, TModel model);

    /// <summary>
    /// RenderViewToStringAsync
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    /// <param name="controllerName"></param>
    /// <param name="viewName"></param>
    /// <param name="model"></param>
    /// <param name="viewLocations"></param>
    /// <returns></returns>

    Task<string> RenderViewToStringAsync<TModel>(string controllerName, string viewName, TModel model, List<string> viewLocations);
}

