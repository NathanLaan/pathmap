using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;  //PageParser
using System.Text.RegularExpressions;
using System.IO;
using System.Web.Services.Protocols;

namespace PathMap.Library
{

  /// <summary>
  /// This class is based in part on example code from a number of places.
  /// 
  /// http://weblogs.asp.net/fmarguerie/archive/2006/01/11/435022.aspx
  /// http://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=104429
  /// http://blogs.msdn.com/tinghaoy/#_Toc122434062
  /// http://msdn.microsoft.com/en-us/library/cc668202.aspx
  /// </summary>
  public class PathMapHttpHandlerFactory : IHttpHandlerFactory
  {

    //private static readonly Regex PageRegex = new Regex(@"[\+\\""\''\d\w\s\.\/:]*[^\?]");
    //private static readonly Regex PageRegex = new Regex(@"[\+\\""\''\-\d\w\s\.\/:]*[^\?]");
    private static readonly Regex PageRegex = new Regex(@"[\+\\""\''\-\d\w\s\.\/:]*[^\?\&\#]");

    private static readonly char[] PageRegexSplitCharacter = new char[] { '/' };
    //private static readonly char[] PageRegexSplitCharacter = new char[] { '/', '?', '&' };

    /// <summary>
    /// Whether or not to output debug information.
    /// </summary>
    public static bool Debug { get; set; }
    public static bool DetectMobile { get; set; }

    public static bool SiteOffline { get; set; }
    public static string SiteOfflinePage { get; set; }

    private static List<PathMapPage> pathMapPageList = new List<PathMapPage>();
    private static List<PathMapHttpHandlerPlugin> pathMapHttpHandlerPluginList = new List<PathMapHttpHandlerPlugin>();

    /// <summary>
    /// Returns NULL or a parsed url.
    /// </summary>
    /// <param name="relativeUrl">The URL to parse into "/" separated components.</param>
    /// <returns>NULL if the URL cannot be split, or</returns>
    public static string[] SplitUrl(string relativeUrl)
    {
      Match matchPage = PathMapHttpHandlerFactory.PageRegex.Match(relativeUrl);
      if (matchPage.Captures.Count > 0)
      {
        Capture capture = matchPage.Captures[0];
        if (capture.Value != null && capture.Value != string.Empty)
        {
          string[] splitArray = capture.Value.Split(PathMapHttpHandlerFactory.PageRegexSplitCharacter);
          //if (splitArray.Length > 1 && string.IsNullOrEmpty(splitArray[0]))
          //{
          //  string[] newArray = new string[splitArray.Length - 1];
          //  splitArray.CopyTo(newArray, 2);
          //  return newArray;
          //}
          return splitArray;
        }
      }
      return null;
    }

    public static string GetRelativeUrl()
    {
      string virtualPathNoTrailer = HttpRuntime.AppDomainAppVirtualPath;
      if (virtualPathNoTrailer.EndsWith("/"))
      {
        virtualPathNoTrailer = virtualPathNoTrailer.TrimEnd('/');
      }
      string relativeUrl = HttpContext.Current.Request.Url.LocalPath.Substring(virtualPathNoTrailer.Length).Trim();
      string relativeUrlTrimmed = relativeUrl;
      if (relativeUrl.EndsWith("/"))
      {
        relativeUrlTrimmed = relativeUrl.Remove(relativeUrl.LastIndexOf("/"));
      }
      return relativeUrlTrimmed;
    }

    #region VirtualPathNoTrailer
    protected string VirtualPathNoTrailer
    {
      get
      {
        string virtualPathNoTrailer = HttpRuntime.AppDomainAppVirtualPath;
        if (virtualPathNoTrailer.EndsWith("/"))
        //if (virtualPathNoTrailer[virtualPathNoTrailer.Length - 1] == '/')
        {
          virtualPathNoTrailer = virtualPathNoTrailer.TrimEnd('/');
        }
        return virtualPathNoTrailer;
      }
    }
    #endregion



    #region GetPathMapPageName

    public static string GetPathMapPageName()
    {
      string rawUrl = HttpContext.Current.Request.RawUrl;
      string retVal = GetPathMapPageName(rawUrl);
      if (retVal != "")
      {
        return retVal;
      }
      string absUrl = HttpContext.Current.Request.Url.AbsolutePath;
      return GetPathMapPageName(absUrl);
    }

    public static string GetPathMapPageName(string rawUrl)
    {
      //string rawUrl = httpContext.Request.RawUrl;
      foreach (PathMapPage fp in PathMapHttpHandlerFactory.pathMapPageList)
      {
        if (rawUrl.Equals(fp.RawUrl))
        {
          return fp.Name;
        }
      }
      foreach (PathMapPage fp in PathMapHttpHandlerFactory.pathMapPageList)
      {
        if (rawUrl.Equals(fp.MapUrl))
        {
          return fp.Name;
        }
      }
      string[] splitArray = SplitUrl(rawUrl);
      foreach (PathMapHttpHandlerPlugin plugin in PathMapHttpHandlerFactory.pathMapHttpHandlerPluginList)
      {
        PathMapHttpHandlerResult result = plugin.FindMatch(splitArray);
        if (result != null)
        {
          return result.Name;
        }
      }
      return "";
    }

