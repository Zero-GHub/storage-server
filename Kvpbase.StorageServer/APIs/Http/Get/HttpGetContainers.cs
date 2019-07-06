﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using SyslogLogging;
using WatsonWebserver;

using Kvpbase.Containers;
using Kvpbase.Core;

namespace Kvpbase
{
    public partial class StorageServer
    {
        public static HttpResponse HttpGetContainers(RequestMetadata md)
        {
            #region Validate-Authentication

            if (md.User == null)
            {
                _Logging.Log(LoggingModule.Severity.Warn, "HttpGetContainers no authentication material");
                return new HttpResponse(md.Http, 401, null, "application/json",
                    Encoding.UTF8.GetBytes(Common.SerializeJson(new ErrorResponse(3, 401, "Unauthorized.", null), true)));
            }

            #endregion

            #region Validate-Request
             
            if (!md.Params.UserGuid.ToLower().Equals(md.User.Guid.ToLower()))
            {
                _Logging.Log(LoggingModule.Severity.Warn, "HttpGetContainers user " + md.User.Guid + " attempting to retrieve container list for user " + md.Params.UserGuid);
                return new HttpResponse(md.Http, 401, null, "application/json",
                    Encoding.UTF8.GetBytes(Common.SerializeJson(new ErrorResponse(3, 401, "Unauthorized.", null), true)));
            }

            #endregion
             
            #region Retrieve-and-Respond

            List<string> containers = new List<string>();
            if (!_ContainerMgr.GetContainersByUser(md.Params.UserGuid, out containers))
            {
                _Logging.Log(LoggingModule.Severity.Warn, "HttpGetContainers unable to retrieve containers for user " + md.Params.UserGuid);
                return new HttpResponse(md.Http, 500, null, "application/json",
                    Encoding.UTF8.GetBytes(Common.SerializeJson(new ErrorResponse(4, 500, null, null), true)));
            }
            else
            {
                if (!md.Params.Stats)
                {
                    return new HttpResponse(md.Http, 200, null, "application/json",
                        Encoding.UTF8.GetBytes(Common.SerializeJson(containers, true)));
                }
                else
                {
                    List<ContainerSettings> ret = new List<ContainerSettings>();
                    foreach (string containerName in containers)
                    {
                        ContainerSettings currSettings = null;
                        if (_ContainerMgr.GetContainerSettings(md.Params.UserGuid, containerName, out currSettings))
                        {
                            ret.Add(currSettings);
                        }
                    }

                    return new HttpResponse(md.Http, 200, null, "application/json",
                        Encoding.UTF8.GetBytes(Common.SerializeJson(ret, true)));
                }
            }

            #endregion 
        }
    }
}