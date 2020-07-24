using System;

namespace OpenSpark.Shared.Queries
{
    public interface IQuery : IMessage
    {
        QueryMetaData MetaData { get; set; }
    }
}