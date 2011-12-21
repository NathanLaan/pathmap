using System;
using System.Web;

namespace PathMap.Library
{
  public class JavascriptHttpHandler : IHttpHandler
  {
    bool IHttpHandler.IsReusable
    {
      get
      {
        return false;
      }
    }
    void IHttpHandler.ProcessRequest(HttpContext httpContext)
    {
      try
      {
        httpContext.Response.Clear();
        httpContext.Response.ContentType = "text/javascript";
        httpContext.Response.WriteFile(httpContext.Request.PhysicalPath);
        httpContext.Response.End();
      }
      catch
      {
      }
    } 
  }
}