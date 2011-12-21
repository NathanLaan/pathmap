using System;

namespace PathMap.Library
{
  public class PathMapHttpHandlerResult
  {

    public string Url { get; set; }
    public string QueryString { get; set; }
    public string Name { get; set; }


    public PathMapHttpHandlerResult(string url, string queryString)
        : this(url, queryString, "")
    {
    }
    public PathMapHttpHandlerResult(string url, string queryString, string name)
    {
      this.Url = url;
      this.QueryString = queryString;
      this.Name = name;
    }

  }
}