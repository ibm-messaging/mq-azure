/**********************************************************************/
/*   <copyright                                                       */
/*   notice="lm-source-program"                                       */
/*   pids="5724-H72,"                                                 */
/*   years="2007,2015"                                                */
/*   crc="2787562084" >                                               */
/*   Licensed Materials - Property of IBM                             */
/*                                                                    */
/*   5724-H72,                                                        */
/*                                                                    */
/*   (C) Copyright IBM Corp. 2007, 2015 All Rights Reserved.          */
/*                                                                    */
/*   US Government Users Restricted Rights - Use, duplication or      */
/*   disclosure restricted by GSA ADP Schedule Contract with          */
/*   IBM Corp.                                                        */
/*   </copyright>                                                     */
/**********************************************************************/
using System;
using System.Collections;
using System.Linq;
using System.Web;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using mqWebSite;
using System.Configuration;
using mqHelpers;
using System.Xml;

namespace mqWebSite
{
    public class Global : HttpApplication
    {
        public static Utilities util = null;

        void Application_Start(object sender, EventArgs e)
        {
            // Code that runs on application startup
            //BundleConfig.RegisterBundles(BundleTable.Bundles);
            //AuthConfig.RegisterOpenAuth();
            //RouteConfig.RegisterRoutes(RouteTable.Routes);

            // Read the configuration
            String mqConStr = ConfigurationManager.ConnectionStrings["IBMMQConnection"].ConnectionString;
            util = new Utilities(mqConStr);

        }

        void Application_End(object sender, EventArgs e)
        {
            //  Code that runs on application shutdown

        }

        void Application_Error(object sender, EventArgs e)
        {
            // Code that runs when an unhandled error occurs

        }
    }
}
