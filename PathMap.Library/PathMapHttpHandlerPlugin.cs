using System;

namespace PathMap.Library
{
  public interface PathMapHttpHandlerPlugin
  {

    PathMapHttpHandlerResult FindMatch(string[] splitArray);

  }
}