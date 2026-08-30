// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.

namespace Microsoft.SCIM
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;

    [Route(ServiceConstants.RouteResourceTypes)]
    [Authorize]
    [ApiController]
    public sealed class ResourceTypesController : ControllerTemplate
    {
        public ResourceTypesController(IProvider provider, IMonitor monitor)
            : base(provider, monitor)
        {
        }

        public ActionResult<QueryResponseBase> Get()
        {
            string correlationIdentifier = null;

            try
            {
                HttpContext httpContext = this.HttpContext;
                if (!httpContext.TryGetRequestIdentifier(out correlationIdentifier))
                {
                    return this.StatusCode((int)HttpStatusCode.InternalServerError);
                }

                IProvider provider = this.provider;
                if (null == provider)
                {
                    return this.StatusCode((int)HttpStatusCode.InternalServerError);
                }

                IReadOnlyCollection<Resource> resources = provider.ResourceTypes;
                QueryResponseBase result = new QueryResponse(resources);

                result.TotalResults =
                    result.ItemsPerPage =
                        resources.Count;
                result.StartIndex = 1;
                return this.Ok(result);

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
                            ServiceNotificationIdentifiers.ResourceTypesControllerGetNotImplementedException);
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
                           ServiceNotificationIdentifiers.ResourceTypesControllerGetNotSupportedException);
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
                           ServiceNotificationIdentifiers.ResourceTypesControllerGetException);
                    monitor.Report(notification);
                }

                return this.StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }
    }
}
