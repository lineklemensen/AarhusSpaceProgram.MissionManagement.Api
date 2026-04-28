using System;
using System.Collections.Generic;
using System.Text;

namespace AarhusSpaceProgram.Shared.DTOs
{
    public sealed class MissionLogDTO
    {
        public Guid Id { get; set; }

        public int MissionId { get; set; }

        public string Message { get; set; } = "";

        public DateTime Timestamp { get; set; }
    }
}
