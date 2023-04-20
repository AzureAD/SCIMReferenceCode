// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

//namespace Microsoft.SCIM
//{
//    using System;
//    using System.Collections.Generic;
//    using System.Web.Http.Dependencies;

//    internal abstract class DependencyResolverDecorator : IDependencyResolver
//    {
//        private const string ArgumentNameInnerResolver = "innerResolver";
//        private const string ArgumentNameServiceType = "serviceType";

//        private readonly object thisLock = new object();

//        protected DependencyResolverDecorator(IDependencyResolver innerResolver)
//        {
//            this.InnerResolver = innerResolver ?? throw new ArgumentNullException(DependencyResolverDecorator.ArgumentNameInnerResolver);
//        }

//        public IDependencyResolver InnerResolver
//        {
//            get;
//            set;
//        }

//        public virtual IDependencyScope BeginScope()
//        {
//            IDependencyScope result = this.InnerResolver.BeginScope();
//            return result;
//        }

//        public void Dispose()
//        {
//            this.Dispose(true);
//            GC.SuppressFinalize(this);
//        }

//        protected virtual void Dispose(bool disposing)
//        {
//            if (disposing)
//            {
//                if (this.InnerResolver != null)
//                {
//                    lock (this.thisLock)
//                    {
//                        if (this.InnerResolver != null)
//                        {
//                            this.InnerResolver.Dispose();
//                            this.InnerResolver = null;
//                        }
//                    }
//                }
//            }
//        }

//        public virtual object GetService(Type serviceType)
//        {
//            if (null == serviceType)
//            {
//                throw new ArgumentNullException(DependencyResolverDecorator.ArgumentNameServiceType);
//            }

//            object result = this.InnerResolver.GetService(serviceType);
//            return result;
//        }

//        public virtual IEnumerable<object> GetServices(Type serviceType)
//        {
//            if (null == serviceType)
//            {
//                throw new ArgumentNullException(DependencyResolverDecorator.ArgumentNameServiceType);
//            }

//            IEnumerable<object> results = this.InnerResolver.GetServices(serviceType);
//            return results;
//        }
//    }
//}
