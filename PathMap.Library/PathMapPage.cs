using System;

namespace PathMap.Library
{
  public class PathMapPage
  {

    /// <summary>
    /// The URL that the user sees in their web browser.
    /// </summary>
    public string RawUrl { get; set; }

    /// <summary>
    /// The URL that the PathMapHttpHandler maps the pathMapUrl to.
    /// </summary>
    public string MapUrl { get; set; }

    /// <summary>
    /// The name assigned to the page and visible to the user.
    /// </summary>
    public string Name { get; set; }

    public PathMapPage()
    {
    }
    public PathMapPage(string name, string rawUrl, string mapUrl)
    {
      this.Name = name;
      this.RawUrl = rawUrl;
      this.MapUrl = mapUrl;
    }

  }
}