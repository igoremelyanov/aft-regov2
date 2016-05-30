﻿using System;
using System.Collections;
using System.Linq;
using AFT.RegoV2.Shared.Logging;
using AFT.RegoV2.Core.Security.ApplicationServices;
using AFT.RegoV2.Shared;
using AutoMapper;
using Elmah;
using Microsoft.Practices.ServiceLocation;

namespace AFT.RegoV2.AdminWebsite.Common.Logger
{
    public class ElmahErrorLogger : ErrorLog
    {
        private readonly LoggingService _logging;

        public ElmahErrorLogger(IDictionary config)
        {
            //todo: consider refactoring this code and removing logging service completely
            _logging = ServiceLocator.Current.GetInstance<LoggingService>();
        }

        public override string Log(Error error)
        {
            var logger = ServiceLocator.Current.GetInstance<ILog>();
            logger.Error(string.Format("Unhandled exception in AdminWebsite application: '{0}'", error.Message), error.Exception);

            //todo: consider refactoring this code and removing logging service completely
            var errorData = Mapper.DynamicMap<Core.Security.Interface.Data.Error>(error);
            errorData.Id = Guid.NewGuid();
            _logging.Log(errorData);
            return error.Message;
        }

        public override ErrorLogEntry GetError(string id)
        {
            var errorId = Guid.Parse(id);
            var error = _logging.GetError(errorId);
            if (error == null)
                throw new RegoException(string.Format("Error with id {0} not found", id));
            var elmahError = new Error
            {
                Message = error.Message,
                Source = error.Source,
                Detail = error.Detail,
                User = error.User,
                HostName = error.HostName,
                Type = error.Type,
                Time = error.Time
            };
            var entry = new ErrorLogEntry(this, id, elmahError);
            return entry;
        }

        public override int GetErrors(int pageIndex, int pageSize, IList errorEntryList)
        {
            foreach (var error in _logging.GetErrors().OrderByDescending(e => e.Time).Skip(pageIndex * pageSize).Take(pageSize))
            {
                var elmahError = new Error
                {
                    Message = error.Message,
                    Source = error.Source,
                    Detail = error.Detail,
                    User = error.User,
                    HostName = error.HostName,
                    Type = error.Type,
                    Time = error.Time
                };

                errorEntryList.Add(new ErrorLogEntry(this, error.Id.ToString(), elmahError));
            }

            return errorEntryList.Count;
        }
    }
}