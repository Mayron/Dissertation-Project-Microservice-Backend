﻿// ReSharper disable UnusedMember.Global

using Microsoft.AspNetCore.Authorization;
using OpenSpark.ApiGateway.Handlers;

namespace OpenSpark.ApiGateway.ApiHub
{
    public partial class ApiHub
    {
        public void FetchGroup(string callback, string groupId) =>
            _mediator.Send(new FetchGroupDetails.Query(Context.ConnectionId, callback, groupId));

        public void FetchGroupProjects(string callback, string groupId) =>
            _mediator.Send(new FetchGroupProjects.Query(Context.ConnectionId, callback, groupId, 5));

        [Authorize]
        public void FetchUserGroups(string callback) =>
            _mediator.Send(new FetchUserGroups.Query(Context.ConnectionId, callback, owned: true));

        [Authorize]
        public void FetchUserMemberships(string callback) =>
            _mediator.Send(new FetchUserGroups.Query(Context.ConnectionId, callback, memberships: true));

        public void FetchCategories(string callback) =>
            _mediator.Send(new FetchCategories.Query(Context.ConnectionId, callback));

        public void SearchGroups(string callback, string searchQuery) =>
            _mediator.Send(new SearchGroups.Query(Context.ConnectionId, callback, searchQuery));

        public void FetchGroupPosts(string callback, string groupId) =>
            _mediator.Send(new FetchGroupPosts.Query(Context.ConnectionId, callback, groupId));
    }
}