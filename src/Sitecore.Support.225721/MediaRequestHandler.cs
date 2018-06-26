using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sitecore.Configuration;
using Sitecore.Diagnostics;
using Sitecore.Resources.Media;

namespace Sitecore.Support.Resources.Media
{
  public class MediaRequestHandler : Sitecore.Resources.Media.MediaRequestHandler
  {

    protected override void SendMediaHeaders(Sitecore.Resources.Media.Media media, HttpContext context)
    {
      Assert.ArgumentNotNull(media, "media");
      Assert.ArgumentNotNull(context, "context");
      DateTime dateTime = media.MediaData.Updated;
      if (dateTime > DateTime.UtcNow)
      {
        dateTime = DateTime.UtcNow;
      }
      HttpCachePolicy cache = context.Response.Cache;
      cache.SetLastModified(dateTime);
      cache.SetETag(media.MediaData.MediaId);

      if (Context.PageMode.IsPreview)
      {
        cache.SetCacheability(HttpCacheability.NoCache);
      }
      else
      {
        cache.SetCacheability(Settings.MediaResponse.Cacheability);
      }

      TimeSpan timeSpan = Settings.MediaResponse.MaxAge;
      if (timeSpan > TimeSpan.Zero)
      {
        if (timeSpan > TimeSpan.FromDays(365.0))
        {
          timeSpan = TimeSpan.FromDays(365.0);
        }
        cache.SetMaxAge(timeSpan);
        cache.SetExpires(DateTime.UtcNow + timeSpan);
      }

      Tristate slidingExpiration = Settings.MediaResponse.SlidingExpiration;
      if (slidingExpiration != 0)
      {
        cache.SetSlidingExpiration(slidingExpiration == Tristate.True);
      }
      string cacheExtensions = Settings.MediaResponse.CacheExtensions;
      if (cacheExtensions.Length > 0)
      {
        cache.AppendCacheExtension(cacheExtensions);
      }
      string varyHeader = GetVaryHeader(media, context);
      if (!string.IsNullOrEmpty(varyHeader))
      {
        context.Response.AppendHeader("vary", varyHeader);
      }
    }
  }
}