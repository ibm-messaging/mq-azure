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
/*                                                                    */
/* Description: Part of front WebRole application that displays data  */
/*              from WorkerRole application.                          */
/**********************************************************************/
using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Drawing;
using mqHelpers;
using IBM.WMQ;

namespace mqWebSite
{
    public partial class UniversityInfo : System.Web.UI.Page
    {
        private String mqUserId = null;
        private String mqPassword = null;

        private static MQQueueManager qm = null;
        private static MQQueue requestQ = null;
        private static MQQueue replyQ = null;
        private static Boolean isConnected = false;
        

        protected void Page_Load(object sender, EventArgs e)
        {
            // Cache UserId and Password from previous page
            mqUserId = (String)Session["User"];
            mqPassword = (String)Session["Password"];

            if (ValidateUser())
            {
                PopulateUnitiversityList();
            }
            else
            {
                Response.Redirect("Default.aspx");
            }
        }

        /// <summary>
        /// Connects to MQ using SSL.
        /// </summary>
        protected bool ValidateUser()
        {
            bool bValidUser = false;

            try
            {
                OpenConnection(Global.util, mqUserId, mqPassword);
                CloseConnection();
                bValidUser = true;
            }
            catch (MQException mqex)
            {
                Session.Add("ERRORMSG","Failed to connect to messaging provider. Error: " + mqex.Reason);
            }
            catch (Exception ex)
            {
                Session.Add("ERRORMSG", "Failed to connect to messaging provider. Error: " + ex.ToString());
            }
            return bValidUser;
        }

        /// <summary>
        /// Populates the page with university list
        /// </summary>
        private void PopulateUnitiversityList()
        {
            try
            {
                String strSendData = Constants.CMD_COMMAND_KEY + "=" + Constants.CMD_QUERY_COLLEGE;

                OpenConnection(Global.util, mqUserId, mqPassword);
                OpenQueues(Global.util);
                byte[] msgId = SendMessage(strSendData, Global.util.MqReplyQ);
                String recvdMsg = ReceiveMessage(msgId);
                CloseConnection();

                if (recvdMsg != "")
                {
                    JSonHelper helper = new JSonHelper();
                    List<UniversityData> universityList = helper.ConvertJSonToObject<List<UniversityData>>(recvdMsg);

                    // Clear existing table entries
                    collegeInfoTable.Rows.Clear();

                    // Traverse through list of objects and update table
                    foreach (UniversityData univ in universityList)
                    {
                        TableRow tr = new TableRow();

                        TableCell tcName = new TableCell();
                        tcName.Text = univ.universityName.Trim();
                        tcName.Width = 400;
                        tr.Cells.Add(tcName);


                        TableCell tcCollege = new TableCell();
                        tcCollege.Text = univ.universityAdress.Trim();
                        tcCollege.Width = 500;
                        tr.Cells.Add(tcCollege);

                        TableCell tcRating = new TableCell();
                        tcRating.Text = univ.universityRating.Trim();
                        tcRating.Width = 100;
                        tr.Cells.Add(tcRating);

                        collegeInfoTable.Rows.Add(tr);
                    }
                }
            }
            catch (Exception ex)
            {
                infoLabel.Text = ex.ToString();
                CloseConnection();
            }
        }

        /// <summary>
        /// Get records from database and display
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void refreshBtn_Click(object sender, EventArgs e)
        {
            PopulateUnitiversityList();
        }

        /// <summary>
        /// Log off
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void logoutBtn_Click(object sender, EventArgs e)
        {
            Session.Add("ERRORMSG", "");
            Response.Redirect("Default.aspx");
        }

        /// <summary>
        /// Connect to queue manager with 
        /// </summary>
        /// <param name="util"></param>
        /// <param name="User"></param>
        /// <param name="Password"></param>
        public void OpenConnection(Utilities util, String User, String Password)
        {
            Hashtable mqProps = new Hashtable();

            try
            {
                // Setup connection properties
                mqProps.Add(MQC.TRANSPORT_PROPERTY, MQC.TRANSPORT_MQSERIES_MANAGED);
                mqProps.Add(MQC.CONNECTION_NAME_PROPERTY, util.MqConnectionName);
                mqProps.Add(MQC.CHANNEL_PROPERTY, util.MqChannel);
                mqProps.Add(MQC.USER_ID_PROPERTY, User);
                mqProps.Add(MQC.PASSWORD_PROPERTY, Password);
                mqProps.Add(MQC.USE_MQCSP_AUTHENTICATION_PROPERTY, true);

                // Connect to queue manager
                qm = new MQQueueManager(util.MqQueueManager, mqProps);

                isConnected = true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Open required queues
        /// </summary>
        /// <param name="util"></param>
        public void OpenQueues(Utilities util)
        {
            try
            {
                if (isConnected)
                {
                    // Open request and reply queues
                    requestQ = qm.AccessQueue(util.MqRequestQ, MQC.MQOO_OUTPUT | MQC.MQOO_FAIL_IF_QUIESCING);
                    replyQ = qm.AccessQueue(util.MqReplyQ, MQC.MQOO_INPUT_AS_Q_DEF | MQC.MQOO_FAIL_IF_QUIESCING);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Send a message to queue
        /// </summary>
        /// <param name="strMsg">Message Id</param>
        public byte[] SendMessage(String strMsg, String replyQName)
        {
            MQMessage msg = null;
            byte[] retId = null;

            try
            {
                msg = new MQMessage();
                msg.Expiry = 3000;
                msg.ReplyToQueueName = replyQName;
                msg.WriteUTF(strMsg);
                requestQ.Put(msg);
                retId = msg.MessageId;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return retId;
        }

        /// <summary>
        /// Receive a reply message
        /// </summary>
        /// <returns></returns>
        public String ReceiveMessage(byte[] messageId)
        {
            String retMsg = "";
            MQMessage mqMsg = null;

            try
            {
                mqMsg = new MQMessage();
                mqMsg.CorrelationId = messageId;
                MQGetMessageOptions gmo = new MQGetMessageOptions();
                gmo.MatchOptions |= MQC.MQMO_MATCH_CORREL_ID;
                gmo.WaitInterval = 3000;
                gmo.Options |= MQC.MQGMO_WAIT;

                replyQ.Get(mqMsg, gmo);
                retMsg = mqMsg.ReadUTF();
            }
            catch (MQException ex)
            {
                if (ex.Reason == 2033)
                    retMsg = "";
                else
                    throw ex;
            }

            return retMsg;
        }

        /// <summary>
        /// Disconnect from MQ
        /// </summary>
        public void CloseConnection()
        {
            if (qm != null)
                qm.Disconnect();
        }

    }
}