    #endregion GetPathMapPageName



    private bool IsImageFile(string url)
    {
      return
        url.EndsWith(".gif") ||
        url.EndsWith(".jpg") ||
        url.EndsWith(".jpeg") ||
        url.EndsWith(".png") ||
        url.EndsWith(".bmp") ||
        url.EndsWith(".ico");
    }


    public static void Add(PathMapPage pathMapPage)
    {
      PathMapHttpHandlerFactory.pathMapPageList.Add(pathMapPage);
    }


    public static void Add(PathMapHttpHandlerPlugin pathMapHttpHandlerPlugin)
    {
      PathMapHttpHandlerFactory.pathMapHttpHandlerPluginList.Add(pathMapHttpHandlerPlugin);
    }


    public PathMapHttpHandlerFactory()
    {
    }

    public IHttpHandler GetHandler(HttpContext httpContext, string requestType, string url, string pathTranslated)
    {
      if (PathMapHttpHandlerFactory.SiteOffline)
      {
        return new SiteOfflineHttpHandler(PathMapHttpHandlerFactory.SiteOfflinePage);
      }

      #region Other HttpHandler Classes
      if (this.IsImageFile(url))
      {
        return new ImageHttpHandler();
      }
      else if (url.EndsWith(".css"))
      {
        return new CssHttpHandler();
      }
      else if (url.EndsWith(".js"))
      {
        return new JavascriptHttpHandler();
      }
      else if (url.EndsWith(".asmx"))
      {
        //System.Web.
        System.Web.Services.Protocols.WebServiceHandlerFactory w = new System.Web.Services.Protocols.WebServiceHandlerFactory();
        IHttpHandler h = w.GetHandler(httpContext, "*", url, pathTranslated);
        return h;
        //return null;
      }
      else if (url.EndsWith(".html") || url.EndsWith(".htm"))
      {
        return new HtmlHttpHandler();
      }
      else if (url.EndsWith(".txt"))
      {
        return new TextHttpHandler();
      }
      #endregion

      string relativeUrl = httpContext.Request.Url.LocalPath.Substring(VirtualPathNoTrailer.Length).Trim();
      string queryString = string.Empty;

      string relativeUrlTrimmed = relativeUrl;
      if (relativeUrl.EndsWith("/"))
      {
        relativeUrlTrimmed = relativeUrl.Remove(relativeUrl.LastIndexOf("/"));
      }
      
      bool matchFound = false;
      string rawUrl = httpContext.Request.RawUrl;
      foreach (PathMapPage fp in PathMapHttpHandlerFactory.pathMapPageList)
      {
        // BUGFIX: Test relative URL as rawUrl sometimes contains query parameters.
        if (rawUrl.Equals(fp.RawUrl) || relativeUrlTrimmed.Equals(fp.RawUrl))
        {
          url = fp.MapUrl;
          matchFound = true;
          break;
        }
      }

      if (!matchFound)
      {
        Match matchPage = PathMapHttpHandlerFactory.PageRegex.Match(relativeUrlTrimmed);
        if (matchPage.Captures.Count > 0)
        {
          Capture capture = matchPage.Captures[0];
          if (capture.Value != null && capture.Value != string.Empty)
          {
            string[] splitArray = capture.Value.Split(PathMapHttpHandlerFactory.PageRegexSplitCharacter);

            foreach (PathMapHttpHandlerPlugin plugin in PathMapHttpHandlerFactory.pathMapHttpHandlerPluginList)
            {
              PathMapHttpHandlerResult result = plugin.FindMatch(splitArray);
              if (result != null)
              {
                url = result.Url;
                queryString = result.QueryString;
                break;
              }
            }

          }
        }
      }

      url = url.ToLower();
      if (url.Equals("/") || url.Equals(string.Empty))
      {
        url = "/default.aspx";
      }

      if (PathMapHttpHandlerFactory.Debug)
      {
        string path = httpContext.Request.Url.GetComponents(UriComponents.Path, UriFormat.SafeUnescaped);
        httpContext.Response.Write("\r\n<!-- -->\r\n");
        httpContext.Response.Write("<code>\r\n");
        httpContext.Response.Write("PATHMAP-PATH: " + path + "<br/>\r\n");
        httpContext.Response.Write("PATHMAP-HOST: " + httpContext.Request.Url.Host + "<br/>\r\n");
        httpContext.Response.Write("PATHMAP-RAWU: " + httpContext.Request.RawUrl + "<br/>\r\n");
        httpContext.Response.Write("PATHMAP-RELU: " + relativeUrl + "<br/>\r\n");
        httpContext.Response.Write("PATHMAP-ACTU: " + url + "<br/>\r\n");
        httpContext.Response.Write("PATHMAP-QSTR: " + queryString + "<br/>\r\n");
        httpContext.Response.Write("</code>\r\n<br/>\r\n");
      }

      bool rebaseClientPath = ((relativeUrl == "/") || (relativeUrl == "/default.aspx"));

      if (DetectMobile)
      {
        string u = httpContext.Request.ServerVariables["HTTP_USER_AGENT"];
        Regex b = new Regex(@"android|avantgo|blackberry|blazer|compal|elaine|fennec|hiptop|iemobile|ip(hone|od)|iris|kindle|lge |maemo|midp|mmp|opera m(ob|in)i|palm( os)?|phone|p(ixi|re)\\/|plucker|pocket|psp|symbian|treo|up\\.(browser|link)|vodafone|wap|windows (ce|phone)|xda|xiino", RegexOptions.IgnoreCase | RegexOptions.Multiline);
        Regex v = new Regex(@"1207|6310|6590|3gso|4thp|50[1-6]i|770s|802s|a wa|abac|ac(er|oo|s\\-)|ai(ko|rn)|al(av|ca|co)|amoi|an(ex|ny|yw)|aptu|ar(ch|go)|as(te|us)|attw|au(di|\\-m|r |s )|avan|be(ck|ll|nq)|bi(lb|rd)|bl(ac|az)|br(e|v)w|bumb|bw\\-(n|u)|c55\\/|capi|ccwa|cdm\\-|cell|chtm|cldc|cmd\\-|co(mp|nd)|craw|da(it|ll|ng)|dbte|dc\\-s|devi|dica|dmob|do(c|p)o|ds(12|\\-d)|el(49|ai)|em(l2|ul)|er(ic|k0)|esl8|ez([4-7]0|os|wa|ze)|fetc|fly(\\-|_)|g1 u|g560|gene|gf\\-5|g\\-mo|go(\\.w|od)|gr(ad|un)|haie|hcit|hd\\-(m|p|t)|hei\\-|hi(pt|ta)|hp( i|ip)|hs\\-c|ht(c(\\-| |_|a|g|p|s|t)|tp)|hu(aw|tc)|i\\-(20|go|ma)|i230|iac( |\\-|\\/)|ibro|idea|ig01|ikom|im1k|inno|ipaq|iris|ja(t|v)a|jbro|jemu|jigs|kddi|keji|kgt( |\\/)|klon|kpt |kwc\\-|kyo(c|k)|le(no|xi)|lg( g|\\/(k|l|u)|50|54|e\\-|e\\/|\\-[a-w])|libw|lynx|m1\\-w|m3ga|m50\\/|ma(te|ui|xo)|mc(01|21|ca)|m\\-cr|me(di|rc|ri)|mi(o8|oa|ts)|mmef|mo(01|02|bi|de|do|t(\\-| |o|v)|zz)|mt(50|p1|v )|mwbp|mywa|n10[0-2]|n20[2-3]|n30(0|2)|n50(0|2|5)|n7(0(0|1)|10)|ne((c|m)\\-|on|tf|wf|wg|wt)|nok(6|i)|nzph|o2im|op(ti|wv)|oran|owg1|p800|pan(a|d|t)|pdxg|pg(13|\\-([1-8]|c))|phil|pire|pl(ay|uc)|pn\\-2|po(ck|rt|se)|prox|psio|pt\\-g|qa\\-a|qc(07|12|21|32|60|\\-[2-7]|i\\-)|qtek|r380|r600|raks|rim9|ro(ve|zo)|s55\\/|sa(ge|ma|mm|ms|ny|va)|sc(01|h\\-|oo|p\\-)|sdk\\/|se(c(\\-|0|1)|47|mc|nd|ri)|sgh\\-|shar|sie(\\-|m)|sk\\-0|sl(45|id)|sm(al|ar|b3|it|t5)|so(ft|ny)|sp(01|h\\-|v\\-|v )|sy(01|mb)|t2(18|50)|t6(00|10|18)|ta(gt|lk)|tcl\\-|tdg\\-|tel(i|m)|tim\\-|t\\-mo|to(pl|sh)|ts(70|m\\-|m3|m5)|tx\\-9|up(\\.b|g1|si)|utst|v400|v750|veri|vi(rg|te)|vk(40|5[0-3]|\\-v)|vm40|voda|vulc|vx(52|53|60|61|70|80|81|83|85|98)|w3c(\\-| )|webc|whit|wi(g |nc|nw)|wmlb|wonu|x700|xda(\\-|2|g)|yas\\-|your|zeto|zte\\-", RegexOptions.IgnoreCase | RegexOptions.Multiline);
        if ((b.IsMatch(u) || v.IsMatch(u.Substring(0, 4))))
        {
          queryString += "&mobile=true";
        }
      }

      if (queryString != string.Empty)
      {
        httpContext.RewritePath(url, null, queryString, rebaseClientPath);
      }
      else
      {
        httpContext.RewritePath(url, rebaseClientPath);
      }

      return PageParser.GetCompiledPageInstance(url, httpContext.Server.MapPath(url), httpContext);
    }

    #region ReleaseHandler
    public void ReleaseHandler(IHttpHandler handler)
    {
      IDisposable disposableObject = handler as IDisposable;
      if (disposableObject != null)
      {
        disposableObject.Dispose();
      }
    }
    #endregion

  }
}