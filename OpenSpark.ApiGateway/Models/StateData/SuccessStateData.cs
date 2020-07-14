using OpenSpark.Shared.ViewModels;
using System;

namespace OpenSpark.ApiGateway.Models.StateData
{
    public class SuccessStateData : BaseSagaStateData
    {
        public PostViewModel AddedPost { get; }
        public string GroupId { get; set; }

        public SuccessStateData(Guid transactionId, PostViewModel addedPost, string groupId) : base(transactionId)
        {
            AddedPost = addedPost;
            GroupId = groupId;
        }
    }
}