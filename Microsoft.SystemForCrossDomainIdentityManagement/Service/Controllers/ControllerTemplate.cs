// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.

namespace Microsoft.SCIM
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Http;

    public abstract class ControllerTemplate : ControllerBase
    {
        internal const string AttributeValueIdentifier = "{identifier}";
        private const string HeaderKeyContentType = "Content-Type";
        private const string HeaderKeyLocation = "Location";

        internal readonly IMonitor monitor;
        internal readonly IProvider provider;

        internal ControllerTemplate(IProvider provider, IMonitor monitor)
        {
            this.monitor = monitor;
            this.provider = provider;
        }

        protected virtual void ConfigureResponse(Resource resource)
        {
            this.Response.ContentType = ProtocolConstants.ContentType;
            this.Response.StatusCode = (int)HttpStatusCode.Created;

            if (null == this.Response.Headers)
            {
                return;
            }

            if (!this.Response.Headers.ContainsKey(ControllerTemplate.HeaderKeyContentType))
            {
                this.Response.Headers.Add(ControllerTemplate.HeaderKeyContentType, ProtocolConstants.ContentType);
            }

            Uri baseResourceIdentifier = this.HttpContext.GetBaseResourceIdentifier();
            Uri resourceIdentifier = resource.GetResourceIdentifier(baseResourceIdentifier);
            string resourceLocation = resourceIdentifier.AbsoluteUri;
            if (!this.Response.Headers.ContainsKey(ControllerTemplate.HeaderKeyLocation))
            {
                this.Response.Headers.Add(ControllerTemplate.HeaderKeyLocation, resourceLocation);
            }
        }

        protected ObjectResult ScimError(HttpStatusCode httpStatusCode, string message)
        {
            return StatusCode((int)httpStatusCode, new Core2Error(message, (int)httpStatusCode));
        }
        protected ObjectResult ScimError(HttpStatusCode httpStatusCode, string message, ErrorType errorType)
        {
            return StatusCode((int)httpStatusCode, new Core2Error(message, (int)httpStatusCode, Enum.GetName(errorType)));
        }

        protected virtual bool TryGetMonitor(out IMonitor monitor)
        {
            monitor = this.monitor;
            if (null == monitor)
            {
                return false;
            }

            return true;
        }
    }

    public abstract class ControllerTemplate<T> : ControllerTemplate where T : Resource
    {
        internal ControllerTemplate(IProvider provider, IMonitor monitor)
            : base(provider, monitor)
        {
        }

        protected abstract IProviderAdapter<T> AdaptProvider(IProvider provider);

        protected virtual IProviderAdapter<T> AdaptProvider()
        {
            IProviderAdapter<T> result = this.AdaptProvider(this.provider);
            return result;
        }


        [HttpDelete(ControllerTemplate.AttributeValueIdentifier)]
        public virtual async Task<IActionResult> Delete(string identifier)
        {
            string correlationIdentifier = null;
            try
            {
                if (string.IsNullOrWhiteSpace(identifier))
                {
                    return this.BadRequest();
                }

                identifier = Uri.UnescapeDataString(identifier);
                HttpContext httpContext = this.HttpContext;
                if (!httpContext.TryGetRequestIdentifier(out correlationIdentifier))
                {
                    return this.StatusCode((int)HttpStatusCode.NotImplemented);
                }

                IProviderAdapter<T> provider = this.AdaptProvider();
                await provider.Delete(httpContext, identifier, correlationIdentifier).ConfigureAwait(false);
                return this.NoContent();
            }
            catch (ArgumentException argumentException)
            {
                if (this.TryGetMonitor(out IMonitor monitor))
                {
                    IExceptionNotification notification =
                        ExceptionNotificationFactory.Instance.CreateNotification(
                            argumentException,
                            correlationIdentifier,
                            ServiceNotificationIdentifiers.ControllerTemplateDeleteArgumentException);
                    monitor.Report(notification);
                }

                return this.ScimError(HttpStatusCode.BadRequest, argumentException.Message);
            }
            catch (CustomHttpResponseException responseException)
            {
                if (responseException?.StatusCode == HttpStatusCode.NotFound)
                {
                    return this.NotFound();
                }
                if (this.TryGetMonitor(out IMonitor monitor))
                {
                    IExceptionNotification notification =
                        ExceptionNotificationFactory.Instance.CreateNotification(
                            responseException.InnerException ?? responseException,
                            correlationIdentifier,
                            ServiceNotificationIdentifiers.ControllerTemplateGetException);
                    monitor.Report(notification);
                }

                return this.ScimError(HttpStatusCode.InternalServerError, responseException.Message);
            }
            catch (NotImplementedException notImplementedException)
            {
                if (this.TryGetMonitor(out IMonitor monitor))
                {
                    IExceptionNotification notification =
                        ExceptionNotificationFactory.Instance.CreateNotification(
                            notImplementedException,
                            correlationIdentifier,
                            ServiceNotificationIdentifiers.ControllerTemplateDeleteNotImplementedException);
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
                            ServiceNotificationIdentifiers.ControllerTemplateDeleteNotSupportedException);
                    monitor.Report(notification);
                }

                return this.StatusCode((int)HttpStatusCode.NotImplemented);
            }
            catch (ScimTypeException scimException)
            {
                if (this.TryGetMonitor(out IMonitor monitor))
                {
                    IExceptionNotification notification =
                        ExceptionNotificationFactory.Instance.CreateNotification(
                            scimException,
                            correlationIdentifier,
                            ServiceNotificationIdentifiers.ControllerTemplateDeleteNotSupportedException);
                    monitor.Report(notification);
                }

                return this.ScimError(HttpStatusCode.BadRequest, scimException.Message, scimException.ErrorType);
            }
            catch (Exception exception)
            {
                if (this.TryGetMonitor(out IMonitor monitor))
                {
                    IExceptionNotification notification =
                        ExceptionNotificationFactory.Instance.CreateNotification(
                            exception,
                            correlationIdentifier,
                            ServiceNotificationIdentifiers.ControllerTemplateDeleteException);
                    monitor.Report(notification);
                }

                return this.ScimError(HttpStatusCode.InternalServerError, exception.Message);
            }
        }

        [HttpGet]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords",
            MessageId = "Get",
            Justification =
                "The names of the methods of a controller must correspond to the names of hypertext markup verbs")]
        public virtual async Task<ActionResult<QueryResponseBase>> Get()
        {
            string correlationIdentifier = null;
            try
            {
                HttpContext httpContext = this.HttpContext;
                if (!httpContext.TryGetRequestIdentifier(out correlationIdentifier))
                {
                    return this.StatusCode((int)HttpStatusCode.InternalServerError);
                }

                IResourceQuery resourceQuery = new ResourceQuery(HttpContext);
                IProviderAdapter<T> provider = this.AdaptProvider();
                QueryResponseBase result =
                    await provider
                        .Query(
                            httpContext,
                            resourceQuery.Filters,
                            resourceQuery.Attributes,
                            resourceQuery.ExcludedAttributes,
                            resourceQuery.PaginationParameters,
                            correlationIdentifier)
                        .ConfigureAwait(false);
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
                            ServiceNotificationIdentifiers.ControllerTemplateQueryArgumentException);
                    monitor.Report(notification);
                }

                return this.ScimError(HttpStatusCode.BadRequest, argumentException.Message);
            }
            catch (NotImplementedException notImplementedException)
            {
                if (this.TryGetMonitor(out IMonitor monitor))
                {
                    IExceptionNotification notification =
                        ExceptionNotificationFactory.Instance.CreateNotification(
                            notImplementedException,
                            correlationIdentifier,
                            ServiceNotificationIdentifiers.ControllerTemplateQueryNotImplementedException);
                    monitor.Report(notification);
                }

                return this.ScimError(HttpStatusCode.NotImplemented, notImplementedException.Message);
            }
            catch (NotSupportedException notSupportedException)
            {
                if (this.TryGetMonitor(out IMonitor monitor))
                {
                    IExceptionNotification notification =
                        ExceptionNotificationFactory.Instance.CreateNotification(
                            notSupportedException,
                            correlationIdentifier,
                            ServiceNotificationIdentifiers.ControllerTemplateQueryNotSupportedException);
                    monitor.Report(notification);
                }

                return this.ScimError(HttpStatusCode.BadRequest, notSupportedException.Message);
            }
            catch (ScimTypeException scimException)
            {
                if (this.TryGetMonitor(out IMonitor monitor))
                {
                    IExceptionNotification notification =
                        ExceptionNotificationFactory.Instance.CreateNotification(
                            scimException,
                            correlationIdentifier,
                            ServiceNotificationIdentifiers.ControllerTemplateDeleteNotSupportedException);
                    monitor.Report(notification);
                }

                return this.ScimError(HttpStatusCode.BadRequest, scimException.Message, scimException.ErrorType);
            }
            catch (CustomHttpResponseException responseException)
            {
                if (responseException?.StatusCode != HttpStatusCode.NotFound)
                {
                    if (this.TryGetMonitor(out IMonitor monitor))
                    {
                        IExceptionNotification notification =
                            ExceptionNotificationFactory.Instance.CreateNotification(
                                responseException.InnerException ?? responseException,
                                correlationIdentifier,
                                ServiceNotificationIdentifiers.ControllerTemplateGetException);
                        monitor.Report(notification);
                    }
                }

                return this.ScimError(HttpStatusCode.InternalServerError, responseException.Message);
            }
            catch (Exception exception)
            {
                if (this.TryGetMonitor(out IMonitor monitor))
                {
                    IExceptionNotification notification =
                        ExceptionNotificationFactory.Instance.CreateNotification(
                            exception,
                            correlationIdentifier,
                            ServiceNotificationIdentifiers.ControllerTemplateQueryException);
                    monitor.Report(notification);
                }

                return this.ScimError(HttpStatusCode.InternalServerError, exception.Message);
            }
        }

        [HttpGet(ControllerTemplate.AttributeValueIdentifier)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords",
            MessageId = "Get",
            Justification =
                "The names of the methods of a controller must correspond to the names of hypertext markup verbs")]
        public virtual async Task<ActionResult<Resource>> Get([FromRoute] string identifier)
        {
            string correlationIdentifier = null;
            try
            {
                if (string.IsNullOrWhiteSpace(identifier))
                {
                    return this.ScimError(HttpStatusCode.BadRequest,
                        SystemForCrossDomainIdentityManagementServiceResources.ExceptionInvalidIdentifier);
                }

                HttpContext httpContext = this.HttpContext;
                if (!httpContext.TryGetRequestIdentifier(out correlationIdentifier))
                {
                    return this.StatusCode((int)HttpStatusCode.InternalServerError);
                }

                IResourceQuery resourceQuery = new ResourceQuery(HttpContext);
                if (resourceQuery.Filters.Any())
                {
                    if (resourceQuery.Filters.Count != 1)
                    {
                        return this.ScimError(HttpStatusCode.BadRequest,
                            SystemForCrossDomainIdentityManagementServiceResources.ExceptionFilterCount);
                    }

                    IFilter filter = new Filter(AttributeNames.Identifier, ComparisonOperator.Equals, identifier);
                    filter.AdditionalFilter = resourceQuery.Filters.Single();
                    IReadOnlyCollection<IFilter> filters =
                        new IFilter[]
                        {
                            filter
                        };
                    IResourceQuery effectiveQuery =
                        new ResourceQuery(
                            filters,
                            resourceQuery.Attributes,
                            resourceQuery.ExcludedAttributes);
                    IProviderAdapter<T> provider = this.AdaptProvider();
                    QueryResponseBase queryResponse =
                        await provider
                            .Query(
                                httpContext,
                                effectiveQuery.Filters,
                                effectiveQuery.Attributes,
                                effectiveQuery.ExcludedAttributes,
                                effectiveQuery.PaginationParameters,
                                correlationIdentifier)
                            .ConfigureAwait(false);
                    if (!queryResponse.Resources.Any())
                    {
                        return this.ScimError(HttpStatusCode.NotFound,
                            string.Format(
                                SystemForCrossDomainIdentityManagementServiceResources.ResourceNotFoundTemplate,
                                identifier));
                    }

                    Resource result = queryResponse.Resources.Single();
                    return this.Ok(result);
                }
                else
                {
                    IProviderAdapter<T> provider = this.AdaptProvider();
                    Resource result =
                        await provider
                            .Retrieve(
                                httpContext,
                                identifier,
                                resourceQuery.Attributes,
                                resourceQuery.ExcludedAttributes,
                                correlationIdentifier)
                            .ConfigureAwait(false);
                    if (null == result)
                    {
                        return this.ScimError(HttpStatusCode.NotFound,
                            string.Format(
                                SystemForCrossDomainIdentityManagementServiceResources.ResourceNotFoundTemplate,
                                identifier));
                    }

                    return result;
                }
            }
            catch (ArgumentException argumentException)
            {
                if (this.TryGetMonitor(out IMonitor monitor))
                {
                    IExceptionNotification notification =
                        ExceptionNotificationFactory.Instance.CreateNotification(
                            argumentException,
                            correlationIdentifier,
                            ServiceNotificationIdentifiers.ControllerTemplateGetArgumentException);
                    monitor.Report(notification);
                }

                return this.ScimError(HttpStatusCode.BadRequest, argumentException.Message);
            }
            catch (NotImplementedException notImplementedException)
            {
                if (this.TryGetMonitor(out IMonitor monitor))
                {
                    IExceptionNotification notification =
                        ExceptionNotificationFactory.Instance.CreateNotification(
                            notImplementedException,
                            correlationIdentifier,
                            ServiceNotificationIdentifiers.ControllerTemplateGetNotImplementedException);
                    monitor.Report(notification);
                }

                return this.ScimError(HttpStatusCode.NotImplemented, notImplementedException.Message);
            }
            catch (NotSupportedException notSupportedException)
            {
                if (this.TryGetMonitor(out IMonitor monitor))
                {
                    IExceptionNotification notification =
                        ExceptionNotificationFactory.Instance.CreateNotification(
                            notSupportedException,
                            correlationIdentifier,
                            ServiceNotificationIdentifiers.ControllerTemplateGetNotSupportedException);
                    monitor.Report(notification);
                }

                return this.ScimError(HttpStatusCode.BadRequest, notSupportedException.Message);
            }
            catch (ScimTypeException scimException)
            {
                if (this.TryGetMonitor(out IMonitor monitor))
                {
                    IExceptionNotification notification =
                        ExceptionNotificationFactory.Instance.CreateNotification(
                            scimException,
                            correlationIdentifier,
                            ServiceNotificationIdentifiers.ControllerTemplateDeleteNotSupportedException);
                    monitor.Report(notification);
                }

                return this.ScimError(HttpStatusCode.BadRequest, scimException.Message, scimException.ErrorType);
            }
            catch (CustomHttpResponseException responseException)
            {
                if (responseException?.StatusCode != HttpStatusCode.NotFound)
                {
                    if (this.TryGetMonitor(out IMonitor monitor))
                    {
                        IExceptionNotification notification =
                            ExceptionNotificationFactory.Instance.CreateNotification(
                                responseException.InnerException ?? responseException,
                                correlationIdentifier,
                                ServiceNotificationIdentifiers.ControllerTemplateGetException);
                        monitor.Report(notification);
                    }
                }

                if (responseException?.StatusCode == HttpStatusCode.NotFound)
                {
                    return this.ScimError(HttpStatusCode.NotFound,
                        string.Format(SystemForCrossDomainIdentityManagementServiceResources.ResourceNotFoundTemplate,
                            identifier));
                }

                return this.ScimError(HttpStatusCode.InternalServerError, responseException.Message);
            }
            catch (Exception exception)
            {
                if (this.TryGetMonitor(out IMonitor monitor))
                {
                    IExceptionNotification notification =
                        ExceptionNotificationFactory.Instance.CreateNotification(
                            exception,
                            correlationIdentifier,
                            ServiceNotificationIdentifiers.ControllerTemplateGetException);
                    monitor.Report(notification);
                }

                return this.ScimError(HttpStatusCode.InternalServerError, exception.Message);
            }
        }

        [HttpPatch(ControllerTemplate.AttributeValueIdentifier)]
        public virtual async Task<ActionResult<Resource>> Patch(string identifier,
            [FromBody] PatchRequest2 patchRequest)
        {
            string correlationIdentifier = null;

            try
            {
                if (string.IsNullOrWhiteSpace(identifier))
                {
                    return this.BadRequest();
                }

                identifier = Uri.UnescapeDataString(identifier);

                if (null == patchRequest)
                {
                    return this.BadRequest();
                }

                HttpContext httpContext = this.HttpContext;
                if (!httpContext.TryGetRequestIdentifier(out correlationIdentifier))
                {
                    return StatusCode((int)HttpStatusCode.InternalServerError);
                }

                IProviderAdapter<T> provider = this.AdaptProvider();
                await provider.Update(httpContext, identifier, patchRequest, correlationIdentifier)
                    .ConfigureAwait(false);

                // If EnterpriseUser, return HTTP code 200 and user object, otherwise HTTP code 204
                if (provider.SchemaIdentifier == SchemaIdentifiers.Core2EnterpriseUser)
                {
                    return await this.Get(identifier).ConfigureAwait(false);
                }
                else
                    return this.NoContent();
            }
            catch (ArgumentException argumentException)
            {
                if (this.TryGetMonitor(out IMonitor monitor))
                {
                    IExceptionNotification notification =
                        ExceptionNotificationFactory.Instance.CreateNotification(
                            argumentException,
                            correlationIdentifier,
                            ServiceNotificationIdentifiers.ControllerTemplatePatchArgumentException);
                    monitor.Report(notification);
                }

                return this.ScimError(HttpStatusCode.BadRequest, argumentException.Message);
            }
            catch (NotImplementedException notImplementedException)
            {
                if (this.TryGetMonitor(out IMonitor monitor))
                {
                    IExceptionNotification notification =
                        ExceptionNotificationFactory.Instance.CreateNotification(
                            notImplementedException,
                            correlationIdentifier,
                            ServiceNotificationIdentifiers.ControllerTemplatePatchNotImplementedException);
                    monitor.Report(notification);
                }

                return StatusCode((int)HttpStatusCode.NotImplemented);
            }
            catch (NotSupportedException notSupportedException)
            {
                if (this.TryGetMonitor(out IMonitor monitor))
                {
                    IExceptionNotification notification =
                        ExceptionNotificationFactory.Instance.CreateNotification(
                            notSupportedException,
                            correlationIdentifier,
                            ServiceNotificationIdentifiers.ControllerTemplatePatchNotSupportedException);
                    monitor.Report(notification);
                }

                return this.StatusCode((int)HttpStatusCode.NotImplemented);
            }
            catch (ScimTypeException scimException)
            {
                if (this.TryGetMonitor(out IMonitor monitor))
                {
                    IExceptionNotification notification =
                        ExceptionNotificationFactory.Instance.CreateNotification(
                            scimException,
                            correlationIdentifier,
                            ServiceNotificationIdentifiers.ControllerTemplateDeleteNotSupportedException);
                    monitor.Report(notification);
                }

                return this.ScimError(scimException.ErrorType == ErrorType.uniqueness ? HttpStatusCode.Conflict : HttpStatusCode.BadRequest, scimException.Message, scimException.ErrorType);
            }
            catch (CustomHttpResponseException responseException)
            {
                if (responseException?.StatusCode == HttpStatusCode.NotFound)
                {
                    return this.NotFound();
                }
                else
                {
                    if (this.TryGetMonitor(out IMonitor monitor))
                    {
                        IExceptionNotification notification =
                            ExceptionNotificationFactory.Instance.CreateNotification(
                                responseException.InnerException ?? responseException,
                                correlationIdentifier,
                                ServiceNotificationIdentifiers.ControllerTemplateGetException);
                        monitor.Report(notification);
                    }
                }

                return this.ScimError(responseException.StatusCode, responseException.Message);
            }
            catch (Exception exception)
            {
                if (this.TryGetMonitor(out IMonitor monitor))
                {
                    IExceptionNotification notification =
                        ExceptionNotificationFactory.Instance.CreateNotification(
                            exception,
                            correlationIdentifier,
                            ServiceNotificationIdentifiers.ControllerTemplatePatchException);
                    monitor.Report(notification);
                }

                return this.StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }

        [HttpPost]
        public virtual async Task<ActionResult<Resource>> Post([FromBody] T resource)
        {
            string correlationIdentifier = null;

            try
            {
                if (null == resource)
                {
                    return this.BadRequest();
                }

                HttpContext httpContext = this.HttpContext;
                if (!httpContext.TryGetRequestIdentifier(out correlationIdentifier))
                {
                    return this.StatusCode((int)HttpStatusCode.InternalServerError);
                }

                IProviderAdapter<T> provider = this.AdaptProvider();
                Resource result = await provider.Create(httpContext, resource, correlationIdentifier)
                    .ConfigureAwait(false);
                this.ConfigureResponse(result);
                return this.CreatedAtAction(nameof(Post), result);
            }
            catch (ArgumentException argumentException)
            {
                if (this.TryGetMonitor(out IMonitor monitor))
                {
                    IExceptionNotification notification =
                        ExceptionNotificationFactory.Instance.CreateNotification(
                            argumentException,
                            correlationIdentifier,
                            ServiceNotificationIdentifiers.ControllerTemplatePostArgumentException);
                    monitor.Report(notification);
                }

                return this.ScimError(HttpStatusCode.BadRequest, argumentException.Message);;
            }
            catch (NotImplementedException notImplementedException)
            {
                if (this.TryGetMonitor(out IMonitor monitor))
                {
                    IExceptionNotification notification =
                        ExceptionNotificationFactory.Instance.CreateNotification(
                            notImplementedException,
                            correlationIdentifier,
                            ServiceNotificationIdentifiers.ControllerTemplatePostNotImplementedException);
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
                            ServiceNotificationIdentifiers.ControllerTemplatePostNotSupportedException);
                    monitor.Report(notification);
                }

                return this.StatusCode((int)HttpStatusCode.NotImplemented);
            }
            catch (ScimTypeException scimException)
            {
                if (this.TryGetMonitor(out IMonitor monitor))
                {
                    IExceptionNotification notification =
                        ExceptionNotificationFactory.Instance.CreateNotification(
                            scimException,
                            correlationIdentifier,
                            ServiceNotificationIdentifiers.ControllerTemplateDeleteNotSupportedException);
                    monitor.Report(notification);
                }

                return this.ScimError(scimException.ErrorType == ErrorType.uniqueness ? HttpStatusCode.Conflict : HttpStatusCode.BadRequest, scimException.Message, scimException.ErrorType);
            }
            catch (CustomHttpResponseException httpResponseException)
            {
                if (this.TryGetMonitor(out IMonitor monitor))
                {
                    IExceptionNotification notification =
                        ExceptionNotificationFactory.Instance.CreateNotification(
                            httpResponseException,
                            correlationIdentifier,
                            ServiceNotificationIdentifiers.ControllerTemplatePostNotSupportedException);
                    monitor.Report(notification);
                }

                if (httpResponseException.StatusCode == HttpStatusCode.Conflict)
                    return this.ScimError(HttpStatusCode.Conflict,
                        SystemForCrossDomainIdentityManagementServiceResources.ExceptionInvalidRequest);
                else
                    return this.ScimError(HttpStatusCode.BadRequest, httpResponseException.Message);
            }
            catch (Exception exception)
            {
                if (this.TryGetMonitor(out IMonitor monitor))
                {
                    IExceptionNotification notification =
                        ExceptionNotificationFactory.Instance.CreateNotification(
                            exception,
                            correlationIdentifier,
                            ServiceNotificationIdentifiers.ControllerTemplatePostException);
                    monitor.Report(notification);
                }

                return this.StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }

        [HttpPut(ControllerTemplate.AttributeValueIdentifier)]
        public virtual async Task<ActionResult<Resource>> Put([FromBody] T resource, string identifier)
        {
            string correlationIdentifier = null;

            try
            {
                if (null == resource)
                {
                    return this.ScimError(HttpStatusCode.BadRequest,
                        SystemForCrossDomainIdentityManagementServiceResources.ExceptionInvalidResource);
                }

                if (string.IsNullOrEmpty(identifier))
                {
                    return this.ScimError(HttpStatusCode.BadRequest,
                        SystemForCrossDomainIdentityManagementServiceResources.ExceptionInvalidIdentifier);
                }

                HttpContext httpContext = this.HttpContext;
                if (!httpContext.TryGetRequestIdentifier(out correlationIdentifier))
                {
                    return this.StatusCode((int)HttpStatusCode.InternalServerError);
                }

                IProviderAdapter<T> provider = this.AdaptProvider();
                Resource result = await provider.Replace(httpContext, resource, correlationIdentifier)
                    .ConfigureAwait(false);
                this.ConfigureResponse(result);
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
                            ServiceNotificationIdentifiers.ControllerTemplatePutArgumentException);
                    monitor.Report(notification);
                }

                return this.ScimError(HttpStatusCode.BadRequest, argumentException.Message);
            }
            catch (NotImplementedException notImplementedException)
            {
                if (this.TryGetMonitor(out IMonitor monitor))
                {
                    IExceptionNotification notification =
                        ExceptionNotificationFactory.Instance.CreateNotification(
                            notImplementedException,
                            correlationIdentifier,
                            ServiceNotificationIdentifiers.ControllerTemplatePutNotImplementedException);
                    monitor.Report(notification);
                }

                return this.ScimError(HttpStatusCode.NotImplemented, notImplementedException.Message);
            }
            catch (NotSupportedException notSupportedException)
            {
                if (this.TryGetMonitor(out IMonitor monitor))
                {
                    IExceptionNotification notification =
                        ExceptionNotificationFactory.Instance.CreateNotification(
                            notSupportedException,
                            correlationIdentifier,
                            ServiceNotificationIdentifiers.ControllerTemplatePutNotSupportedException);
                    monitor.Report(notification);
                }

                return this.ScimError(HttpStatusCode.BadRequest, notSupportedException.Message);
            }
            catch (ScimTypeException scimException)
            {
                if (this.TryGetMonitor(out IMonitor monitor))
                {
                    IExceptionNotification notification =
                        ExceptionNotificationFactory.Instance.CreateNotification(
                            scimException,
                            correlationIdentifier,
                            ServiceNotificationIdentifiers.ControllerTemplateDeleteNotSupportedException);
                    monitor.Report(notification);
                }

                return this.ScimError(scimException.ErrorType == ErrorType.uniqueness ? HttpStatusCode.Conflict : HttpStatusCode.BadRequest, scimException.Message, scimException.ErrorType);
            }
            catch (CustomHttpResponseException httpResponseException)
            {
                if (this.TryGetMonitor(out IMonitor monitor))
                {
                    IExceptionNotification notification =
                        ExceptionNotificationFactory.Instance.CreateNotification(
                            httpResponseException,
                            correlationIdentifier,
                            ServiceNotificationIdentifiers.ControllerTemplatePostNotSupportedException);
                    monitor.Report(notification);
                }

                if (httpResponseException.StatusCode == HttpStatusCode.NotFound)
                    return this.ScimError(HttpStatusCode.NotFound,
                        string.Format(SystemForCrossDomainIdentityManagementServiceResources.ResourceNotFoundTemplate,
                            identifier));
                else if (httpResponseException.StatusCode == HttpStatusCode.Conflict)
                    return this.ScimError(HttpStatusCode.Conflict,
                        SystemForCrossDomainIdentityManagementServiceResources.ExceptionInvalidRequest);
                else
                    return this.ScimError(HttpStatusCode.BadRequest, httpResponseException.Message);
            }
            catch (Exception exception)
            {
                if (this.TryGetMonitor(out IMonitor monitor))
                {
                    IExceptionNotification notification =
                        ExceptionNotificationFactory.Instance.CreateNotification(
                            exception,
                            correlationIdentifier,
                            ServiceNotificationIdentifiers.ControllerTemplatePutException);
                    monitor.Report(notification);
                }

                return this.StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }
    }
}
