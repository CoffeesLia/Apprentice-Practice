using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Application.Interfaces.Repositories
{
    public interface IMemberRepository
    {
        bool IsEmailUnique(string email);
        void AddEntityMember(EntityMember entityMember);
    }
}
