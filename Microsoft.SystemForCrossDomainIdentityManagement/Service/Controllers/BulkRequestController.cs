//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.SCIM
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Threading.Tasks;

    [Route(ServiceConstants.RouteBulk)]
    [Authorize]
    [ApiController]
    public sealed class BulkRequestController : ControllerTemplate
    {
        public BulkRequestController(IProvider provider, IMonitor monitor)
            : base(provider, monitor)
        {
        }

        public async Task<ActionResult<BulkResponse2>> Post([FromBody] BulkRequest2 bulkRequest)
        {
            string correlationIdentifier = null;

            try
            {
                HttpContext httpContext = this.HttpContext;
                if (null == bulkRequest)
                {
                    return this.BadRequest();
                }

                if (!httpContext.TryGetRequestIdentifier(out correlationIdentifier))
                {
                    return this.StatusCode((int)HttpStatusCode.InternalServerError);
                }

                IProvider provider = this.provider;
                if (null == provider)
                {
                    return this.StatusCode((int)HttpStatusCode.InternalServerError);
                }

                IReadOnlyCollection<IExtension> extensions = provider.ReadExtensions();
                IRequest<BulkRequest2> request2 = new SystemForCrossDomainIdentityManagementRequest<BulkRequest2>(httpContext, bulkRequest, correlationIdentifier, extensions);
                BulkResponse2 result = await provider.ProcessAsync(request2).ConfigureAwait(false);
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
                            ServiceNotificationIdentifiers.BulkRequest2ControllerPostArgumentException);
                    monitor.Report(notification);
                }

                return this.BadRequest();
            }
            catch (NotImplementedException notImplementedException)
            {
                if (this.TryGetMonitor(out IMonitor monitor))
                {
                    IExceptionNotification notification =
                        ExceptionNotificationFactory.Instance.CreateNotification(
                            notImplementedException,
                            correlationIdentifier,
                            ServiceNotificationIdentifiers.BulkRequest2ControllerPostNotImplementedException);
                    monitor.Report(notification);
                }
                return this.StatusCode((int)HttpStatusCode.NotImplemented);
            }
            catch (NotSupportedException notSupportedException)
            {
                if (this.TryGetMonitor(out IMonitor monitor))
                {
                    IExceptionNotification notification =
                        ExceptionNotificationFactory.Instance.CreateNotification(
                            notSupportedException,
                            correlationIdentifier,
                            ServiceNotificationIdentifiers.BulkRequest2ControllerPostNotSupportedException);
                    monitor.Report(notification);
                }

                return this.StatusCode((int)HttpStatusCode.NotImplemented);
            }
            catch (Exception exception)
            {
                if (this.TryGetMonitor(out IMonitor monitor))
                {
                    IExceptionNotification notification =
                        ExceptionNotificationFactory.Instance.CreateNotification(
                            exception,
                            correlationIdentifier,
                            ServiceNotificationIdentifiers.BulkRequest2ControllerPostException);
                    monitor.Report(notification);
                }

                return this.StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }
    }
}
