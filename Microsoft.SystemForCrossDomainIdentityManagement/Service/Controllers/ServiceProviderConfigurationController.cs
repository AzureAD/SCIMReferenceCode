// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.

namespace Microsoft.SCIM
{
    using System;
    using System.Net;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;

    [Route(ServiceConstants.RouteServiceConfiguration)]
    [Authorize]
    [ApiController]
    public sealed class ServiceProviderConfigurationController : ControllerTemplate
    {
        public ServiceProviderConfigurationController(IProvider provider, IMonitor monitor)
            : base(provider, monitor)
        {
        }

        public ActionResult<ServiceConfigurationBase> Get()
        {
            string correlationIdentifier = null;

            try
            {
                HttpContext httpContext = this.HttpContext;
                if (!httpContext.TryGetRequestIdentifier(out correlationIdentifier))
                {
                    return this.StatusCode((int) HttpStatusCode.InternalServerError);
                }

                IProvider provider = this.provider;
                if (null == provider)
                {
                    return this.StatusCode((int) HttpStatusCode.InternalServerError);
                }

                ServiceConfigurationBase result = provider.Configuration;
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
                            ServiceNotificationIdentifiers.ServiceProviderConfigurationControllerGetArgumentException);
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
                            ServiceNotificationIdentifiers.ServiceProviderConfigurationControllerGetNotImplementedException);
                    monitor.Report(notification);
                }

                return this.StatusCode((int) HttpStatusCode.NotImplemented);
            }
            catch (NotSupportedException notSupportedException)
            {
                if (this.TryGetMonitor(out IMonitor monitor))
                {
                    IExceptionNotification notification =
                        ExceptionNotificationFactory.Instance.CreateNotification(
                            notSupportedException,
                            correlationIdentifier,
                            ServiceNotificationIdentifiers.ServiceProviderConfigurationControllerGetNotSupportedException);
                    monitor.Report(notification);
                }

                return this.StatusCode((int) HttpStatusCode.NotImplemented);
            }
            catch (Exception exception)
            {
                if (this.TryGetMonitor(out IMonitor monitor))
                {
                    IExceptionNotification notification =
                        ExceptionNotificationFactory.Instance.CreateNotification(
                            exception,
                            correlationIdentifier,
                            ServiceNotificationIdentifiers.ServiceProviderConfigurationControllerGetException);
                    monitor.Report(notification);
                }

                throw;
            }
        }
    }
}
