using System;

namespace DataConnection.Models
{
    public interface ISoftDeleteDataPacket : IDataPacket
    {
        DateTime? DeletedAt { get; set; }
    }
}