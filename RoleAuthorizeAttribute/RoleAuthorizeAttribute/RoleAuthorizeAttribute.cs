﻿using System;
using System.Linq;
using System.Security.Principal;
using System.Web.Mvc;

namespace RoleAuthorize.Mvc
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public sealed class RoleAuthorizeAttribute : System.Web.Mvc.AuthorizeAttribute, IAuthorizationFilter //TODO: Change how this works so we don't inherit AuthorizeAttribute, it brings some junk along.
    {
        private static readonly char[] Delimiter = new char[] { ',' };

        private string _RoleNames;

        public string RoleNames
        {
            get { return _RoleNames ?? String.Empty; }
            set
            {
                _RoleNames = value;
                if (value == null)
                    RoleNamesSplit = new string[0];
                else
                    RoleNamesSplit = value.Split(Delimiter).Select(_ => _.Trim()).Where(_ => !String.IsNullOrEmpty(_)).ToArray();
            }
        }

        private string[] RoleNamesSplit = new string[0];

        protected override bool AuthorizeCore(System.Web.HttpContextBase httpContext)
        {
            if (httpContext == null)
                throw new ArgumentNullException("httpContext");

            IPrincipal user = httpContext.User;
            if (user == null || user.Identity == null || !user.Identity.IsAuthenticated)
                return false;

            var users = RoleNamesSplit.SelectMany(_ => Config.RoleConfig.GetUsers(_)).ToList();
            if (users.Count > 0 && !users.Contains(user.Identity.Name, StringComparer.OrdinalIgnoreCase))
                return false;

            var roles = RoleNamesSplit.SelectMany(_ => Config.RoleConfig.GetRoles(_)).ToList();
            if (roles.Count > 0 && !roles.Any(user.IsInRole))
                return false;

            if (users.Count == 0 && roles.Count == 0 && !Config.RoleConfig.DefaultAllow)
                return false;

            return true;
        }
    }
}

namespace RoleAuthorize.Api
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
    public sealed class RoleAuthorizeAttribute : System.Web.Http.AuthorizeAttribute //TODO: Change how this works so we don't inherit AuthorizeAttribute, it brings some junk along.
    {
        private static readonly char[] Delimiter = new char[] { ',' };

        private string _RoleNames;

        public string RoleNames
        {
            get { return _RoleNames ?? String.Empty; }
            set
            {
                _RoleNames = value;
                if (value == null)
                    RoleNamesSplit = new string[0];
                else
                    RoleNamesSplit = value.Split(Delimiter).Select(_ => _.Trim()).Where(_ => !String.IsNullOrEmpty(_)).ToArray();
            }
        }

        private string[] RoleNamesSplit = new string[0];

        protected override bool IsAuthorized(System.Web.Http.Controllers.HttpActionContext actionContext)
        {
            if (actionContext == null)
                throw new ArgumentNullException("actionContext");

            IPrincipal user = System.Web.HttpContext.Current.User;//TODO: FIX THIS!!!
            //IPrincipal user = actionContext.ControllerContext.Request.RequestContext.Principal;
            if (user == null || user.Identity == null || !user.Identity.IsAuthenticated)
                return false;

            var users = RoleNamesSplit.SelectMany(_ => Config.RoleConfig.GetUsers(_)).ToList();
            if (users.Count > 0 && users.Contains(user.Identity.Name, StringComparer.OrdinalIgnoreCase))
                return true;

            var roles = RoleNamesSplit.SelectMany(_ => Config.RoleConfig.GetRoles(_)).ToList();
            if (roles.Count > 0 && roles.Any(user.IsInRole))
                return true;

            //Not sure about this.
            if (Config.RoleConfig.DefaultAllow)
                return true;

            return false;
        }
    }
}