//using Findx.Extensions;
using Infrastructure.Attribute;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using ZR.Pdf.Core;
namespace ZR.Pdf.Mvc;

public static class ExtensionsHtml
{



    /// <summary>
    ///     StringHtmlDecodedecoding
    /// </summary>
    /// <param name="source">treatHtmlDecodeDecoding string</param>
    /// <returns>HtmlDecodedecoded string</returns>
    public static string ToHtmlDecode(this string source)
    {
        return HttpUtility.HtmlDecode(source);
    }

}

/// <summary>
///     View reading
/// </summary>

[AppService(ServiceType = typeof(IRazorViewRender))]

public class RazorViewRender : IRazorViewRender
{
    private readonly IRazorViewEngine _viewEngine;
    private readonly ITempDataProvider _tempDataProvider;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Ctor
    /// </summary>
    /// <param name="viewEngine"></param>
    /// <param name="tempDataProvider"></param>
    /// <param name="httpContextAccessor"></param>
    /// <param name="serviceProvider"></param>
    public RazorViewRender(IRazorViewEngine viewEngine, ITempDataProvider tempDataProvider, IHttpContextAccessor httpContextAccessor, IServiceProvider serviceProvider)
    {
        _viewEngine = viewEngine;
        _tempDataProvider = tempDataProvider;
        _httpContextAccessor = httpContextAccessor;
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    ///     Reading view content
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    /// <param name="viewName"></param>
    /// <param name="model"></param>
    /// <returns></returns>
    public async Task<string> RenderViewToStringAsync<TModel>(string viewName, TModel model)
    {
        try
        {

            var actionContext = GetActionContext();
            var view = FindView(actionContext, viewName);
            var viewData = new ViewDataDictionary<TModel>(new EmptyModelMetadataProvider(), new ModelStateDictionary())
            {
                Model = model
            };



            Console.WriteLine(" founded View " + view.Path);



            await using var output = new StringWriter();
            var viewContext = new ViewContext(
                actionContext,
                view,
                viewData,
                new TempDataDictionary(actionContext.HttpContext, _tempDataProvider),
                output,
                new HtmlHelperOptions()
            );
            await view.RenderAsync(viewContext);

            Console.WriteLine("Done Render founded View " + view.Path);


            return output.ToString().ToHtmlDecode();
        }
        catch (System.Exception ex)
        {

            var value = $"An error occurred in RenderViewToStringAsync : {ex.Message}";
            Console.ForegroundColor = ConsoleColor.Red;

            Console.WriteLine(value);
            // Console.WriteLine(value);
            Console.WriteLine(ex.StackTrace);


            throw;
        }
    }

    /// <summary>
    ///     Reading view content with specified view locations
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    /// <param name="viewName"></param>
    /// <param name="model"></param>
    /// <param name="viewLocations"></param>
    /// <returns></returns>
    public async Task<string> RenderViewToStringAsync<TModel>(string viewName, TModel model, List<string> viewLocations)
    {
        var actionContext = GetActionContext();
        var view = FindView(actionContext, viewName, viewLocations);
        var viewData = new ViewDataDictionary<TModel>(new EmptyModelMetadataProvider(), new ModelStateDictionary())
        {
            Model = model
        };


        Console.WriteLine(" founded View " + view.Path);


        await using var output = new StringWriter();
        var viewContext = new ViewContext(
            actionContext,
            view,
            viewData,
            new TempDataDictionary(actionContext.HttpContext, _tempDataProvider),
            output,
            new HtmlHelperOptions()
        );
        await view.RenderAsync(viewContext);

        Console.WriteLine("Done Render founded View " + view.Path);

        return output.ToString().ToHtmlDecode();
    }

    private IView FindView(ActionContext actionContext, string viewName, List<string> viewLocations = null)
    {
        if (viewLocations != null && viewLocations.Any())
        {
            foreach (var location in viewLocations)
            {

                var getViewResult = _viewEngine.GetView(executingFilePath: null, viewPath: location, isMainPage: true);
                if (getViewResult.Success)
                {
                    return getViewResult.View;
                }

                // Try with lowercase path
                var lowerCaseLocation = location.ToLowerInvariant();
                getViewResult = _viewEngine.GetView(executingFilePath: null, viewPath: lowerCaseLocation, isMainPage: true);
                if (getViewResult.Success)
                {
                    return getViewResult.View;
                }
            }
        }

        var getViewResultDefault = _viewEngine.GetView(executingFilePath: null, viewPath: viewName, isMainPage: true);
        if (getViewResultDefault.Success)
        {
            return getViewResultDefault.View;
        }

        // Try with lowercase viewName
        var lowerCaseViewName = viewName.ToLowerInvariant();
        getViewResultDefault = _viewEngine.GetView(executingFilePath: null, viewPath: lowerCaseViewName, isMainPage: true);
        if (getViewResultDefault.Success)
        {
            return getViewResultDefault.View;
        }

        var findViewResult = _viewEngine.FindView(actionContext, viewName, isMainPage: true);
        if (findViewResult.Success)
        {
            return findViewResult.View;
        }

        // Try with lowercase viewName in GetView
        findViewResult = _viewEngine.GetView(executingFilePath: null, lowerCaseViewName, isMainPage: true);
        if (findViewResult.Success)
        {
            return findViewResult.View;
        }


        findViewResult = _viewEngine.FindView(actionContext, lowerCaseViewName, isMainPage: true);
        if (findViewResult.Success)
        {
            return findViewResult.View;
        }

        var searchedLocations = getViewResultDefault.SearchedLocations.Concat(findViewResult.SearchedLocations);
        var errorMessage = string.Join(
            Environment.NewLine,
            new[] { $"Unable to find view '{viewName}'. The following locations were searched:" }.Concat(searchedLocations));

        throw new InvalidOperationException(errorMessage);
    }

    private ActionContext GetActionContext()
    {
        return _httpContextAccessor.HttpContext != null ? new ActionContext(_httpContextAccessor.HttpContext, _httpContextAccessor.HttpContext.GetRouteData(), new ActionDescriptor())
            : new ActionContext(new DefaultHttpContext { RequestServices = _serviceProvider }, new RouteData(), new ActionDescriptor());
    }

    /// <summary>
    /// RenderRazorViewToString
    /// </summary>
    /// <param name="controller"></param>
    /// <param name="viewName"></param>
    /// <param name="model"></param>
    /// <returns></returns>
    public async Task<string> RenderRazorViewToString<TModel>(Controller controller, string viewName, TModel model)
    {
        if (controller == null)
        {
            throw new ArgumentNullException(nameof(controller));
        }

        // Set the model in the ViewData
        controller.ViewData.Model = model;

        // Resolve the view engine
        var viewEngine = controller.HttpContext.RequestServices.GetService(typeof(ICompositeViewEngine)) as ICompositeViewEngine;

        var controllerContext = controller.ControllerContext;
        var viewResult = FindView(controllerContext, viewName);

        var viewData = new ViewDataDictionary<TModel>(new EmptyModelMetadataProvider(), new ModelStateDictionary())
        {
            Model = model
        };


        Console.WriteLine(" founded View " + viewResult.Path);


        await using var output = new StringWriter();
        var viewContext = new ViewContext(
            controllerContext,
            viewResult,
            viewData,
            new TempDataDictionary(controller.HttpContext, _tempDataProvider),
            output,
            new HtmlHelperOptions()
        );
        await viewResult.RenderAsync(viewContext);


        Console.WriteLine("Done Render founded View " + viewResult.Path);

        return output.ToString().ToHtmlDecode();
    }

    /// <summary>
    /// RenderRazorViewToString with specified view locations
    /// </summary>
    /// <param name="controller"></param>
    /// <param name="viewName"></param>
    /// <param name="model"></param>
    /// <param name="viewLocations"></param>
    /// <returns></returns>
    public async Task<string> RenderRazorViewToString<TModel>(Controller controller, string viewName, TModel model, List<string> viewLocations)
    {
        if (controller == null)
        {
            throw new ArgumentNullException(nameof(controller));
        }

        // Set the model in the ViewData
        controller.ViewData.Model = model;

        // Resolve the view engine
        var viewEngine = controller.HttpContext.RequestServices.GetService(typeof(ICompositeViewEngine)) as ICompositeViewEngine;

        var controllerContext = controller.ControllerContext;
        var viewResult = FindView(controllerContext, viewName, viewLocations);

        var viewData = new ViewDataDictionary<TModel>(new EmptyModelMetadataProvider(), new ModelStateDictionary())
        {
            Model = model
        };
        Console.WriteLine(" founded View " + viewResult.Path);


        await using var output = new StringWriter();
        var viewContext = new ViewContext(
            controllerContext,
            viewResult,
            viewData,
            new TempDataDictionary(controller.HttpContext, _tempDataProvider),
            output,
            new HtmlHelperOptions()
        );
        await viewResult.RenderAsync(viewContext);
        Console.WriteLine("Done Render founded View " + viewResult.Path);


        return output.ToString().ToHtmlDecode();
    }

    /// <summary>
    ///     Creates an ActionContext with optional controller name override
    /// </summary>
    private ActionContext GetActionContext(string controllerName = null)
    {
        HttpContext httpContext;
        RouteData routeData;

        if (_httpContextAccessor.HttpContext != null)
        {
            httpContext = _httpContextAccessor.HttpContext;
            routeData = httpContext.GetRouteData() ?? new RouteData();
        }
        else
        {
            httpContext = new DefaultHttpContext { RequestServices = _serviceProvider };
            routeData = new RouteData();
        }

        // Override controller name in route data if specified
        if (!string.IsNullOrEmpty(controllerName))
        {
            routeData.Values["controller"] = controllerName;
        }

        return new ActionContext(httpContext, routeData, new ActionDescriptor());
    }

    /// <summary>
    ///     Renders view content to a string with explicit controller name.
    /// </summary>
    /// <typeparam name="TModel">Model type</typeparam>
    /// <param name="controllerName">Name of the controller (used for view location)</param>
    /// <param name="viewName">Name of the view</param>
    /// <param name="model">Model instance</param>
    /// <returns>Rendered view as string</returns>
    public async Task<string> RenderViewToStringAsync<TModel>(string controllerName, string viewName, TModel model)
    {
        var actionContext = GetActionContext(controllerName);
        var view = FindView(actionContext, viewName);
        var viewData = new ViewDataDictionary<TModel>(new EmptyModelMetadataProvider(), new ModelStateDictionary())
        {
            Model = model
        };


        Console.WriteLine(" founded View " + view.Path);


        await using var output = new StringWriter();
        var viewContext = new ViewContext(
            actionContext,
            view,
            viewData,
            new TempDataDictionary(actionContext.HttpContext, _tempDataProvider),
            output,
            new HtmlHelperOptions()
        );
        await view.RenderAsync(viewContext);

        Console.WriteLine("Done Render founded View " + view.Path);

        return output.ToString().ToHtmlDecode();
    }

    /// <summary>
    ///     Renders view content to a string with explicit controller name and specified view locations.
    /// </summary>
    /// <typeparam name="TModel">Model type</typeparam>
    /// <param name="controllerName">Name of the controller (used for view location)</param>
    /// <param name="viewName">Name of the view</param>
    /// <param name="model">Model instance</param>
    /// <param name="viewLocations"></param>
    /// <returns>Rendered view as string</returns>
    public async Task<string> RenderViewToStringAsync<TModel>(string controllerName, string viewName, TModel model, List<string> viewLocations)
    {
        var actionContext = GetActionContext(controllerName);
        var view = FindView(actionContext, viewName, viewLocations);
        var viewData = new ViewDataDictionary<TModel>(new EmptyModelMetadataProvider(), new ModelStateDictionary())
        {
            Model = model,

        };

        Console.WriteLine(" founded View " + view.Path);

        await using var output = new StringWriter();
        var viewContext = new ViewContext(
            actionContext,
            view,
            viewData,
            new TempDataDictionary(actionContext.HttpContext, _tempDataProvider),
            output,
            new HtmlHelperOptions()
        );
        await view.RenderAsync(viewContext);

        Console.WriteLine("Done Render founded View " + view.Path);

        return output.ToString().ToHtmlDecode();
    }
}





