using AarhusSpaceProgram.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace MissionLogGeneratorService
{
    public class MissionListResponseDTO
    {
        public List<MissionSimpleListItemDTO> Data { get; set; } = new();
    }
}
