using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Owin.Security.OpenIdConnect;
using System.Data.Services.Client;
using Events.Helpers;
using Microsoft.Azure.ActiveDirectory.GraphClient;


namespace Events.Controllers
{
    [Authorize]
    public class UsersController : Controller
    {
        public async Task<ActionResult> ShowThumbnail(string id)
        {
            try
            {
                ActiveDirectoryClient client = AuthenticationHelper.GetActiveDirectoryClient();
                IUser user = await client.Users.GetByObjectId(id).ExecuteAsync();
                DataServiceStreamResponse dataServiceStreamResponse = null;
                try
                {
                    dataServiceStreamResponse = await user.ThumbnailPhoto.DownloadAsync();
                    if (dataServiceStreamResponse != null)
                    {
                        return File(dataServiceStreamResponse.Stream, "image/jpeg");
                    }
                }
                catch
                {
                    // Return placeholder
                    var file = Server.MapPath("~/Images/user-placeholder.png");
                    return File(file, "image/png", Path.GetFileName(file));
                }
            }
            catch (Exception e)
            {
                if (Request.QueryString["reauth"] == "True")
                {
                    //
                    // Send an OpenID Connect sign-in request to get a new set of tokens.
                    // If the user still has a valid session with Azure AD, they will not be prompted for their credentials.
                    // The OpenID Connect middleware will return to this controller after the sign-in response has been handled.
                    //
                    HttpContext.GetOwinContext()
                        .Authentication.Challenge(OpenIdConnectAuthenticationDefaults.AuthenticationType);
                }

                //
                // The user needs to re-authorize.  Show them a message to that effect.
                //
                ViewBag.ErrorMessage = "AuthorizationRequired";
            }
            return View();
        }
    }
}