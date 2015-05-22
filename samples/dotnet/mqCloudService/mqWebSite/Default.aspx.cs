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
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Dynamic;
using System.Xml;

namespace mqWebSite
{
    public partial class Login : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            String qryStr = (String)Session["ERRORMSG"];
            if (qryStr != null)
            {
                errorLabel.Text = qryStr;
                errorLabel.Visible = true;
            }
        }

        protected void LoginMQ_Click(object sender, EventArgs e)
        {
            String txtUserName = txtUserId.Text.Trim();
            String txtPwd = txtPassword.Text.Trim();
            Session["User"] = txtUserName;
            Session["Password"] = txtPwd;
            Response.Redirect("UniversityInfo.aspx");
        }
    }
}