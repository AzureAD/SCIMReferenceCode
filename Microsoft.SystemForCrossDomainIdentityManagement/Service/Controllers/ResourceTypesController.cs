// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.

namespace Microsoft.SCIM
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;
    using Microsoft.AspNetCore.Mvc;

    [Route(ServiceConstants.RouteResourceTypes)]
    [ApiController]
    public sealed class ResourceTypesController : ControllerTemplate
    {
        public ResourceTypesController(IProvider provider, IMonitor monitor)
            : base(provider, monitor)
        {
        }

        public IEnumerable<Core2ResourceType> Get()
        {
            string correlationIdentifier = null;

            try
            {
                HttpRequestMessage request = this.ConvertRequest();
                if (!request.TryGetRequestIdentifier(out correlationIdentifier))
                {
                    throw new HttpResponseException(HttpStatusCode.InternalServerError);
                }

                IProvider provider = this.provider;
                if (null == provider)
                {
                    throw new HttpResponseException(HttpStatusCode.InternalServerError);
                }

                IEnumerable<Core2ResourceType> result = provider.ResourceTypes;
                return result;
            }
            catch (ArgumentException argumentException)
            {
                if (this.TryGetMonitor(out IMonitor monitor))
                {
                    IExceptionNotification notification =
                        ExceptionNotificationFactory.Instance.CreateNotification(
                            argumentException,
                            correlationIdentifier,
                            ServiceNotificationIdentifiers.ResourceTypesControllerGetArgumentException);
                    monitor.Report(notification);
                }

                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }
            catch (NotImplementedException notImplementedException)
            {
                if (this.TryGetMonitor(out IMonitor monitor))
                {
                    IExceptionNotification notification =
                        ExceptionNotificationFactory.Instance.CreateNotification(
                            notImplementedException,
                            correlationIdentifier,
                            ServiceNotificationIdentifiers.ResourceTypesControllerGetNotImplementedException);
                    monitor.Report(notification);
                }

                throw new HttpResponseException(HttpStatusCode.NotImplemented);
            }
            catch (NotSupportedException notSupportedException)
            {
                if (this.TryGetMonitor(out IMonitor monitor))
                {
                    IExceptionNotification notification =
                       ExceptionNotificationFactory.Instance.CreateNotification(
                           notSupportedException,
                           correlationIdentifier,
                           ServiceNotificationIdentifiers.ResourceTypesControllerGetNotSupportedException);
                    monitor.Report(notification);
                }

                throw new HttpResponseException(HttpStatusCode.NotImplemented);
            }
            catch (Exception exception)
            {
                if (this.TryGetMonitor(out IMonitor monitor))
                {
                    IExceptionNotification notification =
                       ExceptionNotificationFactory.Instance.CreateNotification(
                           exception,
                           correlationIdentifier,
                           ServiceNotificationIdentifiers.ResourceTypesControllerGetException);
                    monitor.Report(notification);
                }

                throw;
            }
        }
    }
}
