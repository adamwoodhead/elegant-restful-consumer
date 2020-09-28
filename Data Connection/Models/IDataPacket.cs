using System;

namespace DataConnection.Models
{
    public interface IDataPacket
    {
        DateTime CreatedAt { get; set; }
        int? ID { get; set; }
        DateTime UpdatedAt { get; set; }
    }
}