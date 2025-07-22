using Stellantis.ProjectName.Domain.Entities;
using System;

namespace Stellantis.ProjectName.Domain.Entities


{
    public class Knowledge : EntityBase
    {
        public int MemberId { get; set; }
        public Member Member { get; set; } = null!;

        public int ApplicationId { get; set; }
        public ApplicationData Application { get; set; } = null!;

       // armazena o Squad no momento da associação(para regras de negócio)
        public int SquadIdAtAssociationTime { get; set; }

    }
}

