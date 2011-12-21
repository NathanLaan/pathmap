using System;
using System.Web;

namespace PathMap.Library
{
  public class SiteOfflineHttpHandler : IHttpHandler
  {
    private string siteOfflinePage;
    public SiteOfflineHttpHandler(string siteOfflinePage)
    {
      this.siteOfflinePage = siteOfflinePage;
    }
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
        httpContext.Response.ContentType = "text/html";
        httpContext.Response.WriteFile(httpContext.Server.MapPath(this.siteOfflinePage));
        httpContext.Response.End();
      }
      catch
      {
      }
    } 
  }
}