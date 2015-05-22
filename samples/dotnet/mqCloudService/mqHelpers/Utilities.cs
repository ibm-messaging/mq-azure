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
/* Description: Utitlity class helping interaction between WebRole and*/
/*              WorkerRole.                                           */
/**********************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.IO;

namespace mqHelpers
{
    public class Utilities
    {
        //Constatnts
        const String MQ_QM_NAME = "QMName";
        const String MQ_CHANNEL_NAME = "Channel";
        const String MQ_CONN_NAME = "Conname";
        const String MQ_REQUEST_Q = "RequestQueue";
        const String MQ_REPLY_Q = "ReplyQueue";

        const String DB_SERVER = "Server";
        const String DB_PORT = "Port";

        const String USER_ID = "UserId";
        const String PASSWORD = "Password";

        String mqQMName = null;
        String mqRequestQName = null;
        String mqReplyQName = null;
        String mqConName = null;
        String mqChannel = null;
        String mqUserId = null;
        String mqPassword = null;

        String dbServer = null;
        String dbUserid = null;
        String dbPassword = null;
        int dbPort = 0;
        String dbConnString = null;

        public String DbConnection
        {
            get
            {
                return dbConnString;
            }
        }

        public String MqQueueManager
        {
            get
            {
                return mqQMName;
            }
        }

        public String MqChannel
        {
            get
            {
                return mqChannel;
            }
        }

        public String MqConnectionName
        {
            get
            {
                return mqConName;
            }
        }

        public String MqUser
        {
            get
            {
                return mqUserId;
            }
        }

        public String MqPassword
        {
            get
            {
                return mqPassword;
            }
        }

        public String MqRequestQ
        {
            get
            {
                return mqRequestQName;
            }
        }

        public String MqReplyQ
        {
            get
            {
                return mqReplyQName;
            }
        }

        public Utilities(String mqConnStr)
        {
            try
            {
                parseMQConnectionString(mqConnStr);
            }
            catch (Exception ex)
            {
            }
        }

        public Utilities(String mqConnStr, String sqlDBConnStr)
        {
            try
            {
                parseMQConnectionString(mqConnStr);
                dbConnString = sqlDBConnStr;
            }
            catch (Exception ex)
            {
            }
        }

        /// <summary>
        /// Parse MQ connection string
        /// </summary>
        /// <param name="connString"></param>
        /// <returns></returns>
        private bool parseMQConnectionString(String connString)
        {
            bool retVal = false;
            try
            {
                // Split the connection string into a key value pair with '=' delimiter
                String[] connInfo = connString.Split(';');

                foreach (String str in connInfo)
                {
                    // Separate out key value pair
                    String[] keyVal = str.Split('=');
                    foreach (String k in keyVal)
                    {
                        if (k.Equals(MQ_QM_NAME))
                        {
                            mqQMName = keyVal[1];
                        }
                        else if (k.Equals(MQ_CONN_NAME))
                        {
                            mqConName = keyVal[1];
                        }
                        else if (k.Equals(MQ_CHANNEL_NAME))
                        {
                            mqChannel = keyVal[1];
                        }
                        else if (k.Equals(USER_ID))
                        {
                            mqUserId = keyVal[1];
                        }
                        else if (k.Equals(PASSWORD))
                        {
                            mqPassword = keyVal[1];
                        }
                        else if (k.Equals(MQ_REPLY_Q))
                        {
                            mqReplyQName = keyVal[1];
                        }
                        else if (k.Equals(MQ_REQUEST_Q))
                        {
                            mqRequestQName = keyVal[1];
                        }
                    }
                }
                retVal = true;
            }
            catch (Exception ex)
            {
                retVal = false;
            }
            return retVal;
        }

        /// <summary>
        /// Prase SQL database connection string
        /// </summary>
        /// <param name="dbString"></param>
        /// <returns></returns>
        private bool parseDBConnectionString(String dbString)
        {
            bool retVal = false;

            try
            {
                //Server=gzgzcd88lm.database.windows.net;port=1433;UserId=shashikanth;Password=H0neycomb
                // Split the connection string into a key value pair with '=' delimiter
                String[] connInfo = dbString.Split(';');

                foreach (String str in connInfo)
                {
                    // Separate out key value pair
                    String[] keyVal = str.Split('=');
                    foreach (String k in keyVal)
                    {
                        if (k.Equals(DB_SERVER))
                        {
                            dbServer = keyVal[1];
                        }
                        else if (k.Equals(DB_PORT))
                        {
                            dbPort = Convert.ToInt32(keyVal[1]);
                        }
                        else if (k.Equals(USER_ID))
                        {
                            dbUserid = keyVal[1];
                        }
                        else if (k.Equals(PASSWORD))
                        {
                            dbPassword = keyVal[1];
                        }
                    }
                }
                retVal = true;
            }
            catch (Exception ex)
            {
                retVal = false;
            }
            return retVal;
        }
    }

    /// <summary>
    /// Data representation of College
    /// </summary>
    [DataContract]
    public class UniversityData
    {
        [DataMember]
        public String universityName { get; set; }
        [DataMember]
        public String universityAdress { get; set; }
        [DataMember]
        public String universityRating { get; set; }
    }

    /// <summary>
    /// JSon helper methods
    /// </summary>
    public class JSonHelper
    {
        public string ConvertObjectToJSon<T>(T obj)
        {
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T));
            MemoryStream ms = new MemoryStream();
            ser.WriteObject(ms, obj);
            string jsonString = System.Text.Encoding.UTF8.GetString(ms.ToArray());
            ms.Close();
            return jsonString;
        }

        public T ConvertJSonToObject<T>(string jsonString)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
            MemoryStream ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(jsonString));
            T obj = (T)serializer.ReadObject(ms);
            return obj;
        }
    }

    /// <summary>
    /// Userful constants
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// Key to identify the incoming data as a COMMAND
        /// </summary>
        public const String CMD_COMMAND_KEY = "COMMAND";
        /// <summary>
        /// 
        /// </summary>
        public const String CMD_QUERY_STUDENT = "QueryStudentList";
        /// <summary>
        /// 
        /// </summary>
        public const String CMD_INSERT_STUDENT = "InsertStudent";
        /// <summary>
        /// Command to query details of a college
        /// </summary>
        public const String CMD_QUERY_COLLEGE = "QueryCollege";

        /// <summary>
        /// ID
        /// </summary>
        public const String SQL_COLMN_COLLEGE_ID = "universityID";
        /// <summary>
        /// College Name
        /// </summary>
        public const String SQL_COLMN_COLLEGE_NAME = "universityName";
        /// <summary>
        /// College address
        /// </summary>
        public const String SQL_COLMN_COLLEGE_ADRESS = "universityAddress";
        /// <summary>
        /// College rating
        /// </summary>
        public const String SQL_COLMN_COLLEGE_REATING = "universityRating";

    }
}
