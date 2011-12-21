using System;
using System.Web;
using System.IO;

namespace PathMap.Library
{

  public class ImageHttpHandler : IHttpHandler
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
        httpContext.Response.ContentType = GetContentType(httpContext.Request.PhysicalPath);
        httpContext.Response.WriteFile(httpContext.Request.PhysicalPath);
        httpContext.Response.Flush();
        httpContext.Response.Close();
        httpContext.Response.End();
      }
      catch(Exception e)
      {
        httpContext.Response.Clear();
        httpContext.Response.Write(httpContext.Request.PhysicalPath);
        httpContext.Response.Write("<br><br>");
        httpContext.Response.Write(e.ToString());
        //httpContext.Response.ContentType = "Image/jpeg";
        //httpContext.Response.WriteFile(httpContext.Server.MapPath("/img/missing.jpg"));
        httpContext.Response.Flush();
        httpContext.Response.Close();
      }
    }

    private string GetContentType(String path)
    {
      switch (Path.GetExtension(path))
      {
        case ".ico":
          return "Image/ico";
        case ".gif":
          return "Image/gif";
        case ".jpg":
        case ".jpeg":
          return "Image/jpeg";
        case ".png":
          return "Image/png";
        case ".bmp":
          return "Image/bmp";
        default:
          return "Image";
      }
    }

  }

}